using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SporeMods.Core.ModInstallationaa
{
    /// <summary>
    /// Creates a managed mod instance with stored files in the SMM, applying an existing configuration.
    /// Undoing this action deletes the config file.
    /// </summary>
    public class InitManagedModConfigOp : IModSyncOperation
    {
        public ManagedMod mod;
        private readonly string unique;
        private readonly ManagedMod configMod;

        public InitManagedModConfigOp(string unique, ManagedMod configMod)
        {
            this.unique = unique;
            this.configMod = configMod;
        }

        public bool Do()
        {
            mod = new ManagedMod(unique, true, configMod.Configuration)
            {
                Progress = configMod.Progress,
                IsProgressing = configMod.IsProgressing
            };
            return true;
        }

        public void Undo()
        {
            if (mod != null)
            {
                string path = Path.Combine(mod.StoragePath, ManagedMod.MOD_CONFIG);
                File.Delete(path);
            }
        }
    }
}
