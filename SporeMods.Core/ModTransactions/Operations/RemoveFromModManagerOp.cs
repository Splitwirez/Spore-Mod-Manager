using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Removes a mod from the ModsManager list. Undoing this restores the mod in the same position.
    /// </summary>
    public class RemoveFromModManagerOp : IModSyncOperation
    {
        public readonly IInstalledMod mod;
        private int modIndex = -1;

        public RemoveFromModManagerOp(IInstalledMod mod)
        {
            this.mod = mod;
        }

        public bool Do()
        {
            modIndex = ModsManager.InstalledMods.IndexOf(mod);
            ModsManager.RemoveMod(mod);
            return true;
        }

        public void Undo()
        {
            if (modIndex != -1)
            {
                ModsManager.InsertMod(modIndex, mod);
            }
        }
    }
}
