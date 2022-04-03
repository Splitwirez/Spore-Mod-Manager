using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SporeMods.Core.Transactions
{
    public abstract class TransactionBase<TJob> : ITransaction<TJob>
        where TJob : IJob
    {
        Exception _exception = null;
        public Exception Exception
        {
            get => _exception;
            protected set => _exception = value;
        }


        TJob _job = default(TJob);
        public TJob Job
        {
            get => _job;
            set => _job = value;
        }

        // The operations that have executed, in order. This will be used to undo them.
        private ConcurrentStack<IOperation> _operations = new ConcurrentStack<IOperation>();

        // All tasks that have run, we must wait for them to finish before we can undo them.
        private ConcurrentBag<Task<bool>> _executedTasks = new ConcurrentBag<Task<bool>>();

        /// <summary>
        /// Adds an operation to be executed synchronously, immediately executing it.
        /// </summary>
        /// <param name="operation"></param>
        public T Operation<T>(T operation) where T : ISyncOperation
        {
            _operations.Push(operation);
            if (!operation.Do())
            {
                throw new TransactionCommitException(TransactionFailureCause.OperationRejected, operation,
                    /*new List<Exception>()
                    {*/
                        operation.Exception
                    //}
            );
            }
            return operation;
        }

        /// <summary>
        /// Adds an operation to be executed on another thread, immediately starting it but without blocking.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <returns></returns>
        public T OperationNonBlocking<T>(T operation) where T : ISyncOperation
        {
            _operations.Push(operation);
            var task = new Task<bool>(() =>
            {
                if (!operation.Do())
                {
                    throw new TransactionCommitException(TransactionFailureCause.OperationRejected, operation,
                        /*new List<Exception>()
                        {*/
                            operation.Exception
                        //}
                    );
                }
                // we need a return value to add the task to executedTasks
                return true;
            });
            _executedTasks.Add(task);
            task.Start();
            return operation;
        }

        /// <summary>
        /// Adds an operation to be executed asynchronously, and begins executing it.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public async Task<T> OperationAsync<T>(T operation) where T : IAsyncOperation
        {
            _operations.Push(operation);
            var task = operation.DoAsync();
            _executedTasks.Add(task);
            if (!await task)
            {
                throw new TransactionCommitException(TransactionFailureCause.OperationRejected, operation,
                    //(operation.Exception != null) ? operation.Exception  : task.Exception
                    (task.Exception != null) ? task.Exception : operation.Exception
                    /*new List<Exception>()
                    {
                        operation.Exception,
                        task.Exception
                    }*/
                    );
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
            Cmd.WriteLine("Rollback on transaction " + ToString());
            // Wait until all currently running operations have finished running
            Task.WhenAll(_executedTasks).Wait();

            while (!_operations.IsEmpty)
            {
                _operations.TryPop(out IOperation op);
                Cmd.WriteLine(" - undoing " + op.ToString());
                op.Undo();
                op.Dispose();
            }
        }

        /// <summary>
        /// Disposes all operators and deletes them. After calling this, the transaction is useless and not valid anymore.
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var operation in _operations)
            {
                operation.Dispose();
            }
            _operations.Clear();
        }
    }
}
