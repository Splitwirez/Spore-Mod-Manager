using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using System.Windows;

namespace SporeMods.Core
{
    public class SteamInfo
    {
        public static SteamInfo Instance = new SteamInfo();

        public static string MoveToSteam(string path, bool recursive = true)
        {
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }

            if (File.Exists(path + "Steam.exe"))
            {
                return path;
            }

            if (Directory.Exists(path + "Steam"))
            {
                return path + "Steam\\";
            }

            if (recursive)
            {
                // check if the user selected another folder (for example, "steamapps")
                return MoveToSteam(Directory.GetParent(path).ToString(), false);
            }

            return null;
        }

        public static string SteamAppsKey = @"HKEY_CURRENT_USER\Software\Valve\Steam\Apps\";

        public static int GalacticAdventuresSteamID = 24720;

        public static string[] SteamRegistryKeys = {
                                             "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam",
                                             "HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam"
                                         };

        public static string SteamRegistryValue = "InstallPath";

        public static string SteamPath
        {
            get
            {
                string path = RegistryHelper.GetFromRegistry(SteamRegistryKeys, new string[] { SteamRegistryValue });

                // for debugging purposes
                // path = null;

                if (path != null)
                {
                    // move the path to Steam
                    path = MoveToSteam(path);
                }

                // If we didn't find the path in the registry or was not valid, ask the user
                if (path == null || !Directory.Exists(path))
                {

                    if (path == null)
                    {
                        path = Settings.ForcedGalacticAdventuresDataPath;
                        if (path == null || path.Length == 0)
                        {
                            /*var result = MessageBox.Show("CommonStrings.SteamNotFoundSpecifyManual", "CommonStrings.SteamNotFound", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                            if (result == MessageBoxResult.OK)
                            {
                                path = null; //ShowSteamChooserDialog();
                            }*/
                            path = null; //TODO: UI FOR IF STEAM PATH IS NOT FOUND
                        }
                    }
                    else
                    {
                        // move the path to Sporebin
                        path = MoveToSteam(path);
                    }
                }

                return path;
            }
        }
    }
}
