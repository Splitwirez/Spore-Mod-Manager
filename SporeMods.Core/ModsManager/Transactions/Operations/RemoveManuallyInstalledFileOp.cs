/*using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Text;

using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    /// <summary>
    /// Removes a file that is detected as a manually installed file
    /// </summary>
    public class RemoveManuallyInstalledFileOp : IAsyncOperation
    {
        public readonly string fileName;
        public readonly ComponentGameDir targetLocation;
        private ManualInstalledFile manuallyInstalledFile;
        private int manuallyInstalledFileIndex;

        public RemoveManuallyInstalledFileOp(string fileName, ComponentGameDir targetLocation)
        {
            this.fileName = fileName;
            this.targetLocation = targetLocation;
        }

        public bool Do()
        {
            manuallyInstalledFile = ModsManager.GetManuallyInstalledFile(fileName, targetLocation);
            if (manuallyInstalledFile != null)
            {
                manuallyInstalledFileIndex = ModsManager.InstalledMods.IndexOf(manuallyInstalledFile);
                ModsManager.RemoveMod(manuallyInstalledFile);
            }
            return true;
        }

        public void Undo()
        {
            if (manuallyInstalledFile != null)
            {
                ModsManager.InsertMod(manuallyInstalledFileIndex, manuallyInstalledFile);
            }
        }
    }
}*/