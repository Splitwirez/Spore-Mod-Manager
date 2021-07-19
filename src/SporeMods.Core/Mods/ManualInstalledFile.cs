using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SporeMods.Mods.Identity;
using SporeMods.Mods.Identity.V1;
using SporeMods.Utils;

namespace SporeMods.Mods
{
	public class ManualInstalledFile : IInstalledMod, INotifyPropertyChanged
	{
		bool _legacy = false;
		public ManualInstalledFile(string fileName, ComponentGameDir location, bool legacy)
		{
			RealName = fileName;
			Location = location;
			_legacy = legacy;
		}

		public ComponentGameDir Location { get; } = ComponentGameDir.GalacticAdventures;

		public string DisplayName => Path.GetFileNameWithoutExtension(RealName);

		public string Unique => RealName;

		public string RealName { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public bool HasConfigsDirectory => false;

		public string Description => null;

		public Version ModVersion => ModIdentity.UNKNOWN_MOD_VERSION;

		public List<string> Tags { get; } = new List<string>();

		public async Task<bool> UninstallModAsync()
		{
			var task = new Task<bool>(() =>
			{
				try
				{
					FileWrite.SafeDeleteFile(FileWrite.GetFileOutputPath(Location, RealName, _legacy));
					ModsManager.RunOnMainSyncContext(state => ModsManager.InstalledMods.Remove(this));
					return true;
				}
				catch (Exception ex)
				{
					MessageDisplay.RaiseError(new ModErrorEventArgs(ex));
					return false;
				}
			});
			task.Start();
			return await task;
		}

		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event EventHandler IsProgressingChanged;

		public override string ToString()
		{
			return DisplayName;
		}
	}
}
