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
            Console.WriteLine($"haha {nameof(ConfigureModTransaction)} go brrr");
            this.mod = mod;
            ProgressSignifier = new TaskProgressSignifier(mod.DisplayName, TaskCategory.Reconfigure);
        }

        bool _userCancelled = false;
        public override async Task<bool> CommitAsync()
        {
            if (!mod.HasConfigurator)
                return false;

            mod.ProgressSignifier = ProgressSignifier;

            // 1. Show configurator; it already creates a copy of the original config for us
            var configurator = await OperationAsync(new ShowConfiguratorAsyncOp(mod, true));

            _userCancelled = configurator.UserCancelled;
            Console.WriteLine($"Cancelled: {_userCancelled}");
            if (!_userCancelled)
            {
                ProgressSignifier.Status = TaskStatus.Determinate;
                // 2. Apply changes
                await OperationAsync(new ExecuteTransactionAsyncOp(new ApplyModContentTransaction(mod, ProgressSignifier)));
            }

            return !_userCancelled;
        }

        protected override void CompleteProgress(bool dispose)
        {
            base.CompleteProgress(dispose);
            if (_userCancelled)
                ProgressSignifier.Status = TaskStatus.Skipped;
            
            mod.ProgressSignifier = null;
        }
    }
}
