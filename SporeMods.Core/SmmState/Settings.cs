using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using SporeMods.NotifyOnChange;
using static SporeMods.Core.GameInfo;
using static SporeMods.Core.SettingsStore;

namespace SporeMods.Core
{
	public enum WindowingMode
	{
		Fullscreen,
		Windowed,
		BorderlessWindowed
	}
		
		
	public class Settings : NOCSingleInstanceObject<Settings>
	{
		public Settings()
			: base()
		{

			_forceSoftwareRendering = Path.Combine(SmmInfo.StoragePath, "WpfUseSoftwareRendering.info");

			//_developerModeEnabledPath = Path.Combine(SmmInfo.StoragePath, "developerMode.info");


			ReparseSettingsDoc();
		}

		public readonly bool DebugMode = false;

		public readonly string TempFolderPath;

		public string LegacyTempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Spore ModAPI Launcher");

		static string _pathInfoFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Spore Mod Manager");
		static string _pathInfo = Path.Combine(_pathInfoFolder, "path.info");


		static string _forcedGameExeType = "ForcedGameExeType";
		/// <summary>
		/// Explicitly-set SporeApp.exe type, if any.
		/// </summary>
		public string ForcedGameExeType
		{
			get => GetValue(_forcedGameExeType);
			set => SetValue(_forcedGameExeType, value);
		}


		internal static string _forcedGalacticAdventuresSporebinEP1Path = "GalacticAdventuresSporebinEP1Path";
		/// <summary>
		/// Explicitly-set Galactic Adventures SporebinEP1 path, if any.
		/// </summary>
		public string ForcedGalacticAdventuresSporebinEP1Path
		{
			get => GetValue(_forcedGalacticAdventuresSporebinEP1Path);
			set => SetValue(_forcedGalacticAdventuresSporebinEP1Path, value);
		}


		internal static string _forcedGalacticAdventuresDataPath = "GalacticAdventuresDataPath";
		/// <summary>
		/// Explicitly-set Galactic Adventures Data folder path, if any.
		/// </summary>
		public string ForcedGalacticAdventuresDataPath
		{
			get => GetValue(_forcedGalacticAdventuresDataPath);
			set => SetValue(_forcedGalacticAdventuresDataPath, value);
		}


		static string _forcedCoreSporeSporeBinPath = "ForcedCoreSporeSporeBinPath";
		/// <summary>
		/// Explicitly-set Core Spore SporeBin path, if any.
		/// </summary>
		public string ForcedCoreSporeSporeBinPath
		{
			get => GetValue(_forcedCoreSporeSporeBinPath);
			set => SetValue(_forcedCoreSporeSporeBinPath, value);
		}


		internal static string _forcedCoreSporeDataPath = "ForcedCoreSporeDataPath";
		/// <summary>
		/// Explicitly-set Core Spore Data path, if any.
		/// </summary>
		public string ForcedCoreSporeDataPath
		{
			get => GetValue(_forcedCoreSporeDataPath);
			set => SetValue(_forcedCoreSporeDataPath, value);
		}

		static string _steamPath = "SteamPath";
		/// <summary>
		/// Explicitly-set Steam install path, if any.
		/// </summary>
		public string SteamPath
		{
			get => GetValue(_steamPath);
			set => SetValue(_steamPath, value);
		}

		static string _forceGameWindowingMode = "ForceGameWindowingMode";
		/// <summary>
		/// Whether or not to force the game's windowing mode.
		/// </summary>
		public bool ForceGameWindowingMode
		{
			get
			{
				if (bool.TryParse(GetValue(_forceGameWindowingMode), out bool forceWindowingMode))
					return forceWindowingMode;
				else
					return false;
			}
			set => SetValue(_forceGameWindowingMode, value.ToString());
		}

		/*static string _forceWindowedMode = "ForceWindowedMode";
		/// <summary>
		/// If ForcedGameWindowingMode is true, forces fullscreen mode if 0, windowed mode if 1, or borderless windowed if 2.
		/// </summary>
		public int ForceWindowedMode
		{
			get
			{
				if (int.TryParse(GetValue(_forceWindowedMode), out int forceWindowedMode))
					return forceWindowedMode;
				else
					return 0;
			}
			set => SetValue(_forceWindowedMode, value.ToString());
		}*/



		static string _forceWindowedMode = "ForcedWindowingMode";
		/// <summary>
		/// If ForcedGameWindowingMode is true, overrides the game's internally-set windowing mode.
		/// </summary>
		public WindowingMode ForcedWindowingMode
		{
			get
			{
				if (WindowingMode.TryParse(GetValue(_forceWindowedMode), out WindowingMode forcedWindowingMode))
					return forcedWindowingMode;
				else
					return WindowingMode.Fullscreen;
			}
			set => SetValue(_forceWindowedMode, value.ToString());
		}

		static string _forceGameWindowBounds = "ForceGameWindowBounds";
		/// <summary>
		/// Whether or not the game window's bounds are forced to set values.
		/// </summary>
		public bool ForceGameWindowBounds
		{
			get
			{
				if (bool.TryParse(GetValue(_forceGameWindowBounds), out bool force))
					return force;
				else
					return false;
			}
			set => SetValue(_forceGameWindowBounds, value.ToString());
		}

		static string _autoGameWindowBounds = "AutoGameWindowBounds";
		/// <summary>
		/// Whether or not the game window's bounds are calculated automatically by the Mod Launcher.
		/// </summary>
		public bool AutoGameWindowBounds
		{
			get
			{
				if (bool.TryParse(GetValue(_autoGameWindowBounds), out bool force))
					return force;
				else
					return false;
			}
			set => SetValue(_autoGameWindowBounds, value.ToString());
		}

		static string _forcedGameWindowWidth = "ForcedGameWindowWidth";
		/// <summary>
		/// Forced game window width, if any.
		/// </summary>
		public int ForcedGameWindowWidth
		{
			get
			{
				if (int.TryParse(GetValue(_forcedGameWindowWidth), out int gameWidth))
					return gameWidth;
				else
					return 800;
			}
			set => SetValue(_forcedGameWindowWidth, value.ToString());
		}

		static string _forcedGameWindowHeight = "ForcedGameWindowHeight";
		/// <summary>
		/// Forced game window height, if any.
		/// </summary>
		public int ForcedGameWindowHeight
		{
			get
			{
				if (int.TryParse(GetValue(_forcedGameWindowHeight), out int gameHeight))
					return gameHeight;
				else
					return 600;
			}
			set => SetValue(_forcedGameWindowHeight, value.ToString());
		}

		static string _forceGameLocale = "ForceGameLocale";
		/// <summary>
		/// Whether or not the game is forced to use a specific locale.
		/// </summary>
		public bool ForceGameLocale
		{
			get
			{
				if (bool.TryParse(GetValue(_forceGameLocale), out bool force))
					return force;
				else
					return false;
			}
			set => SetValue(_forceGameLocale, value.ToString());
		}

		static string _forcedGameLocale = "ForcedGameLocale";
		/// <summary>
		/// Forced game locale, if any.
		/// </summary>
		public string ForcedGameLocale
		{
			get => GetValue(_forcedGameLocale);
			set => SetValue(_forcedGameLocale, value);
		}

		static string _useCustomGameState = "UseCustomGameState";
		/// <summary>
		/// Whether or not the game is forced to start with a specific launch state
		/// </summary>
		public bool UseCustomGameState
		{
			get
			{
				if (bool.TryParse(GetValue(_useCustomGameState), out bool force))
					return force;
				else
					return false;
			}
			set => SetValue(_useCustomGameState, value.ToString());
		}

		static string _gameState = "GameState";
		/// <summary>
		/// Custom game state specified.
		/// </summary>
		public string GameState
		{
			get => GetValue(_gameState);
			set => SetValue(_gameState, value);
		}

		static string _cmdOptions = "CmdOptions";
		/// <summary>
		/// Additional Commandline options, if any.
		/// </summary>
		public string CommandLineOptions
		{
			get => GetValue(_cmdOptions);
			set => SetValue(_cmdOptions, value);
		}

		/*static string _currentLanguageCode = "CurrentLanguageCode";
		/// <summary>
		/// User-selected Mod Manager language code (en-ca, es-es,...). Lowercase.
		/// </summary>
		public static string CurrentLanguageCode
		{
			get => GetValue(_currentLanguageCode);
			set
			{
				if (value != CurrentLanguageCode)
				{
					SetValue(_currentLanguageCode, value);
				}
			}
		}*/

		
		static string _shaleDarkTheme = "ShaleDarkTheme";
		public bool ShaleDarkTheme
		{
			get
			{
				if (bool.TryParse(GetValue(_shaleDarkTheme), out bool returnValue))
					return returnValue;
				else
					return false;
			}
			set
			{
				SetValue(_shaleDarkTheme, value.ToString());
				NotifyPropertyChanged();
			}
		}

		//Developer mode has been shelved for a post-release update
		//static string _developerModeEnabledPath;
		//readonly bool _developerMode = File.Exists(_developerModeEnabledPath);
		public static bool DeveloperMode
		{
			get => false; //_developerMode;
			set
			{
				/*if (value)
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
				}*/
			}
		}


		static string _useCustomWindowDecorations = "UseCustomWindowDecorations";
		/// <summary>
		/// Whether or not the Spore Mod Manager should use a Mechanism.Wpf.Core.Windows.DecoratableWindow (requires an app restart to take effect)
		/// </summary>
		public bool UseCustomWindowDecorations
		{
			get
			{
				if (File.Exists(Path.Combine(SmmInfo.StoragePath, "forceStandardWindowDecorations.txt")))
					return false;
				else if (bool.TryParse(GetValue(_useCustomWindowDecorations), out bool returnValue))
					return returnValue;
				else
					return !(SmmInfo.IsRunningUnderWine);
			}
			set
			{
				SetValue(_useCustomWindowDecorations, value.ToString());
				NotifyPropertyChanged(nameof(UseCustomWindowDecorations));
			}
		}


		static string _preferredBorderlessMonitor = "PreferredBorderlessMonitor";
		public string PreferredBorderlessMonitor
		{
			get => GetValue(_preferredBorderlessMonitor);
			set => SetValue(_preferredBorderlessMonitor, value);
		}

		/// <summary>
		/// Forces WPF software rendering if true. Some unlucky user with a weird OS+GPU combination needed this option a while back. Also seems to work better under WINE.
		/// </summary>
		static string _useSoftwareRendering = "UseWpfSoftwareRendering";
		readonly string _forceSoftwareRendering;
		public bool ForceSoftwareRendering
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

				if (bool.TryParse(GetValue(_useSoftwareRendering), out bool returnValue))
					return returnValue;
				else
					return SmmInfo.IsRunningUnderWine;
			}
			set => SetValue(_useSoftwareRendering, value.ToString());
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
						if (File.Exists(Path.Combine(SmmInfo.StoragePath, "WpfUseSoftwareRendering.info"));

					}
					else
						return File.Exists(Path.Combine(SmmInfo.StoragePath, "WpfUseSoftwareRendering.info"));
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
		public bool AllowVanillaIncompatibleMods
		{
			get
			{
				if (bool.TryParse(GetValue(_allowVanillaIncompatibleMods), out bool force))
					return force;
				else
					return true;
			}
			set => SetValue(_allowVanillaIncompatibleMods, value.ToString());
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
		public UpdatingModeType UpdatingMode
		{
			get
			{
				if (UpdatingModeType.TryParse(GetValue(_updatingMode), out UpdatingModeType updatingMode))
					return updatingMode;
				else
					return 0;
			}
			set => SetValue(_updatingMode, value.ToString());
		}



		static string _ignoreSteamInstallInfo = "ignoreSteamInstallInfo";
		public bool IgnoreSteamInstallInfo
		{
			get
			{
				if (bool.TryParse(GetValue(_ignoreSteamInstallInfo), out bool ignore))
					return ignore;
				else
					return false;
			}
			set => SetValue(_ignoreSteamInstallInfo, value.ToString());
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
	}

	public class LanguageEventArgs : EventArgs
	{
		public string OldLanguage;
		public string NewLanguage;
		public Dictionary<string, string> OldDictionary;
		public Dictionary<string, string> NewDictionary;
	}
}
