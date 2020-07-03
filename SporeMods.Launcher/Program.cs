using SporeMods.Core;
using SporeMods.Core.Injection;
using SporeMods.Core.InstalledMods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            MessageDisplay.MessageBoxShown += (sneder, args) => MessageBox.Show(args.Content, args.Title);
            MessageDisplay.ErrorOccurred += (sneder, args) =>
            {
                if (args.Content.IsNullOrEmptyOrWhiteSpace())
                    MessageBox.Show(args.Exception.ToString(), args.Title);
                else
                    MessageBox.Show(args.Content + "\n\n" + args.Exception.ToString(), args.Title);
            };
            /*{
                if (string.IsNullOrEmpty(args.Content) || string.IsNullOrWhiteSpace(args.Content) || args.Exception.ToString().Contains(args.Content))
                    MessageBox.Show(args.Exception.ToString(), args.Title);
                else if (args.Exception == null)
                    MessageBox.Show(args.Content, args.Title);
                else
                    MessageBox.Show(args.Content + "\n\n" + args.Exception.ToString(), args.Title);
             };*/


            bool proceed = true;
            if (Permissions.IsAtleastWindowsVista() && Permissions.IsAdministrator())
            {
                proceed = false;
                if (MessageBox.Show(Settings.CurrentLanguage["Globals_DontRunLauncherAsAdmin2"], String.Empty, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    proceed = true;
            }

            if (proceed)
            {
                if (Settings.ForceSoftwareRendering)
                    RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

                SporeLauncher.CaptionHeight = SystemInformation.CaptionHeight;

                if (SporeLauncher.IsInstalledDarkInjectionCompatible())
                    SporeLauncher.LaunchGame();
            }
            else Application.Exit();
        }
    }
}
