using SporeMods.CommonUI;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SporeMods.ViewModels
{
	public class GameLanguageViewModel : NotifyPropertyChangedBase
	{
		string _displayName = null;
		public string DisplayName
		{
			get => _displayName;
			set
			{
				_displayName = value;
				NotifyPropertyChanged();
			}
		}


		string _languageCode = null;
		public string LanguageCode
		{
			get => _languageCode;
			set
			{
				_languageCode = value;
				NotifyPropertyChanged();
			}
		}

		public GameLanguageViewModel(string languageCode, string displayName)
		{
			LanguageCode = languageCode;
			DisplayName = displayName;
		}

		public override string ToString()
			=> DisplayName;
	}
}
