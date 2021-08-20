using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Text;

namespace SporeMods.Core.ModInstallationaa
{
    /// <summary>
    /// Extracts a file from the mod zip. Undoing this operations deletes the file.
    /// You can optionally specify a CountdownEvent; if you do, it will send one signal when the file is extracted.
    /// </summary>
    public class ExtractFileOp : IModSyncOperation
    {
        private readonly ZipArchiveEntry entry;
        private readonly string outputDir;
        private readonly CountdownEvent countdownLatch;

        public ExtractFileOp(ZipArchiveEntry entry, string outputDir, CountdownEvent countdownLatch = null)
        {
            this.entry = entry;
            this.outputDir = outputDir;
            this.countdownLatch = countdownLatch;
        }

        public bool Do()
        {
            string name = entry.Name.ToLowerInvariant();
            bool isModInfo = name.Contains(ManagedMod.MOD_INFO.ToLowerInvariant());

            if (!isModInfo)
            {
                string outPath = Path.Combine(outputDir, entry.Name);
                entry.ExtractToFile(outPath, true);
                Permissions.GrantAccessFile(outPath);

                ModsManager.RemoveMatchingManuallyInstalledFile(entry.Name, ComponentGameDir.GalacticAdventures);
            }
            if (countdownLatch != null) countdownLatch.Signal();
            return true;
        }

        public void Undo()
        {
            string outPath = Path.Combine(outputDir, entry.Name);
            File.Delete(outPath);
        }
    }
}
