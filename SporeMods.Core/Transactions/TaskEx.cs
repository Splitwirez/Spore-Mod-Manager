using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Transactions
{
    public static class TaskEx
    {
        /*public static readonly Action<Task<bool>> CONTINUE_OPERATION =
            t =>
            {
                if (t.IsFaulted)
                {
                    operation.Exception = t.Exception;
                }
            };*/
        public static async Task<bool> BoolTaskEx(this IOperation operation, Func<bool> function)
        {
            return await Task<bool>.Run(function)
                .ContinueWith<bool>(t =>
                {
                    if (t.IsFaulted)
                    {
                        operation.Exception = t.Exception;
                        return false;
                    }
                    return true;
                });
        }
    }
}
