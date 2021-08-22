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

            // 1. Read the mod identity and validate it
            var identityOp = Operation(new ParseIdentityOp(zip, modName));
            var identity = identityOp.Identity;
            if (!identityOp.IsGeneratedIdentity)
            {
                Operation(new ValidateModOp(identity));
            }
            var unique = identity.Unique;
            string modDirectory = Path.Combine(Settings.ModConfigsPath, unique);

            // 2. Show the configurator, if any
            // Needed to show the configurator
            var managedMod = new ManagedMod(true, identity)
            {
                Progress = 0,
                IsProgressing = true
            };

            if (managedMod.HasConfigurator)
            {
                await OperationAsync(new ShowInstallerAsyncOp(managedMod));
            }

            // 3. Create the directory
            Operation(new CreateDirectoryOp(modDirectory));

            // I wanted to extract each file on a separate thread, but it seems the zip library doesn't like that
            await Task.Run(() =>
            {
                // We don't increase all the progress, because EnableMod() will be called
                double totalProgress = 50.0;
                var fileEntries = zip.Entries.Where(x => !x.IsDirectory());
                var numFiles = fileEntries.Count() + 1;

                // 4. Extract the XML file
                Operation(new ExtractXmlIdentityOp(zip, modDirectory, unique, modName));
                managedMod.Progress += totalProgress / numFiles;

                // 5. Extract all mod files
                foreach (ZipArchiveEntry e in fileEntries)
                {
                    Operation(new ExtractFileOp(e, modDirectory));
                    managedMod.Progress += totalProgress / numFiles;
                }
            });

            // We cannot enable the mod until all files are extracted

            // The instance we have of ManagedMod was temporary, only to show the configurator;
            // recreate it now that we have all the files extracted
            managedMod = Operation(new InitManagedModConfigOp(unique, managedMod)).mod;
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
