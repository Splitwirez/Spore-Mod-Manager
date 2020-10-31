using SporeMods.CommonUI;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using static SporeMods.Core.Injection.SporeLauncher;

namespace SporeMods.DragServant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        FileSystemWatcher _launcherWatcher = new FileSystemWatcher(Settings.TempFolderPath)
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = false
        };


        public App()
        {
            var win = new MainWindow();
            win.Show();
            MainWindow = win;
            _launcherWatcher.Created += (sneder, args) =>
            {
                string fileName = Path.GetFileName(args.FullPath);
                bool processed = true;
                switch (fileName)
                {
                    case "LaunchGame":
                        if (!Environment.GetCommandLineArgs().Contains(UpdaterService.IgnoreUpdatesArg))
                            StartLauncher();
                        break;
                    case "OpenUrl":
                        string path = File.ReadAllText(args.FullPath);
                        if (path.StartsWith("http"))
                            Process.Start(path);
                        break;
                    default:
                        processed = false;
                        break;
                }

                if (processed)
                    File.Delete(args.FullPath);
            };
        }
    }
}
