using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static SporeMods.Core.GameInfo;

namespace SporeMods.Core
{
    public static class Settings
    {
        public static bool DebugMode => File.Exists(Path.Combine(Settings.ProgramDataPath, "debug.txt"));

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
                    Directory.CreateDirectory(path);
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
                if (Directory.Exists(CoreLibsPath))
                {
                    foreach (string s in Directory.EnumerateFiles(CoreLibsPath).Where(x => x.EndsWith(".dll")))
                    {
                        string ver = FileVersionInfo.GetVersionInfo(s).FileVersion;
                        if (Version.TryParse(ver, out Version sVersion))
                            versions.Add(sVersion);
                    }
                }
                if (versions.Count() > 0)
                {
                    Version minVer = versions.Min();
                    return new Version(minVer.Major, minVer.Minor, minVer.Build, 0);
                }
                else
                    return new Version(999, 999, 999, 999);
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
                    Directory.CreateDirectory(path);
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
                File.WriteAllText(_settingsFilePath, @"<Settings></Settings>");

            _document = XDocument.Load(_settingsFilePath);
        }

        static XElement rootElement
        {
            get => (_document.Descendants("Settings").ToArray()[0] as XElement);
        }

        public static string TempFolderPath = Path.Combine(ProgramDataPath, "Temp");

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
                    Permissions.GrantAccess(_pathInfo);

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
                    Permissions.GrantAccess(_pathInfo);

                if (Path.GetFileNameWithoutExtension(Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString()).ToLowerInvariant().StartsWith("SporeMods."))
                    File.WriteAllText(_pathInfo, value);
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
        /// Whether or not the game window's bounds are calculated automatically by the ModAPI Launcher.
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

        static string _currentLanguageName = "CurrentLanguageName";
        /// <summary>
        /// User-selected Mod Manager language.
        /// </summary>
        public static string CurrentLanguageName
        {
            get
            {
                var value = GetElementValue(_currentLanguageName);

                if (string.IsNullOrWhiteSpace(value))
                    return "en-ca";
                else return value;
            }
            set => SetElementValue(_currentLanguageName, value);
        }

        /// <summary>
        /// Dictionary for current Mod Manager language.
        /// </summary>
        public static Dictionary<string, string> CurrentLanguage
        {
            get => GetLanguageDictionary(CurrentLanguageName);
        }

        static bool _languageExtracted = false;
        static string _languagesDir = Path.Combine(ProgramDataPath, "Languages");
        static Dictionary<string, string> GetLanguageDictionary(string langName)
        {
            if (!Directory.Exists(_languagesDir))
                Directory.CreateDirectory(_languagesDir);

            string defaultLanguagePath = Path.Combine(_languagesDir, "en-ca.language");

            if (!_languageExtracted)
            {
                File.WriteAllText(defaultLanguagePath, @"# Language info
LanguageName English (Canada)


# Global generic strings
Globals_OK OK
Globals_DontRunMgrAsAdmin For security and practicality reasons, please don't run the Spore Mod Manager as Administrator.
Globals_DontRunMgrAsAdmin2 For security and practicality reasons, explicitly running the Spore Mod Manager as Administrator (by right-clicking and selecting " + "\"Run as Administrator\"" + @") is not recommended. Are you sure you want to proceed?
Globals_DontRunLauncherAsAdmin For security and practicality reasons, please don't run the Spore ModAPI Launcher as Administrator.
Globals_DontRunLauncherAsAdmin2 For security and practicality reasons, explicitly running the Spore ModAPI Launcher as Administrator (by right-clicking and selecting " + "\"Run as Administrator\"" + @") is not recommended. Doing so will also prevent you from being able to load creations into Spore by dragging their PNGs into the game window. Are you sure you want to proceed?


# Main Window text
WindowTitle Spore Mod Manager v%VERSION% [DLLs Build %DLLSBUILD%]

ModsTabItem Mods
TweaksTabItem Tweaks
SettingsTabItem Settings
ConsoleTabItem Console
HelpTabItem Help

GetModsButton Get mods
InstallModFromDiskButton Install mods
UninstallModButton Uninstall selected mods
ConfigureModButton Change Selected mod's Settings

CopyModsListToClipboard Copy Mods List

DropModsHerePrompt Click the Install mods button to get started installing Spore mods!
DropModsHereInstruction Drag-and-drop mods here to install them
DropModsHereBrowseButton Browse...

SearchWatermark Search installed mods...
SearchNames Search mod names
SearchDescriptions Search mod descriptions
SearchTags Search mod tags

ModSwitchOn Enabled
ModSwitchOff Disabled
ModInstallingNow Installing...
ModEnablingNow Enabling...
ModDisablingNow Disabling...
ModUninstallingNow Uninstalling...
ModInstalledManually Manually installed

ExitSporeToManageMods Exit Spore to manage your installed mods.

FoldersHeader Folders
AutoGaDataPath Galactic Adventures Data Path
AutoSporebinEp1Path SporebinEP1 Path
AutoCoreDataPath Core Spore Data Path
AutoSporebinPath Sporebin Path
AutoDetectPath Auto-detect

ModInstallationHeader Mod Installation
BlockVanillaIncompatibleMods Don't install mods unless they are verified as vanilla-compatible
UseDeveloperMode Enable Developer Mode

DeveloperHeader Developer
SideloadCoreDlls Sideloaded DLLs
AddSideloadCoreDlls Add Sideload DLLs
RemoveSideloadCoreDlls Purge Sideload DLLs
BuildSideloadCoreDlls or compile the ModAPI DLLs to %OVERRIDELIBSPATH% yourself, if you feel like getting your hands dirty.
SelectDlls Select DLL bundle
DllBundlesFilter DLL bundles (%EXTENSIONS%)

SwitchOn On
SwitchOff Off
SwitchYes Yes
SwitchNo No

WindowHeader Window
OverrideWindowMode Override Window Mode
WindowModeFullscreen Fullscreen
WindowModeBorderlessWindowed Borderless Windowed
WindowModeWindowed Windowed
OverrideGameResolution Override Game Resolution
ResolutionAuto Automatic Resolution
ResolutionCustom Custom Resolution

GameEntryHeader Game entry
CommandLineState Launch game with a startup State
CommandLineStateName State name
CommandLineLanguage Launch game with a non-default language
CommandLineLocaleName Locale name
CommandLineOptions Additional Command Line options

AppearanceHeader Appearance

SkinOptionsHeader Skin Options
LightSwitchHeader Lights
AccentColourHeader Accent Colour
UseStandardWindowDecorations Use Standard Window Decorations

UpdateHeader Update (COMING SOON)
UpdateQuestion When should the Spore Mod Manager update?
UpdateAutomatically Automatically (recommended)
UpdateAutoCheck Check automatically, ask before installing
UpdateNever Don't update (not recommended)

HelpHeader Need help?
GoToForumThread Contact us to get help!
ShowConfig Copy configuration to clipboard

CloseSporeFirst Please close Spore to continue
SporeCantClose If Spore is not responding, has crashed, or has already been closed, click here:
ForceKillSporeButton Close Spore
ForceKillConfirmTitle Force-close Spore?
ForceKillConfirmDesc ANY UNSAVED PROGRESS WILL BE LOST. Are you sure you wish to force-close Spore?

CustomInstaller_ModInstallerHeader %MODNAME% Installer
CustomInstaller_Install Install

CreditsHeader Credits

RequiresAppRestart %CONTEXT% (requires app restart)

LaunchGameButton Launch Spore

SelectMod Select one or more mods to install
AllSporeModsFilter Spore mods (%EXTENSIONS%)
NowInstalling Installing...
InstallationComplete Installation complete!

Error_SporeCoreFolder Core Spore %DIRNAME%
Error_SporeGAFolder Galactic Adventures %DIRNAME%

Error_FolderNotFound The %FOLDERNAME% folder could not be automatically uniquely identified. Please select from the list below, or specify manually if needed. (This can be changed later under Settings if needed.)
Error_FolderNotFoundNoGuesses The %FOLDERNAME% folder could not be automatically detected. Please specify manually if needed. (This can be changed later under Settings if needed.

Error_ProbablyDiskGuess Probably installed from Disks
Error_ProbablyOriginGuess Probably installed from Origin
Error_ProbablyGOGGuess Probably installed from GOG (or Steam, if you're really unlucky)
");
                _languageExtracted = true;
            }
            var language = new Dictionary<string, string>();
            foreach (string s in File.ReadAllLines(Path.Combine(_languagesDir, langName + ".language")))
            {
                if ((!s.StartsWith("#")) && (!s.IsNullOrEmptyOrWhiteSpace()))
                {
                    int spaceIndex = s.IndexOf(' ');
                    string textKey = s.Substring(0, spaceIndex);
                    string textValue = s.Substring(spaceIndex, s.Length - spaceIndex).TrimStart(' ');
                    language.Add(textKey, textValue);
                }
            }
            return language;
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
            else
                throw new Exception("Unrecognized localization prefix");

            string key = prefixText + identifier;
            try
            {
                return Settings.CurrentLanguage[key];
            }
            catch (Exception ex)
            {
                MessageDisplay.DebugShowMessageBox(ex.ToString() + "\n\nkey: " + key);
                return "***";
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
                        File.WriteAllText(_firstRunPath, string.Empty);
                }
            }
        }

        static string _developerModeEnabledPath = Path.Combine(Settings.ProgramDataPath, "developerMode.info");
        static readonly bool _developerMode = File.Exists(_developerModeEnabledPath);
        public static bool DeveloperMode
        {
            get => _developerMode;
            set
            {
                if (value)
                {
                    if (!File.Exists(_developerModeEnabledPath))
                        File.Create(_developerModeEnabledPath).Close();
                }
                else
                {
                    if (File.Exists(_developerModeEnabledPath))
                        File.Delete(_developerModeEnabledPath);
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
                        File.Create(_forceSoftwareRenderingPath).Close();
                }
            }
        }
        static string _allowVanillaIncompatibleMods = "allowVanillaIncompatibleMods";
        /// <summary>
        /// Whether or not the game is forced to use a specific locale.
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

        //static string _modApiVersion = "ModApiVersion";

        /// <summary>
        /// Current version of the Spore Mod Manager.
        /// </summary>
        public static Version ModManagerVersion
        {
            get => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
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
                if (restrictToModManager && ((exeName == "spore modapi launcher") || (exeName == "spore mod manager")))//(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.ToLowerInvariant().Contains("modapi") && (System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.EndsWith("Manager.exe") | System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.EndsWith("Launcher.exe"))))
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
