using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SporeMods.Core.Transactions
{
    /// <summary>
    /// Exception thrown when the transaction commit fails, and must be rolled back.
    /// </summary>
    public class TransactionCommitException : Exception
    {
        public TransactionFailureCause Cause { get; }
        /// <summary>
        /// The operation that returned false or threw an exception, can be null.
        /// </summary>
        public IOperation Operation { get; }
        /// <summary>
        /// The exception that was raised, if any.
        /// </summary>
        //public ThreadSafeObservableCollection<Exception> Exceptions { get; private set; } = new ThreadSafeObservableCollection<Exception>();

        public TransactionCommitException(TransactionFailureCause cause, IOperation operation, Exception exception)
            : base(cause.ToString(), exception)
        {
            Cause = cause;
            Operation = operation;
            /*if (exceptions != null)
            {
                foreach (var ex in exceptions)
                {
                    Exceptions.Add(ex);
                }
            }*/
        }
    }
}
