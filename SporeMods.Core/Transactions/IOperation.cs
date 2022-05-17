using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Transactions
{
    /// <summary>
    /// Represents a basic operation that can be undone.
    /// </summary>
    public interface IOperation
    {
        public Exception Exception
        {
            get;
            set;
        }


        /// <summary>
        /// Revert the effects of this operation.
        /// </summary>
        public void Undo();

        /// <summary>
        /// Called after the transaction succeeds or fails, must clean any resources used by the operation.
        /// </summary>
        public void Dispose()
        { }
    }
    /// <summary>
    /// An operation that is executed synchronously.
    /// </summary>
    public interface ISyncOperation : IOperation
    {
        public bool Do();
    }

    public abstract class SyncOperationBase : ISyncOperation
    {
        public abstract bool Do();
        public abstract void Undo();


        Exception _exception = null;
        public virtual Exception Exception
        {
            get => _exception;
            set => _exception = value;
        }
    }


    /// <summary>
    /// A operation that is executed asynchronously.
    /// </summary>
    public interface IAsyncOperation : IOperation
    {
        public Task<bool> DoAsync();
    }

    public abstract class AsyncOperationBase : IAsyncOperation
    {
        public abstract Task<bool> DoAsync();
        public abstract void Undo();


        Exception _exception = null;
        public virtual Exception Exception
        {
            get => _exception;
            set => _exception = value;
        }
    }
}
