using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SporeMods.Core.ModTransactions.Operations;

namespace SporeMods.Core.ModTransactions.Transactions
{
    public class InstallLoosePackageTransaction : ModTransaction
    {
        ManagedMod _managedMod = null;

        public readonly string modPath;

        public InstallLoosePackageTransaction(string modPath)
        {
            this.modPath = modPath;
        }

        public override async Task<bool> CommitAsync()
        {
            if (Settings.AllowVanillaIncompatibleMods)
            {
                string name = Path.GetFileName(modPath);
                string noExtensionName = Path.GetFileNameWithoutExtension(modPath).Replace(".", "-");
                ProgressSignifier = new TaskProgressSignifier(noExtensionName, TaskCategory.Install)
                {
                    ProgressTotal = 2
                };

                string dir = Path.Combine(Settings.ModConfigsPath, noExtensionName);

                // 1. We create the folder to store our mod in SMM
                Operation(new CreateDirectoryOp(dir));

                // 2. Generate a basic XML identity for it
                Operation(new ExtractXmlIdentityOp(null, dir, noExtensionName, noExtensionName));

                _managedMod = new ManagedMod(noExtensionName, true)
                {
                    ProgressSignifier = ProgressSignifier
                };

                // 3. Add the mod to the manager and copy the file
                Operation(new AddToModManagerOp(_managedMod));
                Operation(new SafeCopyFileOp(modPath, Path.Combine(dir, name)));

                Operation(new RemoveManuallyInstalledFileOp(name, ComponentGameDir.GalacticAdventures));
                ProgressSignifier.Progress++;


                await OperationAsync(new ExecuteTransactionAsyncOp(new ApplyModContentTransaction(_managedMod)));
                ProgressSignifier.Progress++;

                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void CompleteProgress(bool dispose)
        {
            base.CompleteProgress(dispose);

            if (_managedMod != null)
                _managedMod.ProgressSignifier = null;
        }
    }
}
