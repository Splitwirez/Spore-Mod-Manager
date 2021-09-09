using SporeMods.Core.ModTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
	/// <summary>
	/// An abstract interface representing a mod installed on the manager.
	/// </summary>
	public interface IInstalledMod
	{
		/// <summary>
		/// Queues this mod to be uninstalled using a transaction, returning the transaction exception if something failed.
		/// This method should not throw any exception.
		/// </summary>
		/// <returns></returns>
		ModTransaction CreateUninstallTransaction();

		bool HasConfigsDirectory { get; }

		/// <summary>
		/// The name the user knows this mod by. Only used in UI.
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// The unique identifier for this mod. This value should never change between versions of a mod.
		/// </summary>
		string Unique { get; }

		/// <summary>
		/// The name of the installation directory used by this mod.
		/// </summary>
		string RealName { get; }

		/// <summary>
		/// The mod's in-UI description.
		/// </summary>
		string Description { get; }

		List<string> Tags { get; }

		Version ModVersion { get; }
	}
}
