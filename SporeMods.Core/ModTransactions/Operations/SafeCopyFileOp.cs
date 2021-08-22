using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SporeMods.Core.ModTransactions.Operations
{
    public class SafeCopyFileOp : IModSyncOperation
    {
        private string source;
        private string destination;

        public SafeCopyFileOp(string source, string destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public bool Do()
        {
            FileWrite.SafeCopyFile(source, destination);
            return true;
        }

        public void Undo()
        {
            FileWrite.SafeDeleteFile(destination);
        }
    }
}
