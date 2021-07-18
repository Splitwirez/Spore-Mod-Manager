using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static SporeMods.Core.GameInfo;

namespace SporeMods.Core.Injection
{
	public static class SporeLauncher
	{
		public static Func<string, string> GetLocalizedString = null;

		public const string EXTRACT_ORIGIN_PREREQ = "--originFirstRun";

		public static int CaptionHeight = -1;
		public static IntPtr _processHandle = IntPtr.Zero;
		private static bool _debugMode = File.Exists(Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString(), "debug.txt"));

		static string ModApiHelpThreadURL = "http://davoonline.com/phpBB3/viewtopic.php?f=108&t=6300";
		static string DarkInjectionPageURL = "http://davoonline.com/sporemodder/rob55rod/DarkInjection/";

		public static void OpenDarkInjectionPage()
		{
			Process.Start(new ProcessStartInfo(DarkInjectionPageURL)
			{
				UseShellExecute = true
			});
		}

		//private string SporebinPath;
		private static string _executablePath;
		//private string SteamPath;
		private static GameExecutableType _executableType = GameExecutableType.None;

		// Used for executing Spore and injecting DLLs
		private static STARTUPINFO _startupInfo = new STARTUPINFO();
		private static PROCESS_INFORMATION _processInfo = new PROCESS_INFORMATION();

		public static int CurrentError = 0;

		static void DeleteFolder(string path)
		{
			foreach (string s in Directory.EnumerateFiles(path))
				File.Delete(s);
			foreach (string s in Directory.EnumerateDirectories(path))
				DeleteFolder(s);
			Directory.Delete(path);
		}

		public static Version Spore__1_5_1 = new Version(3, 0, 0, 2818);
		public static Version Spore__March2017 = new Version(3, 1, 0, 22);
		public static void LaunchGame()
		{
			try
			{
				_executablePath = Path.Combine(SporebinEP1, "SporeApp.exe");
				if (File.Exists(_executablePath))
				{
					//IEnumerable<int> rawExeSizes = Enum.GetValues(typeof(GameInfo.)).Cast<long>();
					/*long[] exeSizes = new long[rawExeSizes.Length];
					//exeSizes.CopyTo(rawExeSizes, 0);
					for (int i = 0; i < rawExeSizes.Length; i++)
						exeSizes[i] = (long)rawExeSizes.GetValue(i);*/

					bool exeSizeRecognized = ExecutableFileGameTypes.Keys.Contains(new FileInfo(_executablePath).Length);
					if (IsValidExe())
					{
						try
						{
							_executableType = GetExecutableType();
						}
						catch (Exception ex)
						{
							MessageDisplay.RaiseError(new ErrorEventArgs(ex)); //ex.Message + "\n\n" + ex.StackTrace
							return;
						}

						// Steam users need to do something different...something which doesn't even work most of the time.
						if (!SporeIsInstalledOnSteam())
						{

							MessageDisplay.DebugShowMessageBox("2. Executable type: " + _executableType);

							if (_executableType == GameExecutableType.None)
							{
								// don't execute the game if the user closed the dialog
								return;
							}

							// get the correct executable path
							_executablePath = Path.Combine(SporebinEP1, ExecutableFileNames[_executableType]);

							bool isOriginExe = (_executableType == GameExecutableType.Origin__1_5_1) ||
								(_executableType == GameExecutableType.Origin__March2017);
							bool exeExists = File.Exists(_executablePath);
							//MessageDisplay.ShowMessageBox($"NOT LOCALIZED:\nIs Origin EXE: {isOriginExe}\nEXE exists: {exeExists}");
							if (isOriginExe && (!exeExists))
							{
								try
								{
									CrossProcess.StartLauncher(EXTRACT_ORIGIN_PREREQ, true).WaitForExit();
								}
								catch (Exception ex)
								{
									MessageDisplay.ShowMessageBox($"NOT LOCALIZED: couldn't start laucher:\n\n{ex}");
								}
							}

							string dllEnding = GetExecutableDllSuffix(_executableType);

							MessageDisplay.DebugShowMessageBox("4. DLL suffix: " + dllEnding);

							if (dllEnding == null)
							{
								MessageDisplay.DebugShowMessageBox(GetLocalizedString("LauncherError!GameVersion!NullDllSuffix")); //MessageBox.Show(Strings.VersionNotDetected, CommonStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
								return;
							}

							InjectNormalSporeProcess(dllEnding);

						}
						else
						{
							InjectSteamSporeProcess();
						}
					}
				}

				int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();

				if ((CurrentError != 0) && (CurrentError != 18) && (CurrentError != 87) && (CurrentError != lastError))
				{
					try
					{
						ThrowWin32Exception("Something went wrong", CurrentError);
					}
					catch (Exception ex)
					{
						MessageDisplay.RaiseError(new ErrorEventArgs(ex));
					}
				}

				if ((lastError != 0) && (lastError != 18))
					ThrowWin32Exception("Something went wrong", lastError);
			}
			catch (Exception ex)
			{
				MessageDisplay.RaiseError(new ErrorEventArgs(ex));
				return;
			}
		}

		/*public static void ShowError(Exception ex)
		{
			//MessageBox.Show(Strings.GalacticAdventuresNotExecuted + "\n" + ModApiHelpThreadURL + "\n\n" + ex.GetType() + "\n\n" + ex.Message + "\n\n" + ex.StackTrace, CommonStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
			if (ex is System.ComponentModel.Win32Exception)
			{
				var exc = ex as System.ComponentModel.Win32Exception;
				//"NativeErrorCode: " + exc.NativeErrorCode.ToString() + "\n" + 
				MessageBox.Show("ErrorCode: " + exc.ErrorCode.ToString() + "\n" +
					"NativeErrorCode: " + exc.NativeErrorCode.ToString() + "\n" +
					exc.StackTrace
					/* +
					"HResult: " + exc.HResult.ToString() + "\n"/, "Additional Win32Exception Error Info");

				if (exc.InnerException != null)
				{
					MessageBox.Show("ErrorCode: " + exc.InnerException.GetType() + "\n\n" + exc.InnerException.Message + "\n\n" + exc.InnerException.StackTrace, "Win32Exception InnerException Error Info");
				}
			}
			Process.Start(ModApiHelpThreadURL);
		}*/

		static void InjectDLLs(string dllEnding)
		{
			string coreDllInPath = CoreDllRetriever.GetStoredCoreDllPath(_executableType);
			string coreDllOutPath = CoreDllRetriever.GetInjectableCoreDllPath(_executableType);

			string libInPath = CoreDllRetriever.GetStoredCoreDllPath(_executableType, true);
			string libOutPath = CoreDllRetriever.GetInjectableCoreDllPath(_executableType, true);

			//Copy Core DLL and LIB
			File.Copy(coreDllInPath, coreDllOutPath, true);
			File.Copy(libInPath, libOutPath, true);
			Permissions.GrantAccessFile(coreDllOutPath);
			Permissions.GrantAccessFile(libOutPath);

			//Inject Core DLL
			Injector.InjectDLL(_processInfo, coreDllOutPath);

			//Inject Legacy DLL
			string legacyModApiDLLName = "SporeModAPI-" + dllEnding + ".dll";
			string legacyModApiDLLPath = Path.Combine(Settings.LegacyLibsPath, legacyModApiDLLName);
			Injector.InjectDLL(_processInfo, legacyModApiDLLPath);


			CurrentError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();

			//GetStoredCoreDllPath
			foreach (string s in Directory.EnumerateFiles(Settings.ModLibsPath).Where(x => (!Path.GetFileName(x).Equals(Path.GetFileName(coreDllOutPath), StringComparison.OrdinalIgnoreCase)) && x.ToLowerInvariant().EndsWith(".dll")))
			{
				string debugData = "Now injecting: " + s;
				Console.WriteLine(debugData);
				MessageDisplay.DebugShowMessageBox(debugData);
				Injector.InjectDLL(_processInfo, s);
			}

			if (Directory.Exists(Settings.LegacyLibsPath))
			{
				foreach (var file in Directory.EnumerateFiles(Settings.LegacyLibsPath).Where(x => x.EndsWith((dllEnding + ".dll").ToLowerInvariant())))//"*" + dllEnding + ".dll"))
				{
					string fileName = Path.GetFileName(file);
					MessageDisplay.DebugShowMessageBox("5.* Preparing " + fileName);

					// the ModAPI dll should already be loaded
					if (fileName.ToLowerInvariant() != legacyModApiDLLName.ToLowerInvariant())
					{
						MessageDisplay.DebugShowMessageBox("5.* Injecting " + fileName);
						Injector.InjectDLL(_processInfo, file);
						CurrentError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
					}
					else
						MessageDisplay.DebugShowMessageBox("5.* " + fileName + " was already injected!");
				}
			}
		}

		public static bool IsSporeSuspended() => IsSporeSuspended(false, out bool killed);


		public static bool IsSporeSuspended(bool killIfYes, out bool killed)
		{
			List<ProcessThread> threads = new List<ProcessThread>();
			var proc = Process.GetProcessById((int)_processInfo.dwProcessId);
			bool retVal = proc.Threads.OfType<ProcessThread>().All(x => (x.ThreadState == System.Diagnostics.ThreadState.Wait) && (x.WaitReason == ThreadWaitReason.Suspended));

			if (killIfYes)
			{
				proc.Kill();
				killed = proc.HasExited;
			}
			else
				killed = false;

			return retVal;
		}

		static void InjectNormalSporeProcess(string dllEnding)
		{
			CreateSporeProcess();

			InjectDLLs(dllEnding);

			ResumeSporeProcess();
			//MessageDisplay.ShowMessageBox("EnableBorderless", "_monitorSelected: " + _monitorSelected);
			EnableBorderless(Convert.ToInt32(_processInfo.dwProcessId), _monitor);
		}

		private const int WAIT_ABANDONED = 0x00000080;

		// Steam spore needs special treatment: the game will clsoe if not executed through Steam
		static void InjectSteamSporeProcess()
		{
			var pOpenThreads = new List<IntPtr>();

			string sporeAppName = "SporeApp";
			string steamPath = SteamInfo.SteamPath;
			steamPath = Path.Combine(steamPath, "Steam.exe");
			Process steamProcess = Process.Start(new ProcessStartInfo(steamPath, "-applaunch " + SteamInfo.GalacticAdventuresSteamID.ToString() + " " + GetGameCommandLineOptions())
			{
				UseShellExecute = true
			});
			/*ProcessInfo info = new ProcessInfo("SporeApp.exe");

			info.Started += (sneder, args) =>
			{*/
			/*try
			{
				Process process = Process.GetProcessesByName(sporeAppName)[0];
				//NativeMethods.NtSuspendProcess(Process.GetProcessesByName(sporeAppName)[0].Handle);//(System.Management.EventArrivedEventArgs)args)
				//NtSuspendProcess thingy
				NativeMethods.DebugActiveProcess(process.Id);
				_processInfo.dwProcessId = Convert.ToUInt32(process.Id);
				_processInfo.hProcess = process.Handle;
				InjectDLLs(GameInfo.GetExecutableDllSuffix(GameExecutableType.GogOrSteam__March2017));
				NativeMethods.DebugActiveProcessStop(process.Id);
			}
			catch (Exception ex)
			{*/
			//Thread.Sleep(50);


			// OLD Get process and suspend all threads
			Process[] processes = Process.GetProcessesByName(sporeAppName);
			while (processes.Length == 0)
			{
				// Just wait and try again
				//Thread.Sleep(50);
				//continue;
				processes = Process.GetProcessesByName(sporeAppName);
			}

			var process = processes[0];

			// now pause all threads so we can inject; it's the equivalent to running in suspended state
			foreach (ProcessThread pT in process.Threads)
			{
				IntPtr pOpenThread = NativeMethods.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);
				if (pOpenThread != IntPtr.Zero) //((pOpenThread != IntPtr.Zero) && (Injector.WaitForSingleObject(pOpenThread, 0) != WAIT_ABANDONED))
				{
					if (NativeMethods.SuspendThread(pOpenThread) == -1) //     r/therewasanattempt
					{
						ThrowWin32Exception("Thread suspend failed (NOT LOCALIZED)");
					}
					else
						pOpenThreads.Add(pOpenThread);
				}
			}
			_processInfo.dwProcessId = (uint)process.Id;
			_processInfo.hProcess = process.Handle; //NativeMethods.OpenProcess(NativeMethods.AccessRequired, false, ProcessInfo.dwProcessId);
													// We must detect the executable type now
			_processHandle = _processInfo.hProcess;
			/*this.ProcessExecutableType();
			if (this.ExecutableType == GameVersionType.None)
			{
				// don't execute the game if the user closed the dialog
				return;
			}
			// get the corerct executable path
			if (LauncherSettings.ForcedGalacticAdventuresSporeAppPath == null)
				this.ExecutablePath = process.MainModule.FileName;

			this.ProcessExecutableType();*

			//GameVersion.VersionNames[(int)(GameVersionType.Steam_Patched)]; //string dllEnding = GameVersion.VersionNames[(int)this.ExecutableType];
			*/
			InjectDLLs(GameInfo.GetExecutableDllSuffix(GameExecutableType.GogOrSteam__March2017));

			foreach (IntPtr pOpenThread in pOpenThreads)
			{
				if (pOpenThread == IntPtr.Zero)
				{
					continue;
				}

				/*uint suspendCount = 0;
				do
				{
					suspendCount = */
				if (NativeMethods.ResumeThread(pOpenThread) == -1)
				{
					ThrowWin32Exception("Thread resume failed (NOT LOCALIZED)");
				}
				//} while (suspendCount > 0);

				NativeMethods.CloseHandle(pOpenThread);

			}

			return;
			//}
			//}
			//ModInstallation.InvokeErrorOccurred(new ErrorEventArgs(new Exception("EXPERIMENTAL INJECTION FAILURE")));
			//}

			//};   
		}

		static NativeMethods.MonitorInfoEx _monitor = new NativeMethods.MonitorInfoEx();
		static bool _monitorSelected = false;
		static string GetGameCommandLineOptions()
		{
			var sb = new StringBuilder();
			if ((!Environment.GetCommandLineArgs().Contains(UpdaterService.IgnoreUpdatesArg)) && (Environment.GetCommandLineArgs().Length > 1) && (Environment.GetCommandLineArgs()[1] == Settings.LaunchSporeWithoutManagerOptions))
			{
				int i = 0;
				foreach (string arg in Environment.GetCommandLineArgs())
				{
					if ((i != 0) && (arg.ToLowerInvariant() != Settings.LaunchSporeWithoutManagerOptions.ToLowerInvariant()))
					{
						sb.Append(arg);
						sb.Append(" ");
					}
					i++;
				}
			}
			else
			{
				if (Settings.ForceGameWindowingMode)
				{
					if (Settings.ForceWindowedMode == 1)
						sb.Append("-f");
					else// if (Settings.ForceWindowedMode == 0)
						sb.Append("-w");

					sb.Append(" ");

					string size = "-r:";

					var monitors = NativeMethods.AllMonitors;
					_monitor = NativeMethods.AllMonitors[0];
					if (Settings.ForceWindowedMode == 2)
					{
						string prM = Settings.PreferredBorderlessMonitor;
						if (!prM.IsNullOrEmptyOrWhiteSpace())
						{
							string[] prefMon = prM.Replace(" ", string.Empty).Split(',');
							if (prefMon.Length == 4)
							{
								if (
										int.TryParse(prefMon[0], out int left) &&
										int.TryParse(prefMon[1], out int top) &&
										int.TryParse(prefMon[2], out int right) &&
										int.TryParse(prefMon[3], out int bottom)
									)
								{

									foreach (var monInfo in monitors)
									{
										var bounds = monInfo.rcMonitor;
										if (
												(bounds.Left == left) &&
												(bounds.Top == top) &&
												(bounds.Right == right) &&
												(bounds.Bottom == bottom)
											)
										{
											_monitor = monInfo;
											_monitorSelected = true;
											//MessageDisplay.ShowMessageBox(prM, monitors.IndexOf(_monitor).ToString());
											break;
										}
									}

									if (!_monitorSelected)
                                    {
										int width = right - left;
										int height = bottom - top;
										foreach (var monInfo in monitors)
										{
											var bounds = monInfo.rcMonitor;
											if (
												((bounds.Right - bounds.Left) == width) &&
												((bounds.Bottom - bounds.Top) == height)
												)
											{
												_monitor = monInfo;
												_monitorSelected = true;

												break;
											}
										}

									}
								}
							}
						}
					}

					if (Settings.ForceWindowedMode == 2)
					{
						size += (_monitor.rcMonitor.Right - _monitor.rcMonitor.Left).ToString() + "x" + (_monitor.rcMonitor.Bottom - _monitor.rcMonitor.Top).ToString();
						//MessageDisplay.ShowMessageBox("_monitorSelected: " + _monitorSelected.ToString() + ", size: " + size);
					}
					else if (Settings.ForceGameWindowBounds)
					{
						//MessageDisplay.DebugShowMessageBox("MONITOR: " + monitor.rcMonitor.Left + ", " + monitor.rcMonitor.Top + ", " + monitor.rcMonitor.Right + ", " + monitor.rcMonitor.Bottom + "\n" + +monitor.rcWork.Left + ", " + monitor.rcWork.Top + ", " + monitor.rcWork.Right + ", " + monitor.rcWork.Bottom);

						if (Settings.AutoGameWindowBounds)
						{
							MessageDisplay.DebugShowMessageBox("Settings.AutoGameWindowBounds is true");
							if (Settings.ForceGameWindowingMode)
							{
								MessageDisplay.DebugShowMessageBox("Settings.ForceGameWindowingMode is true, Settings.ForceWindowedMode is " + Settings.ForceWindowedMode);
								if (Settings.ForceWindowedMode == 0)
								{
									size += (_monitor.rcWork.Right - _monitor.rcWork.Left);
								}
								else// if (Settings.ForceWindowedMode == 1)
									size += (_monitor.rcMonitor.Right - _monitor.rcMonitor.Left);
								/*else
									size += Settings.ForcedGameWindowWidth;*/
							}
							else
								size += Settings.ForcedGameWindowWidth;
						}
						else
							size += Settings.ForcedGameWindowWidth;

						size += "x";

						if (Settings.AutoGameWindowBounds)
						{
							if (Settings.ForceGameWindowingMode)
							{
								int maximizedTitlebarHeight = CaptionHeight;
								if (Settings.ForceWindowedMode == 0)
									size += ((_monitor.rcWork.Bottom - _monitor.rcWork.Top) - maximizedTitlebarHeight);
								else if (Settings.ForceWindowedMode == 1)
									size += ((_monitor.rcMonitor.Bottom - _monitor.rcMonitor.Top) - maximizedTitlebarHeight);
								else
									size += Settings.ForcedGameWindowHeight;
							}
							else
								size += Settings.ForcedGameWindowHeight;
						}
						else
							size += Settings.ForcedGameWindowHeight;
					}
					else
						MessageDisplay.DebugShowMessageBox("Settings.ForceGameWindowBounds is false!");

					sb.Append(size);

					sb.Append(" ");
				}

				if (Settings.ForceGameLocale && (!string.IsNullOrWhiteSpace(Settings.ForcedGameLocale)))
				{
					string option = "-locale:";
					if (Settings.ForcedGameLocale.StartsWith("-"))
						option += Settings.ForcedGameLocale.Substring(1);
					else
						option += Settings.ForcedGameLocale;

					sb.Append(option);
					sb.Append(" ");
				}

				if (Settings.UseCustomGameState && (!string.IsNullOrWhiteSpace(Settings.GameState)))
				{
					sb.Append("-state:" + Settings.GameState);
					sb.Append(" ");
				}

				if (!string.IsNullOrWhiteSpace(Settings.CommandLineOptions))
				{
					string[] additionalOptions;
					if (Settings.CommandLineOptions.Contains(" "))
						additionalOptions = Settings.CommandLineOptions.Split(" ".ToCharArray());
					else
						additionalOptions = new string[] { Settings.CommandLineOptions };

					foreach (string arg in additionalOptions)
					{
						sb.Append(arg);
						sb.Append(" ");
					}
				}
			}
			return sb.ToString();
		}


		static void CreateSporeProcess()
		{
			var sb = new StringBuilder();
			if ((!Environment.GetCommandLineArgs().Contains(UpdaterService.IgnoreUpdatesArg)) && (Environment.GetCommandLineArgs().Length > 1) && (Environment.GetCommandLineArgs()[1] == Settings.LaunchSporeWithoutManagerOptions))
			{
				int i = 0;
				foreach (string arg in Environment.GetCommandLineArgs())
				{
					if ((i != 0) && (arg.ToLowerInvariant() != Settings.LaunchSporeWithoutManagerOptions.ToLowerInvariant()))
					{
						sb.Append(arg);
						sb.Append(" ");
					}
					i++;
				}
			}
			else
            {
				sb.Append(GetGameCommandLineOptions());
			}

			/*if (false)
			{
				MessageDisplay.ShowMessageBox("ARGS", GetGameCommandLineOptions());
				//2
				if (Settings.ForceGameWindowingMode)
				{
					if (Settings.ForceWindowedMode == 1)
						sb.Append("-f");
					else// if (Settings.ForceWindowedMode == 0)
						sb.Append("-w");

					sb.Append(" ");

					string size = "-r:";

					if (Settings.ForceWindowedMode == 2)
						size += (_monitor.rcMonitor.Right - _monitor.rcMonitor.Left).ToString() + "x" + (_monitor.rcMonitor.Bottom - _monitor.rcMonitor.Top).ToString();
					else if (Settings.ForceGameWindowBounds)
					{
						//MessageDisplay.DebugShowMessageBox("MONITOR: " + monitor.rcMonitor.Left + ", " + monitor.rcMonitor.Top + ", " + monitor.rcMonitor.Right + ", " + monitor.rcMonitor.Bottom + "\n" + +monitor.rcWork.Left + ", " + monitor.rcWork.Top + ", " + monitor.rcWork.Right + ", " + monitor.rcWork.Bottom);

						if (Settings.AutoGameWindowBounds)
						{
							MessageDisplay.DebugShowMessageBox("Settings.AutoGameWindowBounds is true");
							if (Settings.ForceGameWindowingMode)
							{
								MessageDisplay.DebugShowMessageBox("Settings.ForceGameWindowingMode is true, Settings.ForceWindowedMode is " + Settings.ForceWindowedMode);
								if (Settings.ForceWindowedMode == 0)
								{
									size += (_monitor.rcWork.Right - _monitor.rcWork.Left);
								}
								else// if (Settings.ForceWindowedMode == 1)
									size += (_monitor.rcMonitor.Right - _monitor.rcMonitor.Left);
								/*else
									size += Settings.ForcedGameWindowWidth;*
							}
							else
								size += Settings.ForcedGameWindowWidth;
						}
						else
							size += Settings.ForcedGameWindowWidth;

						size += "x";

						if (Settings.AutoGameWindowBounds)
						{
							if (Settings.ForceGameWindowingMode)
							{
								int maximizedTitlebarHeight = CaptionHeight;
								if (Settings.ForceWindowedMode == 0)
									size += ((_monitor.rcWork.Bottom - _monitor.rcWork.Top) - maximizedTitlebarHeight);
								else if (Settings.ForceWindowedMode == 1)
									size += ((_monitor.rcMonitor.Bottom - _monitor.rcMonitor.Top) - maximizedTitlebarHeight);
								else
									size += Settings.ForcedGameWindowHeight;
							}
							else
								size += Settings.ForcedGameWindowHeight;
						}
						else
							size += Settings.ForcedGameWindowHeight;
					}
					else
						MessageDisplay.DebugShowMessageBox("Settings.ForceGameWindowBounds is false!");

					sb.Append(size);

					sb.Append(" ");
				}

				if (Settings.ForceGameLocale && (!string.IsNullOrWhiteSpace(Settings.ForcedGameLocale)))
				{
					string option = "-locale:";
					if (Settings.ForcedGameLocale.StartsWith("-"))
						option += Settings.ForcedGameLocale.Substring(1);
					else
						option += Settings.ForcedGameLocale;

					sb.Append(option);
					sb.Append(" ");
				}

				if (Settings.UseCustomGameState && (!string.IsNullOrWhiteSpace(Settings.GameState)))
				{
					sb.Append("-state:" + Settings.GameState);
					sb.Append(" ");
				}

				if (!string.IsNullOrWhiteSpace(Settings.CommandLineOptions))
				{
					string[] additionalOptions;
					if (Settings.CommandLineOptions.Contains(" "))
						additionalOptions = Settings.CommandLineOptions.Split(" ".ToCharArray());
					else
						additionalOptions = new string[] { Settings.CommandLineOptions };

					foreach (string arg in additionalOptions)
					{
						sb.Append(arg);
						sb.Append(" ");
					}
				}
			}*/
			/*string currentSporebinPath = string.Empty;
			if (LauncherSettings.ForcedSporebinEP1Path != null)
				currentSporebinPath = LauncherSettings.ForcedSporebinEP1Path;
			else
				currentSporebinPath = this.SporebinPath;*/

			MessageDisplay.DebugShowMessageBox("SporebinPath: " + GameInfo.SporebinEP1 + "\nExecutablePath: " + _executablePath + "\nCommand Line Options: " + GetGameCommandLineOptions());

			if (!NativeMethods.CreateProcess(null, "\"" + _executablePath + "\" " + sb.ToString(), IntPtr.Zero, IntPtr.Zero, false, ProcessCreationFlags.CREATE_SUSPENDED, IntPtr.Zero, GameInfo.SporebinEP1, ref _startupInfo, out _processInfo))
			{
				//throw new InjectException(Strings.ProcessNotStarted);
				int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
				MessageDisplay.ShowMessageBox(GetLocalizedString("LauncherError!Process!Create") + lastError.ToString()); //Strings.ProcessNotStarted);
				throw new System.ComponentModel.Win32Exception(lastError);
			}
		}

		static void ResumeSporeProcess()
		{
			if (NativeMethods.ResumeThread(_processInfo.hThread) != 1)
			{
				/*throw new InjectException(Strings.ProcessNotResumed);*/
				ThrowWin32Exception(GetLocalizedString("LauncherError!Process!Resume")); //ThrowWin32Exception(Strings.ProcessNotResumed);
			}
		}

		static void EnableBorderless(int pid, NativeMethods.MonitorInfoEx monitor)
		{
			if (Settings.ForceWindowedMode == 2)
			{
				var monBounds = _monitor.rcMonitor;
				MessageDisplay.ShowMessageBox("_monitorSelected: " + _monitorSelected.ToString() + "\n_monitor.rcMonitor: " + $"{monBounds.Left},{monBounds.Top},{monBounds.Right},{monBounds.Bottom},,,{monBounds.Right - monBounds.Left},{monBounds.Bottom - monBounds.Top}");
				Process process = Process.GetProcessById(pid);
				Debug.WriteLine("Before loop");

				IntPtr spore = IntPtr.Zero;
				while (!NativeMethods.IsWindow(spore)) //(spore == IntPtr.Zero) ||  //(!process.HasExited) && ((process.MainWindowHandle == IntPtr.Zero) || (!NativeMethods.IsWindow(process.MainWindowHandle))))
				{
					if (process.HasExited)
						break;

					spore = GetSporeMainWindow(pid);
				}
				Debug.WriteLine("After loop");
				if (NativeMethods.IsWindow(spore)) //(!process.HasExited) && (process.MainWindowHandle != IntPtr.Zero) && NativeMethods.IsWindow(process.MainWindowHandle))
				{
					Debug.WriteLine("process.MainWindowTitle: " + process.MainWindowTitle);
					//var monitor = NativeMethods.AllMonitors[0];
					//NativeMethods.SetWindowLong(win.Handle, NativeMethods.GwlExstyle, NativeMethods.GetWindowLong(win.Handle, NativeMethods.GwlExstyle).ToInt32() & ~NativeMethods.WsExNoActivate);
					int winStyle = (Int32)NativeMethods.GetWindowLong(spore, NativeMethods.GwlStyle);
					winStyle &= ~NativeMethods.WsBorder;
					winStyle &= ~NativeMethods.WsCaption;
					NativeMethods.SetWindowLong(spore, NativeMethods.GwlStyle, winStyle);

					int x = monBounds.Left;
					int y = monBounds.Top;
					int cx = monBounds.Right - monBounds.Left;
					int cy = monBounds.Bottom - monBounds.Top;
					uint uFlags = 0x0010 | 0x0004 | 0x0020;

					/*KWin or something on Kubuntu decides that having Spore fill one monitor means it was 
					 * actually supposed to fill the other, and unhelpfully "fixes" it for us, so now we
					 * have to "break" this "feature" in order to prevent that "fix" from "fixing" our
					 * "mistake" that was actually the intended behaviour by intentionally being a pixel
					 * or two off. Fun. Note to self: Figure out how */
					if (false && Settings.NonEssentialIsRunningUnderWine)
					{
						cx--;
						/*var furthestRight = monitor;
						foreach (var mon in NativeMethods.AllMonitors)
						{
							if (mon.rcMonitor.Right > furthestRight.rcMonitor.Right)
								furthestRight = mon;
						}

						if (furthestRight.Equals(monitor))
						{
							x++;
							cx--;
						}
						else
						{
							var furthestLeft = monitor;
							foreach (var mon in NativeMethods.AllMonitors)
							{
								if (mon.rcMonitor.Left > furthestLeft.rcMonitor.Left)
									furthestLeft = mon;
							}

							if (furthestLeft.Equals(monitor))
							{
								cx--;
							}
							else
							{
								x++;
								cx -= 2;
							}
						}*/
					}

					NativeMethods.SetWindowPos(spore, IntPtr.Zero, x, y, cx, cy, uFlags);
				}
			}
		}

		public static Func<int, IntPtr> GetSporeMainWindow = null;

		/*string ProcessSporebinPath()
		{
			string path = PathDialogs.ProcessGalacticAdventures();

			if (path == null || !Directory.Exists(path))
			{

				throw new InjectException(CommonStrings.GalacticAdventuresNotFound);
			}

			this.SporebinPath = path;

			return path;
		}

		string ProcessSteamPath()
		{
			string path = PathDialogs.ProcessSteam();

			if (path == null || !Directory.Exists(path))
			{

				throw new InjectException(CommonStrings.SteamNotFound);
			}

			this.SteamPath = path;

			return path;
		}

		GameVersionType ProcessExecutableType()
		{
			GameVersionType executableType = GameVersion.DetectVersion(this.ExecutablePath);

			// for debugging purposes
			//executableType = GameVersionType.None;

			if (executableType == GameVersionType.None)
			{
				if (LauncherSettings.GameVersion != GameVersionType.None)
				{
					executableType = LauncherSettings.GameVersion;
				}
				else
				{
					executableType = ShowVersionSelectorDialog();

					// The detection should work fine unless you have the wrong version, so tell the user                        
					MessageBox.Show(Strings.MightNotWork, Strings.MightNotWorkTitle,
						MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

				}
			}

			this.ExecutableType = executableType;

			return executableType;
		}
		*/

		public static void ThrowWin32Exception(string info)
		{
			ThrowWin32Exception(info, System.Runtime.InteropServices.Marshal.GetLastWin32Error());
		}

		public static void ThrowWin32Exception(string info, int error)
		{
			if ((error != 0) && (error != 18))
			{
				var exception = new System.ComponentModel.Win32Exception(error);
				//System.Windows.Forms.MessageBox.Show("Error: " + error.ToString() + "\n" + info);
				MessageDisplay.RaiseError(new ErrorEventArgs(exception));
				throw exception;
			}
		}

		public static bool IsInstalledDarkInjectionCompatible()
		{
			string di230 = "di_9_r_beta2-3-0".ToLowerInvariant();
			try
			{
				bool returnValue = true;
				//InstalledMods mods = new InstalledMods();
				//mods.Load();
				MessageDisplay.DebugShowMessageBox("BEGIN MOD CONFIGURATION NAMES");
				foreach (IInstalledMod configuration in ModsManager.InstalledMods)
				{
					MessageDisplay.DebugShowMessageBox(configuration.RealName);
					if (configuration.Unique.ToLowerInvariant().Contains(di230))
					{
						returnValue = false;
						break;
					}
				}
				MessageDisplay.DebugShowMessageBox("END MOD CONFIGURATION NAMES");
				if (!returnValue)
				{
					//TODO: Show error message
					SporeLauncher.OpenDarkInjectionPage();
				}
				return returnValue;
			}
			catch (Exception ex)
			{
				MessageDisplay.DebugShowMessageBox("FAILED TO INSPECT MOD CONFIGURATION NAMES\n" + ex);
				return true;
			}
		}

		public static GameExecutableType GetExecutableType()
		{
			if (Settings.ForcedGameExeType.IsNullOrEmptyOrWhiteSpace())
				return ExecutableFileGameTypes[new FileInfo(_executablePath).Length];
			else if (Enum.TryParse(Settings.ForcedGameExeType, out GameExecutableType exeType))
				return exeType;
			else
				throw new InvalidOperationException("No Spore executable type available. Last-minute UI to specify NYI.");
		}

		public static bool IsValidExe()
		{
			//string errorBase = GetLocalizedString("LauncherError!GameVersion!NotRecognized");
			if (!(Settings.ForcedGameExeType.IsNullOrEmptyOrWhiteSpace()))
				return true;
			else if (TryGetExeVersion(_executablePath, out Version exeVersion))
			{
				if ((exeVersion < Spore__March2017) && (exeVersion != Spore__1_5_1))
				{
					MessageDisplay.RaiseError(new ErrorEventArgs(new InvalidOperationException(GetLocalizedString("LauncherError!GameVersion!TooOld"))));
					return false;
				}
				else if (exeVersion > Spore__March2017)
				{
					MessageDisplay.RaiseError(new ErrorEventArgs(new InvalidOperationException(GetLocalizedString("LauncherError!GameVersion!WaitDidTheyActuallyUpdateSpore"))));
					return false;
				}
				else
					return true;
			}
			else
			{
				MessageDisplay.RaiseError(new ErrorEventArgs(new InvalidOperationException(GetLocalizedString("LauncherError!GameVersion!ReadFailed"))));
				return false;
			}
		}

		public static bool TryGetExeVersion(string executablePath, out Version exeVersion)
		{
			bool retVal = Version.TryParse(FileVersionInfo.GetVersionInfo(executablePath).FileVersion, out exeVersion);
			return retVal;
		}

		static List<Process> GetSporeProcesses()
		{
			List<Process> processes = Process.GetProcessesByName("SporeApp").ToList();
			processes.AddRange(Process.GetProcessesByName("SporeApp_ModAPIFix"));
			return processes;
		}

		public static bool IsSporeRunning()
		{
			return GetSporeProcesses().Count() > 0;
		}

		public static void KillSporeProcesses()
		{
			try
			{
				List<Process> sporeProcesses = GetSporeProcesses();

				for (int i = 0; i < sporeProcesses.Count; i++)
					sporeProcesses[0].Kill();
			}
			catch (Exception ex)
			{
				MessageDisplay.ShowMessageBox(GetLocalizedString("KillSporeError") + "\n\n" + ex.GetType() + ": " + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
			}
		}	
	}
}