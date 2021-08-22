//using Ionic.Zip;
using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows;
using System.Xml.Linq;

namespace SporeMods.Core
{
	public static class ModInstallation
	{
		internal static bool IS_INSTALLING_MODS = false;
		internal static Dictionary<string, Exception> INSTALL_FAILURES = new Dictionary<string, Exception>();

		internal static bool IS_UNINSTALLING_MODS = false;

		internal static bool IS_RECONFIGURING_MODS = false;

		static readonly string MOD_INFO = "ModInfo.xml";


		public static event Func<IEnumerable<string>, bool> UninstallingSaveDataDependencyMod;


		static List<string> _installableMods = new List<string>();
		/*public static ErrorInfo[]*/

		public static void ClearQueues()
		{
			if (Directory.Exists(Settings.ModQueuePath))
			{
				string[] files = Directory.EnumerateFiles(Settings.ModQueuePath).ToArray();
				foreach (string s in files)
				{
					if (File.Exists(s))
						File.Delete(s);
				}
			}

			foreach (string d in Directory.EnumerateDirectories(Settings.ModConfigsPath))
			{
				string[] files = Directory.EnumerateFiles(d).ToArray();
				foreach (string s in files)
				{
					if (Path.GetExtension(s).ToLowerInvariant() == ".completion")
						File.Delete(s);
				}
			}
		}

		public static string AnyInstallActivitiesPath = Path.Combine(Settings.TempFolderPath, "InstallingSomething");
		static int _installActivitiesCounter = 0;
		internal static int InstallActivitiesCounter
		{
			get => _installActivitiesCounter;
			set
			{
				_installActivitiesCounter = value;

				if ((_installActivitiesCounter > 0) && (!File.Exists(AnyInstallActivitiesPath)))
					File.Create(AnyInstallActivitiesPath).Close();
				else if ((_installActivitiesCounter <= 0) && File.Exists(AnyInstallActivitiesPath))
					File.Delete(AnyInstallActivitiesPath);
			}
		}

		public static void UninstallModsAsync(IInstalledMod[] modConfigurations)
		{
			IS_UNINSTALLING_MODS = true;
			//ModsManager.Instance.AddToTaskCount(modConfigurations.Length);
			//InstallActivitiesCounter++;

			List<IInstalledMod> modsToUninstall = modConfigurations.ToList();
			List<IInstalledMod> modsToThinkTwiceBeforeUninstalling = new List<IInstalledMod>();

			foreach (IInstalledMod mod in modsToUninstall.Where(x => (x is ManagedMod xm) && xm.Identity.CausesSaveDataDependency))
				modsToThinkTwiceBeforeUninstalling.Add(mod);

			if (modsToThinkTwiceBeforeUninstalling.Count() > 0)
			{
				List<string> modNames = new List<string>();
				foreach (IInstalledMod mod in modsToThinkTwiceBeforeUninstalling)
					modNames.Add(mod.DisplayName);

				if (!UninstallingSaveDataDependencyMod(modNames))
				{
					foreach (IInstalledMod mod in modsToThinkTwiceBeforeUninstalling)
						modsToUninstall.Remove(mod);
				}
			}


			foreach (IInstalledMod mod in modsToUninstall)
			{
				// This function doesn't throw exceptions, the code inside must handle it
				mod.UninstallModAsync();
			}

			//InstallActivitiesCounter--;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr OpenProcess(int flags, bool inherit, int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, out int lpdwSize);

        public static string GetExecutablePath(this Process process)
        {
            string returnValue = string.Empty;
            StringBuilder stringBuilder = new StringBuilder(1024);
            IntPtr hprocess = OpenProcess(0x1000, false, process.Id);

            if (hprocess != IntPtr.Zero)
            {
                int size = stringBuilder.Capacity;

                if (QueryFullProcessImageName(hprocess, 0, stringBuilder, out size))
                    returnValue = stringBuilder.ToString();
            }

            return returnValue;
        }

        /*public static void VerifyServantIsRunning()
		{
			try
			{
				if (Process.GetProcessesByName("SporeMods.InstallServant").Length == 0)
				{
					string servantPath = Path.Combine(Settings.ManagerInstallLocationPath, "SporeMods.InstallServant.exe");

					if (File.Exists(servantPath))
					{
						var info = new ProcessStartInfo(servantPath);
						if (Permissions.IsAtleastWindowsVista())
							info.Verb = "runas";
						Process.Start(info);
					}
					else
						throw new FileNotFoundException("Where's the Install Servant? We were expecting it here: " + servantPath);
				}
			}
			catch (Exception ex)
			{
				InvokeErrorOccurred(new ErrorEventArgs(ex.Message + "\n" + ex.StackTrace));
			}
		}*/

        /*public static void DoFirstRunVerification()
		{
			Debug.WriteLine("FIRST RUN VERIFICATION");
			if (Settings.IsFirstRun)
			{
				Debug.WriteLine("IS FIRST RUN");
				File.WriteAllText(InstructionPath, "verify");
				Settings.IsFirstRun = false;
			}
		}*/

        public static event EventHandler<ModRegistrationEventArgs> AddModProgress;

		static void InvokeAddModProgress(ManagedMod mod)
		{
			AddModProgress?.Invoke(null, new ModRegistrationEventArgs(mod));
		}

		public static async Task RegisterSporemodModWithInstallerAsync(string modName)
		{
			//DebugMessageBoxShow("Registering mod with installer");
			ManagedMod mod = ModsManager.GetManagedMod(modName);
			Debug.Assert(mod != null);
			if (mod.HasConfigurator)
			{
				bool isProgressing = mod.IsProgressing;
				/*foreach (ModComponent m in mod.Configurator.Components)
				{
					DebugMessageBoxShow("DisplayName: " + m.DisplayName + "\nUnique: " + m.Unique + "\nIsEnabled: " + m.IsEnabled);
				}*/

				//DebugMessageBoxShow("Component count: " + mod.Configurator.Components.Count + "\nXML Mod Identity Version: " + mod.XmlVersion);

				if (await ModsManager.Instance.ShowModConfigurator(mod))
				{
					if (!isProgressing)
						IS_RECONFIGURING_MODS = true;
					//await mod.EnableMod();
					mod.EnableMod();
				}
				else if (!isProgressing)
					mod.Configuration.Reload();
			}
			else
			{
				MessageDisplay.RaiseError(new ErrorEventArgs(new Exception("This mod does not have a configurator!")));
			}
		}

		public static bool TryGetEntry(this ZipArchive archive, string entryName, out ZipArchiveEntry entry)
		{
			entry = archive.Entries.FirstOrDefault(x => x.FullName == entryName);
			return entry != null;
		}

		public static bool IsDirectory(this ZipArchiveEntry entry)
		{
			string eName = entry.Name;
			string eFull = entry.FullName;
			bool backSlash = eFull.EndsWith(@"\");
			bool foreSlash = eFull.EndsWith(@"/");
			bool blankName = string.IsNullOrEmpty(eName);

			return (backSlash || foreSlash) && blankName;
		}
	}

	public class ErrorInfo
	{
		public string RegistrationErrorData = null;
		public string RegistrationStackTrace = null;
		public string InstallationErrorData = null;
		public string InstallationStackTrace = null;
	}

	public class ModRegistrationEventArgs : EventArgs
	{
		public bool HasCustomInstaller { get; set; } = false;
		public ManagedMod Mod { get; set; } = null;
		public ModRegistrationEventArgs(ManagedMod mod)
		{
			Mod = mod;
			HasCustomInstaller = Mod.HasConfigurator;
		}
	}

	public class MessageBoxEventArgs : EventArgs
	{
		public string Title { get; set; } = string.Empty;

		public string Content { get; set; } = string.Empty;

		public MessageBoxEventArgs(string title, string content)
		{
			Title = title;
			Content = content;
		}

		public MessageBoxEventArgs(string content)
		{
			Title = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
			Content = content;
		}
	}


	public class ErrorEventArgs : EventArgs
	{
		public string Title { get; set; } = string.Empty;

		public string Content { get; set; } = string.Empty;

		public Exception Exception { get; set; } = null;

		/*public ErrorEventArgs(string title, string content)
		{
			Title = title;
			Content = content;
		}

		public ErrorEventArgs(string content)
		{
			Title = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
			Content = content;
		}*/

		public ErrorEventArgs(Exception ex)
		{
			Title = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
			//Content = ex.Message + "\n" + ex.StackTrace;
			Exception = ex;
		}

		public ErrorEventArgs(string title, Exception ex)
		{
			Title = title;
			//Content = ex.Message + "\n" + ex.StackTrace;
			Exception = ex;
		}
	}

	/*public class MessageBoxEventArgs : EventArgs
	{
		public string Title { get; set; } = string.Empty;

		public string Content { get; set; } = string.Empty;

		public MessageBoxEventArgs(string title, string content)
		{
			Title = title;
			Content = content;
		}
	}*/

	public class ModInstallationStatus
	{
		public List<string> Successes = new List<string>();

		public bool AnySucceeded
		{
			get => Successes.Count > 0;
		}

		public Dictionary<string, Exception> Failures = new Dictionary<string, Exception>();

		public bool AnyFailed
		{
			get => Failures.Keys.Count > 0;
		}

		/*public ModInstallationStatus(List<IInstalledMod> successes, Dictionary<IInstalledMod, object> failures)
		{
			Successes = successes;
			Failures = failures;
		}*/
	}

}
