using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static SporeMods.Core.GameInfo;

namespace SporeMods.Core.Injection
{
    public static class CoreDllRetriever
    {
        static string LibFileName = "SporeModAPI.lib";


        public static string GetNewDLLName(GameExecutableType type)
        {
            if (type == GameExecutableType.Disk__1_5_1)
                return "SporeModAPI.disk.dll";
            else if ((type == GameExecutableType.Origin__1_5_1) || (type == GameExecutableType.Origin__March2017) || (type == GameExecutableType.GogOrSteam__March2017))
                return "SporeModAPI.march2017.dll";
            else
            {
                //System.Windows.Forms.MessageBox.Show("This version of Spore (" + type.ToString() + ") is not supported. Please inform rob55rod or emd4600 immediately.", "Unsupported Game Version");
                //System.Windows.Forms.MessageBox.Show("If you're using the Steam version of Spore or the GOG version of Spore, please update to version 3.1.0.22 to proceed. If you're using Origin Spore and you see this message, or if you're already using a higher version of Spore, please inform rob55rod or emd4600 immediately.", "Unsupported Game Version");
                Process.Start(@"http://davoonline.com/phpBB3/viewtopic.php?f=108&t=6300"); //(@"https://github.com/emd4600/Spore-ModAPI/issues/new");
                Process.GetCurrentProcess().Kill();
                return string.Empty;
            }
        }

        public static string GetOverrideDllPath(GameExecutableType exeType) => GetOverrideDllPath(exeType, false);
        public static string GetOverrideDllPath(GameExecutableType exeType, bool isLib)
        {
            string fileName;
            if (isLib)
                fileName = LibFileName;
            else
                fileName = GetNewDLLName(exeType);

            return Path.Combine(Settings.OverrideLibsPath, fileName);
        }


        public static string GetStoredCoreDllPath(GameExecutableType exeType) => GetStoredCoreDllPath(exeType, false);
        public static string GetStoredCoreDllPath(GameExecutableType exeType, bool isLib)
        {
            string fileName;
            if (isLib)
                fileName = LibFileName;
            else
                fileName = GetNewDLLName(exeType);

            if (Settings.DeveloperMode || Settings.DebugMode)
            {
                string devOutPath = GetOverrideDllPath(exeType, isLib);
                if (File.Exists(devOutPath))
                    return devOutPath;
            }

            string outPath = Path.Combine(Settings.CoreLibsPath, fileName);
            if (File.Exists(outPath))
                return outPath;
            else
                return null;
        }

        public static string GetInjectableCoreDllPath(GameExecutableType exeType) => GetInjectableCoreDllPath(exeType, false);
        public static string GetInjectableCoreDllPath(GameExecutableType exeType, bool isLib)
        {
            string fileName;
            if (isLib)
                fileName = LibFileName;
            else
                fileName = "SporeModAPI.dll";

            return Path.Combine(Settings.ModLibsPath, fileName);
        }

        public static void InstallOverrideDll(string path, GameExecutableType type)
        {
            if (type == GameExecutableType.None)
                File.Copy(path, GetOverrideDllPath(GameExecutableType.GogOrSteam__March2017, true), true);
            else
                File.Copy(path, GetOverrideDllPath(type), true);
        }

        public static void InstallOverrideDlls(string path)
        {
            using (ZipFile zip = new ZipFile(path))
            {
                for (int i = 0; i < zip.Entries.Count; i++)
                {
                    ZipEntry e = zip.Entries.ElementAt(i);

                    string newFileName = e.FileName.Replace(@"/", @"\");
                    if ((!e.IsDirectory) && newFileName.Contains(@"\"))
                    {
                        newFileName = newFileName.Substring(newFileName.LastIndexOf(@"\"));
                        newFileName = newFileName.Replace(@"\", @"/");
                        e.FileName = newFileName;
                    }
                }

                foreach (ZipEntry e in zip.Entries)
                    e.Extract(Settings.OverrideLibsPath, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public static void PurgeOverrideDlls()
        {
            IEnumerable<string> paths = Directory.EnumerateFiles(Settings.OverrideLibsPath);
            foreach (string s in paths)
                File.Delete(s);

        }
    }
}
