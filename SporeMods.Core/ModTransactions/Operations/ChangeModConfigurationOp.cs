using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Changes the configuration of a managed mod, and saves it to the config path.
    /// Undoing this restores the old configuration, also saving it if it was saved before.
    /// </summary>
    public class ChangeModConfigurationOp : IModSyncOperation
    {
        public readonly ManagedMod mod;
        public readonly ModConfiguration newConfig;
        public ModConfiguration oldConfig;
        // We won't restore the old config file if it didn't exist
        private bool oldConfigHadFile;

        public ChangeModConfigurationOp(ManagedMod mod, ModConfiguration newConfig)
        {
            this.mod = mod;
            this.newConfig = newConfig;
        }

        public bool Do()
        {
            oldConfig = mod.Configuration;
            string path = Path.Combine(mod.StoragePath, ManagedMod.MOD_CONFIG);
            if (File.Exists(path))
            {
                oldConfigHadFile = true;
                File.Delete(path);
            }
            else
            {
                oldConfigHadFile = false;
            }
            mod.Configuration = newConfig;
            newConfig.Save(path);
            return true;
        }

        public void Undo()
        {
            mod.Configuration = oldConfig;
            string path = Path.Combine(mod.StoragePath, ManagedMod.MOD_CONFIG);
            if (oldConfigHadFile)
            {
                oldConfig.Save(path);
            }
            else
            {
                File.Delete(path);
            }
        }
    }
}
