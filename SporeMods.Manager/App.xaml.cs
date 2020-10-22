using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Net;
using System.Windows.Media.Animation;
using System.Threading;
using System.ComponentModel;

namespace SporeMods.Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Process DragServantProcess = null;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            App_Exit(this, null);
            CommonUI.MessageDisplay.ShowException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            App_Exit(this, null);
            if (e.ExceptionObject is Exception exc)
                CommonUI.MessageDisplay.ShowException(exc);
        }

        public static readonly string DragServantIdArg = "-dragServantId:";
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

            CommonUI.Updater.CheckForUpdates();

            Exit += App_Exit;

            if (CommonUI.VersionValidation.IsConfigVersionCompatible(true, out Version previousModMgrVersion))
            {
                if (Permissions.IsAtleastWindowsVista() && (!Permissions.IsAdministrator()))
                {
                    string parentDirectoryPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
                    /*foreach (Process proc in Process.GetProcessesByName("SporeMods.DragServant").ToList())
                        proc.Kill();*/

                    Process p = Process.Start(Path.Combine(parentDirectoryPath, "SporeMods.DragServant.exe"), Process.GetCurrentProcess().Id.ToString());
                    string args = Permissions.GetProcessCommandLineArgs();
                    args += " " + DragServantIdArg + p.Id;
                    if (!Environment.GetCommandLineArgs().Contains(CommonUI.Updater.IgnoreUpdatesArg)) args += " " + CommonUI.Updater.IgnoreUpdatesArg;
                    try
                    {
                        while (p.MainWindowHandle == IntPtr.Zero) { }

                        Permissions.RerunAsAdministrator(args);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.GetType().FullName + "\n\n" + ex.Message + "\n\n" + ex.StackTrace, "Fatal error");
                        foreach (Process proc in Process.GetProcessesByName("SporeMods.DragServant").ToList())
                            proc.Kill();
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

                    if (Permissions.IsAtleastWindowsVista() && (DragServantProcess == null))
                    {
                        proceed = false;
                        if (MessageBox.Show(Settings.GetLanguageString(1, "DontRunAsAdmin").Replace("%APPNAME%", "Spore Mod Manager"), String.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            proceed = true;
                    }
                    else if ((!Permissions.IsAtleastWindowsVista()) && (DragServantProcess == null))
                    {
                        DragServantProcess = Process.Start(Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString(), "SporeMods.DragServant.exe"));
                    }

                    if (proceed)
                    {
                        base.OnStartup(e);

                        //ModInstallation.DoFirstRunVerification();
                        Settings.ManagerInstallLocationPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();

                        Window window;
                        ManagerContent content = new ManagerContent();
                        if (Settings.UseCustomWindowDecorations)
                        {
                            window = new Mechanism.Wpf.Core.Windows.DecoratableWindow()
                            {
                                Content = content,
                                TitlebarHeight = 61
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

        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (DragServantProcess != null)
                DragServantProcess.Kill();
        }
    }
}
