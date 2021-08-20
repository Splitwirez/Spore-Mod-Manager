using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SporeMods.Core.Mods;

namespace SporeMods.Core.ModInstallationaa
{
    public class InstallModTransaction : ModTransaction
    {
        private string sporemodPath;
        // Because we might need to do undo, and some operators might need the zip, we can't do the 'using...' thing
        private ZipArchive zip;

        public InstallModTransaction(string sporemodPath)
        {
            this.sporemodPath = sporemodPath;
        }

        public override async Task<bool> DoAsync()
        {
            zip = ZipFile.OpenRead(sporemodPath);

            // 1. Read the mod identity

            var identity = operation(new ParseIdentityOp(zip)).Identity;

            // 2. Show the configurator, if any
            // Needed to show the configurator
            var modName = Path.GetFileNameWithoutExtension(sporemodPath).Replace(".", "-");
            var managedMod = new ManagedMod(modName, true, identity);
            
            if (managedMod.HasConfigurator)
            {
                await operationAsync(new ShowInstallerOp(managedMod));
            }

            zip.Dispose();
            zip = null;
                
            return true;
        }

        public override void Undo()
        {
            base.Undo();

            if (zip != null)
            {
                zip.Dispose();
                zip = null;
            }
        }
    }
}
