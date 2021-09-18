using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Creates an empty directory, if it didn't already exist. The undo operation
    /// deletes the directory if it originally didn't exist. For that to work, the directory must be empty.
    /// </summary>
    public class CreateDirectoryOp : IModSyncOperation
    {
        public readonly string path;
        private bool directoryExisted;

        public CreateDirectoryOp(string path)
        {
            this.path = path;
        }

        public bool Do()
        {
            if (!Directory.Exists(path))
            {
                directoryExisted = false;
                Directory.CreateDirectory(path);
            }
            else
            {
                directoryExisted = true;
            }
            return true;
        }

        public void Undo()
        {
            if (!directoryExisted)
            {
                Directory.Delete(path);
            }
        }
    }
}
