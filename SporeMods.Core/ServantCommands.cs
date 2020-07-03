using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SporeMods.Core
{
    public static class ServantCommands
    {
        public static void RunLauncher()
        {
            string launchGamePath = Path.Combine(Settings.TempFolderPath, "LaunchGame");
            if (File.Exists(launchGamePath))
                File.Delete(launchGamePath);

            File.WriteAllText(launchGamePath, string.Empty);
        }
    }
}
