using SporeMods.Core.Mods;
using SporeMods.Core.ModTransactions.Operations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions.Transactions
{
    public class UninstallManagedModTransaction : ModTransaction
    {
        public readonly ManagedMod mod;

        public UninstallManagedModTransaction(ManagedMod mod)
        {
            this.mod = mod;
            ProgressSignifier = new TaskProgressSignifier(mod.DisplayName, TaskCategory.Uninstall);
        }

        public override async Task<bool> CommitAsync()
        {
            mod.ProgressSignifier = ProgressSignifier;

            // 1. Delete all enabled mod-related files
            var progressRange = 80.0;
            ProgressSignifier.ProgressTotal = progressRange;
            ProgressSignifier.Status = TaskStatus.Determinate;

            var filesToDelete = mod.GetFilePathsToRemove();
            foreach (var file in filesToDelete)
            {
                Operation(new SafeDeleteFileOp(file));
                ProgressSignifier.Progress += progressRange / filesToDelete.Count;
            }

            // 2. Delete all files in the SMM mod folder
            Operation(new DeleteDirectoryOp(mod.StoragePath));

            // 3. Remove mod from the list
            Operation(new RemoveFromModManagerOp(mod));

            return true;
        }

        protected override void CompleteProgress(bool dispose)
        {
            base.CompleteProgress(dispose);
            mod.ProgressSignifier = null;
        }
    }
}
