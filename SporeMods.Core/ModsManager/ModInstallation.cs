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

		static List<string> _installableMods = new List<string>();
		/*public static ErrorInfo[]*/

		/*public static void ClearQueues()
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
		}*/

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
			Cmd.WriteLine("FIRST RUN VERIFICATION");
			if (Settings.IsFirstRun)
			{
				Cmd.WriteLine("IS FIRST RUN");
				File.WriteAllText(InstructionPath, "verify");
				Settings.IsFirstRun = false;
			}
		}*/

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

#if MOD_IMPL_RESTORE_LATER
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
#endif

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
