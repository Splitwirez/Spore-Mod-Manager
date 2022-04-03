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
    public class InstallModTransaction : ModTransaction
    {
        ModJobBatchModEntry _entry = null;
        public InstallModTransaction(ModJobBatchModEntry entry)
        {
            _entry = entry;
            //ProgressSignifier = new TaskProgressSignifier(_entry.Mod.DisplayName, TaskCategory.Install);
        }

        public override async Task<bool> CommitAsync()
        {
            await OperationAsync(new AddModToRecordOp(_entry.Mod));
            //ProgressSignifier.Status = TaskStatus.Determinate;
            ZipArchive archive = null;
            Job.TrySetActivityRange(0, JobBase.PROGRESS_OVERALL_MAX / 2);
            IAsyncOperation extractOperation = null;
            await Task.Run(() =>
            {
                string extension = Path.GetExtension(_entry.ModPath);

                if (extension.Equals(ModConstants.MOD_FILE_EX_SPOREMOD, StringComparison.OrdinalIgnoreCase))
                    archive = ZipFile.OpenRead(_entry.ModPath);
                
                extractOperation = _entry.Mod.GetExtractRecordFilesAsyncOp(this, _entry.ModPath, archive);
            });

            await OperationAsync(extractOperation);

            archive?.Dispose();

            if (extractOperation.Exception != null)
            {
                Exception = extractOperation.Exception;
                return false;
            }

            
            Job.TrySetActivityRange(JobBase.PROGRESS_OVERALL_MAX / 2, JobBase.PROGRESS_OVERALL_MAX);
            IAsyncOperation applyOperation = null;
            await Task.Run(() =>
            {
                applyOperation = _entry.Mod.GetApplyAsyncOp(this);
                //ProgressSignifier.Progress = 100;
            });

            await OperationAsync(applyOperation);
            
            if (applyOperation.Exception != null)
            {
                Exception = applyOperation.Exception;
                return false;
            }


            return true;
        }

        public override void Rollback()
        {
            base.Rollback();
            if (_entry == null)
                return;
            else if (_entry.Mod == null)
                return;
            
            /*if (ModsManager.InstalledMods.Contains(_entry.Mod))
            {
                ModsManager.InstalledMods.Remove(_entry.Mod);
            }*/
            //ModsManager.AddMod(_entry.Mod);
            //CompleteProgress(TaskStatus.Failed);
        }

        /*public override void Dispose()
        {
            base.Dispose();
            CompleteProgress(TaskStatus.Succeeded);
        }*/
    }
}