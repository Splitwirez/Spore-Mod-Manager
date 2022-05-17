using SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public abstract partial class MI1_0_X_XMod : NotifyPropertyChangedBase, ISporeMod
    {
        public static async Task<ISporeMod> AnalyzeFromSporemodAsync(string inPath, ZipArchive archive)
        {
            /*await Task.Run(() =>
            {
                string debugText1 = "\n\n\nBegin Entries\n";
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    debugText1 += $"\t\t{entry.FullName}\n";
                }
                debugText1 += "End Entries\n\n\n";
                MessageDisplay.ShowMessageBox(debugText1);
            });*/

            return await Task<ISporeMod>.Run(() =>
            {
                if (archive.TryGetEntry(ModConstants.ID_XML_FILE_NAME, out ZipArchiveEntry identityEntry))
                {
                    XDocument doc = null;
                    using (StreamReader reader = new StreamReader(identityEntry.Open()))
                    {
                        string text = reader.ReadToEnd();
                        doc = XDocument.Parse(text, ModConstants.ID_XML_LOAD_OPTIONS);
                    }
                    if (doc == null)
                        return null;

                    var xmlRoot = doc.Root;

                    var versionAttr = xmlRoot.Attribute("installerSystemVersion");
                    Version identityVersion = new Version(0, 0, 0, 0);
                    if (versionAttr == null)
                        throw new FormatException(Externals.GetLocalizedText("Mods!Error!Identity!MissingSysVersion"));
                    else if (Version.TryParse(versionAttr.Value, out identityVersion))
                    {
                        if (
                                   (identityVersion != ModConstants.ID_VER_1_0_0_0)
                                && (identityVersion != ModConstants.ID_VER_1_0_1_0)
                                && (identityVersion != ModConstants.ID_VER_1_0_1_1)
                            )
                            throw new FormatException(Externals.GetLocalizedText("Mods!Error!Identity!UnsupportedSysVersion").Replace("%VERSION%", identityVersion.ToString()));
                    }
                    else
                        throw new FormatException(Externals.GetLocalizedText("Mods!Error!Identity!InvalidAttributeValue").Replace("%ATTRIBUTE%", "installerSystemVersion").Replace("%VALUE%", versionAttr.Value).Replace("%TYPE%", "Version"));


                    var uniqueAttr = xmlRoot.Attribute("unique");
                    if (uniqueAttr == null)
                        throw new FormatException("A mod must have a 'unique' attribute");
                    string unique = uniqueAttr.Value;

                    string modConfigSubfolderName = unique;
                    foreach (char c in Path.GetInvalidPathChars())
                        modConfigSubfolderName = modConfigSubfolderName.Replace(c.ToString(), string.Empty);

                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        modConfigSubfolderName = modConfigSubfolderName.Replace(c, '-');
                    }

                    List<string> fileNames = new List<string>();
                    foreach (var h in archive.Entries)
                    {
                        fileNames.Add(Path.GetFileName(h.Name));
                    }


                    MI1_0_X_XMod mod = null;

                    if (identityVersion == ModConstants.ID_VER_1_0_0_0)
                        mod = new MI1_0_0_0Mod(modConfigSubfolderName, unique, fileNames);
                    else
                        mod = new MI1_0_1_1Mod(modConfigSubfolderName, unique, fileNames);

                    mod.ReadIdentity(doc);

                    return mod;
                }
                return null;
            });
        }


        List<string> _fileNames = new List<string>();
        public MI1_0_X_XMod(string recordDirName, string unique, List<string> fileNames)
        {
            RecordDirName = recordDirName;
            Unique = unique;
            _fileNames = fileNames;
        }


        protected virtual void ReadIdentity(XDocument xml)
        {
            var xmlRoot = xml.Root;
            ReadIdentityRoot(xmlRoot);
            ReadIdentityComponents(xmlRoot);
        }

        protected virtual void ReadIdentityRoot(XElement xmlRoot)
        {
            var displayNameAttr = xmlRoot.Attribute("displayName");
            if (displayNameAttr != null)
                DisplayName = new FixedModText(displayNameAttr.Value);
            else
                DisplayName = new FixedModText(Unique);


            var descriptionAttr = xmlRoot.Attribute("description");
            if (descriptionAttr != null)
                InlineDescription = new FixedModText(descriptionAttr.Value);
        }

        protected void ReadIdentityComponents(XElement xmlRoot)
        {
            foreach (XElement el in xmlRoot.Elements())
            {
                if (TryReadIdentityComponent(el, out ComponentBase component))
                {
                    AllComponents.Add(component);
                    if ((component is Feature) || (component is RadioGroup))
                        FeatureComponents.Add(component);
                }
            }
        }

        protected virtual bool TryReadIdentityComponent(XElement xmlEl, out ComponentBase component)
        {
            string elName = xmlEl.Name.LocalName.ToLowerInvariant();

            component = null;
            
            if (elName == "prerequisite")
                component = Prerequisite.FromXml(this, xmlEl, _fileNames);
            else if (elName == "compatfile")
                component = CompatFile.FromXml(this, xmlEl, _fileNames);
            else if (elName == "componentgroup")
                component = new RadioGroup(this, xmlEl, _fileNames);
            else if (elName == "component")
                component = new Feature(this, xmlEl, _fileNames);

            return component != null;
        }
    }
}
