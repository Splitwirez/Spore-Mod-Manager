using SporeMods.Core.Mods;
using SporeMods.Core.ModTransactions.Operations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions.Transactions
{
    public class UninstallManualModTransaction : ModTransaction
    {
        public readonly ManualInstalledFile mod;

        public UninstallManualModTransaction(ManualInstalledFile mod)
        {
            this.mod = mod;
        }

        public override async Task<bool> CommitAsync()
        {
            // 1. Delete the installed file
            Operation(new SafeDeleteFileOp(
                FileWrite.GetFileOutputPath(mod.Location, mod.RealName, mod.IsLegacy)
                ));

            // 2. Remove from mod list
            Operation(new RemoveFromModManagerOp(mod));

            return true;
        }
    }
}
