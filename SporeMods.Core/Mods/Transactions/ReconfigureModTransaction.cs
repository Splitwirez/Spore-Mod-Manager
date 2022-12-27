using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
    public class ReconfigureModTransaction : ModTransaction
    {
        readonly IConfigurableMod _mod;
        public ReconfigureModTransaction(IConfigurableMod mod)
        {
            _mod = mod;
        }


        public override async Task<bool> CommitAsync()
        {
            Exception exception = await _mod.ApplyAsync(this);
            if (exception != null)
            {
                Exception = exception;
                return false;
            }

            return true;
        }
    }
}
