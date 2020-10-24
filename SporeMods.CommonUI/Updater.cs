using DecoratableWindow = Mechanism.Wpf.Core.Windows.DecoratableWindow;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;

namespace SporeMods.CommonUI
{
    public static class Updater
    {
        public static readonly string IgnoreUpdatesArg = "-ignoreUpdates";


        public static void CheckForUpdates()
        {
            CheckForUpdates(false);
        }

        public static void CheckForUpdates(bool forceInstallUpdate)
        {
            bool ignoreUpdates = Environment.GetCommandLineArgs().Contains(IgnoreUpdatesArg);
            if (!ignoreUpdates)
            {
                if ((!forceInstallUpdate) && (Settings.UpdatingMode == Settings.UpdatingModeType.Disabled))
                    return;

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
                    MessageDisplay.ShowException(ex);
                    return;
                }

                if (hasProgramUpdate)
                {
                    bool update = true;
                    if ((!forceInstallUpdate) && (Settings.UpdatingMode == Settings.UpdatingModeType.AutoCheck))
                    {
                        update = MessageBox.Show(Settings.GetLanguageString("UpdateAvailableText"),
                            Settings.GetLanguageString("UpdateAvailableTitle"), MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                    }

                    if (update)
                    {
                        string updaterPath = null;
                        var progressDialog = GetProgressDialog(Settings.GetLanguageString("UpdatingProgressText"), (s, e) =>
                        {
                            updaterPath = UpdaterService.UpdateProgram(release, (s_, e_) =>
                            {
                                (s as BackgroundWorker).ReportProgress(e_.ProgressPercentage);
                            });
                        });
                        progressDialog.Show();

                        if (progressDialog.Error != null)
                        {
                            MessageDisplay.ShowException(progressDialog.Error);
                            return;
                        }

                        //TODO close and execute program
                        Process.Start(updaterPath, Path.GetDirectoryName(Process.GetCurrentProcess().GetExecutablePath()));
                        Application.Current.Shutdown();
                        return;
                    }
                }

                //TODO remember to restore this
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
                    MessageDisplay.ShowException(ex);
                    return;
                }

                if (webException != null)
                {
                    MessageBox.Show(Settings.GetLanguageString("Error_CannotCheckForUpdates") + "\n" + webException.Message,
                        Settings.GetLanguageString("Error_CannotCheckForUpdatesTitle"));
                    return;
                }

                if (hasDllsUpdate)
                {
                    // If we reach this point with a program update available, it means it didn't update
                    // (as the update restarts the program), so we cannot continue
                    if (hasProgramUpdate)
                    {
                        MessageBox.Show(Settings.GetLanguageString("Error_UpdateAvailableDlls"),
                            Settings.GetLanguageString("Error_UpdateAvailableDllsTitle"));
                    }
                    else
                    {
                        bool update = true;
                        if (Settings.UpdatingMode == Settings.UpdatingModeType.AutoCheck)
                        {
                            update = MessageBox.Show(Settings.GetLanguageString("UpdateAvailableDllsText"),
                                Settings.GetLanguageString("UpdateAvailableDllsTitle"), MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                        }

                        if (update)
                        {
                            var progressDialog = GetProgressDialog(Settings.GetLanguageString("UpdatingProgressDllsText"), (s, e) =>
                            {
                                UpdaterService.UpdateDlls(release, (s_, e_) =>
                                {
                                    (s as BackgroundWorker).ReportProgress(e_.ProgressPercentage);
                                });
                            });
                            progressDialog.Show();

                            if (progressDialog.Error != null)
                            {
                                MessageDisplay.ShowException(progressDialog.Error);
                            }
                        }
                    }
                }
            }
        }

        public static ProgressDialog GetProgressDialog(string text, DoWorkEventHandler action)
        {
            return GetProgressDialog(text, action, false);
        }

        public static ProgressDialog GetProgressDialog(string text, DoWorkEventHandler action, bool testUI)
        {
            ProgressDialog dialog = new ProgressDialog(text, action);

            Window window;
            if (Settings.UseCustomWindowDecorations)
            {
                window = new DecoratableWindow();
                window.SetResourceReference(DecoratableWindow.StyleProperty, typeof(DecoratableWindow));
            }
            else
                window = new Window();

            window.Content = dialog;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.Resources = dialog.Resources;//.MergedDictionaries.Add() //Application.Current.Resources.MergedDictionaries[0].MergedDictionaries[1] = ShaleAccents.Sky.Dictionary;

            if (testUI)
            {
                dialog.SetProgress(50);
                window.Show();
            }

            return dialog;
        }
    }
}
