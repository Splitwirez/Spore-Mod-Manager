using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SporeMods.Core.Transactions
{
    public enum TransactionFailureCause
    {
        /// <summary>
        /// An unhandled exception was raised during the execution
        /// </summary>
        Exception,
        /// <summary>
        /// An operation returned false
        /// </summary>
        OperationRejected,
        /// <summary>
        /// The transaction Commit method returned false
        /// </summary>
        CommitRejected
    }


    public enum JobStatus
    {
        Determinate,
        Indeterminate,
        Rollback,
        Complete
    }
    

    public enum JobOutcome
    {
        Succeeded,
        Failed,
        Skipped
    }
}
