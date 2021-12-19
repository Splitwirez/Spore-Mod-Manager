using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    /// <summary>
    /// A static class for reading an XML mod identity, regardless of the version
    /// </summary>
    public static class XmlModIdentity
    {
        public static Version ParseXmlVersion(XDocument document)
        {
            var xmlVersionAttr = document.Root.Attribute("installerSystemVersion");
            if (xmlVersionAttr != null)
            {
                if (Version.TryParse(xmlVersionAttr.Value, out Version version))
                {
                    return version;
                }
                else
                    throw new FormatException("Mod identity 'installerSystemVersion': '" + xmlVersionAttr.Value + "' is not a valid version");
            }
            else
            {
                throw new FormatException("Mod identity 'installerSystemVersion': '" + xmlVersionAttr.Value + "' is not a valid version");
            }
        }

        private static ModIdentity Parse(XDocument document, ManagedMod mod, Dictionary<string, System.Drawing.Image> images = null)
        {
            Version xmlVersion = ParseXmlVersion(document);
            if (xmlVersion.Major == 1)
            {
                return XmlModIdentityV1.ParseModIdentity(mod, document.Root, images);
            }
            else
            {
                throw new FormatException("Mod identity 'installerSystemVersion': '" + xmlVersion.ToString() + "' is not a supported version");
            }
        }

        /// <summary>
        /// Reads the mod identity from a stream. The parent ManagedMod can be null.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static ModIdentity Parse(Stream stream, ManagedMod mod, Dictionary<string, System.Drawing.Image> images = null)
        {
            return Parse(XDocument.Load(stream), mod, images);
        }

        /// <summary>
        /// Reads the mod identity from a file. The parent ManagedMod can be null.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static ModIdentity Parse(string path, ManagedMod mod)
        {
            return Parse(XDocument.Load(path), mod);
        }

        public static void CreateModInfoXml(string unique, string displayName, string dir, out XDocument document)
        {
            document = XDocument.Parse(@"<mod>
</mod>");
            document.Root.SetAttributeValue("unique", unique);
            document.Root.SetAttributeValue("displayName", displayName);
            document.Root.SetAttributeValue("installerSystemVersion", ModIdentity.XmlModIdentityVersion1_1_0_0.ToString());
            document.Root.SetAttributeValue("copyAllFiles", true.ToString());
            document.Root.SetAttributeValue("canDisable", false.ToString());
        }
    }
}
