using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SporeMods.Core.ModTransactions
{
    public abstract class ModBackupFile
    {
        protected bool _isValid;
        public bool IsValid { get => _isValid; }

        protected bool _safeWrite;
        public bool SafeWrite { get => _safeWrite; }

        public abstract void Restore();

        public abstract void Dispose();
    }

    public static class ModBackupFiles
    {
        class MemoryModBackupFile : ModBackupFile
        {
            private readonly string originalPath;
            private byte[] backupData;

            public MemoryModBackupFile(string originalPath, bool safeWrite)
            {
                this.originalPath = originalPath;
                backupData = File.ReadAllBytes(originalPath);
                Interlocked.Add(ref CURRENT_BUFFER_USAGE, +backupData.Length);
                _isValid = true;
                _safeWrite = safeWrite;
            }

            public override void Restore()
            {
                if (!_isValid)
                {
                    throw new InvalidOperationException("Cannot restore backup file '" + originalPath + "', backup not valid anymore.");
                }
                if (_safeWrite)
                {
                    if (FileWrite.IsUnprotectedFile(originalPath))
                    {
                        if (File.Exists(originalPath))
                            File.Delete(originalPath);

                        File.WriteAllBytes(originalPath, backupData);
                        Permissions.GrantAccessFile(originalPath);
                    }
                }
                else
                {
                    File.WriteAllBytes(originalPath, backupData);
                }
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

            public TempModBackupFile(string originalPath, bool safeWrite)
            {
                this.originalPath = originalPath;
                tmpBackupPath = Path.GetTempFileName();
                File.Copy(originalPath, tmpBackupPath, true);
                _isValid = true;
                _safeWrite = safeWrite;
            }

            public override void Restore()
            {
                if (!_isValid)
                {
                    throw new InvalidOperationException("Cannot restore backup file '" + originalPath + "', backup not valid anymore.");
                }
                if (_safeWrite)
                {
                    FileWrite.SafeCopyFile(tmpBackupPath, originalPath);
                }
                else
                {
                    File.Copy(tmpBackupPath, originalPath, true);
                }
            }

            public override void Dispose()
            {
                File.Delete(tmpBackupPath);
                _isValid = false;
            }
        }

        // No actual backup file, restoring this just deletes the file. This class exists to simplify code.
        class MissingModBackupFile : ModBackupFile
        {
            private readonly string originalPath;

            public MissingModBackupFile(string originalPath, bool safeWrite)
            {
                this.originalPath = originalPath;
                _isValid = true;
                _safeWrite = safeWrite;
            }

            public override void Dispose()
            {
                _isValid = false;
            }

            public override void Restore()
            {
                if (!_isValid)
                {
                    throw new InvalidOperationException("Cannot restore backup file '" + originalPath + "', backup not valid anymore.");
                }
                if (_safeWrite)
                {
                    FileWrite.SafeDeleteFile(originalPath);
                }
                else
                {
                    File.Delete(originalPath);
                }
            }
        }

        // Allow a maximum of 300 MB of memory to be used for backups
        private static long MAX_BUFFER_USAGE = 200 * 1024 * 1024;
        private static long CURRENT_BUFFER_USAGE = 0;

        // Concurrent dictionary is the only collection that lets us remove, we assign it to some random number
        // A list of all backups, used for disposing them at the end of the program; some might not be valid anymore
        private static ConcurrentDictionary<ModBackupFile, int> backups = new ConcurrentDictionary<ModBackupFile, int>();

        /// <summary>
        /// Creates a backup file that can be restored later. It always returns a valid file, even
        /// if the path does not exist; in that case, restoring the backup will remove any file at the path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="safe"></param>
        /// <returns></returns>
        public static ModBackupFile CreateBackup(string path, bool safe = true)
        {
            ModBackupFile backup = null;
            if (File.Exists(path))
            {
                long length = new FileInfo(path).Length;
                long currentUsage = 0;
                if (Interlocked.Read(ref currentUsage) + length <= MAX_BUFFER_USAGE)
                {
                    backup = new MemoryModBackupFile(path, safe);
                }
                else
                {
                    backup = new TempModBackupFile(path, safe);
                }
            }
            else
            {
                backup = new MissingModBackupFile(path, safe);
            }
            backups[backup] = 0;
            return backup;
        }

        public static void DisposeBackup(ModBackupFile backup)
        {
            backup.Dispose();
            backups.Remove(backup, out _);
        }

        public static void DisposeAll()
        {
            foreach (var backup in backups.Keys)
            {
                if (backup.IsValid) backup.Dispose();
            }
            backups.Clear();
        }
    }
}
