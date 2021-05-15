using System;
using System.Diagnostics;
using System.IO;

namespace SporeMods.Core
{
	public static class CrossProcess
	{
		static string DotnetRuntimePath = Path.Combine(Settings.ManagerInstallLocationRootPath, "Runtime");
		static string SmmBinPath = Path.Combine(Settings.ManagerInstallLocationRootPath, "Internal");
		public static Process StartLauncher(string args = null)
		{
			var info = new ProcessStartInfo(Path.Combine(Settings.ManagerInstallLocationRootPath, "Launch Spore.exe"))
			{
				UseShellExecute = true
			};

			if (!string.IsNullOrEmpty(args))
				info.Arguments = args;

			var proc = Process.Start(info);
			proc.WaitForExit();
			if (proc.ExitCode == -1)
				return null;
			else
				return Process.GetProcessById(proc.ExitCode);
		}

		public static void RestartModManagerAsAdministrator(string args = null)
		{
			string exePath = Path.Combine(Settings.ManagerInstallLocationRootPath, "Spore Mod Manager.exe");
			var info = new ProcessStartInfo(exePath)
			{
				UseShellExecute = true,
				Verb = "runas"
			};

			if (!string.IsNullOrEmpty(args))
				info.Arguments = args;

			Process.Start(info);
			Process.GetCurrentProcess().Kill();
		}

		public static Process StartDragServant(string args = null)
		{
			var info = new ProcessStartInfo(Path.Combine(SmmBinPath, "SporeMods.DragServant.exe"))
			{
				UseShellExecute = false
			};

			PropagateDotnetEnvironmentVariables(ref info);
			return Process.Start(info);
		}

		public static Process StartLauncherKitImporter(string args = null)
		{
			var info = new ProcessStartInfo(Path.Combine(SmmBinPath, "SporeMods.KitImporter.exe"))
			{
				//UseShellExecute = true
			};
			PropagateDotnetEnvironmentVariables(ref info);

			if (!string.IsNullOrEmpty(args))
				info.Arguments = args;

			return Process.Start(info);
		}

		public static Process StartLauncherKitImporterAsAdministrator(string args = null)
		{
			string exePath = Path.Combine(Settings.ManagerInstallLocationRootPath, "xLauncherKitImport.exe");
			var info = new ProcessStartInfo(exePath)
			{
				UseShellExecute = true,
				Verb = "runas"
			};
			if (!string.IsNullOrEmpty(args))
				info.Arguments = args;

			var proc = Process.Start(info);
			proc.WaitForExit();
			if (proc.ExitCode == -1)
				return null;
			else
				return Process.GetProcessById(proc.ExitCode);
		}

		public static void PropagateDotnetEnvironmentVariables(ref ProcessStartInfo info)
		{
			if (true)
			{
				string DOTNET_MULTILEVEL_LOOKUP = "0";
				info.EnvironmentVariables["DOTNET_MULTILEVEL_LOOKUP"] = DOTNET_MULTILEVEL_LOOKUP;

				info.EnvironmentVariables["DOTNET_ROOT(x86)"] = DotnetRuntimePath;

				info.EnvironmentVariables["DOTNET_ROOT"] = DotnetRuntimePath;

				string PATH = DotnetRuntimePath + ";" + Environment.ExpandEnvironmentVariables("%PATH%");
				info.EnvironmentVariables["PATH"] = PATH;

				string SYSTEMROOT = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%");
				info.EnvironmentVariables["SYSTEMROOT"] = SYSTEMROOT;

				//MessageDisplay.ShowMessageBox("DOTNET_ROOT=" + DotnetRuntimePath + "\n\n\nDOTNET_ROOT(x86)=" + DotnetRuntimePath + "\n\n\nPATH=" + PATH + "\n\n\nDOTNET_MULTILEVEL_LOOKUP=" + DOTNET_MULTILEVEL_LOOKUP + "\n\n\nSYSTEMROOT=" + SYSTEMROOT);
				//MessageBox(IntPtr.Zero, "DOTNET_ROOT=" + DotnetRuntimePath + "\n\n\nDOTNET_ROOT(x86)=" + DotnetRuntimePath + "\n\n\nPATH=" + PATH + "\n\n\nDOTNET_MULTILEVEL_LOOKUP=" + DOTNET_MULTILEVEL_LOOKUP + "\n\n\nSYSTEMROOT=" + SYSTEMROOT, "Environment variables", 0);
			}
			else
			{
				string e = @"DOTNET_MULTILEVEL_LOOKUP=0\0DOTNET_ROOT(x86)=C:\\Program Files (x86)\\Spore Mod Manager\\Runtime\0DOTNET_ROOT=C:\\Program Files (x86)\\Spore Mod Manager\\Runtime\0PATH=C:\\Program Files (x86)\\Spore Mod Manager\\Runtime;C:\\Windows\\system32;C:\\Windows;C:\\Windows\\System32\\Wbem;C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\;C:\\Program Files\\dotnet\\;C:\\Program Files (x86)\\dotnet\\;C:\\Users\\Splitwirez\\.dotnet\\tools\0SYSTEMROOT=C:\Windows";
			}
		}
	}
}
