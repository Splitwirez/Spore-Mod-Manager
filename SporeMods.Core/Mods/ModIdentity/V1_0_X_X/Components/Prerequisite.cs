using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public class Prerequisite : ComponentBase
    {
        public static Prerequisite FromXml(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
        {
            var ret = new Prerequisite()
            {
                Files = ComponentBase.GetFiles(element),
                Mod = mod
            };
            EnsureFiles(ret.Files.Keys, fileNames, mod.Unique, mod.DisplayName.ToString());

            return ret;
        }
    }
}
