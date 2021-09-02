using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Recursively deletes a directory and all the contents within.
    /// Undoing this will delete any file/directory in the original path and restore the original directory and its contents.
    /// </summary>
    public class DeleteDirectoryOp : IModSyncOperation
    {
        public readonly string path;
        private ModBackupFile backup;

        public DeleteDirectoryOp(string path)
        {
            this.path = path;
        }

        public bool Do()
        {
            backup = ModBackupFiles.BackupFile(path);
            return true;
        }

        public void Undo()
        {
            if (backup != null) backup.Restore();
        }

        public void Dispose()
        {
            if (backup != null) ModBackupFiles.DisposeBackup(backup);
        }
    }
}
