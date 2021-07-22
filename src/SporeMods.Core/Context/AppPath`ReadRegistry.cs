using Microsoft.Win32;
using SporeMods.BaseTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SporeMods.Core.Context
{
    public partial class AppPath : NOCObject
    {
        const string KEY_PATH = @"HKEY_LOCAL_MACHINE\SOFTWARE\Electronic Arts";
        /*static readonly string[] KEY_PATHS =
        {
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Electronic Arts\",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Electronic Arts\"
        };*/

        static readonly Dictionary<ExpansionPack, string> KEY_NAMES = new Dictionary<ExpansionPack, string>()
        {
            {
                ExpansionPack.None,
                "SPORE"
            },
            {
                ExpansionPack.GalacticAdventures,
                "SPORE_EP1"
            }
        };

        static readonly Dictionary<ExpansionPack, string> APP_DIR_SUFFIXES = new Dictionary<ExpansionPack, string>()
        {
            {
                ExpansionPack.None,
                string.Empty
            },
            {
                ExpansionPack.GalacticAdventures,
                "EP1"
            }
        };

        static readonly string[] SZ_NAMES =
        {
            "InstallLoc",
            "Install Dir",
            "DataDir" //Steam and GOG only have this one
		};

        const char SZ_SEPARATOR = '\\';


        bool CorrectGameInstallPath(string subPath, out string fixedPath)
        {
            string output = StripTrailingCharacters(subPath);

            bool isSporePath = true;
            while (!IsPathGameInstallRoot(output, _dlcLevel))
            {
                if (output.Contains(@"\"))
                {
                    //MessageDisplay.DebugShowMessageBox("EnsureGameInstallPathIsInstallRoot output: " + output);
                    if (IsPathGameInstallRoot(output, _dlcLevel))
                        break;
                    else
                        output = output.Substring(0, output.LastIndexOf(@"\"));
                }
                else
                {
                    isSporePath = false;
                    break;
                }
            }

            if (!output.EndsWith(SZ_SEPARATOR))
                output += SZ_SEPARATOR;

            string out2 = output + _dirNameBase + APP_DIR_SUFFIXES[_dlcLevel];
            if (Directory.Exists(out2))
                output = out2;
            else if (_allowNoSuffix)
            {
                out2 = output + _dirNameBase;
                if (Directory.Exists(out2))
                    output = out2;
            }

            fixedPath = output;
            return isSporePath;
        }

        string StripTrailingCharacters(string input)
        {
            string output = input.Replace("\"", string.Empty);
            /*while (output.StartsWith("\""))
				output = output.Substring(output.IndexOf("\""));

			while (output.EndsWith("\""))
				output = output.Substring(0, output.LastIndexOf("\""));*/

            while (output.EndsWith(@"\"))
                output = output.Substring(0, output.LastIndexOf(@"\"));

            while (output.EndsWith(@"/"))
                output = output.Substring(0, output.LastIndexOf(@"/"));

            output = output.Replace(@"/", @"\");

            //MessageDisplay.DebugShowMessageBox("AFTER STRIPPING SLASHES: " + output);
            return output;
        }

        string EnsureGameInstallPathIsInstallRoot(string subPath)
        {
            bool isSporePath = CorrectGameInstallPath(subPath, out string output);

            //MessageDisplay.DebugShowMessageBox("isSporePath: " + isSporePath + ", " + output);

            if (isSporePath)
            {
                //MessageDisplay.DebugShowMessageBox("INSTALL ROOT: " + output);
                return output;
            }
            else
            {
                /*if (!BadGameInstallPaths.Any(x => (x.BasePath == subPath) && (x.BadPath == output) && (x.DlcLevel == dlc) && (!x.IsSporebin)))
                    BadGameInstallPaths.Add(new BadPathEventArgs(subPath, output, dlc));*/
                NeedsExplicitPath = true;
                return string.Empty;
            }
        }

        bool IsPathGameInstallRoot(string path, ExpansionPack dlc)
        {
            if (Directory.Exists(System.IO.Path.Combine(path, _dirNameBase + APP_DIR_SUFFIXES[dlc])))
                return true;
            else if (_allowNoSuffix && (dlc != ExpansionPack.None) && Directory.Exists(System.IO.Path.Combine(path, _dirNameBase)))
                return true;
            else
                return false;
        }

        public ObservableCollection<string> GetAllGameInstallPathsFromRegistry()
        {
            /*string regPath = GetRegistryPath(dlc);
            if (regPath == null)
                return new ObservableCollection<string>();*/
            ObservableCollection<string> allPaths = new ObservableCollection<string>();
            
            string append = SZ_SEPARATOR + KEY_NAMES[_dlcLevel];

            /*foreach (string regPath in KEY_PATHS)
            {*/

                foreach (string stringName in SZ_NAMES)
                {
                    string keyPath = KEY_PATH + append;
                    Debug.WriteLine(keyPath);
                    var regRaw = Registry.GetValue(keyPath, stringName, null);
                    if ((regRaw != null) && (regRaw is string regValue))
                    {
                        regValue = regValue.Trim('"', '\'').Replace('/', '\\');
                        if (CorrectGameInstallPath(regValue, out regValue))
                        {
                            bool add = true;
                            foreach (string t in allPaths)
                            {
                                if (t.Equals(regValue, StringComparison.OrdinalIgnoreCase))
                                {
                                    add = false;
                                    break;
                                }
                            }

                            if (add)
                            {
                                allPaths.Add(regValue);
                            }
                        }
                    }
                }
            //}
            return allPaths;
        }
    }
}
