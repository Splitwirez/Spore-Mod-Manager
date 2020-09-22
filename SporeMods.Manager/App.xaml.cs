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
            ShowException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exc)
                ShowException(exc);
        }

        private static readonly string ARG_DRAGSERVANTID = "-dragServantId:";
        private static readonly string ARG_IGNOREUPDATES = "-ignoreUpdates";

        protected override void OnStartup(StartupEventArgs e)
        {
            bool ignoreUpdates = Environment.GetCommandLineArgs().Contains(ARG_IGNOREUPDATES);
            if (!ignoreUpdates)
            {
                CheckForUpdates();
            }

            Exit += App_Exit;

            if (Permissions.IsAtleastWindowsVista() && (!Permissions.IsAdministrator()))
            {
                string parentDirectoryPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
                /*foreach (Process proc in Process.GetProcessesByName("SporeMods.DragServant").ToList())
                    proc.Kill();*/

                Process p = Process.Start(Path.Combine(parentDirectoryPath, "SporeMods.DragServant.exe"), Process.GetCurrentProcess().Id.ToString());
                string args = Permissions.GetProcessCommandLineArgs();
                args += " " + ARG_DRAGSERVANTID + p.Id;
                if (!ignoreUpdates) args += " " + ARG_IGNOREUPDATES;
                try
                {
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
                    if (targ.StartsWith(ARG_DRAGSERVANTID))
                    {
                        DragServantProcess = Process.GetProcessById(int.Parse(targ.Replace(ARG_DRAGSERVANTID, string.Empty)));
                        break;
                    }
                }

                if (Settings.ForceSoftwareRendering)
                    RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

                bool proceed = true;

                if (Permissions.IsAtleastWindowsVista() && (DragServantProcess == null))
                {
                    proceed = false;
                    if (MessageBox.Show(Settings.GetLanguageString(1, "DontRunMgrAsAdmin2"), String.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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

        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (DragServantProcess != null)
                DragServantProcess.Kill();
        }

        private void CheckForUpdates()
        {
            if (Settings.UpdatingMode == Settings.UpdatingModeType.Disabled) return;

            // Necessary to stablish SSL connection with Github API
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                (SecurityProtocolType)768 | (SecurityProtocolType)3072;

            // We will only show one error even if it cannot check the two updates
            WebException webException = null;

            UpdaterService.GithubRelease release = null;
            bool hasProgramUpdate = false;
            try
            {
                hasProgramUpdate = UpdaterService.HasProgramUpdate(out release);
            }
            catch (WebException ex)
            {
                webException = ex;
            }
            catch (Exception ex)
            {
                ShowException(ex);
                return;
            }

            if (hasProgramUpdate)
            {
                bool update = true;
                if (Settings.UpdatingMode == Settings.UpdatingModeType.AutoCheck)
                {
                    update = MessageBox.Show("An update to the ModAPI DLLs is available. It includes new features and bugfixes, and is required to run modern mods. Do you want to download it?",
                        "ModAPI DLLs Update Available", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                }

                if (update)
                {
                    string updaterPath = null;
                    var progressDialog = new ProgressDialog("Updating ModAPI DLLs, please wait...", (s, e) =>
                    {
                        updaterPath = UpdaterService.UpdateProgram(release, (s_, e_) =>
                        {
                            (s as BackgroundWorker).ReportProgress(e_.ProgressPercentage);
                        });
                    });
                    progressDialog.ShowDialog();

                    if (progressDialog.Error != null)
                    {
                        ShowException(progressDialog.Error);
                        return;
                    }

                    //TODO close and execute program
                    Process.Start(updaterPath, Path.GetDirectoryName(Process.GetCurrentProcess().GetExecutablePath()));
                    Application.Current.Shutdown();
                    return;
                }
            }

            webException = null;

            bool hasDllsUpdate = false;
            try
            {
                hasDllsUpdate = UpdaterService.HasDllsUpdate(out release);
            }
            catch (WebException ex)
            {
                webException = ex;
            }
            catch (Exception ex)
            {
                ShowException(ex);
                return;
            }

            if (webException != null)
            {
                MessageBox.Show("Cannot check for updates, please check your internet connection." + "\n" + webException.Message,
                    "Cannot check for updates");
                return;
            }

            if (hasDllsUpdate)
            {
                // If we reach this point with a program update available, it means it didn't update
                // (as the update restarts the program), so we cannot continue
                if (hasProgramUpdate)
                {
                    MessageBox.Show("An update to the ModAPI DLLs, needed to run modern mods, is available. However, it cannot be installed until you update the program. Please restart the program and allow it to update.",
                        "ModAPI DLLs cannot update");
                }
                else
                {
                    bool update = true;
                    if (Settings.UpdatingMode == Settings.UpdatingModeType.AutoCheck)
                    {
                        update = MessageBox.Show("An update to the ModAPI DLLs is available. It includes new features and bugfixes, and is required to run modern mods. Do you want to download it?",
                            "ModAPI DLLs Update Available", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                    }

                    if (update)
                    {
                        var progressDialog = new ProgressDialog("Updating ModAPI DLLs, please wait...", (s, e) =>
                        {
                            UpdaterService.UpdateDlls(release, (s_, e_) =>
                            {
                                (s as BackgroundWorker).ReportProgress(e_.ProgressPercentage);
                            });
                        });
                        progressDialog.ShowDialog();

                        if (progressDialog.Error != null)
                        {
                            ShowException(progressDialog.Error);
                        }
                    }
                    //TODO error handling
                }
            }
        }

        static bool exceptionShown = false;
        public static void ShowException(Exception exception)
        {
            if (!exceptionShown)
            {
                exceptionShown = true;
                Exception current = exception;
                int count = 0;
                string errorText = "\n\nPlease send the contents this MessageBox and all which follow it to rob55rod\\Splitwirez, along with a description of what you were doing at the time.\n\nThe Spore Mod Manager will exit after the last Inner exception has been reported.";
                string errorTitle = "Something is very wrong here. Layer ";
                while (current != null)
                {
                    MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, errorTitle + count);
                    count++;
                    current = current.InnerException;
                    if (count > 4)
                        break;
                }
                if (current != null)
                {
                    MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, errorTitle + count);
                }
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
