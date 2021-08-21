﻿using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModInstallationaa
{
    public class InstallLoosePackageTransaction : ModTransaction
    {
        private readonly string modPath;

        public InstallLoosePackageTransaction(string modPath)
        {
            this.modPath = modPath;
        }

        public override async Task<bool> CommitAsync()
        {
            if (Settings.AllowVanillaIncompatibleMods)
            {
                string name = Path.GetFileName(modPath);
                string noExtensionName = Path.GetFileNameWithoutExtension(modPath).Replace(".", "-");

                string dir = Path.Combine(Settings.ModConfigsPath, noExtensionName);

                // 1. We create the folder to store our mod in SMM
                Operation(new CreateDirectoryOp(dir));

                // 2. Generate a basic XML identity for it
                Operation(new ExtractXmlIdentityOp(null, dir, noExtensionName, noExtensionName));

                var mod = new ManagedMod(noExtensionName, true)
                {
                    Progress = 0,
                    IsProgressing = true
                };

                // 3. Add the mod to the manager and copy the file
                Operation(new AddToModManagerOp(mod, false));
                Operation(new SafeCopyFileOp(modPath, Path.Combine(dir, name)));

                //TODO ModsManager.RemoveMatchingManuallyInstalledFile(name, ComponentGameDir.GalacticAdventures);
				mod.Progress++;

                Operation(new EnableModOp(mod));

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
