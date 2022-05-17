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
    public partial class PreIdentityMod
    {
        public async Task<Exception> RemoveRecordFilesAsync(ModTransaction transaction, bool removeConfig)
        {
            string recordDirPath = Path.Combine(Settings.ModConfigsPath, RecordDirName);

            return await Task<bool>.Run(() =>
            {
                try
                {
                    double progressStep = JobBase.PROGRESS_OVERALL_MAX / (PackageNames.Count() + DllNames.Count() + 1);

                    string targetPath;
                    foreach (string name in PackageNames)
                    {
                        targetPath = Path.Combine(recordDirPath, name);

                        transaction.Operation(new DeleteFileOp(targetPath));
                        transaction.Job.ActivityRangeProgress += progressStep;
                    }

                    foreach (string name in DllNames)
                    {
                        targetPath = Path.Combine(recordDirPath, name);

                        transaction.Operation(new DeleteFileOp(targetPath));
                        transaction.Job.ActivityRangeProgress += progressStep;
                    }

                    targetPath = Path.Combine(recordDirPath, ModConstants.ID_XML_FILE_NAME);
                    transaction.Operation(new DeleteFileOp(targetPath));
                    transaction.Job.ActivityRangeProgress += progressStep;

                    if (removeConfig)
                    {
                        transaction.Operation(new DeleteDirectoryOp(recordDirPath));
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
