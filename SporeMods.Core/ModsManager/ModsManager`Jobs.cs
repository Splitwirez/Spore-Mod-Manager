using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using SporeMods.Core.Transactions;
using TTimer = System.Timers.Timer;

namespace SporeMods.Core
{
	public partial class ModsManager : NotifyPropertyChangedBase
	{
        public JobManager<ModJob, ModTransaction> Jobs
        {
            get;
        } = new JobManager<ModJob, ModTransaction>();


        public event Action<ModJobsReportViewModel> AllJobsConcluded;


        static IReadOnlyList<Func<ICanInstallFromSporemodFile>> _MOD_TYPES_FROM_SPOREMOD = new List<Func<ICanInstallFromSporemodFile>>()
        {
            () => new MI1_0_1_XMod(),
            () => new MI1_0_0_0Mod(),
            () => new PreIdentityMod(),
        }.AsReadOnly();
        static IReadOnlyList<Func<ICanInstallFromPackageFile>> _MOD_TYPES_FROM_DBPF = new List<Func<ICanInstallFromPackageFile>>()
        {
            () => new PreIdentityMod(),
        }.AsReadOnly();
        /*static List<Func<string, ZipArchive, Task<ISporeMod>>> _ANALYZE_MOD_FROM_SPOREMOD = new List<Func<string, ZipArchive, Task<ISporeMod>>>()
        {
            MI1_0_X_XMod.AnalyzeFromSporemodAsync,
            PreIdentityMod.AnalyzeFromSporemodAsync
        };
        static List<Func<string, Task<ISporeMod>>> _ANALYZE_MOD_FROM_DBPF = new List<Func<string, Task<ISporeMod>>>()
        {
            PreIdentityMod.AnalyzeFromLoosePackageAsync
        };*/



        /// <summary>
        /// Installs a list of mods. Each mod installation will be reverted if it fails, but it won't affect
        /// the other mods being installed (unless there is a dependency).
        /// </summary>
        /// <param name="modPaths"></param>
        /// <returns></returns>
        public async Task InstallModsAsync(string[] modPaths)
        {
            List<ModJob> batch = new List<ModJob>();


            var entryTasks = new List<Task<ModJobBatchEntryBase>>();
            
            ModJobsBatchViewModel overview = new ModJobsBatchViewModel();
            var overviewTask = Modal.Show(overview);
            
            foreach (string modPath in modPaths)
            {
                entryTasks.Add(AnalyzeModAsync(modPath));
            }

            await Task.WhenAll(entryTasks);

            var overviewEntries = new List<ModJobBatchEntryBase>();

#if MOD_PREREQ
            List<Task> ensureModEntryTasks = new List<Task>();
#endif
            
            await Task.Run(() =>
            {
                List<ModJobBatchEntryBase> initialModEntries = new List<ModJobBatchEntryBase>();
                foreach (var task in entryTasks)
                {
                    var result = task.Result;
                    overviewEntries.Add(result);

                    if (result is ModJobBatchErrorEntry errorEntry)
                    {
                        ////////_conclusionEntries.Add(new TasksConclusionAnalysisFailureEntry(errorEntry));
                    }
                    else //if (result is ModJobBatchModEntry resultMod)
                    {
                        initialModEntries.Add(result);
                    }
                }

#if MOD_PREREQ
                ModJobBatchModEntry[] modEntries = initialModEntries.OfType<ModJobBatchModEntry>().ToArray();
                List<ModJobBatchModEntry> modEntriesList = modEntries.ToList();
                foreach (ModJobBatchModEntry modEntry in modEntries)
                {
                    /*if (entry is ModJobBatchModEntry modEntry)
                    {*/
                        ensureModEntryTasks.Add(new Task(async () =>
                        {
                            var ret = await modEntry.Mod.EnsureCanInstall(modEntry, modEntriesList);
                            if (ret != modEntry)
                            {
                                if (ret is ModJobBatchModEntry retMod)
                                    modEntriesList.Insert(modEntriesList.IndexOf(modEntry), retMod);
                                modEntriesList.Remove(modEntry);

                                overviewEntries.Insert(overviewEntries.IndexOf(modEntry), ret);
                                overviewEntries.Remove(modEntry);
                            }
                        }));
                    //}
                }
#endif
            });

#if MOD_PREREQ
            await Task.WhenAll(ensureModEntryTasks);
#endif

            await Task.Run(() =>
            {
                overview.OnAnalysisFinished(overviewEntries);
            });
            var entriesToInstall = await overviewTask;
            if (
                    (entriesToInstall == null) ||
                    (entriesToInstall.Count <= 0)
                )
            return;

                
            /*await Task.Run(() =>
            {
                if (entriesToInstall.Count() > 0)
                {
                    string debugMsgOutput = "===== MODS TO INSTALL =====\n\n\n\n";

                    foreach (var entry in entriesToInstall)
                    {
                        debugMsgOutput += $"\t{entry.ToString()}\n\n";
                    }

                    MessageDisplay.ShowMessageBox(debugMsgOutput);
                }
            });*/

            var installTransactions = new List<InstallModTransaction>();
            foreach (ModJobBatchModEntry entry in entriesToInstall)
            {
                var transaction = new InstallModTransaction(entry);
                //Instance.Tasks.Add(transaction.ProgressSignifier);
                
                batch.Add(new ModJob(transaction, entry.Mod.DisplayName, JobCategory.Install));
                ////////_transactionsModsMap.Add(transaction, entry.Mod);
            }

            
            Conclude(await Jobs.ExecuteBatchAsync(batch));

            /*await Task.Run(() =>
            {
                foreach (var transaction in installTransactions)
                {
                    //installTransactions[transaction.ModPath] = ExecuteAsync(transaction);
                }

                // Await all tasks to see if there were exceptions
                foreach (var task in taskLists)
                {
                    var exception = await task.Value;
                    if (exception != null)
                    {
                        Instance.Tasks.Add(new TaskProgressSignifier(task.Key, TaskCategory.Install)
                        {
                            ProgressTotal = 0,
                            Progress = 0,
                            Status = TaskStatus.Failed
                            //$"An error occurred: {exception} (NOT LOCALIZED)"
                        });
                    }
                }
            });*/
        }

        async Task<ModJobBatchEntryBase> AnalyzeModAsync(string modPath)
        {
            if (!File.Exists(modPath))
                return new ModJobBatchErrorEntry(modPath, "IO!ModFileNotFound", null);

            string extension = Path.GetExtension(modPath);

            if (extension.Equals(ModUtils.MOD_FILE_EX_SPOREMOD, StringComparison.OrdinalIgnoreCase))
            {
                return await AnalyzeSporemodModAsync(modPath);
            }
            else if (extension.Equals(ModUtils.MOD_FILE_EX_DBPF, StringComparison.OrdinalIgnoreCase))
            {
                return await AnalyzeDBPFModAsync(modPath);
            }
            else
            {
                return new ModJobBatchErrorEntry(modPath, "IO!InvalidFileType", null);
            }
        }

        async Task<ModJobBatchEntryBase> AnalyzeSporemodModAsync(string modPath)
        {
            ICanInstallFromSporemodFile mod = null;
            ZipArchive archive = null;
            ModJobBatchEntryBase failureEntry = null;

            ModJobBatchEntryBase zipValidityEntry = await Task<ModJobBatchEntryBase>.Run(() =>
            {
                try
                {
                    //archive = await Task<ZipArchive>.Run(() => ZipFile.OpenRead(modPath));
                    archive = ZipFile.OpenRead(modPath);
                    return null;
                }
                catch (Exception ex)
                {
                    return new ModJobBatchErrorEntry(modPath, "IO!InvalidZipArchive", ex);
                }
            });

            return await Task.Run<ModJobBatchEntryBase>(() =>
            {
                if (zipValidityEntry != null)
                {
                    return zipValidityEntry;
                }
                else if (archive == null)
                {
                    return new ModJobBatchErrorEntry(modPath, "IO!ZipArchiveReadFailedForSomeReason", null);
                }



                //foreach (var createMod in _MOD_TYPES_FROM_SPOREMOD)
                int modTypesCount = _MOD_TYPES_FROM_SPOREMOD.Count;
                int modTypesLastIndex = modTypesCount - 1;
                List<Exception> previousExceptions = new List<Exception>();
                for (int i = 0; i < modTypesCount; i++)
                {
                    var createMod = _MOD_TYPES_FROM_SPOREMOD[i];
                    mod = createMod();
                    if (!mod.TryAnalyzeIncomingSporemodFile(modPath, archive, out Exception ex))
                    {
                        mod = null;

                        if ((ex is ModException mex) && (!mex.ShouldCheckModAgainstOtherTypes))
                        {
                            //mex.InnerException = previousException;
                            foreach (var excP in previousExceptions)
                            {
                                mex.PriorExceptions.Add(excP);
                            }

                            failureEntry = new ModJobBatchErrorEntry(modPath, "IO!UnknownSporemodAnalysisException", mex);
                            break;
                        }
                        else
                        {
                            failureEntry = new ModJobBatchErrorEntry(modPath, "IO!UnknownSporemodAnalysisException", ex);
                            previousExceptions.Add(ex);
                            continue;
                        }
                    }

                    if (mod != null)
                        break;
                }

                archive.Dispose();
                if (mod != null)
                {
                    return new ModJobBatchModEntry(modPath, mod);
                }
                else if (failureEntry != null)
                {
                    return failureEntry;
                }
                else
                {
                    return new ModJobBatchErrorEntry(modPath, "IO!UnknownSporemodAnalysisError", null);
                }
            });
        }


        async Task<ModJobBatchEntryBase> AnalyzeDBPFModAsync(string modPath)
        {
            return await Task.Run<ModJobBatchEntryBase>(() =>
            {
                ModJobBatchEntryBase failureEntry = null;
                ICanInstallFromPackageFile mod = null;

                int modTypesCount = _MOD_TYPES_FROM_DBPF.Count;
                int modTypesLastIndex = modTypesCount - 1;
                for (int i = 0; i < modTypesCount; i++)
                {
                    var createMod = _MOD_TYPES_FROM_DBPF[i];
                    mod = createMod();
                    if (!mod.TryAnalyzeIncomingPackageFile(modPath, out Exception ex))
                    {
                        failureEntry = new ModJobBatchErrorEntry(modPath, "IO!UnknownSporemodAnalysisException", ex);
                        mod = null;

                        if ((ex is ModException mex) && (!mex.ShouldCheckModAgainstOtherTypes))
                            break;
                        else
                            continue;
                    }

                    if (mod != null)
                        break;
                }


                if (mod != null)
                {
                    return new ModJobBatchModEntry(modPath, mod);
                }
                else if (failureEntry != null)
                {
                    return failureEntry;
                }
                else
                {
                    return new ModJobBatchErrorEntry(modPath, "IO!UnknownDBPFAnalysisError", null);
                }
            });
        }


        public async Task UninstallModsAsync(ISporeMod[] mods)
        {
            var batch = new List<ModJob>();
            foreach (ISporeMod mod in mods)
            {
                batch.Add(new ModJob(new UninstallModTransaction(mod), mod.DisplayName, JobCategory.Uninstall));
            }
            
            Conclude(await Jobs.ExecuteBatchAsync(batch));
        }

        void Conclude(IEnumerable<ModJob> batch)
        {
            var vm = new ModJobsReportViewModel(batch);
            Instance.AllJobsConcluded?.Invoke(vm);
        }
    }
}
