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

namespace SporeMods.Manager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		//public static Process DragServantProcess = null;
		
		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			DispatcherUnhandledException += App_DispatcherUnhandledException;
		}

		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			App_Exit(this, null);
			CUIMsg.ShowException(e.Exception);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			App_Exit(this, null);
			if (e.ExceptionObject is Exception exc)
				CUIMsg.ShowException(exc);
		}

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

			Settings.EnsureDllsAreExtracted();
			CommonUI.Updater.CheckForUpdates();
			Settings.ManagerInstallLocationPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();

			Exit += App_Exit;

			if (CommonUI.VersionValidation.IsConfigVersionCompatible(true, out Version previousModMgrVersion))
			{
				if (ServantCommands.RunLkImporter() == null)
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
						args += $" {ServantCommands.CreateDragServant()}";
						
						while (!ServantCommands.DragWindowHwndSent)
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
						ServantCommands.LocateDragServant();

						bool proceed = true;

						if (!ServantCommands.HasDragServant)
						{
							
						}
						else if (Permissions.IsAtleastWindowsVista() && (!ServantCommands.HasDragServant))
						{
							proceed = false;
							if (Settings.NonEssentialIsRunningUnderWine)
								proceed = true;
							else if (MessageBox.Show(LanguageManager.Instance.GetLocalizedText("DontRunAsAdmin").Replace("%APPNAME%", "Spore Mod Manager"), String.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
								proceed = true;
						}
						else if ((!Permissions.IsAtleastWindowsVista()) && (!ServantCommands.HasDragServant))
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

#if OLD_WINDOWING
							Window window;
							MainView content = new MainView()
							{
								DataContext = new MainViewModel(DragServantProcess)
							};

							if (Settings.UseCustomWindowDecorations)
							{
								window = new SporeMods.CommonUI.Windows.DecoratableWindow()
								{
									Content = content,
									TitlebarHeight = 61,
									//ShowFullscreenButton = true,
									//AutohideTitlebarWhenFullscreen = false
								};
							}
							else
							{
								window = new Window()
								{
									Content = content
								};
							}

							window.MinWidth = 700;
							window.MinHeight = 400;
							window.Width = 800;
							window.Height = 450;
							window.DataContext = content.DataContext;
							MainWindow = window;
							//TODO: Restore this stuff
							/*window.ContentRendered += (sneder, args) => content.MainWindow_OnContentRendered(args);
							window.Activated += content.MainWindow_IsActiveChanged;
							window.Deactivated += content.MainWindow_IsActiveChanged;
							window.SizeChanged += content.MainWindow_SizeChanged;
							window.PreviewKeyDown += content.MainWindow_PreviewKeyDown;
							window.Closing += content.MainWindow_Closing;*/
							window.Show();
#endif
							//Resources.MergedDictionaries[0].MergedDictionaries[1] = SporeMods.CommonUI.Themes.Shale.ShaleAccents.Sky.Dictionary;
							//Resources.MergedDictionaries.Add(SporeMods.CommonUI.Themes.Shale.ShaleAccents.Sky);
							if (Settings.ShaleDarkTheme)
								Resources.MergedDictionaries[1].Source = new Uri(@"pack://application:,,,/SporeMods.CommonUI;component/Themes/Shale/Smm/SmmDark.xaml", UriKind.RelativeOrAbsolute);

							
							FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
							{
								DefaultValue = Application.Current.FindResource(typeof(Window))
							});

							if (ServantCommands.TryGetDragServantHwnd(out IntPtr dragWindow))
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

		private void App_Exit(object sender, ExitEventArgs e)
		{
			if (ServantCommands.HasDragServant)
				ServantCommands.CloseDragServant();
		}
	}
}
