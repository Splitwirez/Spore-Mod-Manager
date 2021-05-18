using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SporeMods.Core
{
	public static class ServantCommands
	{
		public static void RunLauncher()
		{
			string launchGamePath = Path.Combine(Settings.TempFolderPath, "LaunchGame");
			if (File.Exists(launchGamePath))
				File.Delete(launchGamePath);

			File.WriteAllText(launchGamePath, string.Empty);
			Permissions.GrantAccessFile(launchGamePath);
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
}
