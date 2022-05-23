using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public class MI1_0_1_1Mod : MI1_0_X_XMod
    {
        public MI1_0_1_1Mod(string recordDirName, string unique, List<string> fileNames)
            : base(recordDirName, unique, fileNames)
        { }

        protected override void ReadIdentityRoot(XElement xmlRoot)
        {
            if (xmlRoot.TryGetAttributeBool("hasCustomInstaller", out bool hasCustomInstaller))
                HasSettings = hasCustomInstaller;

            if (xmlRoot.TryGetAttributeBool("isExperimental", out bool isExperimental))
                IsExperimental = isExperimental;

            if (xmlRoot.TryGetAttributeBool("requiresGalaxyReset", out bool requiresGalaxyReset))
                RequiresGalaxyReset = requiresGalaxyReset;

            if (xmlRoot.TryGetAttributeBool("causesSaveDataDependency", out bool causesSaveDataDependency))
                CausesSaveDataDependency = causesSaveDataDependency;
        }
    }
}
