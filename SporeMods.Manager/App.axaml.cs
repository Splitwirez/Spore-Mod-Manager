using SporeMods.Core;
using SporeMods.CommonUI.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using SporeMods.CommonUI;
using SporeMods.Manager.Views;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;

namespace SporeMods.Manager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static Window MainWindow => (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;
        public override void Initialize()
        {
			//AppDomain.CurrentDomain.FirstChanceException += (s, e) => MessageDisplay.Error(e.Exception);

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            DrunkFontManagerImpl.ServeDrinks();
			OnStartUp.EnsureAllGood(true, () =>
			{
				//Windowing.ApplySmmWindowingPlatform();

				AddStyles(new Uri(@"avares://SporeMods.CommonUI/Styles/Shale/Shale.axaml"));


				DoLights();
				Settings.Instance.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == nameof(Settings.Instance.ShaleDarkTheme))
						DoLights();
				};


				if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				{

					desktop.Exit += App_Exit;

					

					desktop.MainWindow = new ModManagerView();

					/*desktop.MainWindow = new Window
					{
						/*ExtendClientAreaToDecorationsHint = true,
            			ExtendClientAreaTitleBarHeightHint = -1,
						ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome | Avalonia.Platform.ExtendClientAreaChromeHints.OSXThickTitleBar,
						Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),* /
						TransparencyLevelHint = WindowTransparencyLevel.Transparent,
						Content = new ManagerContentView()
					};*/
				}

				base.OnFrameworkInitializationCompleted();
			});
		}

		StyleInclude _prevInclude = null;
		void DoLights()
		{
			string lights = (Settings.Instance.ShaleDarkTheme) ? "Dark" : "Light";
			Uri uri = new Uri(@$"avares://SporeMods.CommonUI/Styles/Shale/Shale{lights}.axaml");
			
			//int index;
			StyleInclude include;
			if ((_prevInclude != null) ? (uri != _prevInclude.Source) : false)
			{
				int index = Styles.IndexOf(_prevInclude);
				Styles.RemoveAt(index);
				include = InsertStyles(uri, index);
			}
			else
				include = AddStyles(uri);
			
			_prevInclude = include;
		}


		
		protected StyleInclude AddStyles(Uri srcUri) =>
			InsertStyles(srcUri, Styles.Count);
		protected StyleInclude InsertStyles(Uri srcUri, int index)
		{
				StyleInclude include = new StyleInclude(new Uri(@"avares://" + Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().GetExecutablePath())))
				{
					Source = srcUri
				};

				Styles.Add(include);
				return include;
		}



		public static Process DragServantProcess = null;

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			App_Exit(this, null);
			if (e.ExceptionObject is Exception exc)
				MessageDisplay.Error(exc);
		}

		public static readonly string DragServantIdArg = "-dragServantId:";
#if NO
		protected override void OnStartup(StartupEventArgs e)
		{
			if (Settings.ForceSoftwareRendering)
				RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

			MessageDisplay.ErrorOccurred += (sender, args) =>
			{
				App_Exit(this, null);
				CommonUI.MessageDisplay.ShowException(args.Exception);
			};
			MessageDisplay.MessageBoxShown += (sneder, args) => Dispatcher.BeginInvoke(new Action(() => CommonUI.MessageDisplay.ShowMessageBox(args.Content, args.Title)));
			MessageDisplay.DebugMessageSent += (sneder, args) => Dispatcher.BeginInvoke(new Action(() => CommonUI.MessageDisplay.ShowMessageBox(args.Content, args.Title)));

			Settings.EnsureDllsAreExtracted();
			CommonUI.Updater.CheckForUpdates();
			Settings.ManagerInstallLocationPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();

			Exit += App_Exit;

			if (CommonUI.VersionValidation.IsConfigVersionCompatible(true, out Version previousModMgrVersion))
			{
				if (ServantCommands.RunLkImporter() == null)
				{
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
						Process p = CrossProcess.StartDragServant(/*Process.GetCurrentProcess().Id.ToString()*/);
						string args = Permissions.GetProcessCommandLineArgs();
						args += " " + DragServantIdArg + p.Id;
						if (!Environment.GetCommandLineArgs().Contains(UpdaterService.IgnoreUpdatesArg)) args += " " + UpdaterService.IgnoreUpdatesArg;
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
						string[] clArgs = Environment.GetCommandLineArgs();
						foreach (string arg in clArgs)
						{
							string targ = arg.Trim(" ".ToCharArray());
							if (targ.StartsWith(DragServantIdArg))
							{
								DragServantProcess = Process.GetProcessById(int.Parse(targ.Replace(DragServantIdArg, string.Empty)));
								break;
							}
						}

						bool proceed = true;

						if (DragServantProcess == null)
						{
							var firstDragServant = Process.GetProcessesByName("SporeMods.DragServant").FirstOrDefault();
							if (firstDragServant != null)
								DragServantProcess = firstDragServant;
						}
						else if (Permissions.IsAtleastWindowsVista() && (DragServantProcess == null))
						{
							proceed = false;
							if (Settings.NonEssentialIsRunningUnderWine)
								proceed = true;
							else if (MessageBox.Show(LanguageManager.Instance.GetLocalizedText("DontRunAsAdmin").Replace("%APPNAME%", "Spore Mod Manager"), String.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
								proceed = true;
						}
						else if ((!Permissions.IsAtleastWindowsVista()) && (DragServantProcess == null))
						{
							/*var dragServantStartInfo = new ProcessStartInfo(Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString(), "SporeMods.DragServant.exe"))
							{
								UseShellExecute = true
							};
							//Permissions.ForwardDotnetEnvironmentVariables(ref dragServantStartInfo);
							DragServantProcess = Process.Start(dragServantStartInfo);*/
							DragServantProcess = CrossProcess.StartDragServant();
						}

						if (proceed)
						{
							base.OnStartup(e);

							//ModInstallation.DoFirstRunVerification();
							CommonUI.VersionValidation.WarnIfMissingOriginPrerequisites(Path.Combine(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName, "Launch Spore.dll"));
							Window window;
							ManagerContent content = new ManagerContent();
							if (Settings.UseCustomWindowDecorations)
							{
								window = new Mechanism.Wpf.Core.Windows.DecoratableWindow()
								{
									Content = content,
									TitlebarHeight = 61,
									ShowFullscreenButton = true,
									AutohideTitlebarWhenFullscreen = false
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
							MainWindow = window;
							window.ContentRendered += (sneder, args) => content.MainWindow_OnContentRendered(args);
							window.Activated += content.MainWindow_IsActiveChanged;
							window.Deactivated += content.MainWindow_IsActiveChanged;
							window.SizeChanged += content.MainWindow_SizeChanged;
							window.PreviewKeyDown += content.MainWindow_PreviewKeyDown;
							window.Closing += content.MainWindow_Closing;
							window.Show();
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
		private void App_Exit(object sender, ControlledApplicationLifetimeExitEventArgs e)
		{
			if ((DragServantProcess != null) && (!DragServantProcess.HasExited))
				DragServantProcess.Kill();
		}
	}
}
