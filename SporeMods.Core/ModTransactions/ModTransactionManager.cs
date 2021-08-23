using SporeMods.Core.Mods;
using SporeMods.Core.ModTransactions.Transactions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions
{
    public static class ModTransactionManager
    {
        internal static bool IS_INSTALLING_MODS = false;
        internal static Dictionary<string, Exception> INSTALL_FAILURES = new Dictionary<string, Exception>();

        internal static bool IS_UNINSTALLING_MODS = false;

        internal static bool IS_RECONFIGURING_MODS = false;

        [DllImport("shlwapi.dll")]
        static extern bool PathIsNetworkPath(string pszPath);

        // Concurrent dictionary is the only collection that lets us remove, we assign it to some random number
        private static readonly ConcurrentDictionary<ModTransaction, int> currentTransactions = new ConcurrentDictionary<ModTransaction, int>();

        public static bool IsExecutingTransactions { get => !currentTransactions.IsEmpty; }

        // Events to show confirmation dialog to user
        public static event Func<IEnumerable<string>, bool> UninstallingSaveDataDependencyMod;

        /// <summary>
        /// Executes a transaction, reversing its actions if something fails.
        /// The method returns null if the transaction was committed correctly.
        /// If it fails, it returns the exception that caused the failure.
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static async Task<ModTransactionCommitException> ExecuteAsync(ModTransaction transaction)
        {
            currentTransactions[transaction] = 0;
            try
            {
                if (!await transaction.CommitAsync())
                {
                    transaction.Rollback();
                    currentTransactions.Remove(transaction, out _);
                    return new ModTransactionCommitException(TransactionFailureCause.CommitRejected, null, null);
                }
                else
                {
                    currentTransactions.Remove(transaction, out _);
                    return null;
                }
            }
            // There is a specific exception for when a transaction fails, so theoretically we should only need to catch ModTransactionCommitException
            // However, we also want to rollback if there was an unexpected exception while executing the code
            // (although that is the developers fault!)
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                transaction.Rollback();
                currentTransactions.Remove(transaction, out _);
                return new ModTransactionCommitException(TransactionFailureCause.Exception, null, e);
            }
        }

        /// <summary>
        /// Installs a list of mods. Each mod installation will be reverted if it fails, but it won't affect
        /// the other mods being installed (unless there is a dependency).
        /// </summary>
        /// <param name="modPaths"></param>
        /// <returns></returns>
        public static async Task InstallModsAsync(string[] modPaths)
        {
            //TODO implement dependencies
            // To do so, you will have to move reading the identity outside of the transaction (it's not a problem,
            // since there is nothing to be reverted there). So, first step, get all the identities. Then, resolve the
            // dependencies, possibly reordering the mods, then do the loop below, which executes the transactions.

            IS_INSTALLING_MODS = true;
            var taskLists = new Dictionary<string, Task<ModTransactionCommitException>>();
            foreach (string path in modPaths)
            {
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
                    INSTALL_FAILURES.Add(Path.GetFileName(path), new Exception("'" + Path.GetExtension(path) + "' is not a valid mod extension."));
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

        public static async Task UninstallModsAsync(IInstalledMod[] mods)
        {
            List<IInstalledMod> modsToUninstall = mods.ToList();
            List<IInstalledMod> modsToThinkTwiceBeforeUninstalling = new List<IInstalledMod>();

            foreach (IInstalledMod mod in modsToUninstall.Where(x => (x is ManagedMod xm) && xm.Identity.CausesSaveDataDependency))
                modsToThinkTwiceBeforeUninstalling.Add(mod);

            if (modsToThinkTwiceBeforeUninstalling.Count() > 0)
            {
                List<string> modNames = new List<string>();
                foreach (IInstalledMod mod in modsToThinkTwiceBeforeUninstalling)
                    modNames.Add(mod.DisplayName);

                if (!UninstallingSaveDataDependencyMod(modNames))
                {
                    foreach (IInstalledMod mod in modsToThinkTwiceBeforeUninstalling)
                        modsToUninstall.Remove(mod);
                }
            }

            foreach (IInstalledMod mod in modsToUninstall)
            {
                // This function doesn't throw exceptions, the code inside must handle it
                await mod.UninstallModAsync();
            }
        }

        /// <summary>
        /// Enables a managed mod, returning to the original state if something fails.
        /// </summary>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static async Task<ModTransactionCommitException> EnableModAsync(ManagedMod mod)
        {
            return await ExecuteAsync(new EnableModTransaction(mod));
        }

        /// <summary>
        /// Disables a managed mod, returning to the original state if something fails.
        /// </summary>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static async Task<ModTransactionCommitException> DisableModAsync(ManagedMod mod)
        {
            return await ExecuteAsync(new DisableModTransaction(mod));
        }

        /// <summary>
        /// Configures a managed mod, returning to the original state if something fails.
        /// </summary>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static async Task<ModTransactionCommitException> ConfigureModAsync(ManagedMod mod)
        {
            return await ExecuteAsync(new ConfigureModTransaction(mod));
        }
    }
}
