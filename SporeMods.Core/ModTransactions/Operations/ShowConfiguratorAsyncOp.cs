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
		public readonly ManagedMod Mod;
		private ModConfiguration _originalConfig;
		internal bool UserCancelled = false;
		
		readonly bool _configuring;
		
		public ShowConfiguratorAsyncOp(ManagedMod mod, bool configuring)
        {
			Mod = mod;
			_configuring = configuring;
        }

		public async Task<bool> DoAsync()
        {
			if (Mod.HasConfigurator)
			{
				_originalConfig = new ModConfiguration(Mod.Configuration);
				var configurator = Mod.Identity.CreateConfigurator(_configuring);
				Console.WriteLine("wwwww");
				UserCancelled = !await Modal.Show(configurator);
				Console.WriteLine("hhhhh");
				return true;
			}
			else
			{
				// This can only happen if we, developers, didn't read the documentation and messed up. No need for a dialog.
				throw new Exception("This mod does not have a configurator!");
			}
		}

        public void Undo()
        {
			Mod.Configuration = _originalConfig;
		}
    }
}
