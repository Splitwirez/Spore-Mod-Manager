using SporeMods.Core.Mods;
using SporeMods.Core.ModTransactions.Operations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions.Transactions
{
    public class ConfigureModTransaction : ModTransaction
    {
        public readonly ManagedMod mod;

        public ConfigureModTransaction(ManagedMod mod)
        {
            this.mod = mod;
        }

        public override async Task<bool> CommitAsync()
        {
            if (!mod.HasConfigurator) return false;

            // 1. Show configurator; it already creates a copy of the original config for us
            await OperationAsync(new ShowConfiguratorAsyncOp(mod));

            // 2. Apply changes
            await OperationAsync(new ExecuteTransactionAsyncOp(new ApplyModContentTransaction(mod)));

            return true;
        }
    }
}
