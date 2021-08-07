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
using SporeMods.NotifyOnChange;
using static SporeMods.Core.GameInfo;

namespace SporeMods.Core
{
	public static partial class SmmInfo// : NOCSingleInstanceObject<SmmInfo>
	{
		static SmmInfo()
		{
			//TODO: Figure out a better place for this on Linux
			string storagePath = Path.Combine(Environment.GetFolderPath(
				IsWindowsLike
					? Environment.SpecialFolder.CommonApplicationData
					: Environment.SpecialFolder.ApplicationData),
				"SporeModManagerStorage");
			Console.WriteLine($"DEFAULT STORAGE PATH: {storagePath}");


			string redirectStorageFilePath = Path.Combine(storagePath, "redirectStorage.txt");
			if (File.Exists(redirectStorageFilePath))
			{
				string redirPath = File.ReadAllText(redirectStorageFilePath);
				if (!Directory.Exists(redirPath))
					Directory.CreateDirectory(redirPath);
				storagePath = redirPath;
			}
			else if (!Directory.Exists(storagePath))
				Directory.CreateDirectory(storagePath);
			
			//_storagePath = AddProperty<string>(nameof(StoragePath), storagePath);
			StoragePath = storagePath;

			
			string EnsureStorageSubdir(string dirName)
			{
				string retPath = Path.Combine(storagePath, dirName);
				if (!Directory.Exists(retPath))
					Directory.CreateDirectory(retPath);
				return retPath;
			}


			//IsWindowsLike = IS_WINDOWS_LIKE;
			//IsNativeLinux = IS_NATIVE_LINUX;



			SettingsStore.ReparseSettingsDoc(storagePath);

			ModsPath = EnsureStorageSubdir("ModConfigs");
			TempFolderPath = EnsureStorageSubdir("Temp");
			LogsPath = EnsureStorageSubdir("Logs");
			ModLibsPath = EnsureStorageSubdir("mLibs");
			CoreLibsPath = EnsureStorageSubdir("coreLibs");
			LegacyLibsPath = EnsureStorageSubdir("legacyLibs");

			
			/*if (!Directory.Exists(TempFolderPath))
				Directory.CreateDirectory(TempFolderPath);*/
			

			_firstRunPath = Path.Combine(storagePath, "firstRun.info");

			
			bool wineMode = false;
			if (!bool.TryParse(SettingsStore.GetValue(_wineRemembered, string.Empty), out wineMode))
			{
				bool wineDetected = TryGetIsRunningUnderWine(out bool _b, out Version _v);
				SettingsStore.SetValue(_wineRemembered, wineDetected.ToString());
				wineMode = wineDetected;
			}
			
			//_wineMode = AddProperty<bool>(nameof(IsRunningUnderWine), wineMode);
			IsRunningUnderWine = true;

			
			

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

			Version dllsBuild = Mods.ModIdentity.UNKNOWN_MOD_VERSION;
			if (versions.Count() > 0)
			{
				Version minVer = versions.Min();
				dllsBuild = new Version(minVer.Major, minVer.Minor, minVer.Build, 0);
			}

			//_currentDllsBuild = AddProperty<Version>(nameof(CurrentDllsBuild), dllsBuild);
			CurrentDllsBuild = dllsBuild;
		}


		public static readonly bool IsWindowsLike = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		public static readonly bool IsNativeLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);


		//public readonly bool IsWindowsLike;
		//public readonly bool IsNativeLinux;

		




		static string _wineRemembered = "WineMode";
		static bool _wineMode;
		/// <summary>
		/// True only if initially installed/configured under WINE.
		/// </summary>
		public static bool IsRunningUnderWine
		{
			get => _wineMode;
			internal set => _wineMode = value;
		}


		
		//static string _prevStacc = null;
		static string CreateTargetFrameworkLabel()
		{
			/*string stacc = new StackTrace().ToString();
			if (_prevStacc == null)
			{
				Console.WriteLine("STACC: \n" + stacc + "\n\n\n");
				_prevStacc = stacc;
			}*/

			string fwLabel;
			string fwPrefix = "dotnet";
			Version envVer = Environment.Version;
			string fwSuffix = $"--{envVer}";

			if (envVer.Major > 4) //.NET 5+, so no action is needed
			{
			}
			else if ((envVer.Major == 3) && (envVer.Minor == 1)) //.NET Core 3.1.x
			{
				fwLabel = $"{fwPrefix}-core";
			}
			else //Presumably .NET Framework...but technically COULD also be a version of .NET Core old enough that we don't care about it.
			{
				fwLabel = $"{fwPrefix}-framework";
			}

			fwLabel = $"{fwPrefix}{fwSuffix}";
			Console.WriteLine($"TargetFrameworkLabel: {fwLabel}");

			return fwLabel;
		}
		
		/// <summary>
		/// SMM-specific label for the current .NET runtime version.
		/// </summary>
		public static readonly string TargetFrameworkLabel = CreateTargetFrameworkLabel(); //$"dotnet-core--{Environment.Version}";
		/*public static string TargetFrameworkLabel
		{
			get => _targetFramework;
		}*/


		
		/// <summary>
		/// Path to wherever the Mod Manager is installed to.
		/// </summary>
		public static string ManagerInstallPath
		{
			get
			{
				if (!Directory.Exists(_pathInfoFolder))
					Directory.CreateDirectory(_pathInfoFolder);

				if (File.Exists(_pathInfo) && Permissions.IsAdministrator())
					Permissions.GrantAccessFile(_pathInfo);

				if (!Permissions.IsFileLocked(_pathInfo, FileAccess.Read))
				{
					string outDirPath = File.ReadAllText(_pathInfo);
					if (File.Exists(Path.Combine(outDirPath, Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName))))
						return outDirPath;
				}

				return Directory.GetParent(Assembly.GetEntryAssembly().Location).ToString();
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
				File.WriteAllText(_pathInfo, value);
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

		/*public string ManagerInstallLocationRootPath
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


		/// <summary>
		/// Path to the mods/settings/etc folder. Default location is '%programdata%\SporeModManagerStorage'.
		/// </summary>
		static string _storagePath;
		public static string StoragePath
		{
			get => _storagePath;
			internal set => _storagePath = value;
		}


		/// <summary>
		/// Path to the ModConfigs subfolder. Used for storing mods' files.
		/// </summary>
		public static readonly string ModsPath;


		/*
		/// <summary>
		/// Path to the ModSettings subfolder. Used for storing mods' settings.
		/// </summary>
		public string ModSettingsPath
		{
			get
			{
				string path = Path.Combine(ProgramDataPath, "ModSettings");
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				return path;
			}
		}*/
		

		/// <summary>
		/// Path to the Logs subfolder. Used for storing error logs and such.
		/// </summary>
		public static readonly string LogsPath;


		/// <summary>
		/// Path to the mLibs subfolder. Used for storing ModAPI mods' DLLs for injection.
		/// </summary>
		public static readonly string ModLibsPath;
		/*{
			get
			{
				string path = Path.Combine(ProgramDataPath, "mLibs");
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				return path;
			}
		}*/

		/// <summary>
		/// Path to the coreLibs subfolder. Used for storing the ModAPI Core DLLs for injection.
		/// </summary>
		public static readonly string CoreLibsPath;
		/*{
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
		}*/

		/// <summary>
		/// Path to the legacyLibs subfolder. Used for storing Legacy ModAPI mods' DLLs for injection.
		/// </summary>
		public static readonly string LegacyLibsPath;
		/*{
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
		}*/

		/*
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
		}*/


		static Version _currentDllsBuild;
		public static Version CurrentDllsBuild
		{
			get => _currentDllsBuild;
			set => _currentDllsBuild = value;
		}


		static bool AreDllsPresent()
		{
			return Directory.Exists(CoreLibsPath) && (Directory.EnumerateFiles(CoreLibsPath).Count() >= 3) && Directory.Exists(LegacyLibsPath) && (Directory.EnumerateFiles(LegacyLibsPath).Count() >= 2);
		}

		static void ExtractDlls()
		{
			ExtractResource(CoreLibsPath, "SporeModAPI.lib");
			ExtractResource(CoreLibsPath, "SporeModAPI.disk.dll");
			ExtractResource(CoreLibsPath, "SporeModAPI.march2017.dll");

			ExtractResource(LegacyLibsPath, "SporeModAPI-disk.dll");
			ExtractResource(LegacyLibsPath, "SporeModAPI-steam_patched.dll");
		}

		public static bool EnsureDllsAreExtracted()
		{
			if (!AreDllsPresent())
			{
				ExtractDlls();
			}

			//intentionally call AreDllsPresent() again, so the caller knows if any DLLs are missing or what-have-you
			return AreDllsPresent();
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

		//string _currentDllsBuildString;
		public static string CurrentDllsBuildString
		{
			get => CurrentDllsBuild.ToString();
			//set => _currentDllsBuildString = value;
			/*{
				string baseVer = CurrentDllsBuild.ToString();
				if (baseVer.Contains("."))
					baseVer = baseVer.Substring(0, baseVer.LastIndexOf('.'));
				return baseVer;
			}*/
		}

		/*
		/// <summary>
		/// Path to the ModQueues subfolder.
		/// </summary>
		public string ModQueuePath
		{
			get
			{
				string path = Path.Combine(ProgramDataPath, "ModQueues");
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				return path;
			}
		}*/

		public static readonly string TempFolderPath;

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

				if (!Permissions.IsFileLocked(_pathInfo, FileAccess.Read))
				{
					string outDirPath = File.ReadAllText(_pathInfo);
					if (File.Exists(Path.Combine(outDirPath, Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName))))
						return outDirPath;
				}

				return Directory.GetParent(Assembly.GetEntryAssembly().Location).ToString();
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
				File.WriteAllText(_pathInfo, value);
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
					Settings.SetValue(_currentLanguageCode, value);
				}
			}
		}*/


		static readonly string _firstRunPath;
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

		internal static string _lastMgrVersion = "LastMgrVersion";
		/// <summary>
		/// The last version of the Spore Mod Manager which accessed the current config
		/// </summary>
		public static Version LastRunVersion
		{
			get
			{
				string verVal = SettingsStore.GetValue(_lastMgrVersion);
				if ((!string.IsNullOrEmpty(verVal)) && (!string.IsNullOrWhiteSpace(verVal)) && (Version.TryParse(verVal, out Version returnValue)))
					return returnValue;
				else
					return null;
			}
			set => SettingsStore.SetValue(_lastMgrVersion, value.ToString());
		}

		/// <summary>
		/// Current version of the Spore Mod Manager.
		/// </summary>
		public static Version CurrentVersion
		{
			get => Assembly.GetExecutingAssembly().GetName().Version;
		}


		internal static void AddToSettingsXmlFile(ref XDocument document)
		{
			bool wine = false;
			try
			{
				wine = TryGetIsRunningUnderWine(out bool _b, out Version _v);
			}
			catch { }

			if (!wine)
			{
				try
				{
						wine = File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "winecfg.exe"));
				}
				catch { }
			}


			if (wine)
			{
				document.Root.Add(new XElement(_wineRemembered, true));
			}
		}
	}
}