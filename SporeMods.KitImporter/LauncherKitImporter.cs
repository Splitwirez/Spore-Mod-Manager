using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using SporeMods.Core;
using SporeMods.Core.Mods;

namespace SporeMods.KitImporter
{
    public static class LauncherKitImporter
    {
        static string GetModName(KitMod mod)
        {
            if (mod.DisplayName != null)
            {
                return mod.DisplayName;
            }
            else
            {
                return mod.Name;
            }
        }

        static void ImportSettings(KitSettings settings)
        {
            if (!settings.ForcedGalacticAdventuresSporeAppPath.IsNullOrEmptyOrWhiteSpace())
            {
                Settings.ForcedGameExeType = settings.ForcedGalacticAdventuresSporeAppPath;
            }
            if (!settings.ForcedCoreSporeDataPath.IsNullOrEmptyOrWhiteSpace())
            {
                Settings.ForcedCoreSporeDataPath = settings.ForcedCoreSporeDataPath;
            }
            if (!settings.ForcedSporebinEP1Path.IsNullOrEmptyOrWhiteSpace())
            {
                Settings.ForcedGalacticAdventuresSporebinEP1Path = settings.ForcedSporebinEP1Path;
            }
            if (!settings.ForcedGalacticAdventuresDataPath.IsNullOrEmptyOrWhiteSpace())
            {
                Settings.ForcedGalacticAdventuresDataPath = settings.ForcedGalacticAdventuresDataPath;
            }
            if (settings.ExecutableType != GameInfo.GameExecutableType.None)
            {
                Settings.ForcedGameExeType = settings.ExecutableType.ToString();
            }
        }

        /// <summary>
        /// Ensures that there are no installed mods in the launcher kit that use EXE custom installers.
        /// If there are, tells the user to either update them or uninstall them.
        /// This keeps repeating until there are no mods left, or the user presses cancel.
        /// Returns true when the program can continue;
        /// </summary>
        /// <param name="kitPath"></param>
        /// <param name="mods"></param>
        /// <returns></returns>
        static bool EnsureNoModsWithExeInstallers(string kitPath, KitInstalledMods mods)
        {
            // Search for EXE custom installers
            var modsWithExeInstallers = new List<KitMod>();
            foreach (var mod in mods.Mods)
            {
                if (mod.ConfiguratorPath != null && !mod.ConfiguratorPath.ToLowerInvariant().EndsWith(".xml"))
                {
                    modsWithExeInstallers.Add(mod);
                }
            }

            if (modsWithExeInstallers.Any())
            {
                string text = "The following mods have EXE custom installers:\n";
                foreach (var mod in modsWithExeInstallers)
                {
                    text += "  " + GetModName(mod) + "\n";
                }
                text += "\nPlease, update them or uninstall them. Clicking OK will open the Easy Uninstaller.";

                var result = MessageBox.Show(text, "Mods need to be updated/uninstalled", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    throw new OperationCanceledException();

                var process = Process.Start(Path.Combine(kitPath, "Spore ModAPI Easy Uninstaller.exe"));
                process.WaitForExit();

                return EnsureNoModsWithExeInstallers(kitPath, mods);
            }
            else
            {
                return true;
            }
        }

        static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);
            Permissions.GrantAccessDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
                Permissions.GrantAccessFile(tempPath);
            }
           
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath);
            }
        }

        static string GetKitModFilePath(string kitPath, ModFile file)
        {
            switch (file.GameDir)
            {
                case ComponentGameDir.GalacticAdventures:
                    return Path.Combine(GameInfo.GalacticAdventuresData, file.Name);
                case ComponentGameDir.Spore:
                    return Path.Combine(GameInfo.CoreSporeData, file.Name);
                case ComponentGameDir.Tweak:
                default:
                    return Path.Combine(kitPath, "mLibs", file.Name);
            }
        }

        static void ImportMods(string kitPath, KitInstalledMods mods)
        {
            EnsureNoModsWithExeInstallers(kitPath, mods);

            // Store the mods that didn't have a mod configs folder
            var modsWithNoConfig = new List<KitMod>();
            // Also keep track of which mods were already on the manager, so we can inform the user that we skipped them
            var modsAlreadyInstalled = new List<KitMod>();
            // Copy over the mod configs folders, creating XML identities for those mods that didn't have it
            foreach (var mod in mods.Mods)
            {
                string modConfigPath = Path.Combine(kitPath, "ModConfigs", mod.Unique);
                if (Directory.Exists(modConfigPath))
                {
                    string managerModConfigPath = Path.Combine(Settings.ModConfigsPath, mod.Unique);
                    if (Directory.Exists(managerModConfigPath))
                    {
                        modsAlreadyInstalled.Add(mod);
                    }
                    else
                    {
                        DirectoryCopy(modConfigPath, managerModConfigPath);

                        bool usesLegacyDlls = true;

                        if (!File.Exists(Path.Combine(managerModConfigPath, "ModInfo.xml")))
                        {
                            string displayName = mod.DisplayName;
                            if (displayName == null) displayName = mod.Name;
                            if (displayName == null) displayName = mod.Unique;
                            ModInstallation.CreateModInfoXml(mod.Unique, mod.Name, managerModConfigPath, out XDocument document);
                        }
                        else
                        {
                            var document = XDocument.Load(Path.Combine(managerModConfigPath, "ModInfo.xml"));
                            var xmlVersionAttr = document.Root.Attribute("installerSystemVersion");
                            if (xmlVersionAttr != null && Version.TryParse(xmlVersionAttr.Value, out Version version)
                                && version != ModIdentity.XmlModIdentityVersion1_0_0_0)
                            {
                                usesLegacyDlls = false;
                            }
                        }

                        if (usesLegacyDlls)
                        {
                            string legacyPath = Path.Combine(managerModConfigPath, "UseLegacyDLLs");
                            File.WriteAllText(legacyPath, string.Empty);
                            Permissions.GrantAccessFile(legacyPath);
                        }
                    }
                }
                else
                {
                    modsWithNoConfig.Add(mod);
                }
            }

            foreach (var mod in modsWithNoConfig)
            {
                // Try to gather the files if they still exist
                var filesToCopy = new List<string>();
                if (mod.ConfiguratorPath != null && File.Exists(mod.ConfiguratorPath))
                {
                    filesToCopy.Add(mod.ConfiguratorPath);
                }
                foreach (var file in mod.Files)
                {
                    string filePath = GetKitModFilePath(kitPath, file);
                    if (File.Exists(filePath)) filesToCopy.Add(filePath);
                }

                if (filesToCopy.Any())
                {
                    string managerModConfigPath = Path.Combine(Settings.ModConfigsPath, mod.Unique);
                    Directory.CreateDirectory(managerModConfigPath);
                    Permissions.GrantAccessDirectory(managerModConfigPath);

                    foreach (var file in filesToCopy)
                    {
                        File.Copy(file, Path.Combine(managerModConfigPath, Path.GetFileName(file)));
                    }

                    bool usesLegacyDlls = true;

                    if (!File.Exists(Path.Combine(managerModConfigPath, "ModInfo.xml")))
                    {
                        string displayName = mod.DisplayName;
                        if (displayName == null) displayName = mod.Name;
                        if (displayName == null) displayName = mod.Unique;
                        ModInstallation.CreateModInfoXml(mod.Unique, mod.Name, managerModConfigPath, out XDocument document);
                    }
                    else
                    {
                        var document = XDocument.Load(Path.Combine(managerModConfigPath, "ModInfo.xml"));
                        var xmlVersionAttr = document.Root.Attribute("installerSystemVersion");
                        if (xmlVersionAttr != null && Version.TryParse(xmlVersionAttr.Value, out Version version)
                            && version != ModIdentity.XmlModIdentityVersion1_0_0_0)
                        {
                            usesLegacyDlls = false;
                        }
                    }

                    if (usesLegacyDlls)
                    {
                        string legacyPath = Path.Combine(managerModConfigPath, "UseLegacyDLLs");
                        File.WriteAllText(legacyPath, string.Empty);
                        Permissions.GrantAccessFile(legacyPath);
                    }
                }
            }

            if (modsAlreadyInstalled.Any())
            {
                string text = "The following mods were already installed on the Mod Manager, so they have been skipped:\n";
                foreach (var mod in modsAlreadyInstalled)
                {
                    text += "  " + GetModName(mod) + "\n";
                }
                MessageBox.Show(text, "A few mods were skipped");
            }
        }

        public static void Import(string kitPath)
        {
            string configPath = Path.Combine(kitPath, "LauncherSettings.config");
            if (File.Exists(configPath))
            {
                var kitSettings = new KitSettings();
                kitSettings.Load(configPath);
                ImportSettings(kitSettings);
            }

            string modsPath = Path.Combine(kitPath, "InstalledMods.config");
            if (File.Exists(modsPath))
            {
                var kitMods = new KitInstalledMods();
                kitMods.Load(modsPath);
                ImportMods(kitPath, kitMods);
            }
        }
    }
}
