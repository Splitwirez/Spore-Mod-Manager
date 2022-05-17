using SporeMods.Core.Transactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
    public abstract partial class MI1_0_X_XMod : NotifyPropertyChangedBase, ISporeMod
    {
        public async Task<Exception> ExtractRecordFilesAsync(ModTransaction transaction, string inPath, ZipArchive archive = null)
        {
            return await Task<Exception>.Run(async () =>
            {
                try
                {
                    string recordDirPath = Path.Combine(Settings.ModConfigsPath, RecordDirName);
                    transaction.Operation(new CreateDirectoryOp(recordDirPath));


                    double progressQuantity = JobBase.PROGRESS_OVERALL_MAX / (archive.Entries.Count + 1);


                    foreach (var entry in archive.Entries)
                    {
                        await transaction.OperationAsync(new ExtractFileOp(entry, Path.Combine(recordDirPath, Path.GetFileName(entry.Name))));
                        transaction.Job.ActivityRangeProgress += progressQuantity;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            });
        }
    }
}
