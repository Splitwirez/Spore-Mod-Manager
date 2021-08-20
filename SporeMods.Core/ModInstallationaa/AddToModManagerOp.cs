using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.ModInstallationaa
{
    /// <summary>
    /// Adds the mod to the mod manager. Undoing this removes the mod from the manager.
    /// </summary>
    public class AddToModManagerOp : IModSyncOperation
    {
        private readonly ManagedMod mod;

        public AddToModManagerOp(ManagedMod mod)
        {
            this.mod = mod;
        }

        public bool Do()
        {
            ModsManager.AddMod(mod);
            return true;
        }

        public void Undo()
        {
            ModsManager.RemoveMod(mod);
        }
    }
}
