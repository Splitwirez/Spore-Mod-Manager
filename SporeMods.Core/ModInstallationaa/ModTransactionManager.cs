using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModInstallationaa
{
    public static class ModTransactionManager
    {
        public static async Task<bool> ExecuteAsync(ModTransaction transaction)
        {
            try
            {
                if (!await transaction.CommitAsync())
                {
                    transaction.Rollback();
                    return false;
                }
                return true;
            }
            // There is a specific exception for when a transaction fails
            // but we also want to rollback if there was an unexpected exception while executing the code
            // (although that is the developers fault!)
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                transaction.Rollback();
                return false;
            }
        }
    }
}
