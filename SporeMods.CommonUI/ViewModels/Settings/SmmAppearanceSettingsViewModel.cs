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
	public class SmmAppearanceSettingsViewModel : NotifyPropertyChangedBase
	{
		public bool AreTheLightsTurnedOn
		{
			get => !Settings.ShaleDarkTheme;
			set
			{
				Settings.ShaleDarkTheme = !value;
				NotifyPropertyChanged();
			}
		}

		public bool UseCSDs
		{
			get => Settings.UseCustomWindowDecorations;
			set
			{
				Settings.UseCustomWindowDecorations = value;
				NotifyPropertyChanged();
			}
		}

		public bool ForceWPFSoftwareRendering
		{
			get => Settings.ForceSoftwareRendering;
			set
			{
				bool prevVal = Settings.ForceSoftwareRendering;
				if (prevVal != value)
				{
					string swrForcePath = Settings.ForceSoftwareRenderingOverrideFilePath;
					bool swrForceExists = File.Exists(swrForcePath);
					if (!swrForceExists)
						Settings.ForceSoftwareRendering = value;
					DialogBox.ShowAsync(
						swrForceExists
							? $"Delete '{swrForcePath}' (PLACEHOLDER) (NOT LOCALIZED)"
							: "Your change to this setting will take effect next time the Spore Mod Manager is restarted. (NOT LOCALIZED)"
						, "Force WPF software rendering (PLACEHOLDER) (NOT LOCALIZED)"
					);
				}
				NotifyPropertyChanged();
			}
		}


		public static SmmAppearanceSettingsViewModel Instance { get; } = new SmmAppearanceSettingsViewModel();


		private SmmAppearanceSettingsViewModel()
			: base()
		{ }
	}
}
