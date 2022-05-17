using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    /// <summary>
    /// Recursively deletes a directory and all the contents within.
    /// Undoing this will delete any file/directory in the original path and restore the original directory and its contents.
    /// </summary>
    public class DeleteDirectoryOp : SyncOperationBase
    {
        public readonly string Path;
        private BackupFile _backup;

        public DeleteDirectoryOp(string path)
        {
            Path = path;
        }

        public override bool Do()
        {
            _backup = BackupFiles.BackupFile(Path);
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
