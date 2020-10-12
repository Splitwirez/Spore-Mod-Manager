using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SporeMods.Core;
using SporeMods.Core.Mods;

namespace SporeMods.KitImporter
{
    public class KitSettings
    {
        public GameInfo.GameExecutableType ExecutableType = GameInfo.GameExecutableType.None;
        public string ForcedCoreSporeDataPath;
        public string ForcedGalacticAdventuresDataPath;
        public string ForcedSporebinEP1Path;
        public string ForcedGalacticAdventuresSporeAppPath;
        public string GamePath;
        public string SporeGamePath;
        public string SteamPath;
        public bool ForceGamePath;

        GameInfo.GameExecutableType ParseExecutableType(string text)
        {
            switch (text.ToLowerInvariant())
            {
                case "disk": return GameInfo.GameExecutableType.Disk__1_5_1;
                case "origin": return GameInfo.GameExecutableType.Origin__1_5_1;
                case "origin_patched": return GameInfo.GameExecutableType.GogOrSteam__March2017;
                case "steam": return GameInfo.GameExecutableType.GogOrSteam__1_5_1;
                case "steam_patched": return GameInfo.GameExecutableType.GogOrSteam__March2017;
                case "none":
                default:
                    return GameInfo.GameExecutableType.None;
            }
        }

        public void Load(string path)
        {
            var document = XDocument.Load(path);

            if (document.Root.Name.LocalName.ToLowerInvariant() != "settings") return;

            foreach (var element in document.Root.Elements())
            {
                switch (element.Name.LocalName.ToLowerInvariant())
                {
                    case "executabletype":
                        ExecutableType = ParseExecutableType(element.Value);
                        break;
                    case "forcedcoresporedatapath":
                        ForcedCoreSporeDataPath = element.Value;
                        break;
                    case "forcedgalacticadventuresdatapath":
                        ForcedGalacticAdventuresDataPath = element.Value;
                        break;
                    case "forcedsporebinep1path":
                        ForcedSporebinEP1Path = element.Value;
                        break;
                    case "forcedgalacticadventuressporeapppath":
                        ForcedGalacticAdventuresSporeAppPath = element.Value;
                        break;
                    case "gamepath":
                        GamePath = element.Value;
                        break;
                    case "sporegamepath":
                        SporeGamePath = element.Value;
                        break;
                    case "steampath":
                        SteamPath = element.Value;
                        break;
                    case "forcegamepath":
                        if (bool.TryParse(element.Value, out bool result))
                        {
                            ForceGamePath = result;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
