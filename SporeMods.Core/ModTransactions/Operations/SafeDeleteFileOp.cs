using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.ModTransactions.Operations
{
    public class SafeDeleteFileOp : IModSyncOperation
    {
        public readonly string path;

        public SafeDeleteFileOp(string path)
        {
            this.path = path;
        }

        public bool Do()
        {
            FileWrite.SafeDeleteFile(path);
            return true;
        }

        public void Undo()
        {
            //TODO backup
        }
    }
}
