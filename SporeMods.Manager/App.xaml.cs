using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
using CUIMsg = SporeMods.CommonUI.MessageDisplay;
using CoreMsg = SporeMods.Core.MessageDisplay;
using SporeMods.Views;
using SporeMods.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace SporeMods.Manager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : SmmApp
	{
		protected override bool ShouldRerunAsAdministrator()
			=> true;
		protected override bool ShouldEnsureUACPartner()
			=> true;


		protected override void FinishStartup(bool isAdmin)
		{
			if (isAdmin)
			{
				//TODO: Implement this stuff correctly
				Core.ModTransactions.Operations.ValidateModOp.InstallingExperimentalMod += s => true;
				Core.ModTransactions.Operations.ValidateModOp.InstallingRequiresGalaxyResetMod += s => true;
				Core.ModTransactions.Operations.ValidateModOp.InstallingSaveDataDependencyMod += s => true;
				Core.ModTransactions.ModTransactionManager.UninstallingSaveDataDependencyMod += m => true;

				MainWindow = new MainView();
				MainWindow.Show();
			}
			else
				UACPartnerCommands.WatchForPartnerSignals = true;
		}

		

#if NO
		protected override void OnStartup(StartupEventArgs e)
		{
			CUIMsg.EnsureConsole();

			if (Settings.ForceSoftwareRendering)
				RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

			Externals.SpecifyFuncCommandType(typeof(FuncCommand<>));
			/*CrossProcess.CREATE_FUNC_COMMAND = (t, e, ce) =>
			{
				
				//return new FuncCommand<typeof(t)>((Action<typeof(t)>)e, (Predicate<typeof(t)>)ce);
			};*/

			CoreMsg.ErrorOccurred += (sender, args) =>
			{
				App_Exit(this, null);
				CUIMsg.ShowException(args.Exception);
			};
			CoreMsg.MessageBoxShown += (sneder, args) => Dispatcher.BeginInvoke(new Action(() => CUIMsg.ShowMessageBox(args.Content, args.Title)));
			CoreMsg.DebugMessageSent += (sneder, args) => Dispatcher.BeginInvoke(new Action(() => CUIMsg.ShowMessageBox(args.Content, args.Title)));

			//TODO: Implement this stuff correctly
			Core.ModTransactions.Operations.ValidateModOp.InstallingExperimentalMod += s => true;
			Core.ModTransactions.Operations.ValidateModOp.InstallingRequiresGalaxyResetMod += s => true;
			Core.ModTransactions.Operations.ValidateModOp.InstallingSaveDataDependencyMod += s => true;
			Core.ModTransactions.ModTransactionManager.UninstallingSaveDataDependencyMod += m => true;

			Settings.EnsureDllsAreExtracted();
			CommonUI.Updater.CheckForUpdates();
			Settings.ManagerInstallLocationPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();

			Exit += App_Exit;

			if (CommonUI.VersionValidation.IsConfigVersionCompatible(true, out Version previousModMgrVersion))
			{
				if (CrossProcessCommands.RunLkImporter() == null)
				{
					//Process dragServant = null;

					if (Permissions.IsAtleastWindowsVista() && (!Permissions.IsAdministrator()))
					{
						if (CrossProcess.AreAnyOtherModManagersRunning)
						{
							if (Environment.GetCommandLineArgs().Length > 1)
							{
								List<string> files = new List<string>();
								foreach (string s in Environment.GetCommandLineArgs().Skip(1))
								{
									string path = s.Trim('\"', ' ');
									if (File.Exists(path))
										files.Add(path);
								}
								string draggedFilesPath = Path.Combine(Settings.TempFolderPath, "draggedFiles");
								File.WriteAllLines(draggedFilesPath, files);
								Permissions.GrantAccessFile(draggedFilesPath);
							}
							else
								MessageBox.Show(LanguageManager.Instance.GetLocalizedText("OneInstanceOnly"));
							Process.GetCurrentProcess().Kill();
						}

						/*foreach (Process proc in Process.GetProcessesByName("SporeMods.DragServant").ToList())
							proc.Kill();*/
						string parentDirectoryPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
						/*var dragServantStartInfo = new ProcessStartInfo(Path.Combine(parentDirectoryPath, "SporeMods.DragServant.exe"), Process.GetCurrentProcess().Id.ToString())
						{
							UseShellExecute = true
						};
						//Permissions.ForwardDotnetEnvironmentVariables(ref dragServantStartInfo);
						Process p = Process.Start(dragServantStartInfo);*/
						string args = Permissions.GetProcessCommandLineArgs();

						var dragServant = CrossProcessCommands.CreateDragServant();
						args += $" {dragServant}";
						
						while (!CrossProcessCommands.DragWindowHwndSent)
						{ }

						if (!Environment.GetCommandLineArgs().Contains(UpdaterService.IgnoreUpdatesArg))
							args += " " + UpdaterService.IgnoreUpdatesArg;

						try
						{
							//while (p.MainWindowHandle == IntPtr.Zero) { }

							//Permissions.RerunAsAdministrator(args);
							CrossProcess.RestartModManagerAsAdministrator(args);
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.GetType().FullName + "\n\n" + ex.Message + "\n\n" + ex.StackTrace, "Fatal error");
							/*foreach (Process proc in Process.GetProcessesByName("SporeMods.DragServant").ToList())
								proc.Kill();*/
							Application.Current.Shutdown();
						}
					}
					else// if (Permissions.IsAdministrator())
					{
						CrossProcessCommands.LocateDragServant();

						bool proceed = true;

						if (!CrossProcessCommands.HasDragServant)
						{
							
						}
						else if (Permissions.IsAtleastWindowsVista() && (!CrossProcessCommands.HasDragServant))
						{
							proceed = false;
							if (Settings.NonEssentialIsRunningUnderWine)
								proceed = true;
							else if (MessageBox.Show(LanguageManager.Instance.GetLocalizedText("DontRunAsAdmin").Replace("%APPNAME%", "Spore Mod Manager"), String.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
								proceed = true;
						}
						else if ((!Permissions.IsAtleastWindowsVista()) && (!CrossProcessCommands.HasDragServant))
						{
							/*var dragServantStartInfo = new ProcessStartInfo(Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString(), "SporeMods.DragServant.exe"))
							{
								UseShellExecute = true
							};
							//Permissions.ForwardDotnetEnvironmentVariables(ref dragServantStartInfo);
							DragServantProcess = Process.Start(dragServantStartInfo);*/
							//dragServant = CrossProcess.StartDragServant();
						}

						if (proceed)
						{
							base.OnStartup(e);

							//ModInstallation.DoFirstRunVerification();
							VersionValidation.WarnIfMissingOriginPrerequisites(Path.Combine(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName, "Launch Spore.dll"));
							var _ = LanguageManager.Instance;

							//Resources.MergedDictionaries[0].MergedDictionaries[1] = SporeMods.CommonUI.Themes.Shale.ShaleAccents.Sky.Dictionary;
							//Resources.MergedDictionaries.Add(SporeMods.CommonUI.Themes.Shale.ShaleAccents.Sky);
							SporeMods.CommonUI.Themes.Shale.ShaleHelper.FlipLightSwitch(!Settings.ShaleDarkTheme);
							/*if (Settings.ShaleDarkTheme)
								Resources.MergedDictionaries[1].Source = new Uri(@"pack://application:,,,/SporeMods.CommonUI;component/Themes/Shale/Smm/SmmDark.xaml", UriKind.RelativeOrAbsolute);*/

							
							FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
							{
								DefaultValue = Application.Current.FindResource(typeof(Window))
							});

							if (CrossProcessCommands.TryGetDragServantHwnd(out IntPtr dragWindow))
								NativeMethods.ShowWindow(dragWindow, 0);
                            
							MainWindow = new MainView();
							MainWindow.Show();
						}
						else
						{
							Application.Current.Shutdown();
						}
					}
				}
			}
		}
#endif
	}
}
