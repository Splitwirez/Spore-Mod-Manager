using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions
{
    /// <summary>
    /// Represents a basic operation that can be undone.
    /// </summary>
    public interface IModOperation
    {
        /// <summary>
        /// Revert the effects of this operation.
        /// </summary>
        public void Undo();

        /// <summary>
        /// Called after the transaction succeeds or fails, must clean any resources used by the operation.
        /// </summary>
        public void Dispose() { }
    }
    /// <summary>
    /// A mod operation that is executed synchronously.
    /// </summary>
    public interface IModSyncOperation : IModOperation
    {
        public bool Do();
    }
    /// <summary>
    /// A mod operation that is executed asynchronously.
    /// </summary>
    public interface IModAsyncOperation : IModOperation
    {
        public Task<bool> DoAsync();
    }
}
