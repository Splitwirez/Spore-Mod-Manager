﻿using SporeMods.Core.Mods;
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
        private readonly bool failIfExists;
        // If we allowed replacing an existing mod, this is the old one
        private ManagedMod previousMod = null;
        private int previousModIndex = -1;

        public AddToModManagerOp(ManagedMod mod, bool failIfExists = true)
        {
            this.mod = mod;
            this.failIfExists = failIfExists;
        }

        public bool Do()
        {
            var previousMod = ModsManager.GetManagedMod(mod.RealName);
            if (previousMod != null)
            {
                if (failIfExists) return false;
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
            if (previousMod != null)
            {
                ModsManager.RemoveMod(mod);
                ModsManager.InsertMod(previousModIndex, previousMod);
            }
        }
    }
}
