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
                Dictionary<string, System.Drawing.Image> images = new Dictionary<string, System.Drawing.Image>();
                
                Console.WriteLine($"Searching ZIP entries for images...");
                foreach (ZipArchiveEntry zipEntry in zip.Entries)
                {
                    if (Path.GetExtension(zipEntry.FullName).TrimStart('.').Equals("png", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"\t- Found '{zipEntry.FullName}'");

                        
                        System.Drawing.Image image = null;

                        using (Stream stream = zipEntry.Open())
						{
							//stream.Seek(0, SeekOrigin.Begin);
							image = System.Drawing.Image.FromStream(stream);
						}

                        if (image != null)
                            images.Add(zipEntry.FullName, image);
                    }
                }

                using (Stream stream = entry.Open())
                {
                    Identity = XmlModIdentity.Parse(stream, null, images);
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
