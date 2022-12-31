using SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
    public partial class MI1_0_X_XMod
    {
        public override async Task<Exception> ApplyAsync(ModTransaction transaction)
        {
            void applyTo(IEnumerable<ComponentBase> components)
            {
                foreach (ComponentBase cmp in components)
                {
                    cmp.Apply(transaction);
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
