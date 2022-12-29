using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SporeMods.Core;
using SporeMods.Core.Mods;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
using System.IO;

namespace SporeMods.ViewModels
{
	public class GameEntrySettingsViewModel : NotifyPropertyChangedBase
	{
		public StringPresetOrCustomSettingViewModel<CommandLineState> CommandLineStates { get; }
		= new StringPresetOrCustomSettingViewModel<CommandLineState>(
			"Settings!GameEntry!StartupEditor!Header",
			"Settings!GameEntry!StartupEditor!ToolTip!Content",
			"Settings!GameEntry!StartupEditor!ToolTip!Warning",
			() => ModsManager.Instance.CommandLineStates.FirstOrDefault(x => x.StateIdentifier.Equals(Settings.GameState)),
			v => Settings.GameState = v,
			() => Settings.UseCustomGameState,
			b => Settings.UseCustomGameState = b,
			st => st.StateIdentifier,
			s => ModsManager.Instance.CommandLineStates.FirstOrDefault(x => x.StateIdentifier.Equals(s)),
			ModsManager.Instance.CommandLineStates,
			true
		);

		public string AdditionalCommandLineOptions
		{
			get => Settings.CommandLineOptions;
			set
			{
				Settings.CommandLineOptions = value;
				NotifyPropertyChanged();
			}
		}


		public GameEntrySettingsViewModel()
			: base()
		{ }
	}
}
