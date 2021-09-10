using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Creates a managed mod instance with stored files in the SMM, applying an existing configuration.
    /// Undoing this action deletes the config file and additional config files (like UseLegacyDlls)
    /// </summary>
    public class InitManagedModConfigOp : IModSyncOperation
    {
        public ManagedMod mod;
        public readonly string unique;
        public readonly ManagedMod configMod;
        private ModBackupFile configBackup;
        private ModBackupFile legacyBackup;

        public InitManagedModConfigOp(string unique, ManagedMod configMod)
        {
            this.unique = unique;
            this.configMod = configMod;
        }

        public bool Do()
        {
            var storagePath = Path.Combine(Settings.ModConfigsPath, unique);
            configBackup = ModBackupFiles.BackupFile(Path.Combine(storagePath, ManagedMod.MOD_CONFIG));
            legacyBackup = ModBackupFiles.BackupFile(Path.Combine(storagePath, ManagedMod.PATH_USELEGACYDLLS));

            mod = new ManagedMod(unique, true, configMod.Configuration)
            {
                ProgressSignifier = configMod.ProgressSignifier
                //Progress = configMod.Progress,
                //IsProgressing = configMod.IsProgressing
            };
            return true;
        }

        public void Undo()
        {
            if (configBackup != null) configBackup.Restore();
            if (legacyBackup != null) legacyBackup.Restore();
        }

        public void Dispose()
        {
            if (configBackup != null) ModBackupFiles.DisposeBackup(configBackup);
            if (legacyBackup != null) ModBackupFiles.DisposeBackup(legacyBackup);
        }
    }
}
