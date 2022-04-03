using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SporeMods.Core.Transactions
{
    public abstract class JobBase : NotifyPropertyChangedBase, IJob
    {
        public const double PROGRESS_OVERALL_MIN = 0;
        public const double PROGRESS_OVERALL_MAX = 100;

        public ObservableCollection<Exception> Exceptions
        {
            get;
        } = new ObservableCollection<Exception>();


        ITransaction _transaction = null;
        public ITransaction Transaction
        {
            get => _transaction;
            set
            {
                _transaction = value;
                NotifyPropertyChanged();
            }
        }

        /*public JobBase(ITransaction transaction)
        {
            Transaction = transaction;
        }
        
        public JobBase(ITransaction transaction, IEnumerable<Exception> exceptions)
            : this(transaction)
        {
            if (exceptions != null)
                Exceptions = exceptions;
        }*/


        double _totalProgress = 0.0;
        public double TotalProgress
        {
            get => _totalProgress;
            protected set
            {
                //ModTransactionManager.Instance.OverallProgress += (value - _progress);
                _totalProgress = value;
                NotifyPropertyChanged();
                //RefreshPercentage();
            }
        }


        double _activityRangeStart = PROGRESS_OVERALL_MIN;
        public double ActivityRangeStart
        {
            get => _activityRangeStart;
        }

        double _activityRangeEnd = PROGRESS_OVERALL_MAX;
        public double ActivityRangeEnd
        {
            get => _activityRangeEnd;
        }


        bool _hasRestrictedActivityRange = false;
        public bool HasRestrictedActivityRange
        {
            get => _hasRestrictedActivityRange;
            protected set
            {
                _hasRestrictedActivityRange = value;
                NotifyPropertyChanged();
            }
        }
        public double ActivityRangeLength
        {
            get => _activityRangeEnd - _activityRangeStart;
        }


        public bool TrySetActivityRange(double start, double end)
        {
            double rangeStart = Math.Clamp(start, PROGRESS_OVERALL_MIN, PROGRESS_OVERALL_MAX);
            double rangeEnd = Math.Clamp(end, PROGRESS_OVERALL_MIN, PROGRESS_OVERALL_MAX);
            /*if (start < PROGRESS_OVERALL_MIN)
                throw new ArgumentOutOfRangeException(nameof(start), $"Must be >= {PROGRESS_OVERALL_MIN}");
            else if (end > PROGRESS_OVERALL_MAX)
                throw new ArgumentOutOfRangeException(nameof(start), $"Must be <= {PROGRESS_OVERALL_MAX}");
            else if (end <= start)
                throw new ArgumentOutOfRangeException($"{nameof(end)} must be > {nameof(start)}");*/

            if (rangeStart != rangeEnd)
            {
                _activityRangeStart = rangeStart;
                _activityRangeEnd = rangeEnd;

                HasRestrictedActivityRange = (_activityRangeStart != PROGRESS_OVERALL_MIN) || (_activityRangeEnd != PROGRESS_OVERALL_MAX);
                NotifyPropertyChanged(nameof(ActivityRangeProgress));
                return true;
            }
            else
                return false;
        }

        public void ClearActivityRange()
            => TrySetActivityRange(PROGRESS_OVERALL_MIN, PROGRESS_OVERALL_MAX);

        static double Map(double inVal, double inRangeStart, double inRangeEnd, double outRangeStart, double outRangeEnd)
        {
            if (Math.Abs(inRangeEnd - inRangeStart) <= 0)
                throw new ArgumentOutOfRangeException($"{nameof(inRangeEnd)} must be != {nameof(inRangeStart)}");
            else if (Math.Abs(outRangeEnd - outRangeStart) <= 0)
                throw new ArgumentOutOfRangeException($"{nameof(outRangeEnd)} must be != {nameof(outRangeStart)}");

            //https://stackoverflow.com/questions/345187/math-mapping-numbers
            return
                (inVal - inRangeStart)
                / (inRangeEnd - inRangeStart)
                * (outRangeEnd - outRangeStart)
                + outRangeStart;
        }

        public double ActivityRangeProgress
        {
            get
            {
                double total = TotalProgress;
                if (_hasRestrictedActivityRange)
                {
                    /*if (total <= _activityRangeStart)
                        return PROGRESS_OVERALL_MIN;
                    else if (total >= _activityRangeEnd)
                        return PROGRESS_OVERALL_MAX;
                    else*/
                        return Map(total, _activityRangeStart, _activityRangeEnd, PROGRESS_OVERALL_MIN, PROGRESS_OVERALL_MAX);
                }
                else
                    return total;
            }
            set
            {
                double finalVal = Math.Clamp(value, PROGRESS_OVERALL_MIN, PROGRESS_OVERALL_MAX);
                if (_hasRestrictedActivityRange)
                    finalVal = Map(finalVal, PROGRESS_OVERALL_MIN, PROGRESS_OVERALL_MAX, _activityRangeStart, _activityRangeEnd);
                
                TotalProgress = finalVal;

                NotifyPropertyChanged();
            }
        }

        
        JobStatus _status = JobStatus.Determinate;
        public JobStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        JobOutcome _outcome = JobOutcome.Failed;
        public JobOutcome Outcome
        {
            get => _outcome;
            set
            {
                _outcome = value;
                NotifyPropertyChanged();
            }
        }

        bool _isConcluded = false;
        public bool IsConcluded
        {
            get => _isConcluded;
            set
            {
                _isConcluded = value;
                NotifyPropertyChanged();
            }
        }
    }
}
