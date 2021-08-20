using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SporeMods.Core.ModInstallationaa
{
    public abstract class ModTransaction
    {
        private ConcurrentStack<IModOperation> steps;

        /// <summary>
        /// Adds a step to be executed synchronously, immediately executing it.
        /// </summary>
        /// <param name="step"></param>
        protected T operation<T>(T step) where T : IModOperation
        {
            steps.Push(step);
            step.Do();
            return step;
        }

        /// <summary>
        /// Adds a step to be executed asynchronously, and begins executing it.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        protected Task<T> operationAsync<T>(T step) where T : IModOperation
        {
            steps.Push(step);
            var task = new Task<T>(() =>
            {
                steps.Push(step);
                step.Do();
                return step;
            });
            task.Start();
            return task;
        }

        public abstract Task<bool> DoAsync();
    }
}
