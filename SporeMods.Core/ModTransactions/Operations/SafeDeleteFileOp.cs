using System;
using System.Collections.Generic;
using System.Text;

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
            backup = ModBackupFiles.CreateBackup(path);
            FileWrite.SafeDeleteFile(path);
            return true;
        }

        public void Undo()
        {
            backup.Restore();
            ModBackupFiles.DisposeBackup(backup);
        }
    }
}
