using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace SporeMods.Setup
{
	internal static class SetupResources
	{
		//static readonly string RUNTIME_SETUP_NAME = "dotnet-desktopruntime-installer.exe";
		//static readonly string RUNTIME_SETUP_FOLDER = Path.GetTempFileName() + "-SMMRuntimeInstaller";

		//static readonly string RUNTIME_SETUP_PATH = Path.Combine(RUNTIME_SETUP_FOLDER, RUNTIME_SETUP_NAME);

		public static readonly string SMM_BIN_PREFIX = "smm-bin?";
		//static readonly string DOTNET_RT_PREFIX = "dotnet-runtime?";
		public static readonly int SMM_BIN_PREFIX_LENGTH = SMM_BIN_PREFIX.Length;
		//static readonly int DOTNET_RT_PREFIX_LENGTH = DOTNET_RT_PREFIX.Length;

		static readonly IEnumerable<string> APP_RESOURCES = Application.ResourceAssembly.GetManifestResourceNames();

		public static bool IsEmbeddedFileResource(string resName) => (!resName.EndsWith(".resources", StringComparison.OrdinalIgnoreCase)); //&& (!resName.Contains(RUNTIME_SETUP_NAME, StringComparison.OrdinalIgnoreCase));


		public static bool IsPartOfSporeModManager(string resName) => resName.StartsWith(SMM_BIN_PREFIX) && IsEmbeddedFileResource(resName);

		//public static bool IsPartOfDotnetRuntime(string resName) => resName.StartsWith(DOTNET_RT_PREFIX) && IsEmbeddedFileResource(resName);

		public static List<string> SporeModManagerFiles
		{
			get => APP_RESOURCES.Where(x => IsPartOfSporeModManager(x)).ToList();
			/*{
				List<string> files = new List<string>();

				foreach (string reso in APP_RESOURCES)
				{
					if (IsPartOfSporeModManager(reso))
						files.Add(reso.Substring(SMM_BIN_PREFIX_LENGTH));
				}
				
				return files;
			}*/
		}
		
		/*public static List<string> DotnetRuntimeFiles
		{
			get
			{
				List<string> files = new List<string>();

				foreach (string reso in APP_RESOURCES)
				{
					if (IsPartOfDotnetRuntime(reso))
						files.Add(reso.Substring(DOTNET_RT_PREFIX_LENGTH));
				}

				return files;
			}
		}*/

		/*public static void EnsureRuntimeIsInstalled(MainWindow window)
		{
#if OFFLINE_INSTALLER
			window.Hide();

			string output = "RESOURCES:\n\n";
			string[] names = Application.ResourceAssembly.GetManifestResourceNames();

			foreach (string o in names)
			{
				output += "\t" + o + "\n";
			}

			
			if (!Directory.Exists(RUNTIME_SETUP_FOLDER))
				Directory.CreateDirectory(RUNTIME_SETUP_FOLDER);

			using (var resource = Application.ResourceAssembly.GetManifestResourceStream(RUNTIME_SETUP_NAME))
			{
				using (var file = new FileStream(RUNTIME_SETUP_PATH, FileMode.Create, FileAccess.Write))
				{
					resource.CopyTo(file);
				}
			}

			Process runtimeInstaller = Process.Start(new ProcessStartInfo(RUNTIME_SETUP_PATH, @"/passive /norestart")
			{
				UseShellExecute = true
			});
			runtimeInstaller.WaitForExit();


			//MessageBox.Show(output, "RESOURCES BE LIKE");
			if (runtimeInstaller.ExitCode != 0)
			{
				MessageBox.Show("Exit code was " + runtimeInstaller.ExitCode + "! SOMETHING MAY BE WRONG. IF YOU SEE THIS, REPORT THE POTENTIAL PROBLEM IMMEDIATELY (NOT LOCALIZED).");
				throw new InvalidOperationException();
			}
			//0 = success, 1602 = not success(?)

			if (Directory.Exists(RUNTIME_SETUP_FOLDER))
				Directory.Delete(RUNTIME_SETUP_FOLDER, true);


			window.Show();
#endif
		}*/
	}
}
