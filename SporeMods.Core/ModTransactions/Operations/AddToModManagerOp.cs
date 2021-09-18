using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Adds the mod to the mod manager. Undoing this removes the mod from the manager.
    /// </summary>
    public class AddToModManagerOp : IModSyncOperation
    {
        public readonly IInstalledMod mod;
        // If we allowed replacing an existing mod, this is the old one
        private IInstalledMod previousMod = null;

        public AddToModManagerOp(IInstalledMod mod, IInstalledMod previousMod = null)
        {
            this.mod = mod;
            this.previousMod = previousMod;
        }

        public bool Do()
        {
            if (previousMod != null)
            {
                int previousModIndex = ModsManager.InstalledMods.IndexOf(previousMod);
                ModsManager.RemoveMod(previousMod);
                ModsManager.InsertMod(previousModIndex, mod);
            }
            else
            {
                ModsManager.AddMod(mod);
            }
            return true;
        }

        public void Undo()
        {
            int index = ModsManager.InstalledMods.IndexOf(mod);
            ModsManager.RemoveMod(mod);
            
            if (previousMod != null)
                ModsManager.InsertMod(index, previousMod);
        }
    }
}
