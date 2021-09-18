using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SporeMods.Core.Mods;
using System.Linq;
using SporeMods.Core.ModTransactions.Operations;

namespace SporeMods.Core.ModTransactions.Transactions
{
    public class InstallModTransaction : ModTransaction
    {
        public ModIdentity Identity;
        // The mod that this mod is replacing, if any
        public IInstalledMod UpgradeFromMod;
        public readonly string ModPath;
        // Because we might need to do undo, and some operators might need the zip, we can't do the 'using...' thing
        private ZipArchive _zip;

        ManagedMod _managedMod = null;

        public InstallModTransaction(string modPath, ManagedMod upgradeFromMod = null)
        {
            this.ModPath = modPath;
            this.UpgradeFromMod = upgradeFromMod;
        }

        /// <summary>
        /// Parses the mod identity from the mod ZIP. This can be executed before the executing the transaction itself,
        /// as it it doesn't modify anything. Still, it can throw a ModTransactionCommitException, in which case Rollback()
        /// should be called to close the ZIP.
        /// </summary>
        /// <returns></returns>
        public ModIdentity ParseModIdentity()
        {
            if (_zip == null)
            {
                _zip = ZipFile.OpenRead(ModPath);
            }

            var modName = Path.GetFileNameWithoutExtension(ModPath).Replace(".", "-");
            var identityOp = Operation(new ParseIdentityOp(_zip, modName));
            Identity = identityOp.Identity;
            if (!identityOp.IsGeneratedIdentity)
            {
                Operation(new ValidateModOp(Identity));
            }

            return Identity;
        }

        public override async Task<bool> CommitAsync()
        {
            if (_zip == null)
            {
                _zip = ZipFile.OpenRead(ModPath);
            }

            // 1. Read the mod identity and validate it
            if (Identity == null)
            {
                ParseModIdentity();
            }
            var unique = Identity.Unique;
            var modName = Identity.DisplayName;
            string modDirectory = Path.Combine(Settings.ModConfigsPath, unique);

            // Needed to show the configurator
            _managedMod = new ManagedMod(true, Identity);

            ProgressSignifier = new TaskProgressSignifier(Identity.DisplayName, (UpgradeFromMod != null) ? TaskCategory.Upgrade : TaskCategory.Install);
            _managedMod.ProgressSignifier = ProgressSignifier;

            // 2. Add the mod to the mod list; 
            Operation(new AddToModManagerOp(_managedMod, UpgradeFromMod));

            if ((UpgradeFromMod != null) && (UpgradeFromMod is ManagedMod mgMod))
            {
                // We use the same settings we had
                _managedMod.Configuration = new ModConfiguration(mgMod.Configuration);
            }

            // 3. Show the configurator, if any
            if (_managedMod.HasConfigurator)
            {
                await OperationAsync(new ShowConfiguratorAsyncOp(_managedMod, false));
            }
            ProgressSignifier.Status = TaskStatus.Determinate;

            if (UpgradeFromMod != null)
            {
                // To upgrade, we just delete all the files and install
                IEnumerable<string> filesToDelete = null;
                if (UpgradeFromMod != null)
                {
                    if (UpgradeFromMod is ManagedMod mMod)
                        filesToDelete = mMod.GetFilePathsToRemove();
                    else if (UpgradeFromMod is ManualInstalledFile manual)
                        filesToDelete = new List<string>()
                        {
                            FileWrite.GetFileOutputPath(manual.Location, manual.RealName, manual.IsLegacy)
                        };
                }

                foreach (var file in filesToDelete)
                {
                    Operation(new SafeDeleteFileOp(file));
                }
                
                if (UpgradeFromMod is ManagedMod mMod2)
                    Operation(new DeleteDirectoryOp(mMod2.StoragePath));
            }
                // 4. Create the directory
                Operation(new CreateDirectoryOp(modDirectory));

                // We don't increase all the progress, because EnableMod() will be called
                double totalProgress = 50.0;
                var fileEntries = _zip.Entries.Where(x => !x.IsDirectory());
                var numFiles = fileEntries.Count() + 1;

                // 5. Extract the XML file
                Operation(new ExtractXmlIdentityOp(_zip, modDirectory, unique, modName));
                ProgressSignifier.Progress += totalProgress / numFiles;

                // 6. Extract all mod files
                foreach (ZipArchiveEntry e in fileEntries)
                {
                    await OperationAsync(new ExtractFileOp(e, modDirectory));
                    ProgressSignifier.Progress += totalProgress / numFiles;
                }

            /*if (false)
            {*/
                // We cannot enable the mod until all files are extracted

                // The instance we have of ManagedMod was temporary, only to show the configurator;
                // recreate it now that we have all the files extracted
                var oldManagedMod = _managedMod;
                _managedMod = Operation(new InitManagedModConfigOp(unique, _managedMod)).mod;
                Operation(new AddToModManagerOp(_managedMod, oldManagedMod));
                Identity = _managedMod.Identity;


                // 7. Apply the mod's contents; 
                // It must fail if we are not upgrading a mod and a mod with this name already exists 
                // (but it will be the developer's fault, as it should never have executed the transaction then)

                await OperationAsync(new ExecuteTransactionAsyncOp(new ApplyModContentTransaction(_managedMod, ProgressSignifier)));
            //}
            // Finally, close the zip file
            _zip.Dispose();
            _zip = null;
                
            return true;
        }

        public override void Rollback()
        {
            if (UpgradeFromMod != null)
                UpgradeFromMod.ProgressSignifier = ProgressSignifier;

            base.Rollback();

            if (_zip != null)
            {
                _zip.Dispose();
                _zip = null;
            }
        }

        protected override void CompleteProgress(bool dispose)
        {
            Debug.WriteLine($"{nameof(CompleteProgress)}\n\t{nameof(_managedMod)}: {_managedMod != null}\n\t{nameof(UpgradeFromMod)}: {UpgradeFromMod != null}");
            base.CompleteProgress(dispose);


            if (_managedMod != null)
            {
                _managedMod.ProgressSignifier = null;
                Debug.WriteLine($"\t{_managedMod}: {_managedMod.ProgressSignifier != null}, {_managedMod.PreventsGameLaunch}");
            }


            if (UpgradeFromMod != null)
            {
                UpgradeFromMod.ProgressSignifier = null;
                Debug.WriteLine($"\t{UpgradeFromMod}: {UpgradeFromMod.ProgressSignifier != null}, {UpgradeFromMod.PreventsGameLaunch}");
            }
        }
    }
}
