using System;
using System.Collections.Generic;
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
        public ModIdentity identity;
        // The mod that this mod is replacing, if any
        public ManagedMod upgradedMod;
        public readonly string modPath;
        // Because we might need to do undo, and some operators might need the zip, we can't do the 'using...' thing
        private ZipArchive zip;

        public InstallModTransaction(string modPath, ManagedMod upgradedMod = null)
        {
            this.modPath = modPath;
            this.upgradedMod = upgradedMod;
        }

        /// <summary>
        /// Parses the mod identity from the mod ZIP. This can be executed before the executing the transaction itself,
        /// as it it doesn't modify anything. Still, it can throw a ModTransactionCommitException, in which case Rollback()
        /// should be called to close the ZIP.
        /// </summary>
        /// <returns></returns>
        public ModIdentity ParseModIdentity()
        {
            if (zip == null)
            {
                zip = ZipFile.OpenRead(modPath);
            }

            var modName = Path.GetFileNameWithoutExtension(modPath).Replace(".", "-");
            var identityOp = Operation(new ParseIdentityOp(zip, modName));
            identity = identityOp.Identity;
            if (!identityOp.IsGeneratedIdentity)
            {
                Operation(new ValidateModOp(identity));
            }

            return identity;
        }

        public override async Task<bool> CommitAsync()
        {
            if (zip == null)
            {
                zip = ZipFile.OpenRead(modPath);
            }

            // 1. Read the mod identity and validate it
            if (identity == null)
            {
                ParseModIdentity();
            }
            var unique = identity.Unique;
            var modName = identity.DisplayName;
            string modDirectory = Path.Combine(Settings.ModConfigsPath, unique);

            // 2. Show the configurator, if any
            // Needed to show the configurator
            var managedMod = new ManagedMod(true, identity)
            {
                Progress = 0,
                IsProgressing = true
            };

            if (upgradedMod != null)
            {
                // We use the same settings we had
                managedMod.Configuration = new ModConfiguration(upgradedMod.Configuration);
            }

            if (managedMod.HasConfigurator)
            {
                await OperationAsync(new ShowConfiguratorAsyncOp(managedMod));
            }

            if (upgradedMod != null)
            {
                // To upgrade, we just delete all the files and install
                var filesToDelete = upgradedMod.GetFilePathsToRemove();
                foreach (var file in filesToDelete)
                {
                    Operation(new SafeDeleteFileOp(file));
                }
                Operation(new DeleteDirectoryOp(upgradedMod.StoragePath));
            }

            // 3. Create the directory
            Operation(new CreateDirectoryOp(modDirectory));

            // We don't increase all the progress, because EnableMod() will be called
            double totalProgress = 50.0;
            var fileEntries = zip.Entries.Where(x => !x.IsDirectory());
            var numFiles = fileEntries.Count() + 1;

            // 4. Extract the XML file
            Operation(new ExtractXmlIdentityOp(zip, modDirectory, unique, modName));
            managedMod.Progress += totalProgress / numFiles;

            // 5. Extract all mod files
            foreach (ZipArchiveEntry e in fileEntries)
            {
                Operation(new ExtractFileOp(e, modDirectory));
                managedMod.Progress += totalProgress / numFiles;
            }

            // We cannot enable the mod until all files are extracted

            // The instance we have of ManagedMod was temporary, only to show the configurator;
            // recreate it now that we have all the files extracted
            managedMod = Operation(new InitManagedModConfigOp(unique, managedMod)).mod;
            identity = managedMod.Identity;

            // 6. Enable the mod and add it to the mod list; 
            // It must fail if we are not upgrading a mod and a mod with this name already exists 
            // (but it will be the developer's fault, as it should never have executed the transaction then)
            Operation(new AddToModManagerOp(managedMod, upgradedMod == null));
            await OperationAsync(new ExecuteTransactionAsyncOp(new EnableModTransaction(managedMod)));

            // Finally, close the zip file
            zip.Dispose();
            zip = null;
                
            return true;
        }

        public override void Rollback()
        {
            base.Rollback();

            if (zip != null)
            {
                zip.Dispose();
                zip = null;
            }
        }
    }
}
