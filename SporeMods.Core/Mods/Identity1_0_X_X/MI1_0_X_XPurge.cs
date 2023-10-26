using SporeMods.Core;
using SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents;
using SporeMods.Core.Transactions;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
    public partial class MI1_0_X_XMod
    {
        public override async Task<Exception> PurgeAsync(ModTransaction transaction)
        {
            double progressStep = JobBase.PROGRESS_OVERALL_MAX / AllComponents.Count;
            void applyTo(IEnumerable<ComponentBase> components)
            {
                foreach (ComponentBase cmp in components)
                {
                    cmp.Purge(transaction);
                    applyTo(cmp.Children);
                    transaction.Job.ActivityRangeProgress += progressStep;
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
