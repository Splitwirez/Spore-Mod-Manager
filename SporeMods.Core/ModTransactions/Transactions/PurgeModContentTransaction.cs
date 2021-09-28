using SporeMods.Core.Mods;
using SporeMods.Core.ModTransactions.Operations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions.Transactions
{
	/// <summary>
	/// [PARTIAL NYI]Queues all of this mod's enabled files for removal, and all disabled files for installation
	/// </summary>
	public class PurgeModContentTransaction : ModTransaction
	{
		public readonly ManagedMod mod;

		public PurgeModContentTransaction(ManagedMod mod)
		{
			this.mod = mod;
		}

		public override async Task<bool> CommitAsync()
		{
			mod.Progress = 0;

			// 1. Delete all files
			double progressRange = 100.0;
			var filesToDelete = mod.GetFilePathsToRemove();
			foreach (var file in filesToDelete)
			{
				Operation(new SafeDeleteFileOp(file));
				mod.Progress += progressRange / filesToDelete.Count;
			}

			// 2. Change the configuration and save it
			var newConfig = new ModConfiguration(mod.Configuration);
			newConfig.IsEnabled = false;
			Operation(new ChangeModConfigurationOp(mod, newConfig));

			//TODO this shouldn't be here?
			mod.Progress = 0;

			return true;
		}
	}
}
