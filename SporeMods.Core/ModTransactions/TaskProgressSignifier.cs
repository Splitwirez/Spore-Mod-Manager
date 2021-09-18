using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.ModTransactions
{
    public enum TaskCategory
    {
        Install,
        Upgrade,
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
                IsConcluded = (Status == TaskStatus.Succeeded) || (Status == TaskStatus.Failed) || (Status == TaskStatus.Skipped);
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
                RefreshPercentage();
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
                RefreshPercentage();
            }
        }

        void RefreshPercentage()
        {
            ProgressPercentage = (int)((_progress / Math.Max(_progressTotal, 1.0)) * 100.0);
        }


        int _progressPercentage = 0;
        public int ProgressPercentage
        {
            get => _progressPercentage;
            private set
            {
                _progressPercentage = value;
                NotifyPropertyChanged();
            }
        }
    }
}
