using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Threading;
using System.Threading;
using System.Linq;
using System.Diagnostics;

namespace SporeMods.Setup
{
	public static class SetupSteps
	{
		public static void InstallSporeModManager(MainWindow window)
		{
			window.SwitchToInstallProgressPage();
			string bkpPath = null;
			Thread thread = new Thread(() =>
			{
				bool bkpPathAccess = false;
				bool installPathAccess = false;
				try
				{
					//MessageBox.Show(Environment.CurrentDirectory, "Initial CurrentDirectory");
					//Environment.CurrentDirectory = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).Directory.ToString();
					//MessageBox.Show(Environment.CurrentDirectory, "After attempting to change CurrentDirectory");
					string[] exeNames =
					{
						"Spore Mod Manager",
						"SporeMods.Manager",
						"SporeMods.DragServant",
						"Launch Spore",
						"SporeMods.Launcher",
						"xLauncherKitImport",
						"SporeMods.KitImporter",
					};

					while (exeNames.Any(x => Process.GetProcessesByName(x).Length > 0))
					{
						/*foreach (string exeName in exeNames)
						{
							foreach (Process proc in Process.GetProcessesByName(exeName))
							{
								proc.Kill
							}
						}*/
						foreach (Process proc in Process.GetProcessesByName(exeNames[2]))
						{
							proc.Kill();
						}
					}

					DebugMessageBox("INSTALL PATH: " + SetupInformation.InstallPath + "\nSTORAGE PATH: " + SetupInformation.StoragePath);


					if (SetupInformation.IsAutoUpdatingModManager)
						SetupInformation.CreateShortcuts = false;

					/*void MoveDir(string oldPath, string newPath)
					{
						foreach (string fil in Directory.EnumerateFiles(oldPath))
						{
							File.Move(fil, Path.Combine(newPath, Path.GetFileName(fil)));
						}


						foreach (string dir in Directory.EnumerateDirectories(oldPath))
						{
							//File.Move(fil, Path.Combine(newPath, Path.GetFileName(fil)));
							MoveDir(dir, Path.Combine(newPath, Path.GetFileName(dir));
						}
					}*/

					if (Directory.Exists(SetupInformation.InstallPath))
					{
						bkpPath = SetupInformation.InstallPath + "-bkp";

						if (Directory.Exists(bkpPath))
						{
							bkpPathAccess = Permissions.GrantAccessDirectory(bkpPath);
							Directory.Delete(bkpPath, true);
						}


						installPathAccess = Permissions.GrantAccessDirectory(SetupInformation.InstallPath);
						Directory.CreateDirectory(bkpPath);
						foreach (string dir in Directory.EnumerateDirectories(SetupInformation.InstallPath))
						{
							string dirName = Path.GetFileName(dir);
							if (!dirName.Equals("Runtime", StringComparison.OrdinalIgnoreCase))
							{
								Directory.Move(dir, Path.Combine(bkpPath, dirName));
							}
						}

						foreach (string fil in Directory.EnumerateFiles(SetupInformation.InstallPath))
							File.Move(fil, Path.Combine(bkpPath, Path.GetFileName(fil)));
					}

					if (!Directory.Exists(SetupInformation.InstallPath))
						Directory.CreateDirectory(SetupInformation.InstallPath);


					IEnumerable<string> smmBinResources = SetupResources.SporeModManagerFiles;
					//IEnumerable<string> dotnetRuntimeResources = SetupResources.DotnetRuntimeFiles;

					int progressOffset = 3;
#if OFFLINE_INSTALLER
					string runtimeDir = AppContext.BaseDirectory;

					progressOffset += Directory.EnumerateFiles(runtimeDir).Count() + Directory.EnumerateDirectories(runtimeDir).Count();
					if (SetupInformation.CreateShortcuts)
						progressOffset += 4;
#endif

					window.SetProgressBarMax(smmBinResources.Count() + progressOffset); //dotnetRuntimeResources.Count() + 4);

					string installPath = Path.Combine(SetupInformation.InstallPath/*, "Internal"*/);
					if (!Directory.Exists(installPath))
						Directory.CreateDirectory(installPath);

					foreach (string s in smmBinResources)
					{
						ExtractResource(s, Path.Combine(installPath, s.Substring(SetupResources.SMM_BIN_PREFIX_LENGTH)));
						window.IncrementProgress();
					}

#if OFFLINE_INSTALLER
					DirectoryCopy(runtimeDir, installPath, (() => window.IncrementProgress()));
#endif
					Permissions.GrantAccessDirectory(installPath);

					/*string smmRedirName = "Spore Mod Manager.exe";
					ExtractResource(smmRedirName, Path.Combine(SetupInformation.InstallPath, smmRedirName));
					window.IncrementProgress();

					smmRedirName = "Launch Spore.exe";
					ExtractResource(smmRedirName, Path.Combine(SetupInformation.InstallPath, smmRedirName));
					window.IncrementProgress();

					smmRedirName = "xLauncherKitImport.exe";
					ExtractResource(smmRedirName, Path.Combine(SetupInformation.InstallPath, smmRedirName));
					window.IncrementProgress();

					smmRedirName = "api-ms-win-core-path-l1-1-0.dll";
					ExtractResource(smmRedirName, Path.Combine(SetupInformation.InstallPath, smmRedirName));
					window.IncrementProgress();*/

					Permissions.GrantAccessDirectory(SetupInformation.InstallPath);

					/*foreach (string s in dotnetRuntimeResources)
					{
						ExtractResource(s, installPath);
						window.IncrementProgress();
					}*/


					/*if (SetupInformation.MgrExePath == null)
					{*/
					if (!SetupInformation.IsAutoUpdatingModManager)
					{
						string redirect = Path.Combine(SetupInformation.DEFAULT_STORAGE_PATH, "redirectStorage.txt");
						if (SetupInformation.StoragePath != SetupInformation.DEFAULT_STORAGE_PATH)
						{
							if (!Directory.Exists(SetupInformation.DEFAULT_STORAGE_PATH))
								Directory.CreateDirectory(SetupInformation.DEFAULT_STORAGE_PATH);


							File.WriteAllText(redirect, SetupInformation.StoragePath);

							Permissions.GrantAccessFile(redirect);
						}
						else if (File.Exists(redirect))
							File.Delete(redirect);

						Permissions.GrantAccessDirectory(SetupInformation.StoragePath);
						/*Dispatcher.BeginInvoke(new Action(() =>
						{
							DebugMessageBox(rresources);
						}));*/

						SetupSteps.CreateShortcuts(window);
					}
					/*
					//purge Launcher Kit shortcuts
					try
					{
						string[] shortcutFilePaths = Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)).ToArray();
						foreach (string s in shortcutFilePaths)
						{
							try
							{
								string shortcutName = Path.GetFileName(s).ToLowerInvariant();
								if (shortcutName.Contains("spore modapi") &&
								(
								shortcutName.EndsWith(".lnk") ||
								shortcutName.EndsWith(".bat")
								))
								{
									if (File.Exists(s))
										File.Delete(s);
								}
							}
							catch { }
						}
					}
					catch { }
					*/
					//}

					if ((!string.IsNullOrEmpty(bkpPath)) && (!string.IsNullOrWhiteSpace(bkpPath)) && Directory.Exists(bkpPath))
					{
						//Directory.Delete(bkpPath, true);
					}

					window.SwitchToInstallCompletedPage();
				}
				catch (Exception ex)
				{
					if ((!string.IsNullOrEmpty(bkpPath)) && (!string.IsNullOrWhiteSpace(bkpPath)))
					{
						string failPath = SetupInformation.InstallPath + "-fail";
						try
						{
							if (Directory.Exists(failPath))
								Directory.Delete(failPath, true);

							Directory.Move(SetupInformation.InstallPath, failPath);
							Directory.Move(bkpPath, SetupInformation.InstallPath);
						}
						catch (Exception e)
						{ }
					}
					MessageBox.Show("Something went wrong while installing/updating the Spore Mod Manager. Any changes have been reverted. (NOT LOCALIZED)\n\n\nDEBUG has administrator privileges? " + Permissions.IsAdministrator() + "\nDEBUG has wouldBeAdmin command-line option? " + Environment.GetCommandLineArgs().Skip(1).Any(x => x == SetupInformation.IS_WOULDBE_ADMIN_PROCESS) + "\nHas access? " + bkpPathAccess + ", " + installPathAccess + "\n\n\n\n" + ex.ToString());
					Process.GetCurrentProcess().Kill();
				}
			});
			thread.Start();
		}


		static void DirectoryCopy(string sourceDirName, string destDirName, Action onCopied)
		{
			if (!Directory.Exists(destDirName))
				Directory.CreateDirectory(destDirName);

			IEnumerable<string> files = Directory.GetFiles(sourceDirName);
			foreach (string file in files)
			{
				string tempPath = Path.Combine(destDirName, Path.GetFileName(file));
				File.Copy(file, tempPath, true);
				
				if (onCopied != null)
					onCopied();
			}


			IEnumerable<string> dirs = Directory.EnumerateDirectories(sourceDirName);
			foreach (string subdir in dirs)
			{
				string destPath = Path.Combine(destDirName, Path.GetFileName(subdir));
				DirectoryCopy(destPath, destPath, null);
				
				if (onCopied != null)
					onCopied();
			}
		}


		static void ExtractResource(string resourceName, string fileOutPath)
		{
			if (false)
			{
				if (resourceName.Contains(@"\"))
				{
					string installPath = null;
					List<string> pathParts = resourceName.Split('\\').ToList();
					List<string> dir = pathParts.Take(pathParts.Count - 1).ToList();

					dir.Insert(0, installPath);
					string dirPath = Path.Combine(dir.ToArray());

					if (!Directory.Exists(dirPath))
						Directory.CreateDirectory(dirPath);

					//fileOutPath = Path.Combine(_installPath, s);

				}
			}
			//else

			//fileOutPath = Path.Combine(SetupInformation.InstallPath, s/*.Replace("SporeMods.Setup.", string.Empty)*/);
			string outDir = Path.GetDirectoryName(fileOutPath);

			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			if (File.Exists(fileOutPath))
				File.Delete(fileOutPath);

			using (var resource = Application.ResourceAssembly.GetManifestResourceStream(resourceName))
			{
				using (var file = new FileStream(fileOutPath, FileMode.Create, FileAccess.Write))
				{
					resource.CopyTo(file);
				}
			}

			Permissions.GrantAccessFile(fileOutPath);

			//rresources += s + "\n";
		}

		public static void CreateShortcuts(MainWindow window)
		{
			string menuShortcutDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Spore Mod Manager");
			if (!Directory.Exists(menuShortcutDir))
				Directory.CreateDirectory(menuShortcutDir);


			string mgrPath = Path.Combine(SetupInformation.InstallPath, "Spore Mod Manager.exe");
			string launcherPath = Path.Combine(SetupInformation.InstallPath, "Launch Spore.exe");


			Shortcut.IShellLinkW mgrMenuShortcut = Shortcut.GetShortcut();
			mgrMenuShortcut.SetPath(mgrPath);
			string mgrMenuOutPath = Path.Combine(menuShortcutDir, "Spore Mod Manager.lnk");
			if (File.Exists(mgrMenuOutPath))
				File.Delete(mgrMenuOutPath);
			((IPersistFile)mgrMenuShortcut).Save(mgrMenuOutPath, false);
			Permissions.GrantAccessFile(mgrMenuOutPath);
			window.IncrementProgress();

			Shortcut.IShellLinkW launcherMenuShortcut = Shortcut.GetShortcut();
			launcherMenuShortcut.SetPath(launcherPath);
			string launcherMenuOutPath = Path.Combine(menuShortcutDir, "Launch Spore.lnk");
			if (File.Exists(launcherMenuOutPath))
				File.Delete(launcherMenuOutPath);
			((IPersistFile)launcherMenuShortcut).Save(launcherMenuOutPath, false);
			Permissions.GrantAccessFile(launcherMenuOutPath);
			window.IncrementProgress();


			if (SetupInformation.CreateShortcuts)
			{
				Shortcut.IShellLinkW mgrDesktopShortcut = Shortcut.GetShortcut();
				mgrDesktopShortcut.SetPath(mgrPath);
				string mgrDesktopOutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Spore Mod Manager.lnk");
				if (File.Exists(mgrDesktopOutPath))
					File.Delete(mgrDesktopOutPath);
				((IPersistFile)mgrDesktopShortcut).Save(mgrDesktopOutPath, false);
				Permissions.GrantAccessFile(mgrDesktopOutPath);
			}
			window.IncrementProgress();

			if (SetupInformation.CreateShortcuts)
			{
				Shortcut.IShellLinkW launcherDesktopShortcut = Shortcut.GetShortcut();
				launcherDesktopShortcut.SetPath(launcherPath);
				string launcherDesktopOutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Launch Spore.lnk");
				if (File.Exists(launcherDesktopOutPath))
					File.Delete(launcherDesktopOutPath);
				((IPersistFile)launcherDesktopShortcut).Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Launch Spore.lnk"), false);
				Permissions.GrantAccessFile(launcherDesktopOutPath);
			}
			window.IncrementProgress();

			Permissions.GrantAccessDirectory(menuShortcutDir);
		}

		public static void DebugMessageBox(string messageBoxText, string caption = "")
		{
			if (SetupInformation.Debug)
				MessageBox.Show(messageBoxText, caption);
		}


		public static Process StartLauncherKitImporterAsAdministrator(string args = null)
		{
			//MessageBox.Show("StartLauncherKitImporterAsAdministrator");
			string exePath = Path.Combine(SetupInformation.InstallPath, "xLauncherKitImport.exe");
			var info = new ProcessStartInfo(exePath)
			{
				UseShellExecute = true,
				Verb = "runas"
			};
			if (!string.IsNullOrEmpty(args))
				info.Arguments = args;
			
			//PropagateDotnetEnvironmentVariables(ref info);

			var proc = Process.Start(info);
			proc.WaitForExit();
			if (proc.ExitCode == -1)
				return null;
			else
				return Process.GetProcessById(proc.ExitCode);
		}

		/*public static void PropagateDotnetEnvironmentVariables(ref ProcessStartInfo info)
		{
			string dotnetRuntimePath = Path.Combine(SetupInformation.InstallPath, "Runtime");
			string DOTNET_MULTILEVEL_LOOKUP = "0";
			info.EnvironmentVariables["DOTNET_MULTILEVEL_LOOKUP"] = DOTNET_MULTILEVEL_LOOKUP;

			info.EnvironmentVariables["DOTNET_ROOT(x86)"] = dotnetRuntimePath;

			info.EnvironmentVariables["DOTNET_ROOT"] = dotnetRuntimePath;

			string PATH = dotnetRuntimePath + ";" + Environment.ExpandEnvironmentVariables("%PATH%");
			info.EnvironmentVariables["PATH"] = PATH;

			string SYSTEMROOT = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%");
			info.EnvironmentVariables["SYSTEMROOT"] = SYSTEMROOT;
		}*/
	}
}