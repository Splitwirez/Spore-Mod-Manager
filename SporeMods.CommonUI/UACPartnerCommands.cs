using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;

using SporeMods.Core;
using SporeMods.CommonUI.Localization;
using SporeMods.Views;
using System.Windows.Threading;

namespace SporeMods.CommonUI
{

	public static class UACPartnerCommands
	{
		const string LIMITED_PARTNER_PID_ARG = "--limited-partner-pid:";
		const string DRAG_HWND_ARG = "--drag-hwnd:";


		/// <summary>
		/// Setup UAC messenger and such from non-administrative process.
		/// </summary>
		/// <returns>Whether or not to proceed as the elevated instance after calling.</returns>
		public static bool PrepareAppForUAC(bool wantsPartnership, bool warnIfUserGaveUnwantedElevation)
		{
			if (wantsPartnership)
			{
				if (Permissions.IsAdministrator())
					_UACAdminPartnerProcess = Process.GetCurrentProcess();
				else
					_UACLimitedPartnerProcess = Process.GetCurrentProcess();
			}

			if (Permissions.IsUACEnabled)
			{
				if (Permissions.IsAdministrator())
				{
					var cmdArgs = Environment.GetCommandLineArgs();
					int relaunchedness = 0;


					if (wantsPartnership)
					{
						var limitedPidArg = cmdArgs.FirstOrDefault(x => x.StartsWith(LIMITED_PARTNER_PID_ARG));
						if (limitedPidArg != null)
						{
							string limitedPidSubstr = limitedPidArg.Substring(LIMITED_PARTNER_PID_ARG.Length);
							Console.WriteLine($"limitedPidSubstr: {limitedPidSubstr}");
							if (int.TryParse(limitedPidSubstr, out int limitedPid))
							{
								_UACLimitedPartnerProcess = Process.GetProcessById(limitedPid);
								relaunchedness++;
							}
							else
								Console.WriteLine("pid arg couldn't be parsed");
						}
						else
							Console.WriteLine("pid arg was null");

						var dragHwndArg = cmdArgs.FirstOrDefault(x => x.StartsWith(DRAG_HWND_ARG));
						if (dragHwndArg != null)
						{
							string dragHwndSubstr = dragHwndArg.Substring(DRAG_HWND_ARG.Length);
							Console.WriteLine($"dragHwndSubstr: {dragHwndSubstr}");
							if (int.TryParse(dragHwndSubstr, out int dragHwndI))
							{
								_dragHwnd = new IntPtr(dragHwndI);
								relaunchedness++;
							}
							else
								Console.WriteLine("hwnd arg couldn't be parsed");
						}
						else
							Console.WriteLine("hwnd arg was null");
					}


					if (warnIfUserGaveUnwantedElevation && (wantsPartnership ? (relaunchedness < 2) : true))
					{
						if (MessageBox.Show(LanguageManager.Instance.GetLocalizedText("DontRunAsAdmin").Replace("%APPNAME%", "Spore Mod Manager"), string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
						{
							if (HasUACLimitedPartnerProcess)
								_UACLimitedPartnerProcess.Kill();
							Application.Current.Shutdown();
						}
					}

					return true;
				}
				else
				{
					string args = Permissions.GetProcessCommandLineArgs() + " " + UpdaterService.IGNORE_UPDATES_ARG;
					
					if (wantsPartnership)
					{
						UACLimitedPartnerDragWindow window = new UACLimitedPartnerDragWindow();
						SmmApp.Current.DragWindow = window;
						window.Show();

						args += " " + LIMITED_PARTNER_PID_ARG + Process.GetCurrentProcess().Id;
						args += " " + DRAG_HWND_ARG + new WindowInteropHelper(window).EnsureHandle();

						window.Width = 0;
						window.Height = 0;

						_UACAdminPartnerProcess = Permissions.RerunAsAdministrator(args, false);
					}
					else
						Permissions.RerunAsAdministrator(args, true);

					return false;
				}
			}
			else
				return true;
		}

		static Process _UACLimitedPartnerProcess = null;
		static Process _UACAdminPartnerProcess = null;
		static IntPtr _dragHwnd = IntPtr.Zero;

		public static bool HasUACLimitedPartnerProcess
		{
			get => (_UACLimitedPartnerProcess != null) && (!_UACLimitedPartnerProcess.HasExited);
		}

		public static bool HasUACAdminPartnerProcess
		{
			get => (_UACAdminPartnerProcess != null) && (!_UACAdminPartnerProcess.HasExited);
		}

		public static bool HasUACPartnership
			=> HasUACLimitedPartnerProcess && HasUACAdminPartnerProcess;

		public static bool IsUACLimitedPartnerProcess
        {
			get => HasUACPartnership ? (_UACLimitedPartnerProcess.Id == Process.GetCurrentProcess().Id) : false;
		}

		public static bool IsUACAdminPartnerProcess
		{
			get => HasUACPartnership ? (_UACAdminPartnerProcess.Id == Process.GetCurrentProcess().Id) : false;
		}

		public static bool TryGetPartnerDragWindowHwnd(out IntPtr hWnd)
        {
			hWnd = _dragHwnd;
			return NativeMethods.IsWindow(_dragHwnd);
        }

		public static bool TryShowHidePartnerDragWindow(bool show)
		{
			if (TryGetPartnerDragWindowHwnd(out IntPtr hWnd))
			{
				NativeMethods.ShowWindow(hWnd, show ? 4 : 0);
				return true;
			}
			return false;
		}



		const string DROPPED_FILES = "dropped_files";
		

		public static void SendDroppedFiles(IEnumerable<string> files)
        {
			string msg = string.Empty; //"FILES: ";
			foreach (string f in files)
			{
				msg += $"\n{f}";
			}
			
			Console.WriteLine("FILES: " + msg.Replace("\n", "\n\t"));

			SendSignal(DROPPED_FILES, msg.Trim('\n'));
		}


		static FileSystemWatcher _dragWatcher = null;

		public static bool WatchForPartnerSignals
        {
			get => (_dragWatcher != null) ? _dragWatcher.EnableRaisingEvents : false;
			set
            {
				if (EnsureWatchingForPartnerSignals(value))
					_dragWatcher.EnableRaisingEvents = value;
			}
		}

		
		static bool EnsureWatchingForPartnerSignals(bool enable)
        {
			if (HasUACPartnership)
			{
				if (enable && (_dragWatcher == null))
				{
					_dragWatcher = new FileSystemWatcher(Settings.TempFolderPath)
					{
						EnableRaisingEvents = false,
						IncludeSubdirectories = false
					};

					_dragWatcher.Created += _dragWatcher_Created;
				}
			}

			return _dragWatcher != null;
		}

		static void _dragWatcher_Created(object sneder, FileSystemEventArgs args)
        {
			string sgnl = Path.GetFileName(args.FullPath);
			if (File.Exists(args.FullPath))
			{
				var csNo = StringComparison.OrdinalIgnoreCase;

				while (Permissions.IsFileLocked(args.FullPath))
				{ }

				bool processed = true;

				if (IsUACLimitedPartnerProcess)
				{
					if (sgnl.Equals(LAUNCH_GAME, csNo))
					{
						CrossProcess.StartLauncher();
					}
					else if (sgnl.Equals(OPEN_URL, csNo))
					{
						string path = File.ReadAllText(args.FullPath);
						if (path.StartsWith("http", csNo))
						{
							Process.Start(new ProcessStartInfo(path)
							{
								UseShellExecute = true
							});
						}
					}
					else if (sgnl.Equals(CHANGE_TEXT, csNo))
					{
						string newText = File.ReadAllText(args.FullPath);
						Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
						{
							SmmApp.Current?.DragWindow?.RefreshText(newText);
						}));
					}
					else
						processed = false;
				}
				else if (IsUACAdminPartnerProcess)
				{
					if (sgnl.Equals(DROPPED_FILES, csNo))
					{
						FilesDropped?.Invoke(null, new FileDropEventArgs(File.ReadAllLines(args.FullPath)));
					}
					else
						processed = false;
				}

				if (processed)
					File.Delete(args.FullPath);
			}
		}

		public static event EventHandler<FileDropEventArgs> FilesDropped;


		public static void CloseOtherPartnerProcess()
		{
			if (HasUACAdminPartnerProcess)
				_UACAdminPartnerProcess.Kill();

			if (HasUACLimitedPartnerProcess)
				_UACLimitedPartnerProcess.Kill();
		}

		const string LAUNCH_GAME = "launch_game";
		public static void RunLauncher()
			=> SendSignal(LAUNCH_GAME);

		const string OPEN_URL = "open_url";

		public static void OpenUrl(string url)
			=> SendSignal(OPEN_URL, url);

		const string CHANGE_TEXT = "change_text";

		static readonly string SignalsPath = Settings.TempFolderPath;
		public static void SendSignal(string signal)
			=> SendSignal(signal, string.Empty);
		public static void SendSignal(string signal, string content)
        {
			string signalPath = Path.Combine(SignalsPath, signal);
			if (File.Exists(signalPath))
				File.Delete(signalPath);
			
			File.WriteAllText(signalPath, content);
			Permissions.GrantAccessFile(signalPath);
		}

		public static Process RunLkImporter()
		{
			return RunLkImporter(true);
		}

		public static Process RunLkImporter(bool exitCaller)
		{
			string forceLkImportPath = Path.Combine(Settings.ProgramDataPath, "ForceLkImport.info");
			string parentDirectoryPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
			if (File.Exists(forceLkImportPath))
			{
				Process process = CrossProcess.StartLauncherKitImporter("\"--relaunch:" + Process.GetCurrentProcess().MainModule.FileName + "\""); /*Process.Start(new ProcessStartInfo(Path.Combine(parentDirectoryPath, "SporeMods.KitImporter.exe"), )
				{
					UseShellExecute = true
				});*/
				if (exitCaller)
					Process.GetCurrentProcess().Kill();
				
				return process;
			}
			return null;
		}
	}

	public class FileDropEventArgs : EventArgs
    {
		public readonly IEnumerable<string> Files = null;
		internal FileDropEventArgs(IEnumerable<string> lines)
        {
			Files = lines;
		}
    }
}
