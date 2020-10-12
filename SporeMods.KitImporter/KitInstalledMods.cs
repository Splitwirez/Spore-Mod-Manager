using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SporeMods.Core.Mods;

namespace SporeMods.KitImporter
{
    public class KitMod
    {
        public string Name;
        public string Unique;
        public string DisplayName;
        public string ConfiguratorPath;
        public List<ModFile> Files = new List<ModFile>();
    }

    /// <summary>
    /// Used to read the list of mods installed in the old launcher kit.
    /// </summary>
    public class KitInstalledMods
    {
        public List<KitMod> Mods = new List<KitMod>();

        public void Load(string path)
        {
            var document = XDocument.Load(path);
            foreach (var element in document.Root.Elements())
            {
                if (element.Name.LocalName.ToLowerInvariant() != "mod") continue;

                var mod = new KitMod();

                var attr = element.Attribute("name");
                if (attr != null) mod.Name = attr.Value;

                attr = element.Attribute("unique");
                if (attr != null) mod.Unique = attr.Value;

                attr = element.Attribute("displayName");
                if (attr != null) mod.DisplayName = attr.Value;

                attr = element.Attribute("configurator");
                if (attr != null) mod.ConfiguratorPath = attr.Value;

                foreach (var fileElem in element.Elements())
                {
                    if (element.Name.LocalName.ToLowerInvariant() != "file") continue;

                    var file = new ModFile();
                    file.Name = element.Value;
                    file.GameDir = ComponentGameDir.ModAPI;

                    attr = element.Attribute("game");
                    if (attr != null)
                    {
                        switch (attr.Value.ToLowerInvariant())
                        {
                            case "galacticadventures":
                                file.GameDir = ComponentGameDir.GalacticAdventures;
                                break;
                            case "spore":
                                file.GameDir = ComponentGameDir.Spore;
                                break;
                            case "modapi":
                            default:
                                file.GameDir = ComponentGameDir.ModAPI;
                                break;
                        }
                    }

                    mod.Files.Add(file);
                }
            }
        }
    }
}
