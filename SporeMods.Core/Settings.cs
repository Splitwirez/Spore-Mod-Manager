using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using static SporeMods.Core.GameInfo;

namespace SporeMods.Core
{
    public static class Settings
    {
        public static bool DebugMode
        {
            get => File.Exists(Path.Combine(Settings.ProgramDataPath, "debug.txt"));
        }

        public static bool NonEssentialIsRunningUnderWine
        {
            get => File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "winecfg.exe"));
        }

        /// <summary>
        /// Path to the mod collection folder in ProgramData.
        /// </summary>
        public static string ProgramDataPath
        {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"SporeModManagerStorage");
                string redirectStorageFilePath = Path.Combine(path, "redirectStorage.txt");
                if (File.Exists(redirectStorageFilePath))
                    return File.ReadAllText(redirectStorageFilePath);
                else
                {
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    return path;
                }
            }
        }

        /// <summary>
        /// Path to the ModConfigs subfolder. Used for storing mods' files.
        /// </summary>
        public static string ModConfigsPath
        {
            get
            {
                string path = Path.Combine(ProgramDataPath, "ModConfigs");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        /// <summary>
        /// Path to the ModSettings subfolder. Used for storing mods' settings.
        /// </summary>
        public static string ModSettingsPath
        {
            get
            {
                string path = Path.Combine(ProgramDataPath, "ModSettings");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        /// <summary>
        /// Path to the mLibs subfolder. Used for storing ModAPI mods' DLLs for injection.
        /// </summary>
        public static string ModLibsPath
        {
            get
            {
                string path = Path.Combine(ProgramDataPath, "mLibs");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        /// <summary>
        /// Path to the coreLibs subfolder. Used for storing the ModAPI DLLs for injection.
        /// </summary>
        public static string CoreLibsPath
        {
            get
            {
                string path = Path.Combine(ProgramDataPath, "coreLibs");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Permissions.GrantAccessDirectory(CoreLibsPath);
                }
                return path;
            }
        }

        /// <summary>
        /// Path to the ModAPI DLL overrides subfolder. Used for storing home-grown alternative ModAPI DLLs for injection.
        /// </summary>
        public static string OverrideLibsPath
        {
            get
            {
                string path = Path.Combine(ProgramDataPath, "coreLibsOverride");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }


        public static Version CurrentDllsBuild
        {
            get
            {
                List<Version> versions = new List<Version>();
                if (!Directory.Exists(CoreLibsPath))
                {
                    Directory.CreateDirectory(CoreLibsPath);
                    Permissions.GrantAccessDirectory(CoreLibsPath);
                }

                if (!Directory.Exists(LegacyLibsPath))
                {
                    Directory.CreateDirectory(LegacyLibsPath);
                    Permissions.GrantAccessDirectory(LegacyLibsPath);
                }

                ExtractResource(CoreLibsPath, "SporeModAPI.lib");
                ExtractResource(CoreLibsPath, "SporeModAPI.disk.dll");
                ExtractResource(CoreLibsPath, "SporeModAPI.march2017.dll");

                ExtractResource(LegacyLibsPath, "SporeModAPI-disk.dll");
                ExtractResource(LegacyLibsPath, "SporeModAPI-steam_patched.dll");




                foreach (string s in Directory.EnumerateFiles(CoreLibsPath).Where(
                    x => x.EndsWith(".dll") && !x.ToLower().EndsWith("sporemodapi.dll")))
                {
                    string ver = FileVersionInfo.GetVersionInfo(s).FileVersion;
                    if (Version.TryParse(ver, out Version sVersion))
                        versions.Add(sVersion);
                }
                
                if (versions.Count() > 0)
                {
                    Version minVer = versions.Min();
                    return new Version(minVer.Major, minVer.Minor, minVer.Build, 0);
                }
                else
                    return Mods.ModIdentity.UNKNOWN_MOD_VERSION;
            }
        }

        public static bool AreDllsPresent()
        {
            return Directory.Exists(Settings.CoreLibsPath) && (Directory.EnumerateFiles(Settings.CoreLibsPath).Count() >= 3) && Directory.Exists(Settings.LegacyLibsPath) && (Directory.EnumerateFiles(Settings.LegacyLibsPath).Count() >= 2);
        }

        public static void EnsureDllsAreExtracted()
        {
            if (!AreDllsPresent())
            {
                Version lolwut = CurrentDllsBuild;
            }
        }

        static void ExtractResource(string outDir, string name)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SporeMods.Core.ModAPIDLLs." + name))
            {
                using (var file = new FileStream(Path.Combine(outDir, name), FileMode.Create, FileAccess.ReadWrite))
                {
                    resource.CopyTo(file);
                }
            }
        }

        public static string CurrentDllsBuildString
        {
            get
            {
                string baseVer = CurrentDllsBuild.ToString();
                if (baseVer.Contains("."))
                    baseVer = baseVer.Substring(0, baseVer.LastIndexOf('.'));
                return baseVer;
            }
        }

        /// <summary>
        /// Path to the legacyLibs subfolder. Used for storing Legacy ModAPI mods' DLLs for injection.
        /// </summary>
        public static string LegacyLibsPath
        {
            get
            {
                string path = Path.Combine(ProgramDataPath, "legacyLibs");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Permissions.GrantAccessDirectory(LegacyLibsPath);
                }
                return path;
            }
        }

        /// <summary>
        /// Path to the ModQueues subfolder.
        /// </summary>
        public static string ModQueuePath
        {
            get
            {
                string path = Path.Combine(ProgramDataPath, "ModQueues");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        static string _settingsFilePath;
        static XDocument _document;

        static Settings()
        {
            _settingsFilePath = Path.Combine(ProgramDataPath, "ModManagerSettings.xml");
            if (!File.Exists(_settingsFilePath))
            {
                File.WriteAllText(_settingsFilePath,
@"<Settings>
    <" + _lastMgrVersion + ">" + ModManagerVersion.ToString() + "</" + _lastMgrVersion + @">
</Settings>");
                Permissions.GrantAccessFile(_settingsFilePath);
            }

            _document = XDocument.Load(_settingsFilePath);
        }

        static XElement rootElement
        {
            get => (_document.Descendants("Settings").ToArray()[0] as XElement);
        }

        static string _tempFolderPath = Path.Combine(ProgramDataPath, "Temp");
        public static string TempFolderPath
        {
            get
            {
                if (!Directory.Exists(_tempFolderPath))
                    Directory.CreateDirectory(_tempFolderPath);
                return _tempFolderPath;
            }
        }

        public static string LegacyTempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Spore ModAPI Launcher");

        static string _pathInfo = Path.Combine(TempFolderPath, "path.info");

        /// <summary>
        /// Path to wherever the Mod Manager is installed to.
        /// </summary>
        public static string ManagerInstallLocationPath
        {
            get
            {
                if (!Directory.Exists(TempFolderPath))
                    Directory.CreateDirectory(TempFolderPath);

                if (File.Exists(_pathInfo))
                    Permissions.GrantAccessFile(_pathInfo);

                if (!Permissions.IsFileLocked(_pathInfo, FileAccess.Write))
                    return File.ReadAllText(_pathInfo);
                else
                    return System.IO.Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
            }
            set
            {
                /*try
                {*/
                if (!Directory.Exists(TempFolderPath))
                    Directory.CreateDirectory(TempFolderPath);

                if (File.Exists(_pathInfo))
                    Permissions.GrantAccessFile(_pathInfo);

                if (Path.GetFileNameWithoutExtension(Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString()).ToLowerInvariant().StartsWith("SporeMods."))
                {
                    File.WriteAllText(_pathInfo, value);
                    Permissions.GrantAccessFile(_pathInfo);
                }
                /*else
                    throw new Exception("Non-ModAPI Management kit software must not interfere.");
            }
            catch (Exception ex)
            {
                ModInstallation.InvokeErrorOccurred(new ErrorEventArgs("Install path could not be written to.\n" + ex.Message + "\n" + ex.StackTrace));
            }*/
            }
        }


        static string _forcedGameExeType = "ForcedGameExeType";
        /// <summary>
        /// Explicitly-set SporeApp.exe type, if any.
        /// </summary>
        public static string ForcedGameExeType
        {
            get => GetElementValue(_forcedGameExeType);
            set => SetElementValue(_forcedGameExeType, value);
        }


        static string _forcedGalacticAdventuresSporebinEP1Path = "GalacticAdventuresSporebinEP1Path";
        /// <summary>
        /// Explicitly-set Galactic Adventures SporebinEP1 path, if any.
        /// </summary>
        public static string ForcedGalacticAdventuresSporebinEP1Path
        {
            get => GetElementValue(_forcedGalacticAdventuresSporebinEP1Path);
            set => SetElementValue(_forcedGalacticAdventuresSporebinEP1Path, value);
        }


        static string _forcedGalacticAdventuresDataPath = "GalacticAdventuresDataPath";
        /// <summary>
        /// Explicitly-set Galactic Adventures Data folder path, if any.
        /// </summary>
        public static string ForcedGalacticAdventuresDataPath
        {
            get => GetElementValue(_forcedGalacticAdventuresDataPath);
            set => SetElementValue(_forcedGalacticAdventuresDataPath, value);
        }


        static string _forcedCoreSporeSporeBinPath = "ForcedCoreSporeSporeBinPath";
        /// <summary>
        /// Explicitly-set Core Spore SporeBin path, if any.
        /// </summary>
        public static string ForcedCoreSporeSporeBinPath
        {
            get => GetElementValue(_forcedCoreSporeSporeBinPath);
            set => SetElementValue(_forcedCoreSporeSporeBinPath, value);
        }


        static string _forcedCoreSporeDataPath = "ForcedCoreSporeDataPath";
        /// <summary>
        /// Explicitly-set Core Spore Data path, if any.
        /// </summary>
        public static string ForcedCoreSporeDataPath
        {
            get => GetElementValue(_forcedCoreSporeDataPath);
            set => SetElementValue(_forcedCoreSporeDataPath, value);
        }

        static string _steamPath = "SteamPath";
        /// <summary>
        /// Explicitly-set Steam install path, if any.
        /// </summary>
        public static string SteamPath
        {
            get => GetElementValue(_steamPath);
            set => SetElementValue(_steamPath, value);
        }

        static string _forceGameWindowingMode = "ForceGameWindowingMode";
        /// <summary>
        /// Whether or not to force the game's windowing mode.
        /// </summary>
        public static bool ForceGameWindowingMode
        {
            get
            {
                if (bool.TryParse(GetElementValue(_forceGameWindowingMode), out bool forceWindowingMode))
                    return forceWindowingMode;
                else
                    return false;
            }
            set => SetElementValue(_forceGameWindowingMode, value.ToString());
        }

        static string _forceWindowedMode = "ForceWindowedMode";
        /// <summary>
        /// If ForcedGameWindowingMode is true, forces Windowed mode if 0 or Fullscreen mode if 1.
        /// </summary>
        public static int ForceWindowedMode
        {
            get
            {
                if (int.TryParse(GetElementValue(_forceWindowedMode), out int forceWindowedMode))
                    return forceWindowedMode;
                else
                    return 0;
            }
            set => SetElementValue(_forceWindowedMode, value.ToString());
        }

        static string _forceGameWindowBounds = "ForceGameWindowBounds";
        /// <summary>
        /// Whether or not the game window's bounds are forced to set values.
        /// </summary>
        public static bool ForceGameWindowBounds
        {
            get
            {
                if (bool.TryParse(GetElementValue(_forceGameWindowBounds), out bool force))
                    return force;
                else
                    return false;
            }
            set => SetElementValue(_forceGameWindowBounds, value.ToString());
        }

        static string _autoGameWindowBounds = "AutoGameWindowBounds";
        /// <summary>
        /// Whether or not the game window's bounds are calculated automatically by the Mod Launcher.
        /// </summary>
        public static bool AutoGameWindowBounds
        {
            get
            {
                if (bool.TryParse(GetElementValue(_autoGameWindowBounds), out bool force))
                    return force;
                else
                    return false;
            }
            set => SetElementValue(_autoGameWindowBounds, value.ToString());
        }

        static string _forcedGameWindowWidth = "ForcedGameWindowWidth";
        /// <summary>
        /// Forced game window width, if any.
        /// </summary>
        public static int ForcedGameWindowWidth
        {
            get
            {
                if (int.TryParse(GetElementValue(_forcedGameWindowWidth), out int gameWidth))
                    return gameWidth;
                else
                    return 800;
            }
            set => SetElementValue(_forcedGameWindowWidth, value.ToString());
        }

        static string _forcedGameWindowHeight = "ForcedGameWindowHeight";
        /// <summary>
        /// Forced game window height, if any.
        /// </summary>
        public static int ForcedGameWindowHeight
        {
            get
            {
                if (int.TryParse(GetElementValue(_forcedGameWindowHeight), out int gameHeight))
                    return gameHeight;
                else
                    return 600;
            }
            set => SetElementValue(_forcedGameWindowHeight, value.ToString());
        }

        static string _forceGameLocale = "ForceGameLocale";
        /// <summary>
        /// Whether or not the game is forced to use a specific locale.
        /// </summary>
        public static bool ForceGameLocale
        {
            get
            {
                if (bool.TryParse(GetElementValue(_forceGameLocale), out bool force))
                    return force;
                else
                    return false;
            }
            set => SetElementValue(_forceGameLocale, value.ToString());
        }

        static string _forcedGameLocale = "ForcedGameLocale";
        /// <summary>
        /// Forced game locale, if any.
        /// </summary>
        public static string ForcedGameLocale
        {
            get => GetElementValue(_forcedGameLocale);
            set => SetElementValue(_forcedGameLocale, value);
        }

        static string _useCustomGameState = "UseCustomGameState";
        /// <summary>
        /// Whether or not the game is forced to start with a specific launch state
        /// </summary>
        public static bool UseCustomGameState
        {
            get
            {
                if (bool.TryParse(GetElementValue(_useCustomGameState), out bool force))
                    return force;
                else
                    return false;
            }
            set => SetElementValue(_useCustomGameState, value.ToString());
        }

        static string _gameState = "GameState";
        /// <summary>
        /// Custom game state specified.
        /// </summary>
        public static string GameState
        {
            get => GetElementValue(_gameState);
            set => SetElementValue(_gameState, value);
        }

        static string _cmdOptions = "CmdOptions";
        /// <summary>
        /// Additional Commandline options, if any.
        /// </summary>
        public static string CommandLineOptions
        {
            get => GetElementValue(_cmdOptions);
            set => SetElementValue(_cmdOptions, value);
        }

        public static string LaunchSporeWithoutManagerOptions = "-NoManagerArgs";


        static string _osLangCode;
        static string GetBetterLanguageForOS()
        {
            if (_osLangCode != null) return _osLangCode;

            string langCode = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
            if (!_availableLanguages.ContainsKey(langCode))
            {
                // Try to get one from the same group. If user has en-us, try to set en-ca, etc
                string langGroup = langCode.Split('-')[0];
                langCode = null;
                foreach (var lang in _availableLanguages.Keys)
                {
                    if (langGroup == lang.Split('-')[0])
                    {
                        langCode = lang;
                        break;
                    }
                }
                if (langCode == null) langCode = "en-ca";
            }

            _osLangCode = langCode;
            return langCode;
        }

        static string _currentLanguageCode = "CurrentLanguageCode";
        /// <summary>
        /// User-selected Mod Manager language code (en-ca, es-es,...). Lowercase.
        /// </summary>
        public static string CurrentLanguageCode
        {
            get
            {
                var value = GetElementValue(_currentLanguageCode);

                if (string.IsNullOrWhiteSpace(value))
                    return GetBetterLanguageForOS();
                else return value;
            }
            set
            {
                if (value != CurrentLanguageCode)
                {
                    _currentLanguage = ReadLanguage(value);
                    SetElementValue(_currentLanguageCode, value);
                }
            }
        }

        /// <summary>
        /// User-selected Mod Manager language name
        /// </summary>
        public static string CurrentLanguageName
        {
            get
            {
                InitializeLanguages();
                return _availableLanguageNames[CurrentLanguageCode];
            }
            set {
                CurrentLanguageCode = _availableLanguageNames.FirstOrDefault(x => x.Value == value).Key;
            }
        }
        
        private static Dictionary<string, string> _availableLanguageNames = new Dictionary<string, string>();
        /// <summary>
        /// The names (English, Español,...) of the available languages for each lang code.
        /// Pairs of { langCode, langName }
        /// </summary>
        public static Dictionary<string, string> AvailableLanguageNames
        {
            get
            {
                InitializeLanguages();
                return _availableLanguageNames;
            }
        }

        /// <summary>
        /// The names (English, Español,...) of the available languages, sorted alphabetically.
        /// </summary>
        public static List<string> SortedLanguageNames
        {
            get
            {
                var list = new List<string>(AvailableLanguageNames.Values);
                list.Sort();
                return list;
            }
        }

        /// <summary>
        /// Dictionary for current Mod Manager language.
        /// </summary>
        // We don't want this to be public, GetLanguageString must be used
        private static Dictionary<string, string> _currentLanguage;

        // { langCode, isInternal }
        static Dictionary<string, bool> _availableLanguages = new Dictionary<string, bool>();

        static bool _languageInitialized = false;
        static string _languagesDir = Path.Combine(ProgramDataPath, "Languages");

        static Dictionary<string, string> ReadLanguage(Stream stream)
        {
            var dictionary = new Dictionary<string, string>();
            using (var reader = new StreamReader(stream))
            {
                string s;
                while ((s = reader.ReadLine()) != null)
                {
                    s = s.Trim();
                    if (!s.StartsWith("#") && !s.IsNullOrEmptyOrWhiteSpace())
                    {
                        var splits = s.Split(new char[] { ' ' }, 2);
                        dictionary.Add(splits[0], splits[1].TrimStart(' '));
                    }
                }
            }
            return dictionary;
        }

        static Dictionary<string, string> ReadLanguage(string langCode)
        {
            if (_availableLanguages[langCode])
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SporeMods.Core.Locale." + langCode + ".txt"))
                {
                    return ReadLanguage(stream);
                }
            }
            else
            {
                using (var stream = new FileStream(Path.Combine(_languagesDir, langCode + ".txt"), FileMode.Open))
                {
                    return ReadLanguage(stream);
                }
            }
        }

        /// <summary>
        /// Creates the Languages folder, loads the language names and loads the current language.
        /// </summary>
        static void InitializeLanguages()
        {
            if (_languageInitialized) return;

            if (!Directory.Exists(_languagesDir))
                Directory.CreateDirectory(_languagesDir);

            // Internal languages
            _availableLanguages["en-ca"] = true;
            _availableLanguages["es-es"] = true;
            _availableLanguages["ca-ca"] = true;

            // User-provided languages
            foreach (var file in Directory.GetFiles(_languagesDir, "*.txt"))
            {
                var langCode = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                if (!_availableLanguages.ContainsKey(langCode))
                {
                    _availableLanguages[langCode] = false;
                }
            }

            var assembly = Assembly.GetExecutingAssembly();
            foreach (var langCode in _availableLanguages.Keys)
            {
                var language = ReadLanguage(langCode);

                if (language.ContainsKey("LanguageName")) 
                {
                    _availableLanguageNames[langCode] = language["LanguageName"];
                }
                else
                {
                    _availableLanguageNames[langCode] = langCode;
                }

                if (langCode == CurrentLanguageCode)
                {
                    _currentLanguage = language;
                }
            }

            if (_currentLanguage == null)
            {
                // For some reason, the current language file is not available
                // Try to default to computer language, or en-ca 
                _currentLanguage = ReadLanguage(GetBetterLanguageForOS());
            }

            _languageInitialized = true;
        }

        public static string GetLanguageString(string identifier)
        {
            return GetLanguageString(0, identifier);
        }

        public static string GetLanguageString(int prefix, string identifier)
        {
            string prefixText = "";

            if (prefix == 0)
                prefixText = string.Empty; //"MainWindow_";
            else if (prefix == 1)
                prefixText = "Globals_";
            else if (prefix == 2)
                prefixText = "CustomInstaller_";
            else if (prefix == 3)
                prefixText = "Error_";
            else if (prefix == 4)
                prefixText = "Importer_";
            else
                throw new Exception("Unrecognized localization prefix");

            string key = prefixText + identifier;
            try
            {
                InitializeLanguages();
                string outStr = _currentLanguage[key];
                /*if (outStr.Contains(@"\\"))
                {
                    outStr.Replace(@"\\", "[SLASH]");
                }
                if (outStr.Contains(@"\n"))
                {
                    outStr.Replace(@"\n", "\n");
                }
                if (outStr.Contains("[SLASH]"))
                {
                    outStr.Replace("[SLASH]", @"\\");
                }*/
                if (outStr.Contains("<br>"))
                {
                    outStr = outStr.Replace("<br>", "\r\n");
                }
                return outStr;
            }
            catch (Exception ex)
            {
                //MessageDisplay.DebugShowMessageBox(ex.ToString() + "\n\nkey: " + key);
                return ex.ToString() + "NOT FOUND: " + key; //"***";
            }
        }


        static string _shaleDarkTheme = "ShaleDarkTheme";
        public static bool ShaleDarkTheme
        {
            get
            {
                if (bool.TryParse(GetElementValue(_shaleDarkTheme), out bool returnValue))
                    return returnValue;
                else
                    return false;
            }
            set => SetElementValue(_shaleDarkTheme, value.ToString(), true);
        }


        static string _firstRunPath = Path.Combine(ProgramDataPath, "firstRun.info");
        /// <summary>
        /// Whether or not this is the first time the Spore Mod Manager is started.
        /// </summary>
        public static bool IsFirstRun
        {
            get => (!File.Exists(_firstRunPath));
            set
            {
                if (value == true)
                {
                    if (File.Exists(_firstRunPath))
                        File.Delete(_firstRunPath);
                }
                else
                {
                    if (!File.Exists(_firstRunPath))
                    {
                        File.WriteAllText(_firstRunPath, string.Empty);
                        Permissions.GrantAccessFile(_firstRunPath);
                    }
                }
            }
        }

        //Developer mode has been shelved for a post-release update
        static string _developerModeEnabledPath = Path.Combine(Settings.ProgramDataPath, "developerMode.info");
        static readonly bool _developerMode = File.Exists(_developerModeEnabledPath);
        public static bool DeveloperMode
        {
            get => false; //_developerMode;
            set
            {
                if (false)
                {
                    if (value)
                    {
                        if (!File.Exists(_developerModeEnabledPath))
                        {
                            File.Create(_developerModeEnabledPath).Close();
                            Permissions.GrantAccessFile(_developerModeEnabledPath);
                        }
                    }
                    else
                    {
                        if (File.Exists(_developerModeEnabledPath))
                            File.Delete(_developerModeEnabledPath);
                    }
                }
            }
        }


        static string _useCustomWindowDecorations = "UseCustomWindowDecorations";
        /// <summary>
        /// Whether or not the Spore Mod Manager should use a Mechanism.Wpf.Core.Windows.DecoratableWindow (requires an app restart to take effect)
        /// </summary>
        public static bool UseCustomWindowDecorations
        {
            get
            {
                if (File.Exists(Path.Combine(ProgramDataPath, "forceStandardWindowDecorations.txt")))
                    return false;
                else if (bool.TryParse(GetElementValue(_useCustomWindowDecorations), out bool returnValue))
                    return returnValue;
                else
                    return true;
            }
            set => SetElementValue(_useCustomWindowDecorations, value.ToString(), true);
        }

        static string _forceSoftwareRenderingPath = Path.Combine(ProgramDataPath, "WpfUseSoftwareRendering.info");
        /// <summary>
        /// Forces WPF software rendering if true. Some unlucky user with a weird OS+GPU combination needed this option a while back.
        /// </summary>
        public static bool ForceSoftwareRendering
        {
            get => (!File.Exists(_forceSoftwareRenderingPath));
            set
            {
                if (value == true)
                {
                    if (File.Exists(_forceSoftwareRenderingPath))
                        File.Delete(_forceSoftwareRenderingPath);
                }
                else
                {
                    if (!File.Exists(_forceSoftwareRenderingPath))
                    {
                        File.Create(_forceSoftwareRenderingPath).Close();
                        Permissions.GrantAccessFile(_forceSoftwareRenderingPath);
                    }
                }
            }
        }

        static string _allowVanillaIncompatibleMods = "allowVanillaIncompatibleMods";
        /// <summary>
        /// Whether or not the Spore Mod Manager will permit installation of mods which can result in non-shareable creations.
        /// </summary>
        public static bool AllowVanillaIncompatibleMods
        {
            get
            {
                if (bool.TryParse(GetElementValue(_allowVanillaIncompatibleMods), out bool force))
                    return force;
                else
                    return true;
            }
            set => SetElementValue(_allowVanillaIncompatibleMods, value.ToString());
        }

        static string _lastMgrVersion = "LastMgrVersion";
        /// <summary>
        /// The last version of the Spore Mod Manager which accessed the current config
        /// </summary>
        public static Version LastModManagerVersion
        {
            get
            {
                string verVal = GetElementValue(_lastMgrVersion);
                if ((!string.IsNullOrEmpty(verVal)) && (!string.IsNullOrWhiteSpace(verVal)) && (Version.TryParse(verVal, out Version returnValue)))
                    return returnValue;
                else
                    return null;
            }
            set => SetElementValue(_lastMgrVersion, value.ToString());
        }

        public enum UpdatingModeType
        {
            /// <summary>
            /// Always check for updates and download automatically if possible
            /// </summary>
            Automatic,
            /// <summary>
            /// Always check for updates, but ask confirmation of user before downloading
            /// </summary>
            AutoCheck,
            /// <summary>
            /// Never check for updates
            /// </summary>
            Disabled
        }
        static string _updatingMode = "UpdatingMode";
        /// <summary>
        /// 0 for automatic updates, 1 for asking the user, 2 for no update checking
        /// </summary>
        public static UpdatingModeType UpdatingMode
        {
            get
            {
                if (UpdatingModeType.TryParse(GetElementValue(_updatingMode), out UpdatingModeType updatingMode))
                    return updatingMode;
                else
                    return 0;
            }
            set => SetElementValue(_updatingMode, value.ToString());
        }

        //static string _modApiVersion = "ModApiVersion";

        /// <summary>
        /// Current version of the Spore Mod Manager.
        /// </summary>
        public static Version ModManagerVersion
        {
            get => Assembly.GetExecutingAssembly().GetName().Version;
        }

        /*static XElement GetElement(string elementName)
        {
            var elements = rootElement.Descendants(elementName);

            if ((elements == null) || (elements.Count() <= 0))
            {
                rootElement.Add(new XElement(elementName, ""));
            }

            return (rootElement.Descendants(elementName)).ToArray()[0] as XElement;
        }

        */static string GetElementValue(string elementName)
        {
            string returnValue = null;
            var element = rootElement.Element(elementName);
            if (element != null)
                returnValue = element.Value;
            return returnValue;
        }

        static void SetElementValue(string elementName, string value)
        {
            SetElementValue(elementName, value, false);
        }

        static void SetElementValue(string elementName, string value, bool restrictToModManager)
        {
            bool canWrite = false;
            if (!restrictToModManager)
                canWrite = true;
            else
            {
                string exeName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName).ToLowerInvariant();
                if (restrictToModManager && ((exeName == "spore mod launcher") || (exeName == "spore mod manager")))//(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.ToLowerInvariant().Contains("modapi") && (System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.EndsWith("Manager.exe") | System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.EndsWith("Launcher.exe"))))
                    canWrite = true;
            }

            if (canWrite)
            {
                if (value.IsNullOrEmptyOrWhiteSpace())
                    rootElement.SetElementValue(elementName, null);
                else
                    rootElement.SetElementValue(elementName, value);
                _document.Save(_settingsFilePath);
            }
            /*if (value == null)
            {
                var element = rootElement.Element(elementName);

                if (element != null)
                    element.Remove();
            }
            else
            {
                bool canWrite = false;

                if (restrictToModApi)
                    canWrite = (System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.ToLowerInvariant().Contains("modapi") && (System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.EndsWith("Manager.exe") | System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.EndsWith("Launcher.exe")));
                else
                    canWrite = true;


                rootElement.SetElementValue(elementName, value);
                //GetElement(elementName).Value = value;
            }
            _document.Save(_settingsFilePath);*/
        }
    }

    public class LanguageEventArgs : EventArgs
    {
        public string OldLanguage;
        public string NewLanguage;
        public Dictionary<string, string> OldDictionary;
        public Dictionary<string, string> NewDictionary;
    }
}
