using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

using SporeMods.Core.Transactions;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
    /// <summary>
    /// Safely copies a file. Safe copying means it will only be done if it has the permissions to modify
    /// the destination file and it's not a protected Spore file. Undoing this restores the original file,
    /// or deletes the destination if there was no original file.
    /// </summary>
    public class SaveXDocumentOp : AsyncOperationBase
    {
        public readonly XDocument Document;
        public readonly string Destination;
        private BackupFile _backup;

        public SaveXDocumentOp(XDocument document, string destination)
        {
            Document = document;
            Destination = destination;
        }

        public override async Task<bool> DoAsync()
        {
            return await this.BoolTaskEx(() =>
            {
                _backup = BackupFiles.BackupFile(Destination);
                Document.Save(Destination);
                return true;
            });
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
