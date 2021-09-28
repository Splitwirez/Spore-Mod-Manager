using SporeMods.Core.ModTransactions;
using SporeMods.Core.ModTransactions.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
	public class ManualInstalledFile : NotifyPropertyChangedBase, IInstalledMod
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

		public bool IsLegacy { get => _legacy; }

		public event PropertyChangedEventHandler PropertyChanged;

		public bool HasConfigsDirectory => false;

		public string Description => null;
		
		public bool HasDescription => false;

		public Version ModVersion => ModIdentity.UNKNOWN_MOD_VERSION;

		public List<string> Tags { get; } = new List<string>();

		public ModTransaction CreateUninstallTransaction()
		{
			return new UninstallManualModTransaction(this);
		}

		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		//public event EventHandler IsProgressingChanged;

		public override string ToString()
		{
			return DisplayName;
		}

		TaskProgressSignifier _progressSignifier = null;
		public TaskProgressSignifier ProgressSignifier
		{
			get => _progressSignifier;
			set
			{
				_progressSignifier = value;
				NotifyPropertyChanged();
			}
		}

		public bool CanUninstall => ProgressSignifier == null;
		public bool CanReconfigure => false;
		public bool PreventsGameLaunch => ProgressSignifier != null;
	}
}
