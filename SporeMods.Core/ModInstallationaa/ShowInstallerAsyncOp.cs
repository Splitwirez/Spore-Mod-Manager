using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModInstallationaa
{
    public class ShowInstallerAsyncOp : IModAsyncOperation
	{
		private ManagedMod mod;
		
		public ShowInstallerAsyncOp(ManagedMod mod)
        {
			this.mod = mod;
        }

		public async Task<bool> DoAsync()
        {
			if (mod.HasConfigurator)
			{
				return await ModsManager.Instance.ShowModConfigurator(mod);
			}
			else
			{
				// This can only happen if we, developers, didn't read the documentation and messed up. No need for a dialog.
				throw new Exception("This mod does not have a configurator!");
			}
		}

        public void Undo()
        {
        }
    }
}
