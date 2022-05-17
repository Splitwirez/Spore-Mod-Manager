using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    /// <summary>
    /// Safely copies a file. Safe copying means it will only be done if it has the permissions to modify
    /// the destination file and it's not a protected Spore file. Undoing this restores the original file,
    /// or deletes the destination if there was no original file.
    /// </summary>
    public class CopyFileOp : SyncOperationBase
    {
        public readonly string Source;
        public readonly string Destination;
        private BackupFile _backup;

        public CopyFileOp(string source, string destination)
        {
            Source = source;
            Destination = destination;
        }

        public override bool Do()
        {
            Cmd.WriteLine($"Doing SafeCopyFileOp '{Source}'...");
            _backup = BackupFiles.BackupFile(Destination);
            
            if (!File.Exists(Source))
            {
                Cmd.WriteLine($"File '{Source}' does not exist");
                //return false;
            }
            else
                Cmd.WriteLine($"Copying file '{Source}' to '{Destination}'...");

            File.Copy(Source, Destination, true);
            Permissions.GrantAccessFile(Destination);
            return true;
        }

        public override void Undo()
        {
            if (_backup != null)
                _backup.Restore();
        }

        public void Dispose()
        {
            if (_backup != null)
                BackupFiles.DisposeBackup(_backup);
        }
    }
}
