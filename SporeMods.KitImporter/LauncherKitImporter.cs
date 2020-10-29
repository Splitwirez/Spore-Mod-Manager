﻿using System;
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
        public static event EventHandler ModSkipped;
        
        
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
                string text = "The following mods have EXE custom installers (which are no longer supported, and have already been deprecated for years now):\n";
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
            //DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            //DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            Permissions.GrantAccessDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            //FileInfo[] files = dir.GetFiles();
            foreach (string file in Directory.EnumerateFiles(sourceDirName))
            {
                string tempPath = Path.Combine(destDirName, Path.GetFileName(file));
                File.Copy(file, tempPath);
                Permissions.GrantAccessFile(tempPath);
            }

            foreach (string subdir in Directory.EnumerateDirectories(sourceDirName))
            {
                string tempPath = Path.Combine(destDirName, Path.GetFileName(subdir));
                DirectoryCopy(subdir, tempPath);
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

        static IEnumerable<ImportFailureEventArgs> ImportMods(string kitPath, KitInstalledMods mods)
        {
            List<ImportFailureEventArgs> failures = new List<ImportFailureEventArgs>();


            EnsureNoModsWithExeInstallers(kitPath, mods);

            // Store the mods that didn't have a mod configs folder
            var modsWithNoConfig = new List<KitMod>();
            // Also keep track of which mods were already on the manager, so we can inform the user that we skipped them
            //var modsAlreadyInstalled = new List<KitMod>();

            string kitModConfigsPath = Path.Combine(kitPath, "ModConfigs");

            // Copy over the mod configs folders, creating XML identities for those mods that didn't have it
            foreach (var mod in mods.Mods)
            {
                try
                {
                    string managerModConfigPath = Path.Combine(Settings.ModConfigsPath, mod.Unique);

                    /*if (!Directory.Exists(managerModConfigPath))
                        managerModConfigPath = Path.Combine(Settings.ModConfigsPath, mod.Name);*/

                    if (Directory.Exists(managerModConfigPath))
                    {
                        //modsAlreadyInstalled.Add(mod);
                        ModSkipped?.Invoke(mod, null);
                        continue;
                    }


                    string modConfigPath = Path.Combine(kitModConfigsPath, mod.Unique);
                    if (!Directory.Exists(modConfigPath))
                    {
                        foreach (string dir in Directory.EnumerateDirectories(kitModConfigsPath))
                        {
                            string modInfoPath = Path.Combine(dir, "ModInfo.xml");
                            if (File.Exists(modInfoPath))
                            {
                                XDocument modInfo = XDocument.Load(modInfoPath);
                                var uniqueAttr = modInfo.Root.Attribute("unique");
                                if (uniqueAttr != null)
                                {
                                    if (uniqueAttr.Value == mod.Unique)
                                    {
                                        modConfigPath = dir;
                                        break;
                                    }
                                }
                            }
                        }
                    }


                    if (Directory.Exists(modConfigPath))
                    {
                        DirectoryCopy(modConfigPath, managerModConfigPath);

                        bool usesLegacyDlls = true;

                        if (!File.Exists(Path.Combine(managerModConfigPath, "ModInfo.xml")))
                        {
                            string displayName = mod.DisplayName;
                            if (displayName == null) displayName = mod.Name;
                            if (displayName == null) displayName = mod.Unique;
                            ModInstallation.CreateModInfoXml(mod.Unique, mod.Name, managerModConfigPath, out XDocument document);

                            foreach (ModFile file in mod.Files)
                            {
                                XElement fileEl = new XElement("prerequisite", file.Name);
                                string gameDir = file.GameDir.ToString();
                                if (!gameDir.IsNullOrEmptyOrWhiteSpace())
                                    fileEl.SetAttributeValue("game", gameDir);
                            }
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
                    else
                    {
                        modsWithNoConfig.Add(mod);
                    }


                    Dictionary<string, string> launcherKitToMgrDllPaths = new Dictionary<string, string>();

                    foreach (ModFile file in mod.Files)
                    {
                        if (file.GameDir == ComponentGameDir.ModAPI)
                        {
                            if (
                                file.Name.ToLowerInvariant().EndsWith("-disk.dll") ||
                                file.Name.ToLowerInvariant().EndsWith("-steam.dll") ||
                                file.Name.ToLowerInvariant().EndsWith("-steam_patched.dll")
                                )
                            {
                                launcherKitToMgrDllPaths.Add(Path.Combine(kitPath, file.Name), Path.Combine(Settings.LegacyLibsPath, file.Name));
                            }
                            else
                            {
                                launcherKitToMgrDllPaths.Add(Path.Combine(kitPath, "mLibs", file.Name), Path.Combine(Settings.LegacyLibsPath, file.Name));
                            }
                        }
                    }

                    foreach (string key in launcherKitToMgrDllPaths.Keys)
                    {
                        File.Copy(key, launcherKitToMgrDllPaths[key]);
                        Permissions.GrantAccessFile(launcherKitToMgrDllPaths[key]);
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(new ImportFailureEventArgs(ex, mod));
                }
            }

            foreach (var mod in modsWithNoConfig)
            {
                try
                {
                    bool usesLegacyDlls = false;
                    Dictionary<string, string> launcherKitToMgrDllPaths = new Dictionary<string, string>();

                    // Try to gather the files if they still exist
                    var filesToCopy = new List<string>();
                    if (mod.ConfiguratorPath != null && File.Exists(mod.ConfiguratorPath))
                    {
                        filesToCopy.Add(mod.ConfiguratorPath);
                    }

                    foreach (var file in mod.Files)
                    {
                        string filePath = GetKitModFilePath(kitPath, file);
                        if (File.Exists(filePath))
                            filesToCopy.Add(filePath);
                        else
                        {
                            if (filePath.ToLowerInvariant().Contains(@"\mlibs\"))
                            {

                                filePath = Path.Combine(kitPath, file.Name);
                                if (File.Exists(filePath))
                                    filesToCopy.Add(filePath);
                            }
                        }

                        if (
                            filePath.ToLowerInvariant().EndsWith("-disk.dll") ||
                            filePath.ToLowerInvariant().EndsWith("-steam.dll") ||
                            filePath.ToLowerInvariant().EndsWith("-steam_patched.dll")
                            )
                        {
                            usesLegacyDlls = true;


                            string kitDllPath = Path.Combine(kitPath, Path.GetFileName(filePath));
                            string mgrDllPath = Path.Combine(Settings.LegacyLibsPath, Path.GetFileName(filePath));
                            MessageBox.Show(kitDllPath + "\n\n\n" + mgrDllPath, "DLL PATHS");
                            launcherKitToMgrDllPaths.Add(kitDllPath, mgrDllPath);
                        }
                        else if (filePath.ToLowerInvariant().Contains(@"\mlibs\"))
                        {
                            launcherKitToMgrDllPaths.Add(filePath, Path.Combine(Settings.ModLibsPath, Path.GetFileName(filePath)));
                        }
                    }


                    string managerModConfigPath = Path.Combine(Settings.ModConfigsPath, mod.Unique);
                    Directory.CreateDirectory(managerModConfigPath);
                    Permissions.GrantAccessDirectory(managerModConfigPath);

                    foreach (var file in filesToCopy)
                    {
                        string outPath = Path.Combine(managerModConfigPath, Path.GetFileName(file));
                        File.Copy(file, outPath);
                        Permissions.GrantAccessFile(outPath);
                    }

                    
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
                            && (version == ModIdentity.XmlModIdentityVersion1_0_0_0))
                        {
                            usesLegacyDlls = true;
                        }
                    }

                    if (usesLegacyDlls)
                    {
                        string legacyPath = Path.Combine(managerModConfigPath, "UseLegacyDLLs");
                        File.WriteAllText(legacyPath, string.Empty);
                        Permissions.GrantAccessFile(legacyPath);
                    }

                    foreach (string key in launcherKitToMgrDllPaths.Keys)
                    {
                        if (!File.Exists(launcherKitToMgrDllPaths[key]))
                        {
                            File.Copy(key, launcherKitToMgrDllPaths[key]);
                            Permissions.GrantAccessFile(launcherKitToMgrDllPaths[key]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(new ImportFailureEventArgs(ex, mod));
                }
            }

            /*if (modsAlreadyInstalled.Count > 0)
            {
                string text = "The following mods were already installed on the Mod Manager, so they have been skipped:\n";
                foreach (var mod in modsAlreadyInstalled)
                {
                    text += "  " + GetModName(mod) + "\n";
                }
                MessageBox.Show(text, "A few mods were skipped");
            }*/
            return failures;
        }

        public static ImportResult Import(string kitPath)
        {
            Exception settingsReason = null;
            //ImportResult result = null;

            try
            {
                string configPath = Path.Combine(kitPath, "LauncherSettings.config");
                if (File.Exists(configPath))
                {
                    var kitSettings = new KitSettings();
                    kitSettings.Load(configPath);
                    ImportSettings(kitSettings);
                }
            }
            catch (Exception ex)
            {
                settingsReason = ex;
            }

            string modsPath = Path.Combine(kitPath, "InstalledMods.config");

            List<KitMod> skippedMods = new List<KitMod>();
            List<ImportFailureEventArgs> failedMods = new List<ImportFailureEventArgs>();
            bool hasRecord = File.Exists(modsPath);
            
            ModSkipped += (sneder, args) =>
            {
                if (sneder is KitMod skipped)
                    skippedMods.Add(skipped);
            };
            KitInstalledMods.ModImportFailed += (sneder, args) =>
            {
                if (sneder is ImportFailureEventArgs failure)
                    failedMods.Add(failure);
            };

            if (hasRecord)
            {
                var kitMods = new KitInstalledMods(modsPath);
                foreach (ImportFailureEventArgs fail in ImportMods(kitPath, kitMods))
                    failedMods.Add(fail);
            }

            return new ImportResult(skippedMods, failedMods, hasRecord, settingsReason);
        }
    }

    public class ImportResult
    {
        public bool HasInstalledModsRecord
        {
            get;
            set;
        } = true;

        public Exception SettingsImportFailedReason
        {
            get;
            set;
        } = null;

        public List<KitMod> SkippedMods
        {
            get;
            set;
        } = new List<KitMod>();

        public List<ImportFailureEventArgs> FailedMods
        {
            get;
            set;
        } = new List<ImportFailureEventArgs>();

        public ImportResult(List<KitMod> skippedMods, List<ImportFailureEventArgs> failedMods, bool hasModsRecord, Exception settingsImported)
        {
            SkippedMods = skippedMods;
            FailedMods = failedMods;
            HasInstalledModsRecord = hasModsRecord;
            SettingsImportFailedReason = settingsImported;
        }
    }

    public class ImportFailureEventArgs : EventArgs
    {
        public Exception Reason
        {
            get;
            set;
        } = null;
        public KitMod Mod
        {
            get;
            set;
        } = null;

        public ImportFailureEventArgs(Exception reason, KitMod mod)
        {
            Reason = reason;
            Mod = mod;
        }
    }
}
