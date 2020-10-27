using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace SporeMods.Core
{
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
                //returnVal = returnVal + "\"" + s + "\" ";
                if (s.Contains(' '))
                    returnVal = returnVal + "\"" + s + "\" ";
                else
                    returnVal = returnVal + s + " ";
            }

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
                var security = File.GetAccessControl(filePath);
                security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), 
                                                             FileSystemRights.FullControl, InheritanceFlags.None,
                                                             PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                File.SetAccessControl(filePath, security);
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