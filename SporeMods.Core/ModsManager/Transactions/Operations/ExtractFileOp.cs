using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    /// <summary>
    /// Extracts a file from the mod zip. Undoing this operations deletes the file.
    /// You can optionally specify a CountdownEvent; if you do, it will send one signal when the file is extracted.
    /// </summary>
    public class ExtractFileOp : AsyncOperationBase
    {
        public readonly ZipArchiveEntry Entry;
        public readonly string OutputPath;
        public readonly CountdownEvent CountdownLatch;
        private bool _isModInfo;
        private BackupFile _backup;
        // It is possible that this file replaces a mod that was detected as a manually installed file
        private int _manuallyInstalledFileIndex;
        private ISporeMod _manuallyInstalledFile;

        private ExtractFileOp(ZipArchiveEntry entry, CountdownEvent countdownLatch)
        {
            Entry = entry;
            CountdownLatch = countdownLatch;
        }
        public ExtractFileOp(ZipArchiveEntry entry, string outputPath, CountdownEvent countdownLatch = null)
            : this(entry, countdownLatch)
        {
            OutputPath = outputPath;
        }
        public ExtractFileOp(ZipArchiveEntry entry, string outputDir, string outFileName, CountdownEvent countdownLatch = null)
            : this(entry, countdownLatch)
        {
            string outName = outFileName != null ? outFileName : Path.GetFileName(entry.FullName);
            OutputPath = Path.Combine(outputDir, outName);
        }

        public override async Task<bool> DoAsync()
        {
            return await this.BoolTaskEx(() =>
            {
                string name = Path.GetFileName(OutputPath);
                /*_isModInfo = name.Equals(ModConstants.ID_XML_FILE_NAME, StringComparison.OrdinalIgnoreCase);

                if (!_isModInfo)
                {*/
                _backup = BackupFiles.BackupFile(OutputPath);
                Entry.ExtractToFile(OutputPath, true);
                Permissions.GrantAccessFile(OutputPath);

                _manuallyInstalledFile = ModsManager.GetManuallyInstalledFile(Entry.Name, ComponentGameDir.GalacticAdventures);
                if (_manuallyInstalledFile != null)
                {
                    _manuallyInstalledFileIndex = ModsManager.InstalledMods.IndexOf(_manuallyInstalledFile);
                    ModsManager.InstalledMods.Remove(_manuallyInstalledFile);
                }
                //}

                if (CountdownLatch != null)
                    CountdownLatch.Signal();

                return true;
            });
        }

        public override void Undo()
        {
            if (!_isModInfo)
            {
                if (_backup != null)
                    _backup.Restore();

                if (_manuallyInstalledFile != null)
                {
                    ModsManager.InstalledMods.Insert(_manuallyInstalledFileIndex, _manuallyInstalledFile);
                }
            }
        }

        public void Dispose()
        {
            if (_backup != null)
                BackupFiles.DisposeBackup(_backup);
        }
    }
}
