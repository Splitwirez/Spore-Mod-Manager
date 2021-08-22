using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModInstallationaa
{
    public static class ModTransactionManager
    {
        internal static bool IS_INSTALLING_MODS = false;
        internal static Dictionary<string, Exception> INSTALL_FAILURES = new Dictionary<string, Exception>();

        internal static bool IS_UNINSTALLING_MODS = false;

        internal static bool IS_RECONFIGURING_MODS = false;

        [DllImport("shlwapi.dll")]
        static extern bool PathIsNetworkPath(string pszPath);

        private static List<ModTransaction> currentTransactions = new List<ModTransaction>();

        public static async Task<Exception> ExecuteAsync(ModTransaction transaction)
        {
            currentTransactions.Add(transaction);
            try
            {
                if (!await transaction.CommitAsync())
                {
                    transaction.Rollback();
                    currentTransactions.Remove(transaction);
                    return new ModTransactionCommitException();
                }
                return null;
            }
            // There is a specific exception for when a transaction fails, so theoretically we should only need to catch ModTransactionCommitException
            // However, we also want to rollback if there was an unexpected exception while executing the code
            // (although that is the developers fault!)
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                transaction.Rollback();
                currentTransactions.Remove(transaction);
                return e;
            }
        }

        public static async Task InstallModsAsync(string[] modPaths)
        {
            var taskLists = new Dictionary<String, Task<Exception>>();
            foreach (string path in modPaths)
            {
                bool validExtension = true;
                Exception result = null;

                if (PathIsNetworkPath(path))
                {
                    INSTALL_FAILURES.Add(Path.GetFileName(path), new Exception("Cannot install mods from network locations. Please move the mod(s) to local storage and try again from there."));
                }
                else if (Path.GetExtension(path).ToLowerInvariant() == ".package")
                {
                    taskLists[path] = ExecuteAsync(new InstallLoosePackageTransaction(path));
                }
                else if (Path.GetExtension(path).ToLowerInvariant() == ".sporemod")
                {
                    taskLists[path] = ExecuteAsync(new InstallModTransaction(path));
                }
                else
                {
                    validExtension = false;
                }
            }

            foreach (var task in taskLists)
            {
                var exception = await task.Value;
                if (exception != null)
                {
                    INSTALL_FAILURES[task.Key] = exception;
                }
            }
        }
    }
}
