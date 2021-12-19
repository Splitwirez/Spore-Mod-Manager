using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using SystemInformation = System.Windows.Forms.SystemInformation;

using SporeMods.Core;
using SporeMods.Core.Injection;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
using SporeMods.Views;
using SporeMods.ViewModels;

using CUIMsg = SporeMods.CommonUI.MessageDisplay;
using CoreMsg = SporeMods.Core.MessageDisplay;

namespace SporeMods.Launcher
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : SmmApp
	{
		protected override void FinishStartup(bool isAdmin)
		{
			if (File.Exists(Path.Combine(Settings.TempFolderPath, "InstallingSomething")))
			{
				MessageBox.Show(GetLocalizedString("LauncherError!ModsInstalling"));
				Process.GetCurrentProcess().Kill();
			}

			bool proceed = true;
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

			SmmApp.Current.Shutdown();
		}


		protected override bool OnErrorOccurred(object sender, Core.ErrorEventArgs e)
		{
			try
			{
				if (SporeLauncher.IsSporeSuspended(true, out bool killed) && killed)
				{
					MessageBox.Show(GetLocalizedString("LauncherError!StartupAborted"));
				}
			}
			catch { }

			return base.OnErrorOccurred(sender, e);
		}




		Func<string, string> GetLocalizedString => CommonUI.Localization.LanguageManager.Instance.GetLocalizedText; 
		
		static IntPtr GetSporeMainWindow(int processId)
		{
			IntPtr spore = IntPtr.Zero;
			//List<IntPtr> hwnds = new List<IntPtr>();
			foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
				NativeMethodsInj.EnumThreadWindows(thread.Id, (hWnd, lParam) =>
				{
					/*StringBuilder bld = new StringBuilder();

					if (NativeMethodsInj.IsWindow(hWnd))
						NativeMethodsInj.GetClassName(hWnd, bld, 7);

					string clss = bld.ToString();
					Cmd.WriteLine($"WINDOW CLASS: {clss}");

					if ((clss == "Canvas") && NativeMethodsInj.IsWindow(hWnd))
					{
						spore = hWnd;
						return false;
					}*/
					//NativeMethodsInj.GetClassName()

					StringBuilder bld = new StringBuilder();
					if ((hWnd != IntPtr.Zero) && NativeMethodsInj.IsWindow(hWnd) && NativeMethodsInj.IsWindowVisible(hWnd))
					{
						NativeMethodsInj.GetClassName(hWnd, bld, 7);

						string clss = bld.ToString();
						Cmd.WriteLine($"WINDOW CLASS: {clss}");

						if ((clss == "Canvas") && (hWnd != IntPtr.Zero) && NativeMethodsInj.IsWindow(hWnd))
						{
							spore = hWnd;
							return false;
						}
					}
					return true;
				}, IntPtr.Zero);
			//Cmd.WriteLine($"THERE ARE {hwnds.Count} HWNDS");

			/*AutomationElementCollection windows = AutomationElement.RootElement.FindAll(TreeScope.Children, Condition.TrueCondition);
			foreach (AutomationElement el in windows)
            {
				if (el.Current.ClassName == "Canvas")
					return new IntPtr(el.Current.NativeWindowHandle);
            }*/

			return spore;
		}
	}
}
