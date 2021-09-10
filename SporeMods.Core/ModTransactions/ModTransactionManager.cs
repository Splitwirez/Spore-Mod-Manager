using SporeMods.Core.Mods;
using SporeMods.Core.ModTransactions.Transactions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions
{
    public class ModTransactionManager : NotifyPropertyChangedBase
    {
        internal static bool IS_INSTALLING_MODS = false;
        internal static Dictionary<string, Exception> INSTALL_FAILURES = new Dictionary<string, Exception>();

        internal static bool IS_UNINSTALLING_MODS = false;

        internal static bool IS_RECONFIGURING_MODS = false;

        [DllImport("shlwapi.dll")]
        static extern bool PathIsNetworkPath(string pszPath);

        private readonly ThreadSafeObservableCollection<TaskProgressSignifier> _tasks = new ThreadSafeObservableCollection<TaskProgressSignifier>();
        public ThreadSafeObservableCollection<TaskProgressSignifier> Tasks
        {
            get => _tasks;
        }



        private static readonly ThreadSafeObservableCollection<ModTransaction> _ongoingTransactions = new ThreadSafeObservableCollection<ModTransaction>();
        private static readonly ThreadSafeObservableCollection<ModTransaction> _concludedTransactions = new ThreadSafeObservableCollection<ModTransaction>();


        public static ModTransactionManager Instance { get; } = new ModTransactionManager();
        private ModTransactionManager()
        { }

        static ModTransactionManager()
        {
            _ongoingTransactions.CollectionChanged += Instance.OngoingTransactions_CollectionChanged;
            _concludedTransactions.CollectionChanged += Instance.ConcludedTransactions_CollectionChanged;
        }

        static bool _handlingCollectionChanged = false;
        private void ConcludedTransactions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_handlingCollectionChanged)
            {
                _handlingCollectionChanged = true;
                if (sender is ThreadSafeObservableCollection<ModTransaction> concluded)
                {
                    if (concluded.Count >= _ongoingTransactions.Count)
                    {
                        _ongoingTransactions.Clear();
                        _concludedTransactions.Clear();
                        Tasks.Clear();
                        /*foreach (TaskProgressSignifier taskSig in new List<TaskProgressSignifier>() {

                            new TaskProgressSignifier("0", TaskCategory.Reconfigure),
            new TaskProgressSignifier("1", TaskCategory.Reconfigure),
            new TaskProgressSignifier("2", TaskCategory.Reconfigure),
            new TaskProgressSignifier("3", TaskCategory.Reconfigure),
            new TaskProgressSignifier("4", TaskCategory.Reconfigure),
            new TaskProgressSignifier("5", TaskCategory.Reconfigure),
            new TaskProgressSignifier("6", TaskCategory.Reconfigure),
            new TaskProgressSignifier("7", TaskCategory.Reconfigure),
            new TaskProgressSignifier("8", TaskCategory.Reconfigure),
            new TaskProgressSignifier("9", TaskCategory.Reconfigure),
            new TaskProgressSignifier("10", TaskCategory.Reconfigure),
            new TaskProgressSignifier("11", TaskCategory.Reconfigure),
            new TaskProgressSignifier("12", TaskCategory.Reconfigure)
                        })
                        {
                            Tasks.Add(taskSig);
                        }*/
                        OverallProgress = 0.0;
                        OverallProgressTotal = 0.0;
                        HasRunningTasks = false;
                        AllTasksConcluded?.Invoke(null);
                    }
                }
                _handlingCollectionChanged = false;
            }
        }

        static int _prevTransactionCount = 0;

        private void OngoingTransactions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ThreadSafeObservableCollection<ModTransaction> collection)
            {
                int newCount = collection.Count;
                if (newCount > 0)
                {
                    HasRunningTasks = true;
                }
                _prevTransactionCount = newCount;
                Debug.WriteLine($"{nameof(_ongoingTransactions)}.Count: {newCount}");
            }
        }

        public event EventHandler<EventArgs> TaskStarted;
        public event Action<ThreadSafeObservableCollection<object>> AllTasksConcluded;

        bool _hasRunningTasks = false;
        public bool HasRunningTasks
        {
            get => _hasRunningTasks;
            private set
            {
                _hasRunningTasks = value;
                NotifyPropertyChanged();
            }
        }

        double _overallProgress = 0.0;
        public double OverallProgress
        {
            get => _overallProgress;
            internal set
            {
                _overallProgress = value;
                NotifyPropertyChanged();
            }
        }

        double _overallProgressTotal = 0.0;
        public double OverallProgressTotal
        {
            get => _overallProgressTotal;
            internal set
            {
                if (
                        (value > _overallProgressTotal)
                        || (value == 0)
                    )
                {
                    _overallProgressTotal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // Events to show confirmation dialog to user
        public static event Func<IEnumerable<string>, bool> UninstallingSaveDataDependencyMod;

        /// <summary>
        /// Executes a transaction, reversing its actions if something fails.
        /// The method returns null if the transaction was committed correctly.
        /// If it fails, it returns the exception that caused the failure.
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        static async Task<ModTransactionCommitException> ExecuteAsync(ModTransaction transaction)
        {
            try
            {
                if (!await transaction.CommitAsync())
                {
                    // Transaction itself returned false, which means it decided to rollback
                    transaction.Rollback();
                    _concludedTransactions.Add(transaction);
                    return new ModTransactionCommitException(TransactionFailureCause.CommitRejected, null, null);
                    Debug.WriteLine("Transaction returned false");
                }
                else
                {
                    // Transaction finished successfully. We must dispose it (for example, to clear the backups).
                    transaction.Dispose();
                    _concludedTransactions.Add(transaction);
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
                //_ongoingTransactions.Remove(transaction);
                _concludedTransactions.Add(transaction);
                Debug.WriteLine("Transaction failed violently");
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
            IS_INSTALLING_MODS = true;

            var packageTransactions = new List<InstallLoosePackageTransaction>();
            var modTransactions = new List<InstallModTransaction>();
            var taskLists = new Dictionary<string, Task<ModTransactionCommitException>>();

            // First, collect all the mod identities.
            foreach (string path in modPaths)
            {
                if (PathIsNetworkPath(path))
                {
                    INSTALL_FAILURES.Add(Path.GetFileName(path), new Exception("Cannot install mods from network locations. Please move the mod(s) to local storage and try again from there."));
                }
                else if (Path.GetExtension(path).ToLowerInvariant() == ".package")
                {
                    var transaction = new InstallLoosePackageTransaction(path);
                    packageTransactions.Add(transaction);
                    _ongoingTransactions.Add(transaction);
                }
                else if (Path.GetExtension(path).ToLowerInvariant() == ".sporemod")
                {
                    try
                    {
                        var transaction = new InstallModTransaction(path);
                        transaction.ParseModIdentity();
                        
                        modTransactions.Add(transaction);
                        _ongoingTransactions.Add(transaction);
                    }
                    catch (Exception e)
                    {
                        // This can happen if the mod provides an invalid DLL
                        INSTALL_FAILURES[path] = e;
                    }
                }
                else
                {
                    INSTALL_FAILURES.Add(Path.GetFileName(path), new Exception("'" + Path.GetExtension(path) + "' is not a valid mod extension."));
                }
            }

            //TODO handle mod versions here
            // For now, just accept the new mod
            // We iterate in reverse order so we can remove from the list 
            // (in case the mod wasn't accepted, for example because it's an older version)
            for (int i = modTransactions.Count - 1; i >= 0; --i)
            {
                var otherMod = ModsManager.GetManagedMod(modTransactions[i].Identity.Unique);
                if (otherMod != null)
                {
                    //TODO usually, here you check versions. We'll just accept the incoming mod
                    modTransactions[i].UpgradeFromMod = otherMod;
                }
            }

            //TODO if you want dependencies, you will have to define an order here

            foreach (var transaction in packageTransactions)
            {
                taskLists[transaction.modPath] = ExecuteAsync(transaction);
            }
            foreach (var transaction in modTransactions)
            {
                taskLists[transaction.ModPath] = ExecuteAsync(transaction);
            }

            // Await all tasks to see if there were exceptions
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

            List<ModTransaction> transactions = new List<ModTransaction>();
            foreach (IInstalledMod mod in modsToUninstall)
            {
                var transaction = mod.CreateUninstallTransaction();
                transactions.Add(transaction);
                _ongoingTransactions.Add(transaction);
            }

            foreach (ModTransaction transaction in transactions)
            {
                // This function doesn't throw exceptions, the code inside must handle it
                await ExecuteAsync(transaction);
            }
        }

        /// <summary>
        /// Configures a managed mod, returning to the original state if something fails.
        /// </summary>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static async Task<ModTransactionCommitException> ConfigureModAsync(ManagedMod mod)
        {
            var transaction = new ConfigureModTransaction(mod);
            _ongoingTransactions.Add(transaction);
            return await ExecuteAsync(transaction);
        }
    }
}
