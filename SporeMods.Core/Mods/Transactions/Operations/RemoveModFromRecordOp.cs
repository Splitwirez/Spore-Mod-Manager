using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using SporeMods.Core;
using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    public class RemoveModFromRecordOp : IAsyncOperation
    {
        ISporeMod _mod = null;
        public RemoveModFromRecordOp(ISporeMod mod)
        {
            _mod = mod;
        }

        Exception _exception = null;
        public Exception Exception
        {
            get => _exception;
        }

        public async Task<bool> DoAsync()
        {
            try
            {
                return await Task<bool>.Run(() =>
                {
                    //if (ModsManager.InstalledMods.Contains(_mod))
                    ModsManager.InstalledMods.Remove(_mod);
                    return true;
                });
            }
            catch (Exception ex)
            {
                _exception = ex;
                throw ex;
            }
        }

        public void Undo()
        {
            ModsManager.InstalledMods.Add(_mod);
        }

        public void Dispose()
        { }
    }
}
