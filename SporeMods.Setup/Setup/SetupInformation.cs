using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace SporeMods.Setup
{
	public static class SetupInformation
	{
		public static int EXIT_RUN_MOD_MGR = 200;
		public static int EXIT_RUN_LK_IMPORTER = 201;

		public static string INSTALL_DIR_LOCATOR_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SporeModManagerStorage", "SporeModManager_InstallPath.info");
		public static string LAST_EXE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SporeModManagerStorage", "LastRunProgram.info");

		public static string IS_WOULDBE_ADMIN_PROCESS = "--wouldBeAdmin";



		[DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern string GetWineVersion();

		public static bool? IsRunningUnderWine(out Version version)
		{
			version = new Version(0, 0, 0, 0);
			try
			{
				string wineVer = GetWineVersion();
				if (!Version.TryParse(wineVer, out version))
					return null;

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}




		public static string SetupAssemblyNameForPackURIs => Assembly.GetExecutingAssembly().GetName().Name; //Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);

		static string _language = null;
		public static string Language
		{
			get => _language;
			private set => _language = value;
		}
		//public static string LkPath = null;
		public static string LauncherKitInstallPath = null;
		public static string MgrExePath = null;


		public static string UsersDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)).Parent.ToString().ToLowerInvariant() + Path.DirectorySeparatorChar;

		static bool _debugCmd = Environment.GetCommandLineArgs().Skip(1).Any(x => x.ToLower() == "--debug");
		static bool _debugFile = File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "smmDebug.txt"));
		public static bool Debug = _debugCmd || _debugFile;
		public static bool IsAutoUpdatingModManager = false;
		//public static bool IsUpdatingFromLauncherKit = false;
		public static bool IsUpgradingFromLauncherKit => (!string.IsNullOrEmpty(LauncherKitInstallPath)) && (!string.IsNullOrWhiteSpace(LauncherKitInstallPath));

		
		public static readonly string DEFAULT_INSTALL_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Spore Mod Manager");
		public static string InstallPath = DEFAULT_INSTALL_PATH;
		public static  readonly string DEFAULT_STORAGE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SporeModManagerStorage");
		public static string StoragePath = DEFAULT_STORAGE_PATH;
		public static string AutoStoragePath = DEFAULT_STORAGE_PATH;

		static List<string> _languageNames = new List<string>()
		{
			"en-ca",
			"es-es",
			"ca-ca"
		};


		static Dictionary<string, ResourceDictionary> _languages = null;

		public static Dictionary<string, ResourceDictionary> Languages
		{
			get
            {
				EnsureAll();
				return _languages;
            }

			private set => _languages = value;
		}


		public static bool CreateShortcuts = true;



		static SetupInformation()
		{
			EnsureAll();
		}

		static bool _ensured = false;
		public static void EnsureAll()
		{
			if (!_ensured)
			{
				var engLangDictionary = App.Current.Resources.MergedDictionaries[0];
				
				_languages = new Dictionary<string, ResourceDictionary>();
				//foreach (string langName in _languageNames)
				for (int langIndex = 0; langIndex < _languageNames.Count; langIndex++)
                {
					_languages[_languageNames[langIndex]] = App.Current.Resources.MergedDictionaries[0];
					App.Current.Resources.MergedDictionaries.RemoveAt(0);
				}

				App.Current.Resources.MergedDictionaries.Add(engLangDictionary);




				string systemLang = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
				if (_languageNames.Contains(systemLang))
				{
					Language = systemLang;
				}
				else
				{
					// Try to get one from the same group. If user has en-us, try to set en-ca, etc
					string systemLangGroup = systemLang.Split('-')[0];
					
					foreach (string lang in _languageNames)
					{
						if (systemLangGroup == lang.Split('-')[0])
						{
							Language = lang;
							break;
						}
					}

				}

				if (Language == null)
					Language = _languageNames[0];




				var args = Environment.GetCommandLineArgs(); //Permissions.GetProcessCommandLineArgs(); //;

				//foreach (string arg in args)
				//bool updateArgFound = false;
				//int updateArgIndex = -1;
				for (int argIndex = 0; argIndex < args.Length; argIndex++)
				{
					string currentArg = args[argIndex].Trim('"', ' ');

					if ((currentArg == "--update") && (args.Length > (argIndex + 1)))
					{
						//updateArgIndex = argIndex;

						string installPath = args[argIndex + 1].Trim('"', ' ');
						if (installPath.TrimEnd('\\').EndsWith("Internal", StringComparison.OrdinalIgnoreCase))
							installPath = new DirectoryInfo(installPath).Parent.ToString();

						InstallPath = installPath;
						IsAutoUpdatingModManager = true;

						//TODO: Fix this
						MgrExePath = Path.Combine(installPath, "Spore Mod Manager.exe");
					}
					else
						SetupSteps.DebugMessageBox(currentArg);

					if (currentArg.Contains("--lang:") && (currentArg.Length > 7))
					{
						string newLangName = currentArg.Substring(currentArg.IndexOf("--lang:") + 7);
						if (_languageNames.Contains(newLangName))
						{
							Language = newLangName;
							SetupSteps.DebugMessageBox("Language from command-line option: " + newLangName);
						}
					}
					//string installPath = args[argIndex + 1].Trim('"', ' ');
				}

				/*if (args.Count() >= 5)
				{
					if (args.Any(x => x.Contains("--update")))
					{
						IsUpdatingModManager = true;

						string mgrPath = args.ElementAt(2).Trim('"', ' ');
						if (Directory.Exists(mgrPath))
						{
							string parentDir = new DirectoryInfo(mgrPath).Parent.ToString();
							if (mgrPath.TrimEnd('\\').EndsWith("Internal", StringComparison.OrdinalIgnoreCase))
								InstallPath = parentDir;
							else
								InstallPath = mgrPath;
						}

						string mgrExePath = args.ElementAt(3).Trim('"', ' ');
						if (File.Exists(mgrExePath))
							MgrExePath = mgrExePath;

						string langArg = args.ElementAt(4).Trim('"', ' ');
						if (langArg.Contains("--lang:"))
							Language = langArg.Substring(langArg.IndexOf("--lang:") + 7, 5);
					}
				}*/

				if (!IsAutoUpdatingModManager)
				{
					foreach (string arg in Environment.GetCommandLineArgs())
					{
						if (IsLauncherKitInstallDir(arg, out string fixedArg))
						{
							LauncherKitInstallPath = fixedArg;
							break;
						}
					}

				}

				if (Permissions.IsAdministrator())
				{
					var fixedDrives = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed);

					DriveInfo mostFreeSpace = fixedDrives.FirstOrDefault();
					foreach (DriveInfo d in fixedDrives)
					{
						if (d.AvailableFreeSpace > mostFreeSpace.AvailableFreeSpace)
							mostFreeSpace = d;
					}
					//DebugMessageBox(mostFreeSpace.RootDirectory.FullName);
					if (!DEFAULT_STORAGE_PATH.ToLowerInvariant().StartsWith(mostFreeSpace.RootDirectory.FullName.ToLowerInvariant()))
					{
						StoragePath = Path.Combine(mostFreeSpace.RootDirectory.FullName, "SporeModManagerStorage");
					}
					AutoStoragePath = StoragePath;


					SetupSteps.DebugMessageBox("_storagePath: " + StoragePath);
					//IsUpdatingFromLauncherKit = (LkPath != null) && (args.Count() > 1);
				}
				_ensured = true;
			}
		}


		static bool IsLauncherKitInstallDir(string rawPath, out string fixedPath)
		{
			fixedPath = null;
			string path = rawPath.Trim('"', ' ');
			//MessageBox.Show("IsLauncherKitInstallDir path: '" + path + "'");

			try
			{
				if ((path != null) && Directory.Exists(path))
				{
					if (
							File.Exists(Path.Combine(path, "Spore ModAPI Launcher.exe")) &&
							File.Exists(Path.Combine(path, "Spore ModAPI Easy Installer.exe")) &&
							File.Exists(Path.Combine(path, "Spore ModAPI Easy Uninstaller.exe")) &&
							(!File.Exists(Path.Combine(path, "Spore Mod Manager.exe"))) &&
							(!File.Exists(Path.Combine(path, "Launch Spore.exe")))
						)
					{
						fixedPath = path;
						//MessageBox.Show("IsLauncherKitInstallDir(): true; " + fixedPath, "fixedPath");
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				//MessageBox.Show("IsLauncherKitInstallDir(): oof\n\n" + ex.ToString());
			}
			//MessageBox.Show("IsLauncherKitInstallDir(): false");
			return false;
		}
	}
}
