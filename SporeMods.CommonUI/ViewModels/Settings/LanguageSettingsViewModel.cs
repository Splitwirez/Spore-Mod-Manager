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
	public class LanguageSettingsViewModel : NotifyPropertyChangedBase
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


		public LanguageSettingsViewModel()
			: base()
		{ }
	}
}
