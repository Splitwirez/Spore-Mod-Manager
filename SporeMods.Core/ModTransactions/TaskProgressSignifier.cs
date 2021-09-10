using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.ModTransactions
{
    public enum TaskCategory
    {
        Install,
        Reconfigure,
        Uninstall,
        Rollback
    }

    public enum TaskStatus
    {
        Determinate,
        Indeterminate,
        Succeeded,
        Failed,
        Skipped
    }

    public class TaskProgressSignifier : NotifyPropertyChangedBase
    {
        public TaskProgressSignifier(string title, TaskCategory category)
        {
            Title = title;
            Category = category;

            ProgressTotal = 100.0;

            ModTransactionManager.Instance.Tasks.Add(this);
        }


        string _title = string.Empty;
        public string Title
        {
            get => _title;
            internal set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }


        TaskCategory _category = TaskCategory.Install;
        public TaskCategory Category
        {
            get => _category;
            internal set
            {
                _category = value;
                NotifyPropertyChanged();
            }
        }

        TaskStatus _status = TaskStatus.Indeterminate;
        public TaskStatus Status
        {
            get => _status;
            internal set
            {
                _status = value;
                NotifyPropertyChanged();
                IsConcluded = (_status == TaskStatus.Failed) || (_status == TaskStatus.Skipped) || (_status == TaskStatus.Succeeded);
            }
        }

        bool _isConcluded = false;
        public bool IsConcluded
        {
            get => _isConcluded;
            internal set
            {
                _isConcluded = value;
                NotifyPropertyChanged();
            }
        }



        double _progress = 0.0;
        public double Progress
        {
            get => _progress;
            internal set
            {
                ModTransactionManager.Instance.OverallProgress += (value - _progress);
                _progress = value;
                NotifyPropertyChanged();
            }
        }


        double _progressTotal = 0.0;
        public double ProgressTotal
        {
            get => _progressTotal;
            internal set
            {
                ModTransactionManager.Instance.OverallProgressTotal = (ModTransactionManager.Instance.OverallProgressTotal - _progressTotal) + value;
                _progressTotal = value;
                NotifyPropertyChanged();
            }
        }
    }
}
