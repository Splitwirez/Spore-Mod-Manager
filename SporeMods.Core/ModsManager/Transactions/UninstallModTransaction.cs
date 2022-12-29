using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using SporeMods.Core.Mods;
using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    public class UninstallModTransaction : ModTransaction
    {
        ISporeMod _mod = null;
        public UninstallModTransaction(ISporeMod mod)
        {
            _mod = mod;
            //ProgressSignifier = new TaskProgressSignifier(_entry.Mod.DisplayName, TaskCategory.Uninstall);
        }

        public override async Task<bool> CommitAsync()
        {
            //ProgressSignifier.Status = TaskStatus.Determinate;
            
            Exception exception = await _mod.PurgeAsync(this);
            if (exception != null)
            {
                Exception = exception;
                return false;
            }


            exception = await _mod.RemoveRecordFilesAsync(this, true);
            if (exception != null)
            {
                Exception = exception;
                return false;
            }


            await OperationAsync(new RemoveModFromRecordOp(_mod));
            Job.Outcome = JobOutcome.Succeeded;
            return true;
        }

        /*public override void Rollback()
        {
            base.Rollback();
            CompleteProgress(TaskStatus.Failed);
        }

        public override void Dispose()
        {
            base.Dispose();
            CompleteProgress(TaskStatus.Succeeded);
        }*/
    }
}