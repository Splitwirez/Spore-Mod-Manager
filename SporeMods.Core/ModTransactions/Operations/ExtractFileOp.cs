using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Extracts a file from the mod zip. Undoing this operations deletes the file.
    /// You can optionally specify a CountdownEvent; if you do, it will send one signal when the file is extracted.
    /// </summary>
    public class ExtractFileOp : IModAsyncOperation
    {
        public readonly ZipArchiveEntry entry;
        public readonly string outputDir;
        public readonly CountdownEvent countdownLatch;
        private bool isModInfo;
        private ModBackupFile backup;
        // It is possible that this file replaces a mod that was detected as a manually installed file
        private int manuallyInstalledFileIndex;
        private ManualInstalledFile manuallyInstalledFile;

        public ExtractFileOp(ZipArchiveEntry entry, string outputDir, CountdownEvent countdownLatch = null)
        {
            this.entry = entry;
            this.outputDir = outputDir;
            this.countdownLatch = countdownLatch;
        }

        public async Task<bool> DoAsync()
        {
            return await Task<bool>.Run(() =>
            {
                string name = entry.Name.ToLowerInvariant();
                isModInfo = name.Contains(ManagedMod.MOD_INFO.ToLowerInvariant());

                if (!isModInfo)
                {
                    string outPath = Path.Combine(outputDir, entry.Name);
                    backup = ModBackupFiles.BackupFile(outPath);
                    entry.ExtractToFile(outPath, true);
                    Permissions.GrantAccessFile(outPath);

                    manuallyInstalledFile = ModsManager.GetManuallyInstalledFile(entry.Name, ComponentGameDir.GalacticAdventures);
                    if (manuallyInstalledFile != null)
                    {
                        manuallyInstalledFileIndex = ModsManager.InstalledMods.IndexOf(manuallyInstalledFile);
                        ModsManager.RemoveMod(manuallyInstalledFile);
                    }
                }
                
                if (countdownLatch != null)
                    countdownLatch.Signal();
                
                return true;
            });
        }

        public void Undo()
        {
            if (!isModInfo)
            {
                if (backup != null) backup.Restore();

                if (manuallyInstalledFile != null)
                {
                    ModsManager.InsertMod(manuallyInstalledFileIndex, manuallyInstalledFile);
                }
            }
        }

        public void Dispose()
        {
            if (backup != null) ModBackupFiles.DisposeBackup(backup);
        }
    }
}
