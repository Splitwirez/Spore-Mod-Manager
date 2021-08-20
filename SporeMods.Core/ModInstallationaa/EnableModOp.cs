using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModInstallationaa
{
    public class EnableModOp : IModOperation
    {
        private ManagedMod mod;

        public EnableModOp(ManagedMod mod)
        {
            this.mod = mod;
        }

        public async Task<bool> DoAsync()
        {
            return await mod.EnableMod();
        }

        public void Undo()
        {
        }
    }
}
