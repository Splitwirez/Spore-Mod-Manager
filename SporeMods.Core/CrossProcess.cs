using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SporeMods.Core
{
	public static class CrossProcess
	{
		public const string LAUNCHER_EXE = "Launch Spore";
		public const string MGR_EXE = "Spore Mod Manager";
		public const string DRAG_EXE = "SporeMods.DragServant";
		public const string IMPORTER_EXE = "xLauncherKitImport";

		static string SmmBinPath => Settings.ManagerInstallLocationPath;
		public static Process StartLauncher(string args = null, bool runAsAdmin = false)
			=> RunExecutable(LAUNCHER_EXE, args, runAsAdmin);
		public static async Task<Process> StartLauncherAsync(string args = null, bool runAsAdmin = false)
			=> await Task.Run(() => StartLauncher(args, runAsAdmin));


		public static void RestartModManagerAsAdministrator(string args = null)
		{
			RunExecutable(MGR_EXE, args, true);
			Process.GetCurrentProcess().Kill();
		}


		public static Process StartDragServant(string args = null) =>
			RunExecutable(DRAG_EXE, args, false);
		

		public static Process StartLauncherKitImporter(string args = null, bool runAsAdmin = false) =>
			RunExecutable(IMPORTER_EXE, args, runAsAdmin);
		public static Process StartLauncherKitImporterAsAdministrator(string args = null) =>
			StartLauncherKitImporter(args, true);


		static Process RunExecutable(string exeName, string args = null, bool runAsAdmin = false)
		{
			string exePath = Path.Combine(SmmBinPath, $"{exeName}.exe");

			var info = new ProcessStartInfo(exePath)
			{
				UseShellExecute = true
			};

			if (runAsAdmin)
				info.Verb = "runas";

			if (!string.IsNullOrEmpty(args))
				info.Arguments = Cmd.ShowsConsole ? $"{args} {Cmd.SHOW_CONSOLE_CMD}" : args;
			else if (Cmd.ShowsConsole)
				info.Arguments = Cmd.SHOW_CONSOLE_CMD;

			return Process.Start(info);
		}


		public static bool AreAnyOtherSmmProcessesRunning
        {
			get
			{
				Process[] launcher = Process.GetProcessesByName(LAUNCHER_EXE);
				Process[] mgr = Process.GetProcessesByName(MGR_EXE);
				Process[] drag = Process.GetProcessesByName(DRAG_EXE);
				Process[] import = Process.GetProcessesByName(IMPORTER_EXE);
				return (launcher.Length + mgr.Length + drag.Length + import.Length) > 1;
			}
        }

		public static bool AreAnyOtherModManagersRunning =>
			Process.GetProcessesByName(MGR_EXE).Length > 1;
	}
}
