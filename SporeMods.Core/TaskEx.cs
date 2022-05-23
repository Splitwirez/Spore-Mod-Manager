using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core
{
    public static class TaskEx<TResult>
    {
        public static async Task<TResult> Run<TResult>(Func<TResult> function)
        {
            return await Task<TResult>.Run(function)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        if (t.Exception is AggregateException aggregate)
                        {
                            var inner = aggregate.GetBaseException();
                            if (inner == null)
                                inner = aggregate.InnerException;
                            throw new Exception(inner.Message, inner);
                        }
                        else
                            throw t.Exception;
                    }
                    return t.Result;
                }
            );
        }
    }

    public static class TaskEx
    {
        public static async Task Run(Action action)
        {
            await Task.Run(action)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        throw t.Exception;
                }
            );
        }
    }
}
