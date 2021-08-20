using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SporeMods.Core.ModInstallationaa
{
    /// <summary>
    /// Parses the ModInfo.xml identity from the mod zip, or creates a simple identity if it is not present.
    /// </summary>
    public class ParseIdentityOp : IModSyncOperation
    {
        public ModIdentity Identity;
        private readonly ZipArchive zip;
        private readonly string name;

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
                }
            }
            else
            {
                Identity = new ModIdentity(null, name);
                Identity.DisplayName = name;
            }
            return true;
        }

        public void Undo()
        {
        }
    }
}
