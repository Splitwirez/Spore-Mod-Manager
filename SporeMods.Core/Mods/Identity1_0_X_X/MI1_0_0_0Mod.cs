using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public class MI1_0_0_0Mod : MI1_0_X_XMod
    {
        protected override void ReadIdentityRoot(XElement xmlRoot)
        {
            base.ReadIdentityRoot(xmlRoot);

            /*var compatOnlyAttr = xmlRoot.Attribute("compatOnly");
            if ((compatOnlyAttr != null) && bool.TryParse(compatOnlyAttr.Value, out bool compatOnly))
            {
                HasSettings = !compatOnly;
            }*/
            if (xmlRoot.TryGetAttributeBool("compatOnly", out bool compatOnly))
                HasSettings = !compatOnly;
            else
                HasSettings = true;
        }

        protected override Version EnsureIdentityVersion(XDocument doc)
        {
            Version identityVersion = base.EnsureIdentityVersion(doc);
            if (ModUtils.IsID_1_0_1_X(identityVersion))
                throw new ModException(true);

            return identityVersion;
        }
    }
}