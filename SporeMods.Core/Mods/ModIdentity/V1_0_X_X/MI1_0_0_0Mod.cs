using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public class MI1_0_0_0Mod : MI1_0_X_XMod
    {
        public MI1_0_0_0Mod(string recordDirName, string unique, List<string> fileNames)
            : base(recordDirName, unique, fileNames)
        {
            HasSettings = true;
        }

        protected override void ReadIdentityRoot(XElement xmlRoot)
        {
            var compatOnlyAttr = xmlRoot.Attribute("compatOnly");
            if ((compatOnlyAttr != null) && bool.TryParse(compatOnlyAttr.Value, out bool compatOnly))
            {
                HasSettings = !compatOnly;
            }
        }
    }
}
