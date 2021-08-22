using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModInstallationaa
{
    /// <summary>
    /// Enables a mod; this is a blocking operation. Undoing this operation disables the mod.
    /// </summary>
    public class EnableModOp : IModSyncOperation
    {
        public readonly ManagedMod mod;

        public EnableModOp(ManagedMod mod)
        {
            this.mod = mod;
        }

        public bool Do()
        {
            return mod.EnableMod();
        }

        public void Undo()
        {
            mod.DisableMod();
        }
    }
}
