using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SporeMods.Core.Transactions
{
    public interface ITransaction
    {
        Exception Exception { get; }


        /// <summary>
        /// Adds an operation to be executed synchronously, immediately executing it.
        /// </summary>
        /// <param name="operation"></param>
        T Operation<T>(T operation) where T : ISyncOperation;

        /// <summary>
        /// Adds an operation to be executed on another thread, immediately starting it but without blocking.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <returns></returns>
        T OperationNonBlocking<T>(T operation) where T : ISyncOperation;

        /// <summary>
        /// Adds an operation to be executed asynchronously, and begins executing it.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        Task<T> OperationAsync<T>(T operation) where T : IAsyncOperation;

        /// <summary>
        /// Executes the transaction, by creating operations that can later be reverted. It must be an asynchronous method.
        /// This method must be implemented by subclasses. Return false if the transaction must be reverted.
        /// </summary>
        /// <returns></returns>
        Task<bool> CommitAsync();

        /// <summary>
        /// Undoes all the executed operations in reverse order, so that the effects of the transaction are cancelled.
        /// This will also dispose and delete all operators, rendering the transaction useless after this.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Disposes all operators and deletes them. After calling this, the transaction is useless and not valid anymore.
        /// </summary>
        void Dispose();
    }

    public interface ITransaction<TJob> : ITransaction
        where TJob : IJob
    {
        TJob Job { get; set; }
    }
}
