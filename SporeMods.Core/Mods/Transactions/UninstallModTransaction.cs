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
            IAsyncOperation purgeOperation = await Task<IAsyncOperation>.Run(() => _mod.GetPurgeAsyncOp(this));

            await OperationAsync(purgeOperation);

            if (purgeOperation.Exception != null)
            {
                Exception = purgeOperation.Exception;
                return false;
            }


            IAsyncOperation removeFromRecordOperation = await Task<IAsyncOperation>.Run(() => _mod.GetRemoveRecordFilesAsyncOp(this, true));
            await Task.Run(() =>
            {
                removeFromRecordOperation = _mod.GetRemoveRecordFilesAsyncOp(this, true);
            });

            await OperationAsync(removeFromRecordOperation);
            
            if (removeFromRecordOperation.Exception != null)
            {
                Exception = removeFromRecordOperation.Exception;
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