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
using System.Threading;

namespace SporeMods.CommonUI
{
    public static class Updater
    {


        public static void CheckForUpdates()
        {
            CheckForUpdates(false);
        }

        public static void CheckForUpdates(bool forceInstallUpdate)
        {
            bool ignoreUpdates = Environment.GetCommandLineArgs().Contains(UpdaterService.IgnoreUpdatesArg);
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
                        string updaterPath = Path.Combine(Settings.TempFolderPath, "SporeModManagerSetup.exe");
                        bool updateDownloadFinished = false;

                        /*while ((!File.Exists(updaterPath)) || Permissions.IsFileLocked(updaterPath) || (!updateDownloadFinished))
                        { }*/
                        
                        var progressDialog = GetProgressDialog(Settings.GetLanguageString("UpdatingProgressText"), (s, e) =>
                        {
                            /*Thread prgThread = new Thread(() =>
                            {*/
                                updateDownloadFinished = UpdaterService.UpdateProgram(release, (s_, e_) =>
                                {
                                    (s as BackgroundWorker).ReportProgress(e_.ProgressPercentage);
                                });

                            /*});
                            prgThread.Start();*/
                        });
                        progressDialog.ShowDialog();

                        if (progressDialog.Error != null)
                        {
                            MessageDisplay.ShowException(progressDialog.Error);
                            return;
                        }

                        while (Permissions.IsFileLocked(updaterPath))
                        { }
                        Process.Start(updaterPath, "--update \"" + Path.GetDirectoryName(Process.GetCurrentProcess().GetExecutablePath()) + "\" \"" + Process.GetCurrentProcess().GetExecutablePath() + "\" --lang:" + Settings.CurrentLanguageCode);
                        Process.GetCurrentProcess().Kill();
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
                    MessageBox.Show(Settings.GetLanguageString("Error_CannotCheckForUpdates") + "\n" + webException.ToString(), Settings.GetLanguageString("Error_CannotCheckForUpdatesTitle"));
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
                            progressDialog.ShowDialog();

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
            if (false) //Settings.UseCustomWindowDecorations)
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
