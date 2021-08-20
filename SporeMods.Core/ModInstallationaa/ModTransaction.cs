using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SporeMods.Core.ModInstallationaa
{
    public abstract class ModTransaction
    {
        // The operations that have executed, in order. This will be used to undo them.
        private ConcurrentStack<IModOperation> operations;

        // Number of operations that are currently running. We must wait for them to finish before we can undo them.
        private CountdownEvent numRunningOperations;

        /// <summary>
        /// Adds an operation to be executed synchronously, immediately executing it.
        /// </summary>
        /// <param name="operation"></param>
        protected T operation<T>(T operation) where T : IModOperation
        {
            operations.Push(operation);
            numRunningOperations.AddCount();
            operation.Do();
            numRunningOperations.Signal();
            return operation;
        }

        /// <summary>
        /// Adds an operation to be executed asynchronously, and begins executing it.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        protected Task<T> operationAsync<T>(T operation) where T : IModOperation
        {
            operations.Push(operation);
            var task = new Task<T>(() =>
            {
                operations.Push(operation);
                numRunningOperations.AddCount();
                operation.Do();
                numRunningOperations.Signal();
                return operation;
            });
            task.Start();
            return task;
        }

        public abstract Task<bool> DoAsync();

        public virtual void Undo()
        {
            // Wait until all currently running operations have finished running
            numRunningOperations.Wait();

            while (!operations.IsEmpty)
            {
                operations.TryPop(out IModOperation op);
                op.Undo();
            }
        }
    }
}
