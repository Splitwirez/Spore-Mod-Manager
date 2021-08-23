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
        }

        public override async Task<bool> CommitAsync()
        {
            //TODO maybe this shouldn't go here? because when the transaction fails, no one calls this
            mod.Progress = 0;
            mod.IsProgressing = true;

            // 1. Delete all enabled mod-related files
            var progressRange = 80.0;
            var filesToDelete = mod.GetFilePathsToRemove();
            foreach (var file in filesToDelete)
            {
                Operation(new SafeDeleteFileOp(file));
                mod.Progress += progressRange / filesToDelete.Count;
            }

            // 2. Delete all files in the SMM mod folder
            Operation(new DeleteDirectoryOp(mod.StoragePath));

            // 3. Remove mod from the list
            Operation(new RemoveFromModManagerOp(mod));

            //TODO maybe this shouldn't go here? because when the transaction fails, no one calls this
            mod.Progress = 0;
            mod.IsProgressing = false;

            return true;
        }
    }
}
