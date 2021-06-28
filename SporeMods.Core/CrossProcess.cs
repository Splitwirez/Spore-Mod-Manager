using System;
using System.Diagnostics;
using System.IO;

namespace SporeMods.Core
{
	public static class CrossProcess
	{
		static string SmmBinPath => Settings.ManagerInstallLocationPath;
		public static Process StartLauncher(string args = null, bool runAsAdmin = false) =>
			RunExecutable("Launch Spore", args, runAsAdmin);


		public static void RestartModManagerAsAdministrator(string args = null)
		{
			RunExecutable("Spore Mod Manager", args, true);
			Process.GetCurrentProcess().Kill();
		}


		public static Process StartDragServant(string args = null) =>
			RunExecutable("SporeMods.DragServant", args, false);
		

		public static Process StartLauncherKitImporter(string args = null, bool runAsAdmin = false) =>
			RunExecutable("xLauncherKitImport", args, runAsAdmin);
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
				info.Arguments = args;

			return Process.Start(info);
		}
	}
}
