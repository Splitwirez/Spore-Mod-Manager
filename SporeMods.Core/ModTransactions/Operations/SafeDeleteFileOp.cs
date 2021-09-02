using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Safely deletes a file. Safe deletion means it will only be done if it has the permissions and 
    /// it's not a protected Spore file. Undoing this restores the original file.
    /// </summary>
    public class SafeDeleteFileOp : IModSyncOperation
    {
        public readonly string path;
        private ModBackupFile backup;

        public SafeDeleteFileOp(string path)
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
