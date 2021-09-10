using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions.Operations
{
    /// <summary>
    /// Executes a transaction, returning false if the transaction fails. Undoing this rolls back the transaction.
    /// </summary>
    public class ExecuteTransactionAsyncOp : IModOperation, IModAsyncOperation
    {
        public readonly ModTransaction transaction;
        // If the transaction failed, the exception that caused it
        public Exception failureException;

        public ExecuteTransactionAsyncOp(ModTransaction transaction)
        {
            this.transaction = transaction;
        }

        public async Task<bool> DoAsync()
        {
            try
            {
                if (!await transaction.CommitAsync())
                {
                    failureException = new ModTransactionCommitException(TransactionFailureCause.CommitRejected, null, null);
                    return false;
                }
                return true;
            }
            // There is a specific exception for when a transaction fails, so theoretically we should only need to catch ModTransactionCommitException
            // However, we also want to rollback if there was an unexpected exception while executing the code
            // (although that is the developers fault!)
            catch (Exception e)
            {
                failureException = e;
                return false;
            }
        }

        public void Undo()
        {
            transaction.Rollback();
        }

        public void Dispose()
        {
            transaction.Dispose();
        }
    }
}
