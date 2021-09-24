using System;
using System.Collections.Generic;
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
	public class GameWindowSettingsViewModel : NotifyPropertyChangedBase
	{
		public bool OverrideWindowingMode
		{
			get => Settings.ForceGameWindowingMode;
			set
			{
				Settings.ForceGameWindowingMode = value;
				NotifyPropertyChanged();
			}
		}


		bool _allowSet = false;


		bool _windowed = false;
		public bool Windowed
		{
			get => _windowed;
			set
			{
				_windowed = value;
				NotifyPropertyChanged();

				if (_windowed)
					WindowMode = 0;
			}
		}


		bool _fullscreen = false;
		public bool Fullscreen
		{
			get => _fullscreen;
			set
			{
				_fullscreen = value;
				NotifyPropertyChanged();

				if (_fullscreen)
					WindowMode = 1;
			}
		}


		bool _borderless = false;
		public bool Borderless
		{
			get => _borderless;
			set
			{
				_borderless = value;
				NotifyPropertyChanged();

				if (_borderless)
					WindowMode = 2;
			}
		}




		public bool OverrideWindowResolution
		{
			get => Settings.ForceGameWindowBounds;
			set
			{
				Settings.ForceGameWindowBounds = value;
				NotifyPropertyChanged();
			}
		}

		public bool WindowedAutoWindowResolution
		{
			get => Settings.AutoGameWindowBounds;
			set
			{
				Settings.AutoGameWindowBounds = value;
				NotifyPropertyChanged();
			}
		}

		public FuncCommand<object> ChoosePreferredMonitorCommand { get; }
			= new FuncCommand<object>(o =>
			{
				//TODO: Implement ChoosePreferredMonitorCommand
				DialogBox.ShowAsync("Specifying a preferred monitor is not yet reimplemented. (PLACEHOLDER) (NOT LOCALIZED)");
			});


		public GameWindowSettingsViewModel()
		{
			if (WindowMode == 0)
				Windowed = true;
			else if (WindowMode == 1)
				Fullscreen = true;
			else if (WindowMode == 2)
				Borderless = true;

			_allowSet = true;
		}

		int WindowMode
        {
			get => Settings.ForceWindowedMode;
			set
            {
				if (
						_allowSet &&
						(value >= 0) &&
						(value <= 2)
					)
                {
					Settings.ForceWindowedMode = value;
                }

				NotifyPropertyChanged();
			}
        }
	}
}
