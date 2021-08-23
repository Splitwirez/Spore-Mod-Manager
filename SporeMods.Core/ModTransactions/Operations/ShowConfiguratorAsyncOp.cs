using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions.Operations
{
	/// <summary>
	/// Shows the configurator of a mod, allowing the user to modify the current mod configuration.
	/// The changes are not saved in a file, however. Undoing this reassigns the old configuration (without saving it to a file neither).
	/// The operation will fail if the mod does not have a configurator.
	/// </summary>
    public class ShowConfiguratorAsyncOp : IModAsyncOperation
	{
		public readonly ManagedMod mod;
		private ModConfiguration originalConfig;
		
		public ShowConfiguratorAsyncOp(ManagedMod mod)
        {
			this.mod = mod;
        }

		public async Task<bool> DoAsync()
        {
			if (mod.HasConfigurator)
			{
				originalConfig = new ModConfiguration(mod.Configuration);
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
			mod.Configuration = originalConfig;
		}
    }
}
