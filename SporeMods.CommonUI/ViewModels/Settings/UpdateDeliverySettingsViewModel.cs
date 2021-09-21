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
using UpdatingModeType = SporeMods.Core.Settings.UpdatingModeType;

namespace SporeMods.ViewModels
{
	public class UpdateDeliverySettingsViewModel : NotifyPropertyChangedBase
	{
		bool _allowSet = false;


		bool _auto = false;
		public bool Auto
		{
			get => _auto;
			set
			{
				_auto = value;
				NotifyPropertyChanged();

				if (_auto)
					Mode = UpdatingModeType.Automatic;
			}
		}


		bool _askFirst = false;
		public bool AskFirst
		{
			get => _askFirst;
			set
			{
				_askFirst = value;
				NotifyPropertyChanged();

				if (_allowSet && _askFirst)
					Mode = UpdatingModeType.AutoCheck;
			}
		}


		bool _never = false;
		public bool Never
		{
			get => _never;
			set
			{
				_never = value;
				NotifyPropertyChanged();

				if (_never)
					Mode = UpdatingModeType.Disabled;
			}
		}


		public UpdateDeliverySettingsViewModel()
		{
			if (Mode == UpdatingModeType.Automatic)
				Auto = true;
			else if (Mode == UpdatingModeType.AutoCheck)
				AskFirst = true;
			else if (Mode == UpdatingModeType.Disabled)
				Never = true;

			_allowSet = true;
		}


		UpdatingModeType Mode
        {
			get => Settings.UpdatingMode;
			set
            {
				if (_allowSet)
					Settings.UpdatingMode = value;

				NotifyPropertyChanged();
			}
        }
	}
}
