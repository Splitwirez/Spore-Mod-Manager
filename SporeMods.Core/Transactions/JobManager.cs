using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SporeMods.Core.Transactions
{
    public class JobManager<TJob, TTransaction> : NotifyPropertyChangedBase
        where TJob : IJob
        where TTransaction : ITransaction<TJob>
    {
        //readonly ThreadSafeObservableCollection<TTransaction> _ongoingTransactions = new ThreadSafeObservableCollection<TTransaction>();
        //bool _isCurrentBatchChanging = false;
        public ThreadSafeObservableCollection<TJob> CurrentBatchJobs
        {
            get;
        } = new ThreadSafeObservableCollection<TJob>();

        int _currentJobIndex = -1;
        public int CurrentJobIndex
        {
            get => _currentJobIndex;
            protected set
            {
                _currentJobIndex = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(CurrentJob));
            }
        }

        public TJob CurrentJob
        {
            get
            {
                int index = CurrentJobIndex;
                //var jobs = CurrentBatchJobs;
                if ((index >= 0) && (index < CurrentBatchJobs.Count))
                    return CurrentBatchJobs[index];
                else
                    return default(TJob);
            }
        }


        //readonly List<TJob> _conclusionEntries = new List<TJob>();


        //Func<TTransaction, IEnumerable<Exception>, TJob> _createJob = null;
        public JobManager() //Func<TTransaction, IEnumerable<Exception>, TJob> createJob)
        {
            //_createJob = createJob;
            CurrentBatchJobs.CollectionChanged += (s, e) =>
            {
                if (CurrentBatchJobs.Count <= 0)
                    CurrentJobIndex = -1;
                /*if (!_isCurrentBatchChanging)
                    throw new AccessViolationException($"Cannot modify '{nameof(CurrentBatchJobs)}' externally - callers should treat this as a read-only collection.");*/
            };
        }

        //public event Action<IEnumerable<TJob>> AllTransactionsConcluded;

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

        /// <summary>
        /// Executes a transaction, reversing its actions if something fails.
        /// The method returns null if the transaction was committed correctly.
        /// If it fails, it returns the exception that caused the failure.
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        async Task ExecuteAsync(TJob job)
        {
            TTransaction transaction = (TTransaction)(job.Transaction);

            try
            {
                if (!await transaction.CommitAsync())
                {
                    Cmd.WriteLine("Transaction returned false");
                    // Transaction itself returned false, which means it decided to rollback
                    transaction.Rollback();

                    /*job = _createJob
                    (
                        transaction
                        , new List<Exception>()
                        {
                            //new ModTransactionCommitException(TransactionFailureCause.CommitRejected, null, transaction.Exception),
                            transaction.Exception
                        }
                    );*/
                    job.Exceptions.Add(transaction.Exception);
                }
                else
                {
                    // Transaction finished successfully. We must dispose it (for example, to clear the backups).
                    transaction.Dispose();
                    //job = _createJob(transaction, null);
                }
            }
            // There is a specific exception for when a transaction fails, so theoretically we should only need to catch ModTransactionCommitException
            // However, we also want to rollback if there was an unexpected exception while executing the code
            // (although that is the developers fault!)
            catch (Exception e)
            {
                Cmd.WriteLine("Transaction failed violently");
                Cmd.WriteLine(e.ToString());
                transaction.Rollback();
                    

                /*job = _createJob
                (
                    transaction
                    , new List<Exception>()
                    {
                        e,
                        transaction.Exception
                    }
                );*/
                job.Exceptions.Add(e);
                job.Exceptions.Add(transaction.Exception);
            }

            job.IsConcluded = true;
        }

        /*protected virtual TTask CreateTransactionReportEntry(TTransaction transaction, IEnumerable<Exception> exceptions)
        {
            var exceptions2 = new List<Exception>();
            
            if (exceptions.Count() > 0)
            {
                foreach (var ex in exceptions)
                {
                    exceptions2.Add(ex);
                }

                Exception e2 = exceptions.First();
                while (e2 != null)
                {
                    var inner = e2.InnerException;
                    if
                    (
                            (inner != null)
                        && (inner != e2)
                    )
                    {
                        exceptions2.Add(inner);
                    }
                    e2 = inner;
                }
                exceptions2.AddRange(exceptions);
            }


            return new TTask()
            (
                transaction
                , exceptions2
            );
        }*/


        public async Task<IEnumerable<TJob>> ExecuteBatchAsync(IEnumerable<TJob> jobs)
        {
            await Task.Run(() =>
            {   
                foreach (TJob job in jobs)
                {
                    //_isCurrentBatchChanging = true;
                    ((TTransaction)(job.Transaction)).Job = job;
                    CurrentBatchJobs.Add(job);
                    //_isCurrentBatchChanging = false;
                }
            });

            if /*(*/(CurrentBatchJobs.Count > 0)// && (_conclusionEntries.Count() <= 0))
            {
                CurrentJobIndex = 0;
                await ExecuteNextTransaction();
            }
            

            return await Task<IEnumerable<TJob>>.Run(() =>
            {
                List<TJob> reportEntries = new List<TJob>();
                foreach (var entry in CurrentBatchJobs)
                {
                    reportEntries.Add(entry);
                }
                CurrentBatchJobs.Clear();
                
                //AllTransactionsConcluded?.Invoke(reportEntries);
                return reportEntries;
            });
        }

        async Task ExecuteNextTransaction()
        {
            TJob initial = CurrentJob; //CurrentBatchJobs[_currentTransactionIndex];
            await ExecuteAsync(initial);
            
            /*_isCurrentBatchChanging = true;
            int index = CurrentBatchJobs.IndexOf(initial);
                if (replacement != null)
                CurrentBatchJobs.Insert(index, replacement);
            CurrentBatchJobs.Remove(initial);
            _isCurrentBatchChanging = false;*/

            if ((CurrentJobIndex + 1) < CurrentBatchJobs.Count)
            {
                CurrentJobIndex++;
                await ExecuteNextTransaction();
            }
        }
    }
}
