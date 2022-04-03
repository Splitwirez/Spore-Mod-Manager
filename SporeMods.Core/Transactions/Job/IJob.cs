using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SporeMods.Core.Transactions
{
    public interface IJob
    {
        public ObservableCollection<Exception> Exceptions {
            get;
        }

        public ITransaction Transaction
        {
            get;
            set;
        }

        double TotalProgress
        {
            get;
            //set;
        }


        double ActivityRangeStart
        {
            get;
        }
        double ActivityRangeEnd
        {
            get;
        }
        bool TrySetActivityRange(double start, double end);
        void ClearActivityRange();
        bool HasRestrictedActivityRange
        {
            get;
        }

        double ActivityRangeLength
        {
            get;
        }

        double ActivityRangeProgress
        {
            get;
            set;
        }

        
        JobStatus Status
        {
            get;
            set;
        }

        JobOutcome Outcome
        {
            get;
            set;
        }

        bool IsConcluded
        {
            get;
            set;
        }
    }
}
