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
        class TempModBackupFile : ModBackupFile
        {
            private readonly string originalPath;
            private readonly string tmpBackupPath;
            private readonly string drive;
            private readonly bool mustCopy;
            // We keep a file stream open to avoid someone deleting our backup
            private FileStream fs;

            public TempModBackupFile(string originalPath, bool safeWrite)
            {
                this.originalPath = originalPath;

                // We cannot use Move between different drives
                drive = Path.GetPathRoot(new FileInfo(originalPath).FullName);
                if (drive == systemTempDrive)
                {
                    tmpBackupPath = Path.GetTempFileName();
                    File.Move(originalPath, tmpBackupPath, true);
                    mustCopy = false;
                }
                else if (drive == smmTempDrive)
                {
                    // We get a temp folder path by creating a random name, so we must check the very unlikely case that it already exists
                    tmpBackupPath = Path.Combine(smmTempDirectory, Path.GetRandomFileName());
                    while (Directory.Exists(tmpBackupPath) || File.Exists(tmpBackupPath))
                    {
                        tmpBackupPath = Path.Combine(smmTempDirectory, Path.GetRandomFileName());
                    }
                    File.Move(originalPath, tmpBackupPath, true);
                    mustCopy = false;
                }
                else
                {
                    // None of the possible temp folders are in the same drive, so we must copy the file
                    tmpBackupPath = Path.GetTempFileName();
                    File.Copy(originalPath, tmpBackupPath, true);
                    mustCopy = true;
                }

                // We keep a file stream open to avoid someone deleting our backup
                fs = new FileStream(tmpBackupPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                _isValid = true;
                _safeWrite = safeWrite;
            }

            public override void Restore()
            {
                if (!_isValid)
                {
                    throw new InvalidOperationException("Cannot restore backup file '" + originalPath + "', backup not valid anymore.");
                }
                // Close the file stream so we can move the file
                fs.Close();
                fs = null;
                if (_safeWrite)
                {
                    if (FileWrite.IsUnprotectedFile(originalPath))
                    {
                        if (File.Exists(originalPath))
                            File.Delete(originalPath);

                        if (mustCopy)
                        {
                            File.Copy(tmpBackupPath, originalPath, true);
                            File.Delete(tmpBackupPath);
                        }
                        else File.Move(tmpBackupPath, originalPath, true);

                        Permissions.GrantAccessFile(originalPath);
                    }
                }
                else
                {
                    if (mustCopy)
                    {
                        File.Copy(tmpBackupPath, originalPath, true);
                        File.Delete(tmpBackupPath);
                    }
                    else File.Move(tmpBackupPath, originalPath, true);
                }

                _isValid = false;
            }

            public override void Dispose()
            {
                if (fs != null) fs.Close();
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

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }
        private static void CopyDirectory(string sourcePath, string targetPath)
        {
            Directory.CreateDirectory(targetPath);
            CopyFilesRecursively(new DirectoryInfo(sourcePath), new DirectoryInfo(targetPath));
        }

        class DirectoryModBackupFile : ModBackupFile
        {
            private readonly string originalPath;
            private readonly string tmpBackupPath;
            private readonly string drive;
            private readonly bool mustCopy;

            public DirectoryModBackupFile(string originalPath, bool safeWrite)
            {
                this.originalPath = originalPath;

                // We cannot use Move between different drives
                drive = Path.GetPathRoot(new FileInfo(originalPath).FullName);
                if (drive == systemTempDrive)
                {
                    // We get a temp folder path by creating a random name, so we must check the very unlikely case that it already exists
                    tmpBackupPath = Path.Combine(systemTempDirectory, Path.GetRandomFileName());
                    while (Directory.Exists(tmpBackupPath) || File.Exists(tmpBackupPath))
                    {
                        tmpBackupPath = Path.Combine(systemTempDirectory, Path.GetRandomFileName());
                    }
                    Directory.Move(originalPath, tmpBackupPath);
                    mustCopy = false;
                }
                else if (drive == smmTempDrive)
                {
                    // We get a temp folder path by creating a random name, so we must check the very unlikely case that it already exists
                    tmpBackupPath = Path.Combine(smmTempDirectory, Path.GetRandomFileName());
                    while (Directory.Exists(tmpBackupPath) || File.Exists(tmpBackupPath))
                    {
                        tmpBackupPath = Path.Combine(smmTempDirectory, Path.GetRandomFileName());
                    }
                    Directory.Move(originalPath, tmpBackupPath);
                    mustCopy = false;
                }
                else
                {
                    // None of the possible temp folders are in the same drive, so we must copy the file
                    // We get a temp folder path by creating a random name, so we must check the very unlikely case that it already exists
                    tmpBackupPath = Path.Combine(systemTempDirectory, Path.GetRandomFileName());
                    while (Directory.Exists(tmpBackupPath) || File.Exists(tmpBackupPath))
                    {
                        tmpBackupPath = Path.Combine(systemTempDirectory, Path.GetRandomFileName());
                    }
                    CopyDirectory(originalPath, tmpBackupPath);
                    mustCopy = true;
                }

                // To avoid someone deleting the backup while it's in use
                File.SetAttributes(tmpBackupPath, FileAttributes.ReadOnly);

                _isValid = true;
                _safeWrite = safeWrite;
            }

            public override void Restore()
            {
                if (!_isValid)
                {
                    throw new InvalidOperationException("Cannot restore backup file '" + originalPath + "', backup not valid anymore.");
                }

                // Restore them so we can move the directory
                File.SetAttributes(tmpBackupPath, FileAttributes.Normal);

                if (File.Exists(originalPath))
                {
                    File.Delete(originalPath);
                }
                else if (Directory.Exists(originalPath))
                {
                    Directory.Delete(originalPath, true);
                }

                if (mustCopy)
                {
                    CopyDirectory(tmpBackupPath, originalPath);
                    Directory.Delete(tmpBackupPath, true);
                }
                else
                {
                    Directory.Move(tmpBackupPath, originalPath);
                }

                _isValid = false;
            }

            public override void Dispose()
            {
                if (_isValid && Directory.Exists(tmpBackupPath))
                {
                    File.SetAttributes(tmpBackupPath, FileAttributes.Normal);
                    Directory.Delete(tmpBackupPath, true);
                }
                _isValid = false;
            }
        }

        // Concurrent dictionary is the only collection that lets us remove, we assign it to some random number
        // A list of all backups, used for disposing them at the end of the program; some might not be valid anymore
        private static ConcurrentDictionary<ModBackupFile, int> backups = new ConcurrentDictionary<ModBackupFile, int>();

        // Information to ensure we don't attempt to move between different drives
        private static string systemTempDirectory = null;
        private static string systemTempDrive = null;
        private static string smmTempDirectory = null;
        private static string smmTempDrive = null;

        /// <summary>
        /// Creates a backup file that can be restored later, deleting the original file. It always returns a valid file, even
        /// if the path does not exist; in that case, restoring the backup will remove any file at the path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="safe"></param>
        /// <returns></returns>
        public static ModBackupFile BackupFile(string path, bool safe = true)
        {
            if (systemTempDirectory == null)
            {
                systemTempDirectory = Path.GetTempPath();
                smmTempDirectory = Settings.TempFolderPath;
                systemTempDrive = Path.GetPathRoot(new FileInfo(systemTempDirectory).FullName);
                smmTempDrive = Path.GetPathRoot(new FileInfo(smmTempDirectory).FullName);
            }

            ModBackupFile backup = null;
            if (File.Exists(path))
            {
                backup = new TempModBackupFile(path, safe);
            }
            else if (Directory.Exists(path))
            {
                backup = new DirectoryModBackupFile(path, safe);
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
