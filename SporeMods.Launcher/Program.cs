using SporeMods.Core;
using SporeMods.Core.Injection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Automation;
using static SporeMods.Core.GameInfo;

namespace SporeMods.Launcher
{
	class Program
	{
		static Func<string, string> GetLocalizedString = CommonUI.Localization.LanguageManager.Instance.GetLocalizedText;

		public static void ExtractModAPIFix()
		{
			using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SporeMods.Launcher.ModAPIFix.SporeApp_ModAPIFix.exe"))
			using (var file = new FileStream(Path.Combine(GameInfo.SporebinEP1, "SporeApp_ModAPIFix.exe"), FileMode.Create, FileAccess.Write))
			{
				resource.CopyTo(file);
			}

			using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SporeMods.Launcher.ModAPIFix.steam_api.dll"))
			using (var file = new FileStream(Path.Combine(GameInfo.SporebinEP1, "steam_api.dll"), FileMode.Create, FileAccess.Write))
			{
				resource.CopyTo(file);
			}
		}

		static IntPtr GetSporeMainWindow(int processId)
		{
			IntPtr spore = IntPtr.Zero;
			List<IntPtr> hwnds = new List<IntPtr>();
			foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
				NativeMethods.EnumThreadWindows(thread.Id,
					(hWnd, lParam) =>
					{
						/*StringBuilder bld = new StringBuilder();
						
						if (NativeMethods.IsWindow(hWnd))
							NativeMethods.GetClassName(hWnd, bld, 7);
						
						string clss = bld.ToString();
						Debug.WriteLine($"WINDOW CLASS: {clss}");

						if ((clss == "Canvas") && NativeMethods.IsWindow(hWnd))
						{
							spore = hWnd;
							return false;
						}*/
						var autoEl = AutomationElement.FromHandle(hWnd);
						if (autoEl.Current.ClassName == "Canvas")
                        {
							//hwnds.Add(hWnd);
							spore = hWnd;
							return false;
                        }
						//hwnds.Add(hWnd);
						return true;
					}, IntPtr.Zero);
			//Debug.WriteLine($"THERE ARE {hwnds.Count} HWNDS");
			
			/*AutomationElementCollection windows = AutomationElement.RootElement.FindAll(TreeScope.Children, Condition.TrueCondition);
			foreach (AutomationElement el in windows)
            {
				if (el.Current.ClassName == "Canvas")
					return new IntPtr(el.Current.NativeWindowHandle);
            }*/
			
			return spore;
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] programArgs)
		{
			var _ = CommonUI.Localization.LanguageManager.Instance;

			MessageDisplay.ErrorOccurred += (sender, args) =>
			{
				var exc = args.Exception;
				while (exc != null)
				{
					CommonUI.MessageDisplay.ShowMessageBox(exc.ToString(), "oh no (TEMP) (NOT LOCALIZED)");
					if (exc.InnerException == exc)
						break;
					else
						exc = exc.InnerException;
				}

				CommonUI.MessageDisplay.ShowException(args.Exception, false);
				try
				{
					if (SporeLauncher.IsSporeSuspended(true, out bool killed) && killed)
					{
						MessageBox.Show(GetLocalizedString("LauncherError!StartupAborted"));
					}
				}
				catch { }

				Process.GetCurrentProcess().Close();
			};
			MessageDisplay.MessageBoxShown += (sender, args) => CommonUI.MessageDisplay.ShowMessageBox(args.Content, args.Title);
			MessageDisplay.DebugMessageSent += (sneder, args) => CommonUI.MessageDisplay.ShowMessageBox(args.Content, args.Title);
			/*{
				if (args.Content.IsNullOrEmptyOrWhiteSpace())
					MessageBox.Show(args.Exception.ToString(), args.Title);
				else
					MessageBox.Show(args.Content + "\n\n" + args.Exception.ToString(), args.Title);
			};*/
			/*{
				if (string.IsNullOrEmpty(args.Content) || string.IsNullOrWhiteSpace(args.Content) || args.Exception.ToString().Contains(args.Content))
					MessageBox.Show(args.Exception.ToString(), args.Title);
				else if (args.Exception == null)
					MessageBox.Show(args.Content, args.Title);
				else
					MessageBox.Show(args.Content + "\n\n" + args.Exception.ToString(), args.Title);
			 };*/

			CommonUI.VersionValidation.WarnIfMissingOriginPrerequisites(Assembly.GetExecutingAssembly());

			if (CommonUI.VersionValidation.IsConfigVersionCompatible(false, out Version previousModMgrVersion))
			{
				if (ServantCommands.RunLkImporter() == null)
				{
					if (programArgs.Any(x => x == SporeLauncher.EXTRACT_ORIGIN_PREREQ))
					{
						ExtractModAPIFix();
						return;
					}
					else
					{
						if (Settings.ForceSoftwareRendering)
							RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
						//uncomment below to test ProgressDialog appearance
						/*var progressDialog = CommonUI.Updater.GetProgressDialog(string.Empty, null, true);
						Application.Run();*/
						if (File.Exists(Path.Combine(Settings.TempFolderPath, "InstallingSomething")))
						{
							MessageBox.Show(GetLocalizedString("LauncherError!ModsInstalling"));
							Process.GetCurrentProcess().Kill();
						}


						if (!CrossProcess.AreAnyOtherSmmProcessesRunning)
						{
							CommonUI.Updater.CheckForUpdates();
						}

						Settings.ManagerInstallLocationPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();

						bool proceed = true;
						if (Permissions.IsAtleastWindowsVista() && Permissions.IsAdministrator())
						{
							proceed = false;
							if (Settings.NonEssentialIsRunningUnderWine)
								proceed = true;
							else if (MessageBox.Show(GetLocalizedString("DontRunAsAdmin").Replace("%APPNAME%", "Launch Spore"), String.Empty, MessageBoxButtons.YesNo) == DialogResult.Yes)
								proceed = true;
						}

						try
						{
							if (!Settings.AreDllsPresent())
							{
								MessageBox.Show(GetLocalizedString("LauncherError!RunMgr"));
								proceed = false;
							}
						}
						catch (Exception ex)
						{
							MessageBox.Show(GetLocalizedString("LauncherError!RunMgr"));
							proceed = false;
						}

						if (proceed)
						{
							//GameInfo..badBadGameInstallPath += (sneder, args) =>
							if (GameInfo.BadGameInstallPaths.Any())
							{
								MessageBox.Show(GetLocalizedString("LauncherError!RunMgr")); //Please run the Spore Mod Manager at least once before running the Spore Mod Launcher.
								Process.GetCurrentProcess().Kill();
							}//;

							SporeLauncher.CaptionHeight = SystemInformation.CaptionHeight;
							SporeLauncher.GetSporeMainWindow = GetSporeMainWindow;

							if (SporeLauncher.IsInstalledDarkInjectionCompatible())
								SporeLauncher.LaunchGame();
						}
						else
							Application.Exit();
					}
				}
			}
		}
	}
}
