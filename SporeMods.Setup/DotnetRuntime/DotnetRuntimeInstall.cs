using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace SporeMods.Setup
{
    internal static class DotnetRuntimeInstall
    {
        static readonly string RUNTIME_SETUP_NAME = "dotnet-desktopruntime-installer.exe";
        static readonly string RUNTIME_SETUP_FOLDER = Path.GetTempFileName() + "-SMMRuntimeInstaller";

        static readonly string RUNTIME_SETUP_PATH = Path.Combine(RUNTIME_SETUP_FOLDER, RUNTIME_SETUP_NAME);

        public static bool IsPartOfSporeModManager(string resName)
        {
            return (!resName.EndsWith(".resources", StringComparison.OrdinalIgnoreCase)) && (!resName.Contains(RUNTIME_SETUP_NAME, StringComparison.OrdinalIgnoreCase));
        }

        public static void EnsureRuntimeIsInstalled(MainWindow window)
        {
            window.Hide();

            string output = "RESOURCES:\n\n";
            string[] names = Application.ResourceAssembly.GetManifestResourceNames();

            foreach (string o in names)
            {
                output += "\t" + o + "\n";
            }

            
            if (!Directory.Exists(RUNTIME_SETUP_FOLDER))
                Directory.CreateDirectory(RUNTIME_SETUP_FOLDER);

            using (var resource = Application.ResourceAssembly.GetManifestResourceStream(RUNTIME_SETUP_NAME))
            {
                using (var file = new FileStream(RUNTIME_SETUP_PATH, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }

            Process runtimeInstaller = Process.Start(new ProcessStartInfo(RUNTIME_SETUP_PATH/*, @"/passive /norestart"*/)
            {
                UseShellExecute = true
            });
            runtimeInstaller.WaitForExit();


            //MessageBox.Show(output, "RESOURCES BE LIKE");
            MessageBox.Show("Exit code was " + runtimeInstaller.ExitCode);
            //1602 = cancelled?

            if (Directory.Exists(RUNTIME_SETUP_FOLDER))
                Directory.Delete(RUNTIME_SETUP_FOLDER, true);


            window.Show();
        }
    }
}
