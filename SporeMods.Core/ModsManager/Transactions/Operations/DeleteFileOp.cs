using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    /// <summary>
    /// Safely deletes a file. Safe deletion means it will only be done if it has the permissions and 
    /// it's not a protected Spore file. Undoing this restores the original file.
    /// </summary>
    public class DeleteFileOp : SyncOperationBase
    {
        public readonly string Path;
        private BackupFile _backup;

        public DeleteFileOp(string path)
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
