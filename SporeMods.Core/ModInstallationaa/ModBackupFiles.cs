using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SporeMods.Core.ModInstallationaa
{
    public abstract class ModBackupFile
    {
        protected bool _isValid;
        public bool IsValid { get => _isValid; }

        public abstract void Restore();

        public abstract void Dispose();
    }

    public static class ModBackupFiles
    {
        class MemoryModBackupFile : ModBackupFile
        {
            private readonly string originalPath;
            private byte[] backupData;

            public MemoryModBackupFile(string originalPath)
            {
                this.originalPath = originalPath;
                backupData = File.ReadAllBytes(originalPath);
                Interlocked.Add(ref CURRENT_BUFFER_USAGE, +backupData.Length);
                _isValid = true;
            }

            public override void Restore()
            {
                if (!_isValid)
                {
                    throw new InvalidOperationException("Cannot restore backup file '" + originalPath + "', backup not valid anymore.");
                }
                File.WriteAllBytes(originalPath, backupData);
            }

            public override void Dispose()
            {
                Interlocked.Add(ref CURRENT_BUFFER_USAGE, -backupData.Length);
                _isValid = false;
                backupData = null;
            }
        }

        class TempModBackupFile : ModBackupFile
        {
            private readonly string originalPath;
            private readonly string tmpBackupPath;

            public TempModBackupFile(string originalPath)
            {
                this.originalPath = originalPath;
                tmpBackupPath = Path.GetTempFileName();
                File.Copy(originalPath, tmpBackupPath, true);
                _isValid = true;
            }

            public override void Restore()
            {
                if (!_isValid)
                {
                    throw new InvalidOperationException("Cannot restore backup file '" + originalPath + "', backup not valid anymore.");
                }
                File.Copy(tmpBackupPath, originalPath, true);
            }

            public override void Dispose()
            {
                File.Delete(tmpBackupPath);
                _isValid = false;
            }
        }

        // Allow a maximum of 300 MB of memory to be used for backups
        private static long MAX_BUFFER_USAGE = 200 * 1024 * 1024;
        private static long CURRENT_BUFFER_USAGE = 0;

        // A list of all backups, used for disposing them at the end of the program; some might not be valid anymore
        private static ConcurrentBag<ModBackupFile> backups = new ConcurrentBag<ModBackupFile>();

        public static ModBackupFile CreateBackup(string path)
        {
            ModBackupFile backup = null;
            long length = new FileInfo(path).Length;
            long currentUsage = 0;
            if (Interlocked.Read(ref currentUsage) + length <= MAX_BUFFER_USAGE)
            {
                backup = new MemoryModBackupFile(path);
            }
            else
            {
                backup = new TempModBackupFile(path);
            }
            backups.Add(backup);
            return backup;
        }

        public static void DisposeBackup(ModBackupFile backup)
        {
            backup.Dispose();
        }

        public static void DisposeAll()
        {
            foreach (var backup in backups)
            {
                if (backup.IsValid) backup.Dispose();
            }
            backups.Clear();
        }
    }
}
