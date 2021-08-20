using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModInstallationaa
{
    public interface IModOperation
    {
        public void Undo();
    }
    public interface IModSyncOperation : IModOperation
    {
        public bool Do();
    }
    public interface IModAsyncOperation : IModOperation
    {
        public Task<bool> DoAsync();
    }
}
