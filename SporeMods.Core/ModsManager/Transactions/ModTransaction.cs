using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SporeMods.Core.Transactions;
using SporeMods.Core.Mods;
using System.Linq;

namespace SporeMods.Core.Mods
{
    public abstract class ModTransaction : TransactionBase<ModJob>
    {
        public override void Dispose()
        {
            base.Dispose();
            Job.Outcome = JobOutcome.Succeeded;
        }
    }
}