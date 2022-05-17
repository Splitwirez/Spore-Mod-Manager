using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
    public abstract partial class MI1_0_X_XMod : NotifyPropertyChangedBase, ISporeMod
    {
        public Task<Exception> RemoveRecordFilesAsync(ModTransaction transaction, bool removeConfig)
        {
            throw new NotImplementedException();
        }
    }
}
