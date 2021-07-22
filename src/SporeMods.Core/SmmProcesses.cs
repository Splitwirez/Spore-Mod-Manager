using SporeMods.BaseTypes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SporeMods.Core
{
	public class SmmProcesses : NOCSingleInstanceObject<SmmProcesses>
	{
		const string LAUNCHER_EXE = "Launch Spore";
		const string MGR_EXE = "Spore Mod Manager";
		const string UAC_MSGR_EXE = "xUacMessenger";
		const string LK_IMPORTER_EXE = "xLauncherKitImport";

		public Process StartLauncher(string args = null, bool runAsAdmin = false) =>
			RunExecutable(LAUNCHER_EXE, args, runAsAdmin);

		bool CurrentExeNameEquals(string other) => Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName).Equals(other, StringComparison.OrdinalIgnoreCase);

		public bool IsMgr => CurrentExeNameEquals(MGR_EXE);
		public bool IsLauncher => CurrentExeNameEquals(LAUNCHER_EXE);
		public bool IsUacMsgr => CurrentExeNameEquals(UAC_MSGR_EXE);
		public bool IsImporter => CurrentExeNameEquals(LK_IMPORTER_EXE);


		public void RestartModManagerAsAdministrator(string args = null)
		{
			RunExecutable(MGR_EXE, args, true);
			Process.GetCurrentProcess().Kill();
		}


		public Process StartUacMessenger(string args = null) =>
			RunExecutable(UAC_MSGR_EXE, args, false);
		

		public Process StartLauncherKitImporter(string args = null, bool runAsAdmin = false) =>
			RunExecutable(LK_IMPORTER_EXE, args, runAsAdmin);
		public Process StartLauncherKitImporterAsAdministrator(string args = null) =>
			StartLauncherKitImporter(args, true);


		Process RunExecutable(string exeName, string args = null, bool runAsAdmin = false)
		{
			string exePath = Path.Combine(SmmStorage.Instance.SmmInstallDirectory, $"{exeName}.exe");

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


		public bool AreAnyOtherSmmProcessesRunning
        {
			get
			{
				Process[] launcher = Process.GetProcessesByName(LAUNCHER_EXE);
				Process[] mgr = Process.GetProcessesByName(MGR_EXE);
				Process[] drag = Process.GetProcessesByName(UAC_MSGR_EXE);
				Process[] import = Process.GetProcessesByName(LK_IMPORTER_EXE);
				return (launcher.Length + mgr.Length + drag.Length + import.Length) > 1;
			}
        }

		public bool AreAnyOtherModManagersRunning =>
			Process.GetProcessesByName(MGR_EXE).Length > 1;
	}
}
