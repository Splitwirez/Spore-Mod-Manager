using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Safely copies a file. Safe copying means it will only be done if it has the permissions to modify
    /// the destination file and it's not a protected Spore file. Undoing this restores the original file,
    /// or deletes the destination if there was no original file.
    /// </summary>
    public class SafeCopyFileOp : IModSyncOperation
    {
        public readonly string source;
        public readonly string destination;
        private ModBackupFile backup;

        public SafeCopyFileOp(string source, string destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public bool Do()
        {
            //TODO is there a reason why this doesn't check whether or not `destination` already exists before trying to make a backup of it?
            backup = ModBackupFiles.BackupFile(destination);
            
            Exception ex = FileWrite.SafeCopyFile(source, destination);

            if (ex == null)
            {
                return true;
            }
            else
            {
                throw ex;
                return false;
            }
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
