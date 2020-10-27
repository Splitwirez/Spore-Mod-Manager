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

namespace SporeMods.KitImporter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            if (!Permissions.IsAdministrator())
            {
                Process process = Permissions.RerunAsAdministrator(false);
                process.WaitForExit();
                if (process.ExitCode == 300)
                {
                    foreach (string s in Environment.GetCommandLineArgs())
                    {
                        string arg = s.Trim('"', ' ');
                        if (arg.StartsWith("--relaunch:"))
                        {
                            string probablyPath = arg.Substring("--relaunch:".Length);
                            if (File.Exists(probablyPath))
                            {
                                Process.Start(probablyPath);
                            }
                        }
                    }
                }
            Process.GetCurrentProcess().Kill();
            }
            else
                base.OnStartup(e);
        }
    }
}
