using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SporeMods.Core.ModInstallationaa
{
    public class ParseIdentityOp : IModOperation
    {
        public ModIdentity Identity;
        private ZipArchive zip;

        public ParseIdentityOp(ZipArchive zip)
        {
            this.zip = zip;
        }

        public bool Do()
        {
            if (zip.TryGetEntry(ManagedMod.MOD_INFO, out ZipArchiveEntry entry))
            {
                using (Stream stream = entry.Open())
                {
                    Identity = XmlModIdentity.Parse(stream, null);
                }
                return true;
            }
            else return false;
        }
        public void Undo()
        {
            throw new NotImplementedException();
        }
    }
}
