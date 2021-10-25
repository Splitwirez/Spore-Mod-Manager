using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
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




		[DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern string GetWineVersion();

		private static bool? IsRunningUnderWine(out Version version)
		{
			version = new Version(0, 0, 0, 0);
			try
			{
				_getWineVersionResult = GetWineVersion();
				if (!Version.TryParse(_getWineVersionResult, out version))
					return null;

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		static string _isWineMode = "WineMode";
		/// <summary>
		/// True only if initially installed/configured under WINE.
		/// </summary>
		public static bool NonEssentialIsRunningUnderWine
		{
			get
			{
				if (bool.TryParse(GetElementValue(_isWineMode), out bool wineMode))
					return wineMode;
				else
				{
					bool retVal = IsRunningUnderWine(out Version wineVer) != false;
					SetElementValue(_isWineMode, retVal.ToString());
					return retVal;
				}
			}
		}

		static string _getWineVersionResult = null;
		public static string GetWineVersionResult
		{
			get
			{
				if (_getWineVersionResult == null)
				{
					var _ = IsRunningUnderWine(out Version __);
				}
				
				return _getWineVersionResult;
			}
		}

		
		/*static string CreateTargetFrameworkString()
		{
			string targetFramework = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RuntimeVersion"))
			{
				StreamReader reader = new StreamReader(stream);
				targetFramework = reader.ReadToEnd();
			}
			for (int charIndex = 0; charIndex < targetFramework.Length; charIndex++)
			{
				char current = targetFramework[charIndex];
				if (!current.IsLetter())
				{
					targetFramework = targetFramework.Substring(0, charIndex);
					break;
				}
			}
			return targetFramework + Environment.Version;
		}*/


		//https://docs.microsoft.com/en-us/dotnet/core/tutorials/libraries#how-to-multitarget
		static readonly string TARGET_FRAMEWORK_PREFIX =
#if NET
			"dotnet"
#elif NETCOREAPP
			"dotnet-core"
#elif NETFRAMEWORK
			"dotnet-framework"
#else
		"unknown-target"
#endif
			;


/*		static readonly string TARGET_FRAMEWORK_PREFIX = "TARGETS="
#if NET
			+ "NET,"
#endif
#if NETCORE
			+ "NETCORE,"
#endif
#if NETCOREAPP
			+ "NETCOREAPP,"
#endif
#if NETCOREAPP3_1
			+ "NETCOREAPP3_1,"
#endif
#if NETFRAMEWORK
			+ "NETFRAMEWORK,"
#endif
#if NETFX
			+ "NETFX"
#endif
			;*/
		

		static readonly string TARGET_FRAMEWORK_VERSION =
#if NETFRAMEWORK
	#if NET48
			"4-8"
	#elif NET472
			"4-7-2"
	#elif NET471
			"4-7-1"
	#elif NET47
			"4-7-0"
	#elif NET462
			"4-6-2"
	#elif NET461
			"4-6-2"
	#elif NET46
			"4-6"
	#elif NET452
			"4-5-2"
	#elif NET451
			"4-5-1"
	#elif NET45
			"4-5"
	#elif NET40
			"4-0"
	#elif NET35
			"3-5"
	#elif NET20
			"2-0"
	#else
			"unknown-netfx-version"
	#endif
			+ "_" +
#endif
			Environment.Version.ToString().Replace('.', '-');


		static readonly string _targetFramework = $"{TARGET_FRAMEWORK_PREFIX}--{TARGET_FRAMEWORK_VERSION}";
		public static string TargetFramework
		{
			get => _targetFramework;
		}

		static string GetBuildChannel()
		{
#if SET_BUILD_CHANNEL
			string buildChannelRaw = string.Empty;
			
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BuildChannel"))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					buildChannelRaw += reader.ReadLine();
				}
			}
			buildChannelRaw = buildChannelRaw.Replace("\r", string.Empty).Replace("\n", string.Empty);

			string buildChannel = string.Empty;
			foreach (Char c in buildChannelRaw)
			{
				buildChannel += Char.IsLetterOrDigit(c) ? Char.ToUpper(c) : '_';
			}

			return buildChannel.Replace("\r", string.Empty).Replace("\n", string.Empty)
	#if DEBUG	
				+ " (DEBUG)"
	#endif
			;
#elif DEBUG
			return "DEBUG";
#else
			return string.Empty;
#endif
		}
		
		public static readonly string BuildChannel = GetBuildChannel();

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
				{
					string redirPath = File.ReadAllText(redirectStorageFilePath);
					if (!Directory.Exists(redirPath))
						Directory.CreateDirectory(redirPath);
					return redirPath;
				}
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
		/// Path to the Logs subfolder. Used for storing error logs and such.
		/// </summary>
		public static string LogsPath
		{
			get
			{
				string path = Path.Combine(ProgramDataPath, "Logs");
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

				if (!AreDllsPresent())
					ExtractDlls();




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

		public static void ExtractDlls()
		{
			ExtractResource(CoreLibsPath, "SporeModAPI.lib");
			ExtractResource(CoreLibsPath, "SporeModAPI.disk.dll");
			ExtractResource(CoreLibsPath, "SporeModAPI.march2017.dll");

			ExtractResource(LegacyLibsPath, "SporeModAPI-disk.dll");
			ExtractResource(LegacyLibsPath, "SporeModAPI-steam_patched.dll");
		}

		public static void EnsureDllsAreExtracted()
		{
			if (!AreDllsPresent())
			{
				ExtractDlls();
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

		static string _settingsFilePath = null;
		static XDocument _document;

		public static void Ensure()
		{
			ReparseSettingsDoc();
		}
		
		public static bool IsLoaded
		{
			get => (_document != null) ? (rootElement != null) : false;
		}


		public static void ReparseSettingsDoc()
        {
			_settingsFilePath = Path.Combine(ProgramDataPath, "ModManagerSettings.xml");
			if (!File.Exists(_settingsFilePath))
			{
				WriteSettingsXmlFile();
			}

			try
			{
				_document = XDocument.Load(_settingsFilePath);
			}
			catch (XmlException ex)
			{
				WriteSettingsXmlFile();
				_document = XDocument.Load(_settingsFilePath);
			}
		}

		static void WriteSettingsXmlFile()
		{
			XDocument document = new XDocument(new XElement("Settings"));
			document.Root.Add(new XElement(_lastMgrVersion, ModManagerVersion));

			/*string xmlStart = @"<Settings>";
			string xmlMiddle = @"
	<" + _lastMgrVersion + ">" + ModManagerVersion.ToString() + "</" + _lastMgrVersion + ">";
			string xmlEnd = @"</Settings>";*/

			bool wine = false;
			try
			{
				wine = IsRunningUnderWine(out Version ver) == true;
			}
			catch { }

			try
			{
				if (!wine)
					wine = File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "winecfg.exe"));
			}
			catch { }


			if (wine)
			{
				document.Root.Add(new XElement(_isWineMode, true));
			}
			//xmlMiddle += "\n    <" + _isWineMode + ">True</" + _isWineMode + ">";

			/*File.WriteAllText(_settingsFilePath, xmlStart + xmlMiddle + xmlEnd);
			Permissions.GrantAccessFile(_settingsFilePath);*/
			document.Save(_settingsFilePath);
		}

		static XElement rootElement
		{
			get => _document.Root; //(_document.Descendants("Settings").ToArray()[0] as XElement);
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

		static string _pathInfoFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Spore Mod Manager");
		static string _pathInfo = Path.Combine(_pathInfoFolder, "path.info");

		/// <summary>
		/// Path to wherever the Mod Manager is installed to.
		/// </summary>
		public static string ManagerInstallLocationPath
		{
			get
			{
				if (!Directory.Exists(_pathInfoFolder))
					Directory.CreateDirectory(_pathInfoFolder);

				if (File.Exists(_pathInfo) && Permissions.IsAdministrator())
					Permissions.GrantAccessFile(_pathInfo);

				/*if (!Permissions.IsFileLocked(_pathInfo, FileAccess.Read))
				{
					string outDirPath = File.ReadAllText(_pathInfo);
					if (File.Exists(Path.Combine(outDirPath, Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName))))
						return outDirPath;
				}*/

				return Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();
			}
			set
			{
				/*try
				{*/
				if (!Directory.Exists(_pathInfoFolder))
					Directory.CreateDirectory(_pathInfoFolder);

				if (File.Exists(_pathInfo) && Permissions.IsAdministrator())
					Permissions.GrantAccessFile(_pathInfo);

				/*if (Path.GetFileNameWithoutExtension(Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString()).ToLowerInvariant().StartsWith("SporeMods."))
				{*/
				File.WriteAllText(_pathInfo, Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString());
				if (Permissions.IsAdministrator())
					Permissions.GrantAccessFile(_pathInfo);
				//}
				/*else
					throw new Exception("Non-ModAPI Management kit software must not interfere.");
			}
			catch (Exception ex)
			{
				ModInstallation.InvokeErrorOccurred(new ErrorEventArgs("Install path could not be written to.\n" + ex.Message + "\n" + ex.StackTrace));
			}*/
			}
		}

		/*public static string ManagerInstallLocationRootPath
		{
			get
			{
				var dir = new DirectoryInfo(ManagerInstallLocationPath);
				//MessageDisplay.ShowMessageBox(dir.Name + "\n\nis Internal? " + dir.Name.Equals("Internal", StringComparison.OrdinalIgnoreCase) + "\n\nis Runtime? " + dir.Name.Equals("Runtime", StringComparison.OrdinalIgnoreCase));
				if (dir.Name.Equals("Internal", StringComparison.OrdinalIgnoreCase) || dir.Name.Equals("Runtime", StringComparison.OrdinalIgnoreCase))
					dir = dir.Parent;
				return dir.FullName;
			}
		}*/


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


		/*static string _currentLanguageCode = "CurrentLanguageCode";
		/// <summary>
		/// User-selected Mod Manager language code (en-ca, es-es,...). Lowercase.
		/// </summary>
		public static string CurrentLanguageCode
		{
			get => GetElementValue(_currentLanguageCode);
			set
			{
				if (value != CurrentLanguageCode)
				{
					SetElementValue(_currentLanguageCode, value);
				}
			}
		}*/

		
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
			set => SetElementValue(_shaleDarkTheme, value.ToString());
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
		/// Whether or not the Spore Mod Manager should use a SporeMods.CommonUI.Windows.DecoratableWindow (requires an app restart to take effect)
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
					return !NonEssentialIsRunningUnderWine;
			}
			set => SetElementValue(_useCustomWindowDecorations, value.ToString());
		}


		static string _preferredBorderlessMonitor = "PreferredBorderlessMonitor";
		public static string PreferredBorderlessMonitor
		{
			get => GetElementValue(_preferredBorderlessMonitor);
			set => SetElementValue(_preferredBorderlessMonitor, value);
		}

		/// <summary>
		/// Forces WPF software rendering if true. Some unlucky user with a weird OS+GPU combination needed this option a while back. Also seems to work better under WINE.
		/// </summary>
		static string _useSoftwareRendering = "UseWpfSoftwareRendering";
		static string _forceSoftwareRendering = Path.Combine(ProgramDataPath, "WpfUseSoftwareRendering.info");
		public static bool ForceSoftwareRendering
		{
			get
			{
				try
				{
					if (File.Exists(_forceSoftwareRendering))
					{
						if (bool.TryParse(File.ReadAllText(_forceSoftwareRendering), out bool forceSoftwareRendering))
							return forceSoftwareRendering;
						else
							return true;
					}
				}
				catch { }

				if (bool.TryParse(GetElementValue(_useSoftwareRendering), out bool returnValue))
					return returnValue;
				else
					return NonEssentialIsRunningUnderWine;
			}
			set => SetElementValue(_useSoftwareRendering, value.ToString());
		}
		/*public static bool ForceSoftwareRendering
		{
			get
			{
				bool isWine = NonEssentialIsRunningUnderWine;
				try
				{
					if (isWine)
					{
						if (File.Exists(Path.Combine(ProgramDataPath, "WpfUseSoftwareRendering.info"));

					}
					else
						return File.Exists(Path.Combine(ProgramDataPath, "WpfUseSoftwareRendering.info"));
				}
				catch (Exception ex)
				{
					return isWine;
				}
			}
			/*set
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
			}*
		}*/

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



		static string _ignoreSteamInstallInfo = "ignoreSteamInstallInfo";
		public static bool IgnoreSteamInstallInfo
		{
			get
			{
				if (bool.TryParse(GetElementValue(_ignoreSteamInstallInfo), out bool ignore))
					return ignore;
				else
					return false;
			}
			set => SetElementValue(_ignoreSteamInstallInfo, value.ToString());
		}

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

		*/

		public static string GetElementValue(string elementName, string defaultValue = null)
		{
			XElement element = rootElement.Element(elementName);
			
			if (element != null)
				return element.Value;
			else
				return defaultValue;
		}

		public static void SetElementValue(string elementName, string value)
		{
			if (value.IsNullOrEmptyOrWhiteSpace())
				rootElement.SetElementValue(elementName, null);
			else
				rootElement.SetElementValue(elementName, value);
			_document.Save(_settingsFilePath);
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
