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
    public class AddModToRecordOp : AsyncOperationBase
    {
        ISporeMod _mod = null;
        public AddModToRecordOp(ISporeMod mod)
        {
            _mod = mod;
        }

        public override async Task<bool> DoAsync()
        {
            try
            {
                return await this.BoolTaskEx(() =>
                {
                    ModsManager.InstalledMods.Add(_mod);
                    return true;
                });
            }
            catch (Exception ex)
            {
                Exception = ex;
                throw ex;
            }
        }

        public override void Undo()
        {
            if (ModsManager.InstalledMods.Contains(_mod))
                ModsManager.InstalledMods.Remove(_mod);
        }
    }
}
