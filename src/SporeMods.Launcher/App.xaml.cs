using Mechanism.Wpf.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
using SporeMods.Context;
using SporeMods.Injection;
using SporeMods.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SporeMods.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
	{
		static Func<string, string> GetLocalizedString = LanguageManager.Instance.GetLocalizedText;

		static IntPtr GetSporeMainWindow(int processId)
		{
			IntPtr spore = IntPtr.Zero;
			//List<IntPtr> hwnds = new List<IntPtr>();
			foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
				InjNativeMethods.EnumThreadWindows(thread.Id, (hWnd, lParam) =>
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
					//NativeMethods.GetClassName()

					StringBuilder bld = new StringBuilder();
					if ((hWnd != IntPtr.Zero) && InjNativeMethods.IsWindow(hWnd) && InjNativeMethods.IsWindowVisible(hWnd))
					{
						InjNativeMethods.GetClassName(hWnd, bld, 7);

						string clss = bld.ToString();
						Debug.WriteLine($"WINDOW CLASS: {clss}");

						if ((clss == "Canvas") && (hWnd != IntPtr.Zero) && InjNativeMethods.IsWindow(hWnd))
						{
							spore = hWnd;
							return false;
						}
					}
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

		protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

			MessageDisplay.ErrorOccurred += (sender, args) =>
			{
				var exc = args.Exception;
				while (exc != null)
				{
					MessageBox.Show(exc.ToString(), "oh no (TEMP) (NOT LOCALIZED)");
					if (exc.InnerException == exc)
						break;
					else
						exc = exc.InnerException;
				}

				MessageDisplayUI.ShowException(args.Exception, false);
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
			MessageDisplay.MessageBoxShown += (sender, args) => MessageDisplayUI.ShowMessageBox(args.Content, args.Title);
			MessageDisplay.DebugMessageSent += (sneder, args) => MessageDisplayUI.ShowMessageBox(args.Content, args.Title);
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

			VersionValidation.WarnIfMissingOriginPrerequisites();

			if (VersionValidation.IsConfigVersionCompatible(false, out Version previousModMgrVersion))
			{
				if (ServantCommands.RunLkImporter() == null)
				{
					if (Environment.GetCommandLineArgs().Any(x => x == SporeLauncher.EXTRACT_ORIGIN_PREREQ))
					{
						SporeLauncher.ExtractOriginPrerequisites();
						return;
					}
					else
					{
						if (Settings.ForceSoftwareRendering)
							RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
						//uncomment below to test ProgressDialog appearance
						/*var progressDialog = Common.Updater.GetProgressDialog(string.Empty, null, true);
						Application.Run();*/
						if (File.Exists(Path.Combine(Settings.TempFolderPath, "InstallingSomething")))
						{
							MessageBox.Show(GetLocalizedString("LauncherError!ModsInstalling"));
							Process.GetCurrentProcess().Kill();
						}


						if (!CrossProcess.AreAnyOtherSmmProcessesRunning)
						{
							Updater.CheckForUpdates();
						}

						Settings.ManagerInstallLocationPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();

						bool proceed = true;
						if (Permissions.IsAtleastWindowsVista() && Permissions.IsAdministrator())
						{
							proceed = false;
							if (Settings.NonEssentialIsRunningUnderWine)
								proceed = true;
							else if (MessageBox.Show(GetLocalizedString("DontRunAsAdmin").Replace("%APPNAME%", "Launch Spore"), String.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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

							SporeLauncher.CaptionHeight = (int)SystemScaling.WpfUnitsToRealPixels(SystemParameters.CaptionHeight);
							SporeLauncher.GetSporeMainWindow = GetSporeMainWindow;

							if (SporeLauncher.IsInstalledDarkInjectionCompatible())
								SporeLauncher.LaunchGame();
						}
						else
							Shutdown();
					}
				}
			}


			Shutdown();
		}
    }
}
