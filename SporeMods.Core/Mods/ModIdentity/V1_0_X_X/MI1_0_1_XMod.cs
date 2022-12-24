using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public class MI1_0_1_XMod : MI1_0_X_XMod
    {
        protected override void ReadIdentityRoot(XElement xmlRoot)
        {
            base.ReadIdentityRoot(xmlRoot);

            if (xmlRoot.TryGetAttributeBool("hasCustomInstaller", out bool hasCustomInstaller))
                HasSettings = hasCustomInstaller;
            else
                HasSettings = false;

            if (xmlRoot.TryGetAttributeBool("isExperimental", out bool isExperimental))
                IsExperimental = isExperimental;

            if (xmlRoot.TryGetAttributeBool("requiresGalaxyReset", out bool requiresGalaxyReset))
                RequiresGalaxyReset = requiresGalaxyReset;

            if (xmlRoot.TryGetAttributeBool("causesSaveDataDependency", out bool causesSaveDataDependency))
                CausesSaveDataDependency = causesSaveDataDependency;
        }

        protected override Version EnsureIdentityVersion(XDocument doc)
        {
            Version identityVersion = base.EnsureIdentityVersion(doc);
            if (!ModUtils.IsID_1_0_1_X(identityVersion))
                throw new ModException(true);
                
            return identityVersion;
        }
    }
}