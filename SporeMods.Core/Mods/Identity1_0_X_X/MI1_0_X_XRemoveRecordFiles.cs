using SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using SporeMods.Core.Transactions;
using System.IO;

namespace SporeMods.Core.Mods
{
    public partial class MI1_0_X_XMod
    {
        public override async Task<Exception> RemoveRecordFilesAsync(ModTransaction transaction, bool removeConfig)
        {
            double progressStep = JobBase.PROGRESS_OVERALL_MAX / AllComponents.Count;
            void applyTo(IEnumerable<ComponentBase> components)
            {
                foreach (ComponentBase cmp in components)
                {
                    
                    foreach (string name in cmp.FileNames)
                    {
                        string recordDirPath = Path.Combine(Settings.ModConfigsPath, RecordDirName);
                        string targetPath = Path.Combine(recordDirPath, name);

                        transaction.Operation(new DeleteFileOp(targetPath));
                        transaction.Job.ActivityRangeProgress += progressStep;
                    }
                    applyTo(cmp.Children);
                }
            }

            return await Task<Exception>.Run(() =>
            {
                applyTo(AllComponents);
                return (Exception)null;
            });
        }
    }
}
