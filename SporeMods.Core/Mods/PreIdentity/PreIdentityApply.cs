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
        public async Task<Exception> ApplyAsync(ModTransaction transaction)
        {   
            return await Task<Exception>.Run(() =>
            {
                try
                {
                    string modConfigsSubdir = Path.Combine(Settings.ModConfigsPath, RecordDirName);
                        
                    double progressStep = JobBase.PROGRESS_OVERALL_MAX / (PackageNames.Count() + DllNames.Count());
                    foreach (string name in PackageNames)
                    {
                        string fromPath = Path.Combine(modConfigsSubdir, name);
                        string toPath = Path.Combine(GameInfo.GalacticAdventuresData, name);

                        transaction.Operation(new CopyFileOp(fromPath, toPath));
                        transaction.Job.ActivityRangeProgress += progressStep;
                    }

                    foreach (string name in DllNames)
                    {
                        string fromPath = Path.Combine(modConfigsSubdir, name);
                        string toPath = Path.Combine(Settings.LegacyLibsPath, name);

                        transaction.Operation(new CopyFileOp(fromPath, toPath));
                        transaction.Job.ActivityRangeProgress += progressStep;
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
