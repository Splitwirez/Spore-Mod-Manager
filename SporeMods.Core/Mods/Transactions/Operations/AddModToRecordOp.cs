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
    public class AddModToRecordOp : IAsyncOperation
    {
        ISporeMod _mod = null;
        public AddModToRecordOp(ISporeMod mod)
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
                    ModsManager.InstalledMods.Add(_mod);
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
            if (ModsManager.InstalledMods.Contains(_mod))
                ModsManager.InstalledMods.Remove(_mod);
        }

        public void Dispose()
        { }
    }
}
