using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
//using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Interop;

namespace SporeMods.Setup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern string GetWineVersion();

        private static bool? IsRunningUnderWine(out Version version)
        {
            version = new Version(0, 0, 0, 0);
            try
            {
                string wineVer = GetWineVersion();
                if (!Version.TryParse(wineVer, out version))
                    return null;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }




        public static string SetupAssemblyNameForPackURIs => Assembly.GetExecutingAssembly().GetName().Name; //Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);

        public static string Language = null;
        public static string LkPath = null;
        public static string MgrExePath = null;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exc)
                ShowException(exc);
        }

        static bool EXCEPTION_SHOWN = false;
        public static void ShowException(Exception exception)
        {
            if (!EXCEPTION_SHOWN)
            {
                EXCEPTION_SHOWN = true;
                Exception current = exception;
                int count = 0;
                string errorText = "\n\nPlease send the contents this MessageBox and all which follow it to rob55rod\\Splitwirez, along with a description of what you were doing at the time.\n\nSpore Mod Manager Setup will exit after the last Inner exception has been reported.";
                string errorTitle = "Something is very wrong here. Layer ";
                while (current != null)
                {
                    MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, errorTitle + count);
                    count++;
                    current = current.InnerException;
                    if (count > 4)
                        break;
                }
                if (current != null)
                {
                    MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, errorTitle + count);
                }
                Process.GetCurrentProcess().Close();
            }
        }


        bool AllowInstallOnUnsupportedOS => Environment.GetCommandLineArgs().Any(x => x == "--allow-unsupported-platforms");

        string _areYouSureAboutThat = " Installing the Spore Mod Manager on an unsupported platform could damage your install of Spore or your save data, or leave you stranded on an outdated version of the Spore Mod Manager (and thus locked out of mods which require newer versions). Are you ABSOLUTELY 100% CERTAIN you wish to proceed with installation? (NOT LOCALIZED)";

        void WarnOrRejectUnsupportedOS(string baseText)
        {
            string msg = baseText;
            if (AllowInstallOnUnsupportedOS)
            {
                msg += _areYouSureAboutThat;

                if (MessageBox.Show(msg, string.Empty, button: MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown(-1);
                }
            }
            else
            {
                MessageBox.Show(msg, string.Empty);
                Application.Current.Shutdown(-1);
            }
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            
            /*Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/" + SetupExeName + ";component/Locale/en-ca.xaml", UriKind.RelativeOrAbsolute)
            });*/
            IEnumerable<string> args = Environment.GetCommandLineArgs().Skip(1);

            foreach (string p in args)
            {
                //MessageBox.Show("p: " + p);
                if (IsLauncherKitInstallDir(p, out string fixedPath))
                {
                    //MessageBox.Show("fixedPath: " + fixedPath);
                    LkPath = fixedPath;
                    break;
                }
            }

            if (LkPath == null)
            {
                string lkPathFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Spore ModAPI Launcher", "path.info");
                if (File.Exists(lkPathFilePath))
                {
                    string lkPathFilePathText = File.ReadAllText(lkPathFilePath);

                    //MessageBox.Show("lkPathFilePathText: " + lkPathFilePathText);
                    if (IsLauncherKitInstallDir(lkPathFilePathText, out string fixedLkPath))
                    {
                        //MessageBox.Show("fixedLkPath: " + fixedLkPath);
                        LkPath = fixedLkPath;
                    }
                }
            }

            if (!Environment.GetCommandLineArgs().Skip(1).Any(x => x == SetupInfo.IS_WOULDBE_ADMIN_PROCESS))
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                try
                {
                    Process proc = Permissions.RerunAsAdministrator(Permissions.GetProcessCommandLineArgs() + " " + SetupInfo.IS_WOULDBE_ADMIN_PROCESS, false);
                    proc.WaitForExit();

                    if (File.Exists(SetupInfo.LAST_EXE_PATH))
                    {
                        string path = File.ReadAllText(SetupInfo.LAST_EXE_PATH);
                        if (File.Exists(path))
                            MgrExePath = path;
                    }
                    else if (File.Exists(SetupInfo.INSTALL_DIR_LOCATOR_PATH))
                    {
                        /*string[] paths = File.ReadAllText(SetupInfo.INSTALL_DIR_LOCATOR_PATH);
                        if (proc.ExitCode == SetupInfo.EXIT_RUN_MOD_MGR)
                        {
                            Process.Start(Path.Combine(paths[0], "Spore Mod Manager.exe"));
                        }
                        else if ((proc.ExitCode == SetupInfo.EXIT_RUN_LK_IMPORTER) && (_lkPath != null))
                        {
                            Process.Start(Path.Combine(paths[0], "SporeMods.KitImporter.exe"), "\"" + _lkPath + "\"");
                        }*/

                        string mgrPath = File.ReadAllText(SetupInfo.INSTALL_DIR_LOCATOR_PATH);

                        if (MgrExePath == null)
                            MgrExePath = Path.Combine(mgrPath, "Spore Mod Manager.exe");
                    }


                    if ((MgrExePath != null) && File.Exists(MgrExePath))
                    {
                        if ((proc != null) && proc.HasExited && (proc.ExitCode == 300))
                            Process.Start(new ProcessStartInfo(MgrExePath)
                            {
                                UseShellExecute = true
                            });
                    }

                    /*else
                        MessageBox.Show("Spore Mod Manager install location was not returned. You should never see this message, so if you somehow do see it, inform Splitwirez or emd immediately.");*/

                    Shutdown();
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    Shutdown();
                }
            }
            else
            {
                base.OnStartup(e);


                bool? wine = IsRunningUnderWine(out Version wineVersion);

                if (wine == true)
                {
                    if (wineVersion < new Version(6, 0))
                        WarnOrRejectUnsupportedOS("This version of WINE is not supported. The Spore Mod Manager requires WINE 6.0 or newer to function as intended. (NOT LOCALIZED)");
                }
                else if (wine == null)
                {
                    WarnOrRejectUnsupportedOS("Unable to determine currently-used WINE version. The Spore Mod Manager requires at least WINE 6.0. (NOT LOCALIZED)");
                }
                else if (Environment.OSVersion.Version < new Version(6, 1, 7601))
                {
                    WarnOrRejectUnsupportedOS("This version of Windows is not supported. The Spore Mod Manager requires Windows 7 SP1 or newer to function as intended. (NOT LOCALIZED)");
                }


                MainWindow = new MainWindow();
                if (Language == null)
                {
                    new LanguagesWindow().ShowDialog();
                }
                else
                {
                    try
                    {
                        Resources.MergedDictionaries[0] = new ResourceDictionary()
                        {
                            Source = new Uri("/" + SetupAssemblyNameForPackURIs + ";component/Locale/" + Language + ".xaml", UriKind.RelativeOrAbsolute)
                        };
                    }
                    catch
                    {
                        new LanguagesWindow().ShowDialog();
                    }
                }
                MainWindow.Show();
            }
        }

        bool IsLauncherKitInstallDir(string rawPath, out string fixedPath)
        {
            string path = rawPath.Trim('"', ' ');
            //MessageBox.Show("IsLauncherKitInstallDir path: '" + path + "'");

            try
            {
                if ((path != null) && Directory.Exists(path))
                {
                    if (
                            File.Exists(Path.Combine(path, "Spore ModAPI Launcher.exe")) &&
                            File.Exists(Path.Combine(path, "Spore ModAPI Easy Installer.exe")) &&
                            File.Exists(Path.Combine(path, "Spore ModAPI Easy Uninstaller.exe")) &&
                            (!File.Exists(Path.Combine(path, "Spore Mod Manager.exe"))) &&
                            (!File.Exists(Path.Combine(path, "Spore Mod Launcher.exe")))
                        )
                    {
                        fixedPath = path;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            fixedPath = null;
            return false;
        }

        /*void EnsureAdmin()
        {
            if (!Permissions.IsAdministrator())
                Permissions.RerunAsAdministrator();
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(args.Name);
            byte[] block = new byte[s.Length];
            s.Read(block, 0, block.Length);
            Assembly a2 = Assembly.Load(block);
            return a2;
        }*/
    }


    public static class SetupInfo
    {
        public static int EXIT_RUN_MOD_MGR = 200;
        public static int EXIT_RUN_LK_IMPORTER = 201;

        public static string INSTALL_DIR_LOCATOR_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SporeModManagerStorage", "SporeModManager_InstallPath.info");
        public static string LAST_EXE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SporeModManagerStorage", "LastRunProgram.info");

        public static string IS_WOULDBE_ADMIN_PROCESS = "--wouldBeAdmin";
    }



    public static class Permissions
    {
        public static bool IsAtleastWindowsVista()
        {
            return Environment.OSVersion.Version.Major >= 6;
        }

        public static bool IsAdministrator()
        {
            if (Environment.OSVersion.Version.Major <= 5)
                return true;
            else
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static Process RerunAsAdministrator()
        {
            return RerunAsAdministrator(GetProcessCommandLineArgs());
        }

        public static string GetProcessCommandLineArgs()
        {
            var args = Environment.GetCommandLineArgs().ToList();
            if (args.Count > 0)
                args.RemoveAt(0);

            string returnVal = string.Empty;
            foreach (string s in args)
            {
                //MessageBox.Show(s, "arg");
                if (s.Contains(' '))
                    returnVal = returnVal + "\"" + s + "\" ";
                else
                    returnVal = returnVal + s + " ";
            }
            //MessageBox.Show(returnVal, "args");

            return returnVal;
        }

        public static Process RerunAsAdministrator(string args)
        {
            return RerunAsAdministrator(args, true);
        }

        public static Process RerunAsAdministrator(bool closeCurrent)
        {
            return RerunAsAdministrator(GetProcessCommandLineArgs(), closeCurrent);
        }

        public static Process RerunAsAdministrator(string args, bool closeCurrent)
        {
            //https://stackoverflow.com/questions/133379/elevating-process-privilege-programmatically/10905713
            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            Process process = null;
            ProcessStartInfo startInfo = new ProcessStartInfo(exeName, args)
            {
                UseShellExecute = true,
                Verb = "runas"
            };
            /*try
            {*/
            //System.Windows.Forms.MessageBox.Show(args);
            process = Process.Start(startInfo);
            /*}
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }*/

            if (closeCurrent && (process != null))
                Process.GetCurrentProcess().Kill();

            return process;
        }

        //https://stackoverflow.com/questions/9108399/how-to-grant-full-permission-to-a-file-created-by-my-application-for-all-users
        /// <summary>
        /// If the current application is running as Administrator, attempts to grant full access to a specific directory and its files.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool GrantAccessDirectory(string fullPath)
        {
            if (Permissions.IsAdministrator() && Directory.Exists(fullPath))
            {
                DirectoryInfo dInfo = new DirectoryInfo(fullPath);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl,
                                                                 InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                                                                 PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                dInfo.SetAccessControl(dSecurity);
                return true;
            }
            return false;
        }

        /// <summary>
        /// If the current application is running as Administrator, attempts to grant full access to a specific file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool GrantAccessFile(string filePath)
        {
            if (Permissions.IsAdministrator() && File.Exists(filePath))
            {
                //var security = File.GetAccessControl(filePath);
                var sec = new FileSecurity(filePath, AccessControlSections.All);
                sec.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                                                             FileSystemRights.FullControl, InheritanceFlags.None,
                                                             PropagationFlags.NoPropagateInherit, AccessControlType.Allow));

                return true;

                //string parentPath = Path.GetDirectoryName(filePath);
                //if (Directory.Exists(parentPath))
                //{
                //    return GrantAccess(parentPath);
                //}
            }
            return false;
        }

        public static bool IsFileLocked(string filePath)
        {
            return IsFileLocked(filePath, FileAccess.Read);
        }

        //https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
        public static bool IsFileLocked(string filePath, FileAccess access)
        {
            FileStream stream = null;

            try
            {
                stream = new FileInfo(filePath).Open(FileMode.Open, access, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}