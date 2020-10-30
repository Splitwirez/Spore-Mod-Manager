using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public static class XmlModIdentityV1
    { 
        static List<ModFile> ParseFiles(string fileNamesStr, string fileGamesStr, string errorTitle, string attributeGame)
        {
            var output = new List<ModFile>();
            string[] fileNames = { };
            string[] fileGames = { };

            if (!string.IsNullOrWhiteSpace(fileNamesStr))
                fileNames = fileNamesStr.Split('?');

            if (fileGamesStr != null)
            {
                fileGames = fileGamesStr.Split('?');
                if (fileGames.Length != fileNames.Length)
                    throw new FormatException(errorTitle + ": the '" + attributeGame + "' attribute must have as many elements as files in the component");
            }

            for (int i = 0; i < fileNames.Length; ++i)
            {
                var file = new ModFile();
                file.Name = fileNames[i];
                if (fileGames.Any())
                {
                    if (!Enum.TryParse(fileGames[i], out file.GameDir))
                        throw new FormatException(errorTitle + ": '" + fileGames[i] + "' is not a valid value for '" + attributeGame + "'");
                }
                else
                {
                    file.GameDir = ComponentGameDir.ModAPI;
                }

                output.Add(file);
            }

            return output;
        }

        static ModComponent ParseModComponent(ModIdentity identity, XElement node)
        {
            var uniqueAttr = node.Attribute("unique");
            if (uniqueAttr == null)
                throw new FormatException("A component must have a 'unique' attribute");

            var component = new ModComponent(identity, uniqueAttr.Value);

            var displayAttr = node.Attribute("displayName");
            if (displayAttr != null)
                component.DisplayName = displayAttr.Value;


            if (node.Name.LocalName.ToLowerInvariant() == "componentgroup")
            {
                foreach (XElement subNode in node.Elements())
                {
                    var subComponent = ParseModComponent(identity, subNode);
                    subComponent.Parent = component;
                    component.SubComponents.Add(subComponent);
                }
                if (!component.SubComponents.Any())
                {
                    throw new FormatException("Component group " + component.Unique + ": a 'componentgroup' must have at least one subcomponent");
                }
            }
            else
            {
                var descAttr = node.Attribute("description");
                if (descAttr != null)
                    component.Description = descAttr.Value;

                var gameAttr = node.Attribute("game");

                component.Files.AddRange(ParseFiles(node.Value, gameAttr == null ? null : gameAttr.Value, 
                    "Component " + component.Unique, "game"));

                var defaultCheckedAttr = node.Attribute("defaultChecked");
                if (defaultCheckedAttr != null)
                {
                    if (bool.TryParse(defaultCheckedAttr.Value, out bool isEnabled))
                    {
                        component.EnabledByDefault = isEnabled;
                    }
                    else
                        throw new FormatException("Component " + component.Unique + ": '" + defaultCheckedAttr.Value + "' is not a boolean");
                }
            }

            var imagePlacement = node.Attribute("imagePlacement");
            if (imagePlacement != null)
            {
                if (Enum.TryParse(imagePlacement.Value, out ImagePlacementType placement))
                    component.ImagePlacement = placement;
                else
                    throw new FormatException("Component " + component.Unique + ": '" + imagePlacement.Value + "' is not a valid value for 'imagePlacement'");
            }

            return component;
        }

        public static ModIdentity ParseModIdentity(ManagedMod mod, XElement node)
        {
            var uniqueAttr = node.Attribute("unique");
            if (uniqueAttr == null)
                throw new FormatException("A mod must have a 'unique' attribute");

            var identity = new ModIdentity(mod, uniqueAttr.Value);

            var displayAttr = node.Attribute("displayName");
            if (displayAttr != null)
                identity.DisplayName = displayAttr.Value;

            var descAttr = node.Attribute("description");
            if (descAttr != null)
                identity.Description = descAttr.Value;

            var modVersionAttr = node.Attribute("modVersion");
            if (modVersionAttr != null)
            {
                if (Version.TryParse(modVersionAttr.Value, out Version version))
                {
                    identity.ModVersion = version;
                }
                else
                    throw new FormatException(Settings.GetLanguageString(3, "ModIdentityCantParseValue").Replace("%ATTRIBUTE%", "modVersion").Replace("%VALUE%", modVersionAttr.Value).Replace("%TYPE%", "version"));
            }

            var xmlVersionAttr = node.Attribute("installerSystemVersion");
            if (xmlVersionAttr != null)
            {
                if (Version.TryParse(xmlVersionAttr.Value, out Version version))
                {
                    identity.InstallerSystemVersion = version;
                }
                else
                    throw new FormatException(Settings.GetLanguageString(3, "ModIdentityCantParseValue").Replace("%ATTRIBUTE%", "installerSystemVersion").Replace("%VALUE%", xmlVersionAttr.Value).Replace("%TYPE%", "Version")); //throw new FormatException("Mod identity 'installerSystemVersion': '" + xmlVersionAttr.Value + "' is not a valid version");
            }

            var canDisableAttr = node.Attribute("canDisableMod");
            if (canDisableAttr != null)
            {
                if (bool.TryParse(canDisableAttr.Value, out bool canDisable))
                {
                    identity.CanDisable = canDisable;
                }
                else
                    throw new FormatException(Settings.GetLanguageString(3, "ModIdentityCantParseValue").Replace("%ATTRIBUTE%", "canDisableMod").Replace("%VALUE%", canDisableAttr.Value).Replace("%TYPE%", "bool")); //throw new FormatException("Mod identity 'canDisableMod': '" + canDisableAttr.Value + "' is not a boolean");
            }

            var hasCustomInstallerAttr = node.Attribute("hasCustomInstaller");
            if (hasCustomInstallerAttr != null)
            {
                if (bool.TryParse(hasCustomInstallerAttr.Value, out bool hasCustomInstaller))
                {
                    identity.HasCustomInstaller = hasCustomInstaller;
                }
                else
                    throw new FormatException(Settings.GetLanguageString(3, "ModIdentityCantParseValue").Replace("%ATTRIBUTE%", "hasCustomInstaller").Replace("%VALUE%", hasCustomInstallerAttr.Value).Replace("%TYPE%", "bool")); //throw new FormatException("Mod identity 'hasCustomInstaller': '" + hasCustomInstallerAttr.Value + "' is not a boolean");
            }
            else if (identity.InstallerSystemVersion == ModIdentity.XmlModIdentityVersion1_0_0_0)
            {
                identity.HasCustomInstaller = true;
                var compatOnlyAttr = node.Attribute("compatOnly");
                if (compatOnlyAttr != null)
                {
                    if (bool.TryParse(compatOnlyAttr.Value, out bool compatOnly))
                    {
                        identity.HasCustomInstaller = !compatOnly;
                    }
                    else
                        throw new FormatException(Settings.GetLanguageString(3, "ModIdentityCantParseValue").Replace("%ATTRIBUTE%", "compatOnly").Replace("%VALUE%", compatOnlyAttr.Value).Replace("%TYPE%", "bool").Replace("%TYPE%", "bool")); //throw new FormatException("Mod identity 'compatOnly': '" + compatOnlyAttr.Value + "' is not a boolean");
                }
            }

            var warnAttr = node.Attribute("isExperimental");
            if (warnAttr != null)
            {
                if (bool.TryParse(warnAttr.Value, out bool isExperimental) && isExperimental)
                    identity.IsExperimental = isExperimental;
            }
            
            warnAttr = node.Attribute("requiresGalaxyReset");
            if (warnAttr != null)
            {
                if (bool.TryParse(warnAttr.Value, out bool requiresGalaxyReset) && requiresGalaxyReset)
                    identity.RequiresGalaxyReset = requiresGalaxyReset;
            }

            warnAttr = node.Attribute("causesSaveDataDependency");
            if (warnAttr != null)
            {
                if (bool.TryParse(warnAttr.Value, out bool causesSaveDataDependency) && causesSaveDataDependency)
                    identity.CausesSaveDataDependency = causesSaveDataDependency;
            }


            var tagsElem = node.Element("tags");
            if (tagsElem != null)
            {
                foreach (XElement element in tagsElem.Elements("tag"))
                    identity.Tags.Add(element.Value);
            }

            foreach (XElement subNode in node.Elements())
            {
                string nodeName = subNode.Name.LocalName.ToLowerInvariant();

                if (nodeName == "prerequisite")
                {
                    var gameAttr = subNode.Attribute("game");

                    identity.Files.AddRange(ParseFiles(subNode.Value, gameAttr == null ? null : gameAttr.Value,
                        "Mod 'prerequisite'", "game"));
                }
                else if (nodeName == "remove")
                {
                    var gameAttr = subNode.Attribute("game");

                    identity.FilesToRemove.AddRange(ParseFiles(subNode.Value, gameAttr == null ? null : gameAttr.Value,
                        "Mod 'remove'", "game"));
                }
                else if (nodeName == "compatfile")
                {
                    var targetAttr = subNode.Attribute("compatTargetFileName");
                    var targetGameAttr = subNode.Attribute("compatTargetGame");
                    var gameAttr = subNode.Attribute("game");

                    var compatibility = new ModCompatibilityFix();
                    identity.CompatibilityFixes.Add(compatibility);

                    compatibility.FilesToAdd.AddRange(ParseFiles(subNode.Value, gameAttr == null ? null : gameAttr.Value,
                        "'compatFile' files", "game"));

                    compatibility.RequiredFiles.AddRange(ParseFiles(targetAttr.Value, targetGameAttr == null ? null : targetGameAttr.Value,
                        "'compatFile' target files", "compatTargetGame"));

                    var removeTargetAttr = subNode.Attribute("removeTargets");
                    if (removeTargetAttr != null)
                    {
                        if (bool.TryParse(removeTargetAttr.Value, out bool removeTargets))
                        {
                            if (removeTargets)
                            {
                                compatibility.FilesToRemove.AddRange(compatibility.RequiredFiles);
                            }
                        }
                        else
                            throw new FormatException(Settings.GetLanguageString(3, "ModIdentityCantParseValue").Replace("%ATTRIBUTE%", "removeTargets").Replace("%VALUE%", removeTargetAttr.Value).Replace("%TYPE%", "bool"));  //"'compatFile' 'removeTargets': '" + removeTargetAttr.Value + "' is not a boolean");
                    }
                }
                else if (nodeName == "component" || nodeName == "componentgroup")
                {
                    var component = ParseModComponent(identity, subNode);
                    component.Parent = identity;
                    if (nodeName == "componentgroup") component.IsGroup = true;
                    identity.SubComponents.Add(component);
                }
                else
                {
                    throw new FormatException(Settings.GetLanguageString(3, "ModIdentityUnrecognizedTag").Replace("%TAGNAME%", subNode.Name.LocalName));//"Unknown element '" + subNode.Name.LocalName + "'");
                }
            }

            return identity;
        }
    }
}
