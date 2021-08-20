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

        public InstallModTransaction(string sporemodPath)
        {
            this.sporemodPath = sporemodPath;
        }

        public override async Task<bool> DoAsync()
        {
            using (ZipArchive zip = ZipFile.OpenRead(sporemodPath))
            {
                var identity = operation(new ParseIdentityOp(zip)).Identity;
                // Needed to show the configurator
                var modName = Path.GetFileNameWithoutExtension(sporemodPath).Replace(".", "-");
                var managedMod = new ManagedMod(modName, true, identity);
                await operationAsync(new ShowInstallerOp(managedMod));
                
                return true;
            }
        }
    }
}
