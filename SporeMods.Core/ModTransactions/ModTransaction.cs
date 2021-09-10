using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SporeMods.Core.ModTransactions
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

    /// <summary>
    /// Exception thrown when the transaction commit fails, and must be rolled back.
    /// </summary>
    public class ModTransactionCommitException : Exception
    {
        public TransactionFailureCause Cause { get; }
        /// <summary>
        /// The operation that returned false or threw an exception, can be null.
        /// </summary>
        public IModOperation Operation { get; }
        /// <summary>
        /// The exception that was raised, if any.
        /// </summary>
        public Exception CauseException { get; }

        public ModTransactionCommitException(TransactionFailureCause cause, IModOperation operation, Exception exception)
        {
            Cause = cause;
            Operation = operation;
            CauseException = exception;
        }
    }

    public abstract class ModTransaction
    {
        protected TaskProgressSignifier ProgressSignifier = null;

        // The operations that have executed, in order. This will be used to undo them.
        private ConcurrentStack<IModOperation> operations = new ConcurrentStack<IModOperation>();

        // All tasks that have run, we must wait for them to finish before we can undo them.
        private ConcurrentBag<Task<bool>> executedTasks = new ConcurrentBag<Task<bool>>();

        /// <summary>
        /// Adds an operation to be executed synchronously, immediately executing it.
        /// </summary>
        /// <param name="operation"></param>
        internal T Operation<T>(T operation) where T : IModSyncOperation
        {
            operations.Push(operation);
            if (!operation.Do())
            {
                throw new ModTransactionCommitException(TransactionFailureCause.OperationRejected, operation, null);
            }
            return operation;
        }

        /// <summary>
        /// Adds an operation to be executed on another thread, immediately starting it but without blocking.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <returns></returns>
        internal T OperationNonBlocking<T>(T operation) where T : IModSyncOperation
        {
            operations.Push(operation);
            var task = new Task<bool>(() =>
            {
                if (!operation.Do())
                {
                    throw new ModTransactionCommitException(TransactionFailureCause.OperationRejected, operation, null);
                }
                // we need a return value to add the task to executedTasks
                return true;
            });
            executedTasks.Add(task);
            task.Start();
            return operation;
        }

        /// <summary>
        /// Adds an operation to be executed asynchronously, and begins executing it.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        internal async Task<T> OperationAsync<T>(T operation) where T : IModAsyncOperation
        {
            operations.Push(operation);
            var task = operation.DoAsync();
            executedTasks.Add(task);
            if (!await task)
            {
                throw new ModTransactionCommitException(TransactionFailureCause.OperationRejected, operation, null);
            }
            return operation;
        }

        /// <summary>
        /// Executes the transaction, by creating operations that can later be reverted. It must be an asynchronous method.
        /// This method must be implemented by subclasses. Return false if the transaction must be reverted.
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> CommitAsync();

        /// <summary>
        /// Undoes all the executed operations in reverse order, so that the effects of the transaction are cancelled.
        /// This will also dispose and delete all operators, rendering the transaction useless after this.
        /// </summary>
        public virtual void Rollback()
        {
            if (ProgressSignifier != null)
                ProgressSignifier.Category = TaskCategory.Rollback;

            Debug.WriteLine("Rollback on transaction " + ToString());
            // Wait until all currently running operations have finished running
            Task.WhenAll(executedTasks).Wait();

            while (!operations.IsEmpty)
            {
                operations.TryPop(out IModOperation op);
                Debug.WriteLine(" - undoing " + op.ToString());
                op.Undo();
                op.Dispose();
            }

            CompleteProgress(false);
        }

        /// <summary>
        /// Disposes all operators and deletes them. After calling this, the transaction is useless and not valid anymore.
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var operation in operations)
            {
                operation.Dispose();
            }
            operations.Clear();

            CompleteProgress(true);
        }

        protected virtual void CompleteProgress(bool dispose)
        {
            if (ProgressSignifier != null)
            {
                ProgressSignifier.Progress = ProgressSignifier.ProgressTotal;
                ProgressSignifier.Status = dispose ? TaskStatus.Succeeded : TaskStatus.Failed;
            }
        }
    }
}
