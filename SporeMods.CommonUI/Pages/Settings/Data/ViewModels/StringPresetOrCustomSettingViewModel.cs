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
	public class StringPresetOrCustomSettingViewModel<T> : NotifyPropertyChangedBase
	{
		object _currentValue = null;
		public object CurrentValue
		{
			get => _currentValue;
			set
			{
				_currentValue = value;
				if (_currentValue is T model)
				{
					if (ManuallyEnter)
						ManuallyEnter = false;
					
					_setValue(_toString(model));
				}
				else if ((!ManuallyEnter) && (_currentValue is string))
					ManuallyEnter = true;
				
				NotifyPropertyChanged();
			}
		}

		
		string _customValueText = null;
		public string CustomValueText
		{
			get => _customValueText;
			set
			{
				_customValueText = value;
				if (ManuallyEnter)
					_setValue(value);
				NotifyPropertyChanged();
			}
		}



		
		public bool Override
		{
			get => _getHasValue();
			set
			{
				_setHasValue(value);
				NotifyPropertyChanged();
			}
		}

		static bool _manuallyEnter = false;
		public bool ManuallyEnter
		{
			get => _manuallyEnter;
			set
			{
				_manuallyEnter = value;
				NotifyPropertyChanged();
			}
		}


		string _header = null;
		public string Header
		{
			get => _header;
			set
			{
				_header = value;
				NotifyPropertyChanged();
			}
		}

		string _toolTipContent = null;
		public string ToolTipContent
		{
			get => _toolTipContent;
			set
			{
				_toolTipContent = value;
				NotifyPropertyChanged();
			}
		}

		string _toolTipWarning = null;
		public string ToolTipWarning
		{
			get => _toolTipWarning;
			set
			{
				_toolTipWarning = value;
				NotifyPropertyChanged();
			}
		}

		bool _usesLocalizedDisplayNames = false;
		public bool UsesLocalizedDisplayNames
		{
			get => _usesLocalizedDisplayNames;
			set
			{
				_usesLocalizedDisplayNames = value;
				NotifyPropertyChanged();
			}
		}

		//static readonly object _separator = new System.Windows.Controls.Separator();
		ObservableCollection<object> _availableValues = new ObservableCollection<object>();
		public ObservableCollection<object> AvailableValues
		{
			get => _availableValues;
		}


		string _headerKey = null;
		string _toolTipContentKey = null;
		string _toolTipWarningKey = null;
		
		readonly Func<T> _getValue = null;
		readonly Action<string> _setValue = null;
		
		readonly Func<bool> _getHasValue = null;
		readonly Action<bool> _setHasValue = null;
		
		readonly Func<string, T> _fromString = null;
		readonly Func<T, string> _toString = null;
		
		readonly IEnumerable<T> _presets = null;
		
		public StringPresetOrCustomSettingViewModel(string headerKey, string toolTipContentKey, string toolTipWarningKey, Func<T> getValue, Action<string> setValue, Func<bool> getHasValue, Action<bool> setHasValue, Func<T, string> toString, Func<string, T> fromString, IEnumerable<T> presets, bool useLocalizedDisplayNames)
		{
			_headerKey = headerKey;
			_toolTipContentKey = toolTipContentKey;
			_toolTipWarningKey = toolTipWarningKey;

			_getValue = getValue;
			_setValue = setValue;
			
			_getHasValue = getHasValue;
			_setHasValue = setHasValue;

			_fromString = fromString;
			_toString = toString;

			_presets = presets;
			//Settings.ForcedGameLocale
			//Settings!GameEntry!GameLanguage!Header

			{
				T outValue = _getValue();
				if (outValue != null)
					_currentValue = outValue;
				else
					_currentValue = string.Empty;
			}

			{
				var val = _getValue();
				_customValueText = val != null ? _toString(val) : null;
			}

			{
				_manuallyEnter = _getValue() == null;
			}

			{
				foreach (T lang in _presets)
				{
					_availableValues.Add(lang);
				}
				
				//collection.Add(_separator);
				_availableValues.Add(string.Empty);
			}

			UsesLocalizedDisplayNames = useLocalizedDisplayNames;

			Header = LanguageManager.Instance.GetLocalizedText(_headerKey);
			ToolTipContent = LanguageManager.Instance.GetLocalizedText(_toolTipContentKey);
			ToolTipWarning = LanguageManager.Instance.GetLocalizedText(_toolTipWarningKey);
			
			LanguageManager.LanguageChanged += (s, e) =>
			{
				Header = LanguageManager.Instance.GetLocalizedText(_headerKey);
				ToolTipContent = LanguageManager.Instance.GetLocalizedText(_toolTipContentKey);
				ToolTipWarning = LanguageManager.Instance.GetLocalizedText(_toolTipWarningKey);
			};
		}
	}
}
