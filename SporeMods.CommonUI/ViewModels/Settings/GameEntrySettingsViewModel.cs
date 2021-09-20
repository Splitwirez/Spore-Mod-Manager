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
		static readonly List<GameLanguageViewModel> _gameLanguages = new List<GameLanguageViewModel>()
		{
			new GameLanguageViewModel("en-us", "English (US)"),
			new GameLanguageViewModel("en-gb", "English (GB)"),
			new GameLanguageViewModel("cs-cz", "Čeština"),
			new GameLanguageViewModel("da-dk", "Dansk"),
			new GameLanguageViewModel("de-de", "Deutsch"),
			new GameLanguageViewModel("el-gr", "Ελληνικά"),
			new GameLanguageViewModel("es-es", "Español"),
			new GameLanguageViewModel("fi-fi", "Suomi"),
			new GameLanguageViewModel("fr-fr", "Français"),
			new GameLanguageViewModel("hu-hu", "Magyar"),
			new GameLanguageViewModel("it-it", "Italiano"),
			new GameLanguageViewModel("ja-jp", "日本()語"),
			new GameLanguageViewModel("ko-kr", "한국어"),
			new GameLanguageViewModel("nl-nl", "Nederlands"),
			new GameLanguageViewModel("no-no", "Norsk"),
			new GameLanguageViewModel("pl-pl", "Polski"),
			new GameLanguageViewModel("pt-pt", "Português"),
			new GameLanguageViewModel("pt-br", "Português (Brazil)"),
			new GameLanguageViewModel("ru-ru", "Русский"),
			new GameLanguageViewModel("sv-se", "Svenska"),
			new GameLanguageViewModel("th-th", "ภาษาไทย"),
			new GameLanguageViewModel("zh-cn", "简体中文"),
			new GameLanguageViewModel("zh-tw", "繁體中文")
		};



#if NAH
		object _currentGameLanguage = new Func<object>(() =>
		{
			string inLocale = Settings.ForcedGameLocale;
			GameLanguageViewModel outLocale = _gameLanguages.FirstOrDefault(x => x.LanguageCode.Equals(inLocale, StringComparison.OrdinalIgnoreCase));
			if (outLocale != null)
				return outLocale;
			else
				return string.Empty;
		})();
		public object CurrentGameLanguage
		{
			get => _currentGameLanguage;
			set
			{
				_currentGameLanguage = value;
				if (_currentGameLanguage is GameLanguageViewModel model)
				{
					if (ManuallyEnterGameLanguage)
						ManuallyEnterGameLanguage = false;
					
					Settings.ForcedGameLocale = model.LanguageCode;
				}
				else if ((!ManuallyEnterGameLanguage) && (_currentGameLanguage is string))
					ManuallyEnterGameLanguage = true;
				
				NotifyPropertyChanged();
			}
		}

		
		string _forcedGameLanguageText = Settings.ForcedGameLocale;
		public string ForcedGameLanguageText
		{
			get => _forcedGameLanguageText;
			set
			{
				_forcedGameLanguageText = value;
				if (ManuallyEnterGameLanguage)
					Settings.ForcedGameLocale = value;
				NotifyPropertyChanged();
			}
		}



		
		public bool OverrideGameLanguage
		{
			get => Settings.ForceGameLocale;
			set
			{
				Settings.ForceGameLocale = value;
				NotifyPropertyChanged();
			}
		}

		static bool _manuallyEnterGameLanguage = new Func<bool>(() =>
		{
			string targetLocale = Settings.ForcedGameLocale;
			return _gameLanguages.FirstOrDefault(x => x.LanguageCode.Equals(targetLocale, StringComparison.OrdinalIgnoreCase)) == null;
		})();
		public bool ManuallyEnterGameLanguage
		{
			get => _manuallyEnterGameLanguage;
			set
			{
				_manuallyEnterGameLanguage = value;
				NotifyPropertyChanged();
			}
		}

		//static readonly object _separator = new System.Windows.Controls.Separator();
		ObservableCollection<object> _availableGameLanguages = new Func<ObservableCollection<object>>(() =>
		{
			var collection = new ObservableCollection<object>();
			foreach (GameLanguageViewModel lang in _gameLanguages)
			{
				collection.Add(lang);
			}
			
			//collection.Add(_separator);
			collection.Add(string.Empty);
			return collection;
		})();
		public ObservableCollection<object> AvailableGameLanguages
		{
			get => _availableGameLanguages;
		}
#endif

		public StringPresetOrCustomSettingViewModel<GameLanguageViewModel> GameLanguage { get; }
		= new StringPresetOrCustomSettingViewModel<GameLanguageViewModel>(
			"Settings!GameEntry!GameLanguage!Header",
			"Settings!GameEntry!GameLanguage!ToolTip!Content",
			"Settings!GameEntry!GameLanguage!ToolTip!Warning",
			() => _gameLanguages.FirstOrDefault(x => x.LanguageCode.Equals(Settings.ForcedGameLocale)),
			v => Settings.ForcedGameLocale = v,
			() => Settings.ForceGameLocale,
			b => Settings.ForceGameLocale = b,
			l => l.LanguageCode,
			s => _gameLanguages.FirstOrDefault(x => x.LanguageCode.Equals(s)),
			_gameLanguages,
			false
		);

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


		public GameEntrySettingsViewModel()
			: base()
		{ }
	}
}
