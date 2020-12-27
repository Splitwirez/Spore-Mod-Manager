using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SporeMods.Core
{
    public static class GameInfo
    {
        public enum GameExecutableType
        {
            Disk__1_5_1,
            Origin__1_5_1,
            Origin__March2017,
            GogOrSteam__1_5_1,
            GogOrSteam__March2017,
            None
        }

        public static Dictionary<long, GameExecutableType> ExecutableFileGameTypes = new Dictionary<long, GameExecutableType>
        {
            [24909584] = GameExecutableType.Disk__1_5_1,
            [31347984] = GameExecutableType.Origin__1_5_1,
            [24898224] = GameExecutableType.Origin__March2017,
            [24888320] = GameExecutableType.GogOrSteam__1_5_1,
            [24885248] = GameExecutableType.GogOrSteam__March2017,
            [0] = GameExecutableType.None
        };

        /*public static int[] ExecutableSizes = { 
                                       /* DISK*    24909584,
                                       /* ORIGIN * 31347984,
                                       /* ORIGIN_P * 24898224,
                                       /* STEAM *  24888320,
                                       /* STEAM_P *24885248,
                                       /*NONE* 0};

        public static string[] VersionNames = {
                                                  "disk",
                                                  "steam_patched",  // origin uses the steam_patched one
                                                  "steam_patched",  // origin uses the steam_patched one
                                                  "steam",
                                                  "steam_patched",
                                                  null };*/
        public static string GetExecutableDllSuffix(GameExecutableType version)
        {
            if (version == GameExecutableType.Disk__1_5_1)
                return "disk";
            else if (version == GameExecutableType.GogOrSteam__1_5_1)
                return "steam";
            else
                return "steam_patched";
        }


        static string _sporeApp = "SporeApp.exe";
        static string _sporeAppModApiFix = "SporeApp_ModAPIFix.exe";

        public static Dictionary<GameExecutableType, string> ExecutableFileNames = new Dictionary<GameExecutableType, string>
        {
            [GameExecutableType.Disk__1_5_1] = _sporeApp,
            [GameExecutableType.Origin__1_5_1] = _sporeAppModApiFix,
            [GameExecutableType.Origin__March2017] = _sporeAppModApiFix,
            [GameExecutableType.GogOrSteam__1_5_1] = _sporeApp,
            [GameExecutableType.GogOrSteam__March2017] = _sporeApp,
            [GameExecutableType.None] = null
        };

        public enum GameDlc
        {
            CoreSpore,
            CreepyAndCute,
            GalacticAdventures,
            None
        }

        public static Dictionary<GameDlc, string> RegistrySuffixes = new Dictionary<GameDlc, string>
        {
            
            [GameDlc.CoreSpore] = @"SPORE",
            [GameDlc.CreepyAndCute] = @"SPORE(TM) Creepy & Cute Parts Pack",
            [GameDlc.GalacticAdventures] = @"SPORE_EP1",
            [GameDlc.None] = null
        };

        static string _registryPath64bit = @"SOFTWARE\Wow6432Node\Electronic Arts";
        static string _registryPath32bit = @"SOFTWARE\Electronic Arts";

        public static string[] RegistryValueNames =
        {
            "InstallLoc",
            "Install Dir",
            "DataDir" //Steam and GOG only have this one
        };


        public static string GetRegistryPath(GameDlc dlc)
        {
            if (dlc != GameDlc.None)
            {
                string output = null;

                RegistryKey key = Registry.LocalMachine.OpenSubKey(_registryPath64bit, false);
                if (key == null)
                    key = Registry.LocalMachine.OpenSubKey(_registryPath32bit, false);

                if (key != null)
                {
                    output = key.Name;

                    string slash = @"\";

                    if (!output.EndsWith(slash))
                        output += slash;

                    output += RegistrySuffixes[dlc];
                }

                //MessageDisplay.DebugShowMessageBox("REGISTRY PATH: " + output);
                return output;
            }
            else
                throw new InvalidOperationException("Cannot retrieve registry path for GameDlc.None");
        }

        /*static string[] _installDirSubfolders =
        {
            "dataep1",
            "data",
            "sporebinep1",
            "sporebin",
            "support"
        };*/

        public static bool CorrectGameInstallPath(string subPath, GameDlc dlc, out string fixedPath)
        {
            string output = StripTrailingCharacters(subPath);

            bool isSporePath = true;
            while (!IsPathGameInstallRoot(output, dlc))
            {
                if (output.Contains(@"\"))
                {
                    //MessageDisplay.DebugShowMessageBox("EnsureGameInstallPathIsInstallRoot output: " + output);
                    if (IsPathGameInstallRoot(output, dlc))
                        break;
                    else
                        output = output.Substring(0, output.LastIndexOf(@"\"));
                }
                else
                {
                    isSporePath = false;
                    break;
                }
            }

            fixedPath = output;
            return isSporePath;
        }

        static string EnsureGameInstallPathIsInstallRoot(string subPath, GameDlc dlc)
        {
            bool isSporePath = CorrectGameInstallPath(subPath, dlc, out string output);

            //MessageDisplay.DebugShowMessageBox("isSporePath: " + isSporePath + ", " + output);

            if (isSporePath)
            {
                //MessageDisplay.DebugShowMessageBox("INSTALL ROOT: " + output);
                return output;
            }
            else
            {
                if (!BadGameInstallPaths.Any(x => (x.BasePath == subPath) && (x.BadPath == output) && (x.DlcLevel == dlc) && (!x.IsSporebin)))
                    BadGameInstallPaths.Add(new BadPathEventArgs(subPath, output, dlc));
                return string.Empty;
            }
        }

        static bool IsPathGameInstallRoot(string path, GameDlc dlc)
        {
            if (Directory.Exists(Path.Combine(path, _dCr)))
                return true;
            else if ((dlc == GameDlc.GalacticAdventures) && Directory.Exists(Path.Combine(path, _dEp1)))
                return true;
            else
                return false;
        }

        public static List<string> GetAllGameInstallPathsFromRegistry(GameDlc dlc)
        {
            string regPath = GetRegistryPath(dlc);
            if (regPath == null)
                return new List<string>();

            List<string> allPaths = new List<string>();

            foreach (string s in RegistryValueNames)
            {
                var regVal = Registry.GetValue(regPath, s, null);
                if ((regVal != null) && (regVal is string regValue))
                {
                    bool add = true;
                    foreach (string t in allPaths)
                    {
                        if (t.ToLowerInvariant() == regValue.ToLowerInvariant())
                        {
                            add = false;
                            break;
                        }
                    }

                    if (add)
                        allPaths.Add(regValue);
                }
            }

            return allPaths;
        }

        public static List<string>[] GameMultiplePaths = new List<string>[]
        {
            null,
            null,
            null
        };


        public static string GetGameInstallPathFromRegistry(GameDlc dlc)
        {
            List<string> paths = GetAllGameInstallPathsFromRegistry(dlc);

            if (paths.Count == 1)
            {
                return paths.ToArray()[0];
                //return null;
            }
            else
            {
                if (paths.Count > 1)
                {
                    bool areSame = true;
                    string comparison = string.Empty;
                    foreach (string p in paths)
                    {
                        if (comparison == string.Empty)
                            comparison = p;
                        else
                        {
                            if (Directory.Exists(p))
                            {
                                string newVal = EnsureGameInstallPathIsInstallRoot(p, dlc);
                                areSame = EnsureGameInstallPathIsInstallRoot(comparison, dlc).ToLowerInvariant() == newVal.ToLowerInvariant();

                                if (areSame)
                                    comparison = newVal;
                                else
                                    break;
                            }
                        }
                    }

                    if (areSame)
                    {
                        //MessageDisplay.DebugShowMessageBox("areSame, comparison: " + comparison);
                        return comparison;
                    }
                }

                if (dlc == GameDlc.CoreSpore)
                {
                    GameMultiplePaths[0] = paths;
                    string output = "";
                    foreach (string s in GameMultiplePaths[0])
                        output += s + ", ";
                    //MessageDisplay.DebugShowMessageBox("GameMultiplePaths fail: " + output);
                }
                else if (dlc == GameDlc.CreepyAndCute)
                {
                    GameMultiplePaths[1] = paths;
                    string output = "";
                    foreach (string s in GameMultiplePaths[1])
                        output += s + ", ";
                    //MessageDisplay.DebugShowMessageBox("GameMultiplePaths fail: " + output);
                }
                else if (dlc == GameDlc.GalacticAdventures)
                {
                    GameMultiplePaths[2] = paths;
                    string output = "";
                    foreach (string s in GameMultiplePaths[2])
                        output += s + ", ";
                    //MessageDisplay.DebugShowMessageBox("GameMultiplePaths fail: " + output);
                }
                return null;
                //return paths;
            }
        }


        static string _sbEp1 = "sporebinep1";
        /// <summary>
        /// Provides the path to the automatically-detected SporebinEP1 folder, if any.
        /// </summary>
        public static string AutoSporebinEP1
        {
            get
            {
                string gaPath = GetGameInstallPathFromRegistry(GameDlc.GalacticAdventures);
                if (gaPath != null)
                {
                    if (!gaPath.ToLowerInvariant().EndsWith(_sbEp1))
                        gaPath = Path.Combine(EnsureGameInstallPathIsInstallRoot(gaPath, GameDlc.GalacticAdventures), _sbEp1);

                    //MessageDisplay.DebugShowMessageBox("AutoSporebinEp1: " + gaPath);
                }
                else if (!BadGameInstallPaths.Any(x => (x.BasePath == null) && (x.BadPath == null) && (x.DlcLevel == GameDlc.GalacticAdventures) && x.IsSporebin))
                {
                    BadGameInstallPaths.Add(new BadPathEventArgs(null, null, GameDlc.GalacticAdventures)
                    {
                        IsSporebin = true
                    });
                }

                return gaPath;
                /*string gaPath = GetGameInstallPathFromRegistry(GameDlc.GalacticAdventures);
                if (gaPath != null)
                {


                    if (!gaPath.ToLowerInvariant().EndsWith(_sbEp1))
                        gaPath = Path.Combine(gaPath, _sbEp1);
                    MessageDisplay.DebugShowMessageBox("AutoSporebinEp1: " + gaPath);
                }
                return gaPath;*/
            } // _autoSporebinEp1;
        }


        /// <summary>
        /// Provides the path to the SporebinEP1 folder, if it exists.
        /// </summary>
        public static string SporebinEP1
        {
            get
            {
                /*string output = AutoSporebinEP1;
                if (Settings.ForcedGalacticAdventuresSporebinEP1Path != null)
                {
                    string root = EnsureGameInstallPathIsInstallRoot(Settings.ForcedGalacticAdventuresSporebinEP1Path, GameDlc.GalacticAdventures);
                    if (root != null)
                        output = Path.Combine(root, _sbEp1);
                    else
                        output = Settings.ForcedGalacticAdventuresSporebinEP1Path;
                    //MessageDisplay.DebugShowMessageBox("Forced SporebinEP1: " + output);
                }
                else
                {
                    //MessageDisplay.DebugShowMessageBox("Automatic SporebinEP1: " + output);
                }

                return output;*/
                if (!Settings.ForcedGalacticAdventuresSporebinEP1Path.IsNullOrEmptyOrWhiteSpace())
                    return Settings.ForcedGalacticAdventuresSporebinEP1Path;
                else
                    return AutoSporebinEP1;
            }
        }


        static string _dEp1 = "dataep1";
        /// <summary>
        /// Provides the path to the automatically-detected Galactic Adventures Data folder, if any.
        /// </summary>
        public static string AutoGalacticAdventuresData
        {
            get
            {
                string gaPath = GetGameInstallPathFromRegistry(GameDlc.GalacticAdventures);
                if (gaPath != null)
                {
                    if ((!gaPath.ToLowerInvariant().EndsWith(_dCr)) && (!gaPath.ToLowerInvariant().EndsWith(_dEp1)))
                    {
                        string dataEp1 = Path.Combine(EnsureGameInstallPathIsInstallRoot(gaPath, GameDlc.GalacticAdventures), _dEp1);
                        if (Directory.Exists(dataEp1))
                            gaPath = dataEp1;
                        else
                            gaPath = Path.Combine(EnsureGameInstallPathIsInstallRoot(gaPath, GameDlc.GalacticAdventures), _dCr);
                    }
                    //MessageDisplay.DebugShowMessageBox("AutoGAData: " + gaPath);
                }
                else if (!BadGameInstallPaths.Any(x => (x.BasePath == null) && (x.BadPath == null) && (x.DlcLevel == GameDlc.GalacticAdventures) && (!x.IsSporebin)))
                    BadGameInstallPaths.Add(new BadPathEventArgs(null, null, GameDlc.GalacticAdventures)
                    {
                        IsSporebin = false
                    });
                return gaPath;
            }
        }


        /// <summary>
        /// Provides the path to the GA Data folder, if it exists.
        /// </summary>
        public static string GalacticAdventuresData
        {
            get
            {
                if (!Settings.ForcedGalacticAdventuresDataPath.IsNullOrEmptyOrWhiteSpace())
                    return Settings.ForcedGalacticAdventuresDataPath;
                else
                    return AutoGalacticAdventuresData;
                /*string output = AutoGalacticAdventuresData;
                if (Settings.ForcedGalacticAdventuresDataPath != null)
                {
                    string root = EnsureGameInstallPathIsInstallRoot(Settings.ForcedGalacticAdventuresDataPath, GameDlc.GalacticAdventures);
                    if (root != null)
                    {
                        output = Path.Combine(root, _dEp1);
                        if (!Directory.Exists(output))
                            output = Path.Combine(root, _dCr);
                    }
                    else
                        output = Settings.ForcedGalacticAdventuresDataPath;
                    //MessageDisplay.DebugShowMessageBox("Forced GA Data: " + output);
                }

                return output;*/
            }
        }


        /*static string _sbCr = "sporebin";
        /// <summary>
        /// Provides the path to the automatically-detected SporeBin folder, if any.
        /// </summary>
        public static string AutoSporebin
        {
            get
            {
                string crPath = GetGameInstallPathFromRegistry(GameDlc.CoreSpore);
                if (crPath != null)
                {
                    if (!crPath.ToLowerInvariant().EndsWith(_sbCr))
                        crPath = Path.Combine(EnsureGameInstallPathIsInstallRoot(crPath, GameDlc.CoreSpore), _sbCr);
                    //MessageDisplay.DebugShowMessageBox("AutoSporebin: " + crPath);
                }
                else
                    BadGameInstallPath?.Invoke(null, new BadPathEventArgs(null, null, GameDlc.CoreSpore)
                    {
                        IsSporebin = true
                    });

                return crPath;
                /*string crPath = StripTrailingCharacters(GetGameInstallPathFromRegistry(GameDlc.CoreSpore));
                if (!crPath.ToLowerInvariant().EndsWith(_sbCr))
                    crPath = Path.Combine(crPath, _sbCr);
                MessageDisplay.DebugShowMessageBox("AutoSporebin: " + crPath);
                return crPath;*
            } // _autoSporebinEp1;
        }


        /// <summary>
        /// Provides the path to the SporeBin folder, if it exists.
        /// </summary>
        public static string Sporebin
        {
            get
            {
                string output = Sporebin;
                if (Settings.ForcedCoreSporeSporeBinPath != null)
                {
                    string root = EnsureGameInstallPathIsInstallRoot(Settings.ForcedCoreSporeSporeBinPath, GameDlc.CoreSpore);
                    if (root != null)
                        output = Path.Combine(root, _sbCr);
                    else
                        output = Settings.ForcedCoreSporeSporeBinPath;
                    //MessageDisplay.DebugShowMessageBox("Forced SporeBin: " + output);
                }
                /*else
                    MessageDisplay.DebugShowMessageBox("Automatic SporeBin: " + output);*

                return output;
            }
        }*/


        static string _dCr = "data";
        /// <summary>
        /// Provides the path to the automatically-detected Core Spore Data folder, if any.
        /// </summary>
        public static string AutoCoreSporeData
        {
            get
            {
                string crPath = GetGameInstallPathFromRegistry(GameDlc.CoreSpore);
                if (crPath != null)
                {
                    if (!crPath.ToLowerInvariant().EndsWith("data"))
                        crPath = Path.Combine(EnsureGameInstallPathIsInstallRoot(crPath, GameDlc.CoreSpore), _dCr);

                    //MessageDisplay.DebugShowMessageBox("AutoCoreSporeData: " + crPath);
                }
                else if (!BadGameInstallPaths.Any(x => (x.BasePath == null) && (x.BadPath == null) && (x.DlcLevel == GameDlc.CoreSpore) && (!x.IsSporebin)))
                    BadGameInstallPaths.Add(new BadPathEventArgs(null, null, GameDlc.CoreSpore)
                    {
                        IsSporebin = false
                    });
                return crPath;
            }
        }


        /// <summary>
        /// Provides the path to the Core Spore Data folder, if it exists.
        /// </summary>
        public static string CoreSporeData
        {
            get
            {
                /*string output = AutoCoreSporeData;
                //MessageDisplay.DebugShowMessageBox("Settings.ForcedCoreSporeDataPath != null: " + (Settings.ForcedCoreSporeDataPath != null));
                if (Settings.ForcedCoreSporeDataPath != null)
                {
                    string root = EnsureGameInstallPathIsInstallRoot(Settings.ForcedCoreSporeDataPath, GameDlc.CoreSpore);
                    if (root != null)
                        output = Path.Combine(root, _dCr);
                    else
                        output = Settings.ForcedCoreSporeDataPath;
                    //MessageDisplay.DebugShowMessageBox("Forced Core Spore Data: " + output);
                }
                /*else
                    MessageDisplay.DebugShowMessageBox("Automatic Core Spore Data: " + output);*

                return output;*/

                if (!Settings.ForcedCoreSporeDataPath.IsNullOrEmptyOrWhiteSpace())
                    return Settings.ForcedCoreSporeDataPath;
                else
                    return AutoCoreSporeData;
            }
        }

        //public static event EventHandler<BadPathEventArgs> BadGameInstallPath;
        public static List<BadPathEventArgs> BadGameInstallPaths = new List<BadPathEventArgs>();

        //public static event EventHandler<MultiplePathEventArgs> MultiplePossibleGameInstallPaths;

        public static bool SporeIsInstalledOnSteam()
        {
            if (Settings.IgnoreSteamInstallInfo)
                return false;
            else
            {
                object result = Registry.GetValue(SteamInfo.SteamAppsKey + SteamInfo.GalacticAdventuresSteamID.ToString(), "Installed", 0);
                // returns null if the key does not exist, or default value if the key existed but the value did not
                return result == null ? false : ((int)result == 0 ? false : true);
            }
        }

        public static List<DetectionFailureGuessFolder> GetFailureGuessFolders(GameDlc dlc, bool isSporebin)
        {
            List<DetectionFailureGuessFolder> guesses = new List<DetectionFailureGuessFolder>();

            string diskBase = string.Empty;
            string originBase = string.Empty;
            string steamBase = string.Empty;
            string gogBase = string.Empty;
            if (Environment.Is64BitOperatingSystem)
            {
                string x86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                diskBase = x86;
                originBase = x86;
                steamBase = x86;
                gogBase = x86;
            }
            else
            {
                string x64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                diskBase = x64;
                originBase = x64;
                steamBase = x64;
                gogBase = x64;
            }

            diskBase = Path.Combine(diskBase, "Electronic Arts", "SPORE");
            originBase = Path.Combine(originBase, "Origin Games");
            steamBase = Path.Combine(steamBase, @"Steam\SteamApps\Common\Spore");

            if (dlc == GameDlc.CoreSpore)
            {
                originBase = Path.Combine(originBase, "Spore");

                if (isSporebin)
                {
                    diskBase = Path.Combine(diskBase, "SporeBin");
                    originBase = Path.Combine(originBase, "SporeBin");
                    steamBase = Path.Combine(steamBase, "SporeBin");
                }
                else
                {
                    diskBase = Path.Combine(diskBase, "Data");
                    originBase = Path.Combine(originBase, "Data");
                    steamBase = Path.Combine(steamBase, "Data");
                }
            }
            else
            {
                diskBase += "_EP1";
                originBase = Path.Combine(originBase, "SPORE Galactic Adventures");

                if (isSporebin)
                {
                    diskBase = Path.Combine(diskBase, "SporebinEP1");
                    originBase = Path.Combine(originBase, "SporebinEP1");
                    steamBase = Path.Combine(steamBase, "SporebinEP1");
                }
                else
                {
                    diskBase = Path.Combine(diskBase, "Data");
                    originBase = Path.Combine(originBase, "Data");
                    steamBase = Path.Combine(steamBase, "DataEP1");
                }
            }

            if (Directory.Exists(diskBase))
                guesses.Add(new DetectionFailureGuessFolder(diskBase, GameExecutableType.Disk__1_5_1));

            if (Directory.Exists(originBase))
                guesses.Add(new DetectionFailureGuessFolder(originBase, GameExecutableType.Origin__March2017));

            if (Directory.Exists(steamBase))
                guesses.Add(new DetectionFailureGuessFolder(steamBase, GameExecutableType.GogOrSteam__March2017));

            if (false && Directory.Exists(gogBase))
                guesses.Add(new DetectionFailureGuessFolder(gogBase, GameExecutableType.GogOrSteam__March2017));

            return guesses;
        }

        public static string StripTrailingCharacters(string input)
        {
            string output = input.Replace("\"", string.Empty);
            /*while (output.StartsWith("\""))
                output = output.Substring(output.IndexOf("\""));

            while (output.EndsWith("\""))
                output = output.Substring(0, output.LastIndexOf("\""));*/

            while (output.EndsWith(@"\"))
                output = output.Substring(0, output.LastIndexOf(@"\"));

            while (output.EndsWith(@"/"))
                output = output.Substring(0, output.LastIndexOf(@"/"));

            output = output.Replace(@"/", @"\");

            //MessageDisplay.DebugShowMessageBox("AFTER STRIPPING SLASHES: " + output);
            return output;
        }
    }

    public class BadPathEventArgs : EventArgs
    {
        public GameInfo.GameDlc DlcLevel;

        public string BasePath;
        public string BadPath;

        public bool IsSporebin = false;
        public BadPathEventArgs(string bAse, string bad, GameInfo.GameDlc dlc)
        {
            BasePath = bAse;
            BadPath = bad;
            DlcLevel = dlc;
        }
    }

    public class MultiplePathEventArgs : EventArgs
    {
        GameInfo.GameDlc DlcLevel;

        public List<string> Paths;
        public MultiplePathEventArgs(List<string> list, GameInfo.GameDlc dlc)
        {
            Paths = list;
            DlcLevel = dlc;
        }
    }
}
