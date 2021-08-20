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

        private static ModIdentity Parse(XDocument document, ManagedMod mod)
        {
            Version xmlVersion = ParseXmlVersion(document);
            if (xmlVersion.Major == 1)
            {
                return XmlModIdentityV1.ParseModIdentity(mod, document.Root);
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
        public static ModIdentity Parse(Stream stream, ManagedMod mod)
        {
            return Parse(XDocument.Load(stream), mod);
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
    }
}
