using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModInstallationaa
{
    public interface IModOperation
    {
        public bool Do()
        {
            throw new NotSupportedException("This step can only be called asynchronously.");
        }
        public Task<bool> DoAsync() 
        {
            throw new NotSupportedException("This step can only be called synchronously.");
        }
        public void Undo();
    }
}
