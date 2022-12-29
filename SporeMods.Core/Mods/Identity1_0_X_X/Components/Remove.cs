using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public class Remove : ComponentBase
    {
        public static Remove FromXml(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
        {
            var ret = new Remove()
            {
                Files = ComponentBase.GetFiles(element),
                Mod = mod
            };

            return ret;
        }

        public override void Apply(ModTransaction transaction)
        {
            base.Purge(transaction);
        }

        public override void Purge(ModTransaction transaction)
        {

        }
    }
}
