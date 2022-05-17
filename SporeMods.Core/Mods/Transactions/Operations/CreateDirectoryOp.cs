using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    /// <summary>
    /// Creates an empty directory, if it didn't already exist. The undo operation
    /// deletes the directory if it originally didn't exist. For that to work, the directory must be empty.
    /// </summary>
    public class CreateDirectoryOp : SyncOperationBase
    {
        public readonly string Path;
        private bool _directoryExisted;

        public CreateDirectoryOp(string path)
        {
            Path = path;
        }

        public override bool Do()
        {
            if (!Directory.Exists(Path))
            {
                _directoryExisted = false;
                Directory.CreateDirectory(Path);
            }
            else
            {
                _directoryExisted = true;
            }
            return true;
        }

        public override void Undo()
        {
            if (!_directoryExisted)
            {
                Directory.Delete(Path);
            }
        }
    }
}
