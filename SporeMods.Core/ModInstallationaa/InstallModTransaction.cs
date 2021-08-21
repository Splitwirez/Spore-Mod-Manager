using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SporeMods.Core.Mods;
using System.Linq;

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

        public override async Task<bool> CommitAsync()
        {
            zip = ZipFile.OpenRead(sporemodPath);

            var modName = Path.GetFileNameWithoutExtension(sporemodPath).Replace(".", "-");

            // 1. Read the mod identity
            var identity = Operation(new ParseIdentityOp(zip, modName)).Identity;
            var unique = identity.Unique;
            string modDirectory = Path.Combine(Settings.ModConfigsPath, unique);

            // 2. Show the configurator, if any
            // Needed to show the configurator
            var managedMod = new ManagedMod(true, identity);
            
            if (managedMod.HasConfigurator)
            {
                await OperationAsync(new ShowInstallerAsyncOp(managedMod));
            }

            // 3. Create the directory
            Operation(new CreateDirectoryOp(modDirectory));

            // I wanted to extract each file on a separate thread, but it seems the zip library doesn't like that
            await Task.Run(() =>
            {
                // 4. Extract the XML file
                Operation(new ExtractXmlIdentityOp(zip, modDirectory, unique, modName));

                // 5. Extract all mod files
                var fileEntries = zip.Entries.Where(x => !x.IsDirectory());
                foreach (ZipArchiveEntry e in fileEntries)
                {
                    Operation(new ExtractFileOp(e, modDirectory));
                }
            });

            // We cannot enable the mod until all files are extracted

            // The instance we have of ManagedMod was temporary, only to show the configurator;
            // recreate it now that we have all the files extracted
            managedMod = Operation(new InitManagedModConfigOp(unique, managedMod.Configuration)).mod;
            identity = managedMod.Identity;

            // 6. Enable the mod and add it to the mod list
            Operation(new AddToModManagerOp(managedMod));
            Operation(new EnableModOp(managedMod));

            // Finally, close the zip file
            zip.Dispose();
            zip = null;
                
            return true;
        }

        public override void Rollback()
        {
            base.Rollback();

            if (zip != null)
            {
                zip.Dispose();
                zip = null;
            }
        }
    }
}
