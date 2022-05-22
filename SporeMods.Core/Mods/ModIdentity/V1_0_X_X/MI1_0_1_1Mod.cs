﻿using System;
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
            var hasCustomInstallerAttr = xmlRoot.Attribute("hasCustomInstaller");
            if ((hasCustomInstallerAttr != null) && bool.TryParse(hasCustomInstallerAttr.Value, out bool hasCustomInstaller))
            {
                HasSettings = hasCustomInstaller;
            }
        }
    }
}