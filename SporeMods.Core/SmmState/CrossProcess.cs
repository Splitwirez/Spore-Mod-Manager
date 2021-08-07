using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SporeMods.Core
{
	public static class CrossProcess
	{
		public const string LAUNCHER_EXE = "Launch Spore";
		public const string MGR_EXE = "Spore Mod Manager";
		public const string DRAG_EXE = "SporeMods.DragServant";
		public const string IMPORTER_EXE = "xLauncherKitImport";




		public static Process StartLauncher(string args = null, bool runAsAdmin = false, string prefixPath = null)
		{
			#if LINUX_BUILD
				
			string settings = "ModManagerSettings.xml";
			string inPath = Path.Combine(SmmInfo.StoragePath, settings);
			
			IEnumerable<string> inLines = File.ReadAllLines(inPath);
			List<string> lines = new List<string>();
			foreach (string line in inLines)
			{
				if (!(line.Contains(Settings._forcedCoreSporeDataPath) || line.Contains(Settings._forcedGalacticAdventuresDataPath) || line.Contains(Settings._forcedGalacticAdventuresSporebinEP1Path)))
					lines.Add(line);
			}
			
			string outDir = Path.Combine(prefixPath, "drive_c", "ProgramData", "SporeModManagerStorage");
			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			CopyAllFiles(SmmInfo.CoreLibsPath, Path.Combine(outDir, Path.GetFileName(SmmInfo.CoreLibsPath)));
			CopyAllFiles(SmmInfo.ModLibsPath, Path.Combine(outDir, Path.GetFileName(SmmInfo.ModLibsPath)));
			CopyAllFiles(SmmInfo.LegacyLibsPath, Path.Combine(outDir, Path.GetFileName(SmmInfo.LegacyLibsPath)));
			
			string outPath = Path.Combine(outDir, settings);
			
			File.WriteAllLines(outPath, lines); //.Copy(inPath, Path.Combine(outDir, settings), true);

			string translatedPath = GameInfo.SporebinEP1.Replace(prefixPath, string.Empty).TrimStart('/').Replace('/', '\\');
			if (translatedPath.Contains("drive_"))
			{
				translatedPath = translatedPath.Substring(6);
				translatedPath = translatedPath.Insert(1, ":");
			}
			Console.WriteLine("TRANSLATED PATH: " + translatedPath);


			var info = new ProcessStartInfo("wine", "\"" + Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().GetExecutablePath()) + "/Launch Spore.exe\" " + Cmd.RunUnderWineFromLinux + " \"" + translatedPath + "\"");
			Console.WriteLine("ARGS: " + info.Arguments);
			info.Environment.Add("WINEPREFIX", prefixPath);
			/*{
				EnvironmentVariables = "\"WINEPREFIX=/home/splitwirez/wine/spore/smm/debugging\""
			});*/
			return Process.Start(info);
			#else
			return RunExecutable(LAUNCHER_EXE, args, runAsAdmin);
			#endif
		}


		static void CopyAllFiles(string source, string dest)
		{
			if (Directory.Exists(source))
			{
				if (!Directory.Exists(dest))
					Directory.CreateDirectory(dest);
				foreach (string path in Directory.EnumerateFiles(source))
				{
					File.Copy(path, Path.Combine(dest, Path.GetFileName(path)), true);
				}
			}
		}

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
			string exePath = Path.Combine(SmmInfo.ManagerInstallPath, SmmInfo.IsWindowsLike ? $"{exeName}.exe" : exeName);

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
