using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
//using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Interop;

namespace SporeMods.Setup
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		

		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			DispatcherUnhandledException += App_DispatcherUnhandledException;
		}

		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			ShowException(e.Exception);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception exc)
				ShowException(exc);
		}

		static bool EXCEPTION_SHOWN = false;
		public static void ShowException(Exception exception)
		{
			if (!EXCEPTION_SHOWN)
			{
				EXCEPTION_SHOWN = true;
				Exception current = exception;
				int count = 0;
				string errorText = "\n\nPlease send the contents this MessageBox and all which follow it to rob55rod\\Splitwirez, along with a description of what you were doing at the time.\n\nSpore Mod Manager Setup will exit after the last Inner exception has been reported.";
				string errorTitle = "Something is very wrong here. Layer ";
				while (current != null)
				{
					MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, errorTitle + count);
					count++;
					current = current.InnerException;
					if (count > 4)
						break;
				}
				if (current != null)
				{
					MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, errorTitle + count);
				}
				Process.GetCurrentProcess().Close();
			}
		}


		bool AllowInstallOnUnsupportedOS => Environment.GetCommandLineArgs().Any(x => x == "--allow-unsupported-platforms");

		string _areYouSureAboutThat = " Installing the Spore Mod Manager on an unsupported platform could damage your install of Spore or your save data, or leave you stranded on an outdated version of the Spore Mod Manager (and thus locked out of mods which require newer versions). Are you ABSOLUTELY 100% CERTAIN you wish to proceed with installation? (NOT LOCALIZED)";

		void WarnOrRejectUnsupportedOS(string baseText)
		{
			string msg = baseText;
			if (AllowInstallOnUnsupportedOS)
			{
				msg += _areYouSureAboutThat;

				if (MessageBox.Show(msg, string.Empty, button: MessageBoxButton.YesNo) != MessageBoxResult.Yes)
				{
					Application.Current.Shutdown(-1);
				}
			}
			else
			{
				MessageBox.Show(msg, string.Empty);
				Application.Current.Shutdown(-1);
			}
		}


		protected override void OnStartup(StartupEventArgs e)
		{
			SetupInformation.EnsureAll();
			RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

			if (!Environment.GetCommandLineArgs().Skip(1).Any(x => x == SetupInformation.IS_WOULDBE_ADMIN_PROCESS))
			{
				ShutdownMode = ShutdownMode.OnExplicitShutdown;
				try
				{
					Process proc = Permissions.RerunAsAdministrator(Permissions.GetProcessCommandLineArgs() + " " + SetupInformation.IS_WOULDBE_ADMIN_PROCESS, false);
					proc.WaitForExit();

					Shutdown();
				}
				catch (System.ComponentModel.Win32Exception)
				{
					Shutdown();
				}
			}
			else
			{
				base.OnStartup(e);


				bool? wine = SetupInformation.IsRunningUnderWine(out Version wineVersion);

				if (wine == true)
				{
					if (wineVersion < new Version(6, 0))
						WarnOrRejectUnsupportedOS("This version of WINE is not supported. The Spore Mod Manager requires WINE 6.0 or newer to function as intended. (NOT LOCALIZED)");
				}
				else if (wine == null)
				{
					WarnOrRejectUnsupportedOS("Unable to determine currently-used WINE version. The Spore Mod Manager requires at least WINE 6.0. (NOT LOCALIZED)");
				}
				else if (Environment.OSVersion.Version < new Version(6, 1, 7601))
				{
					WarnOrRejectUnsupportedOS("This version of Windows is not supported. The Spore Mod Manager requires Windows 7 SP1 or newer to function as intended. (NOT LOCALIZED)");
				}

				ShutdownMode = ShutdownMode.OnExplicitShutdown;
				bool isLangNullOrEmpty = string.IsNullOrEmpty(SetupInformation.Language);
				bool isLangNullOrWhiteSpace = string.IsNullOrWhiteSpace(SetupInformation.Language);
				
				SetupSteps.DebugMessageBox("isLangNullOrEmpty: " + isLangNullOrEmpty + "\nisLangNullOrWhiteSpace: " + isLangNullOrWhiteSpace, "Language null?");
				
				if (isLangNullOrEmpty || isLangNullOrWhiteSpace)
				{
					SetupSteps.DebugMessageBox("Reason: (" + nameof(isLangNullOrEmpty) + " || " + nameof(isLangNullOrWhiteSpace) + ") was true", "Showing LanguagesWindow");
					new LanguagesWindow().ShowDialog();
				}
				else
				{
					try
					{
						Resources.MergedDictionaries[0] = new ResourceDictionary()
						{
							Source = new Uri("/" + SetupInformation.SetupAssemblyNameForPackURIs + ";component/Locale/" + SetupInformation.Language + ".xaml", UriKind.RelativeOrAbsolute)
						};
					}
					catch (Exception langMergeEx)
					{
						SetupSteps.DebugMessageBox("Reason:\n\n\n\n" + langMergeEx.ToString(), "Showing LanguagesWindow");
						new LanguagesWindow().ShowDialog();
					}
				}
				MainWindow = new MainWindow();
				(MainWindow as MainWindow).Start();
				ShutdownMode = ShutdownMode.OnLastWindowClose;
			}
		}

		

		/*void EnsureAdmin()
		{
			if (!Permissions.IsAdministrator())
				Permissions.RerunAsAdministrator();
		}

		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(args.Name);
			byte[] block = new byte[s.Length];
			s.Read(block, 0, block.Length);
			Assembly a2 = Assembly.Load(block);
			return a2;
		}*/
	}

	public static class Permissions
	{
		public static bool IsAtleastWindowsVista()
		{
			return Environment.OSVersion.Version.Major >= 6;
		}

		public static bool IsAdministrator()
		{
			if (Environment.OSVersion.Version.Major <= 5)
				return true;
			else
			{
				WindowsIdentity identity = WindowsIdentity.GetCurrent();
				WindowsPrincipal principal = new WindowsPrincipal(identity);
				return principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
		}

		public static Process RerunAsAdministrator()
		{
			return RerunAsAdministrator(GetProcessCommandLineArgs());
		}

		public static string GetProcessCommandLineArgs()
		{
			var args = Environment.GetCommandLineArgs().ToList();
			if (args.Count > 0)
				args.RemoveAt(0);

			string returnVal = string.Empty;
			foreach (string s in args)
			{
				//MessageBox.Show(s, "arg");
				if (s.Contains(' '))
					returnVal = returnVal + "\"" + s + "\" ";
				else
					returnVal = returnVal + s + " ";
			}
			//MessageBox.Show(returnVal, "args");

			return returnVal;
		}

		public static Process RerunAsAdministrator(string args)
		{
			return RerunAsAdministrator(args, true);
		}

		public static Process RerunAsAdministrator(bool closeCurrent)
		{
			return RerunAsAdministrator(GetProcessCommandLineArgs(), closeCurrent);
		}

		public static Process RerunAsAdministrator(string args, bool closeCurrent)
		{
			ProcessStartInfo startInfo;
			Process process = null;
			string exeName;
#if OFFLINE_INSTALLER
			//https://stackoverflow.com/questions/133379/elevating-process-privilege-programmatically/10905713
			exeName = Process.GetCurrentProcess().MainModule.FileName;
			startInfo = new ProcessStartInfo(exeName, args)
			{
				UseShellExecute = true,
				Verb = "runas",
				WorkingDirectory = new FileInfo(exeName).Directory.ToString()
			};
			/*try
			{*/
			//System.Windows.Forms.MessageBox.Show(args);
			bool disableAutoRelaunch = Environment.CurrentDirectory.Contains(SetupInformation.InstallPath, StringComparison.OrdinalIgnoreCase)
			|| SetupInformation.InstallPath.Contains(Environment.CurrentDirectory, StringComparison.OrdinalIgnoreCase);
			
			if (disableAutoRelaunch)
				MessageBox.Show("Auto-relaunch after this update has been disabled. If you see this message, it means the workaround for that problem I was screaming about on Discord has taken effect...so if this update somehow STILL fails, please let me know.\n\nThis message should never appear outside of pre-release builds of the Spore Mod Manager. If it does, something is wrong. (NOT LOCALIZED)");
			
			process = Process.Start(startInfo);
			
			if (disableAutoRelaunch)
				Process.GetCurrentProcess().Kill();
#else
			string extraArgs = "--elevateUpdater";
			exeName = Path.Combine(SetupInformation.InstallPath, "Spore Mod Manager.exe");
			startInfo = new ProcessStartInfo(exeName)
			{
				//Arguments = args + " ,
				UseShellExecute = true,
				Verb = "runas"
			};
			if ((!string.IsNullOrEmpty(args)) && (!string.IsNullOrWhiteSpace(args)))
				startInfo.Arguments = args + " " + extraArgs;
			else
				startInfo.Arguments = extraArgs;
			//MessageBox.Show("exeName: " + exeName + "\n\n\n\n\nArgs: " + startInfo.Arguments);

			var proc = Process.Start(startInfo);
			proc.WaitForExit();
			if (proc.ExitCode == -1)
				process = null;
			else
				process = Process.GetProcessById(proc.ExitCode);
#endif
			/*}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}*/

			if (closeCurrent && (process != null))
				Process.GetCurrentProcess().Kill();

			return process;
		}

		//https://stackoverflow.com/questions/9108399/how-to-grant-full-permission-to-a-file-created-by-my-application-for-all-users
		/// <summary>
		/// If the current application is running as Administrator, attempts to grant full access to a specific directory and its files.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static bool GrantAccessDirectory(string fullPath, bool recursive = true)
		{
			if (Permissions.IsAdministrator() && Directory.Exists(fullPath))
			{
				DirectoryInfo dInfo = new DirectoryInfo(fullPath);
				DirectorySecurity dSecurity = dInfo.GetAccessControl();
				dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl,
																 InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
																 PropagationFlags.None, AccessControlType.Allow));
				dInfo.SetAccessControl(dSecurity);

				if (recursive)
				{
					foreach (string fil in Directory.EnumerateFiles(fullPath))
					{
						if (!GrantAccessFile(fil))
							return false;
					}

					foreach (string dir in Directory.EnumerateDirectories(fullPath))
					{
						if (!GrantAccessDirectory(dir, true))
							return false;
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// If the current application is running as Administrator, attempts to grant full access to a specific file.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static bool GrantAccessFile(string filePath)
		{
			if (Permissions.IsAdministrator() && File.Exists(filePath))
			{
				//var security = File.GetAccessControl(filePath);
				var sec = new FileSecurity(filePath, AccessControlSections.All);
				sec.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
															 FileSystemRights.FullControl, InheritanceFlags.None,
															 PropagationFlags.None, AccessControlType.Allow));

				return true;

				//string parentPath = Path.GetDirectoryName(filePath);
				//if (Directory.Exists(parentPath))
				//{
				//	return GrantAccess(parentPath);
				//}
			}
			return false;
		}

		public static bool IsFileLocked(string filePath)
		{
			return IsFileLocked(filePath, FileAccess.Read);
		}

		//https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
		public static bool IsFileLocked(string filePath, FileAccess access)
		{
			FileStream stream = null;

			try
			{
				stream = new FileInfo(filePath).Open(FileMode.Open, access, FileShare.None);
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			//file is not locked
			return false;
		}
	}
}