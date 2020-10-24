using SporeMods.Core;
using SporeMods.Core.Injection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using static SporeMods.Core.GameInfo;

namespace SporeMods.Launcher
{
    class Program
    {
        public static void ExtractModAPIFix(string folderPath)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SporeMods.Launcher.ModAPIFix.SporeApp_ModAPIFix.exe"))
            using (var file = new FileStream(Path.Combine(folderPath, "SporeApp_ModAPIFix.exe"), FileMode.Create, FileAccess.Write))
            {
                resource.CopyTo(file);
            }

            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SporeMods.Launcher.ModAPIFix.steam_api.dll"))
            using (var file = new FileStream(Path.Combine(folderPath, "steam_api.dll"), FileMode.Create, FileAccess.Write))
            {
                resource.CopyTo(file);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] programArgs)
        {
            MessageDisplay.ErrorOccurred += (sender, args) => CommonUI.MessageDisplay.ShowException(args.Exception);
            MessageDisplay.MessageBoxShown += (sender, args) => CommonUI.MessageDisplay.ShowMessageBox(args.Content, args.Title);
            MessageDisplay.DebugMessageSent += (sneder, args) => CommonUI.MessageDisplay.ShowMessageBox(args.Content, args.Title);
            /*{
                if (args.Content.IsNullOrEmptyOrWhiteSpace())
                    MessageBox.Show(args.Exception.ToString(), args.Title);
                else
                    MessageBox.Show(args.Content + "\n\n" + args.Exception.ToString(), args.Title);
            };*/
            /*{
                if (string.IsNullOrEmpty(args.Content) || string.IsNullOrWhiteSpace(args.Content) || args.Exception.ToString().Contains(args.Content))
                    MessageBox.Show(args.Exception.ToString(), args.Title);
                else if (args.Exception == null)
                    MessageBox.Show(args.Content, args.Title);
                else
                    MessageBox.Show(args.Content + "\n\n" + args.Exception.ToString(), args.Title);
             };*/

            if (CommonUI.VersionValidation.IsConfigVersionCompatible(false, out Version previousModMgrVersion))
            {
                if (programArgs.Length > 1 && programArgs[0] == "--modapifix")
                {
                    ExtractModAPIFix(programArgs[1]);
                    return;
                }
                else
                    CommonUI.Updater.CheckForUpdates();

                bool proceed = true;
                if (Permissions.IsAtleastWindowsVista() && Permissions.IsAdministrator())
                {
                    proceed = false;
                    if (MessageBox.Show(Settings.GetLanguageString(1, "DontRunAsAdmin").Replace("%APPNAME%", "Spore Mod Launcher"), String.Empty, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        proceed = true;
                }

                if (proceed)
                {
                    if (Settings.ForceSoftwareRendering)
                        RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

                    GameInfo.BadGameInstallPath += (sneder, args) =>
                    {
                        MessageBox.Show(Settings.GetLanguageString(3, "RunModManagerFirst")); //Please run the Spore Mod Manager at least once before running the Spore Mod Launcher.
                        Process.GetCurrentProcess().Kill();
                    };

                    SporeLauncher.CaptionHeight = SystemInformation.CaptionHeight;

                    if (SporeLauncher.IsInstalledDarkInjectionCompatible())
                        SporeLauncher.LaunchGame();
                }
                else Application.Exit();
            }
        }
    }
}
