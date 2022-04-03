using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    public class ModJobsReportViewModel : ModalViewModel<object>
    {
        string _outcomeText = null;
        public string OutcomeText
        {
            get => _outcomeText;
            protected set
            {
                _outcomeText = value;
                NotifyPropertyChanged();
            }
        }



        ThreadSafeObservableCollection<ModJob> _jobs = new ThreadSafeObservableCollection<ModJob>();
        public ThreadSafeObservableCollection<ModJob> Jobs
        {
            get => _jobs;
            protected set
            {
                _jobs = value;
                NotifyPropertyChanged();
            }
        }

        /*IEnumerable<TasksConcludedOverviewEntryBase> _failedEntries = null;
        public IEnumerable<TasksConcludedOverviewEntryBase> FailedEntries
        {
            get => _failedEntries;
            protected set
            {
                _failedEntries = value;
                NotifyPropertyChanged();
            }
        }*/

        public ModJobsReportViewModel(IEnumerable<ModJob> jobs)
            : base()
        {
            DismissCommand = Externals.CreateCommand<object>(o => CompletionSource.TrySetResult(null));
            if (jobs != null)
            {
                foreach (var entry in jobs)
                {
                    Jobs.Add(entry);
                }
            }
        }

        public override string GetViewTypeName()
        {
            return "SporeMods.Views.ModJobsReportView";
        }
    }



    /*public abstract class TasksConclusionEntryBase : NotifyPropertyChangedBase
    {

    }

    public class TasksConclusionTransactionEntry : TasksConclusionEntryBase
    {
        ISporeMod _mod = null;
        public ISporeMod Mod
        {
            get => _mod;
            protected set
            {
                _mod = value;
                NotifyPropertyChanged();
            }
        }


        ModTransaction _transaction = null;
        public ModTransaction Transaction
        {
            get => _transaction;
            protected set
            {
                _transaction = value;
                NotifyPropertyChanged();
            }
        }

        
        List<Exception> _exceptions = new List<Exception>();
        public List<Exception> Exceptions
        {
            get => _exceptions;
            protected set
            {
                _exceptions = value;
                NotifyPropertyChanged();
            }
        }


        public TasksConclusionTransactionEntry(ISporeMod mod, ModTransaction transaction)
            : base()
        {
            Mod = mod;
            Transaction = transaction;
        }


        public TasksConclusionTransactionEntry(ISporeMod mod, ModTransaction transaction, IEnumerable<Exception> exceptions)
            : this(mod, transaction)
        {
            if (exceptions != null)
            {
                foreach (var exc in exceptions)
                {
                    Exceptions.Add(exc);
                }
            }
        }
    }


    public class TasksConclusionAnalysisFailureEntry : TasksConclusionEntryBase
    {
        InstallOverviewErrorEntry _installEntry = null;
        public InstallOverviewErrorEntry InstallEntry
        {
            get => _installEntry;
            protected set
            {
                _installEntry = value;
                NotifyPropertyChanged();
            }
        }


        public TasksConclusionAnalysisFailureEntry(InstallOverviewErrorEntry installEntry)
            : base()
        {
            InstallEntry = installEntry;
        }
    }*/
}
