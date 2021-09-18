using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Parses the ModInfo.xml identity from the mod zip, or creates a simple identity if it is not present.
    /// </summary>
    public class ParseIdentityOp : IModSyncOperation
    {
        public ModIdentity Identity;
        // True if there was no identity file in the mod and it had to be generated
        public bool IsGeneratedIdentity;
        public readonly ZipArchive zip;
        public readonly string name;

        public ParseIdentityOp(ZipArchive zip, string name)
        {
            this.zip = zip;
            this.name = name;
        }

        public bool Do()
        {
            if (zip.TryGetEntry(ManagedMod.MOD_INFO, out ZipArchiveEntry entry))
            {
                using (Stream stream = entry.Open())
                {
                    Identity = XmlModIdentity.Parse(stream, null);
                    IsGeneratedIdentity = false;
                }
            }
            else
            {
                Identity = new ModIdentity(null, name);
                Identity.DisplayName = name;
                IsGeneratedIdentity = true;
            }
            return true;
        }

        public void Undo()
        {
        }
    }
}
