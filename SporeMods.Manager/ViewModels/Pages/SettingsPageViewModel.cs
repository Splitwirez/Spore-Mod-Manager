using Avalonia;
using SporeMods.CommonUI;
using SporeMods.Core;
using SporeMods.NotifyOnChange;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace SporeMods.Manager.ViewModels
{
    public class SettingsPageViewModel : NOCObject
    {

        public SettingsPageViewModel()
            : base()
        {
            _currentManualCmdStateProp = AddProperty(new NOCRespondProperty<string>(nameof(CurrentManualCmdState), Instance.GameState)
            {
                ValueChangeResponse = ((x, o, n) =>
                {
                    if (CmdStateManualEntry)
                        Settings.Instance.GameState = n;
                })
            });

            

            _currentCmdStateProp = AddProperty(new NOCRespondProperty<CommandLineState>(nameof(CurrentCmdState))
            {
                ValueChangeResponse = ((x, o, n) =>
                {
                    if (n != null)
                    {
                        CmdStateManualEntry = false;

                        Settings.Instance.GameState = n.StateIdentifier;
                        CurrentManualCmdState = Settings.Instance.GameState;

                        Console.WriteLine($"New state: {n.StateIdentifier}");
                    }
                    else
                        CmdStateManualEntry = true;
                })
            });
            

            string targetState = Instance.GameState;
            CommandLineState currentState = ModsManager.Instance.CommandLineStates.FirstOrDefault(x => x.StateIdentifier == targetState);
            
            _cmdStateManualEntryProp = AddProperty(nameof(CmdStateManualEntry), currentState == null);
            
            CurrentCmdState = currentState;


            string currentGameLang = Settings.Instance.ForcedGameLocale;


            _languageManualEntryProp = AddProperty(nameof(LanguageManualEntry), currentGameLang.IsNullOrEmptyOrWhiteSpace() || (!GameLanguages.Any(x => x.LocaleIdentifier == currentGameLang)));
            _currentGameLanguageProp = AddProperty(new NOCRespondProperty<GameLanguage>(nameof(CurrentGameLanguage), GameLanguages.FirstOrDefault(x => x.LocaleIdentifier == currentGameLang)));

            _currentManualGameLanguageProp = AddProperty(new NOCRespondProperty<string>(nameof(CurrentManualGameLanguage), currentGameLang)
            {
                ValueChangeResponse = ((x, o, n) =>
                {
                    if (n != null)// && GameLanguages.Any(x => x.LocaleIdentifier == n))
                    {
                        Settings.Instance.ForcedGameLocale = n;

                        /*if (!LanguageManualEntry)
                            CurrentGameLanguage = GameLanguages.FirstOrDefault(x => x.LocaleIdentifier == n);*/
                    }
                })
            });

            _currentGameLanguageProp.ValueChangeResponse = ((x, o, n) =>
            {
                Console.WriteLine($"new language: {n}");
                if (n != null)
                {
                    Settings.Instance.ForcedGameLocale = n.LocaleIdentifier;

                    if (!LanguageManualEntry)
                        CurrentManualGameLanguage = n.LocaleIdentifier;
                    else
                        LanguageManualEntry = false;
                }
            });


            _languageManualEntryDropDownOpenProp = AddProperty(new NOCRespondProperty<bool>(nameof(LanguageManualEntryDropDownOpen))
            {
                ValueChangeResponse = ((x, o, e) =>
                {
                    if ((!e) && LanguageManualEntry)
                    {
                        string lang = CurrentManualGameLanguage;
                        CurrentGameLanguage = GameLanguages.FirstOrDefault(x => x.LocaleIdentifier == lang);
                    }
                })
            });
        }
        
        public Settings Instance
        {
            get => Settings.Instance;
        }

        NOCProperty<bool> _cmdStateManualEntryProp;
        public bool CmdStateManualEntry
        {
            get => _cmdStateManualEntryProp.Value;
            set => _cmdStateManualEntryProp.Value = value;
        }

        NOCRespondProperty<CommandLineState> _currentCmdStateProp;
        public CommandLineState CurrentCmdState
        {
            get => _currentCmdStateProp.Value;
            set => _currentCmdStateProp.Value = value;
        }

        NOCRespondProperty<string> _currentManualCmdStateProp;
        public string CurrentManualCmdState
        {
            get => _currentManualCmdStateProp.Value;
            set => _currentManualCmdStateProp.Value = value;
        }



        ObservableCollection<GameLanguage> _gameLanguages = new ObservableCollection<GameLanguage>()
        {
            /*
                                    <ComboBoxItem Tag="en-us">English (US)</ComboBoxItem>
                                    <ComboBoxItem Tag="en-gb">English (GB)</ComboBoxItem>
                                    <ComboBoxItem Tag="cs-cz">Čeština</ComboBoxItem>
                                    <ComboBoxItem Tag="da-dk">Dansk</ComboBoxItem>
                                    <ComboBoxItem Tag="de-de">Deutsch</ComboBoxItem>
                                    <ComboBoxItem Tag="el-gr">Ελληνικά</ComboBoxItem>
                                    <ComboBoxItem Tag="es-es">Español</ComboBoxItem>
                                    <ComboBoxItem Tag="fi-fi">Suomi</ComboBoxItem>
                                    <ComboBoxItem Tag="fr-fr">Français</ComboBoxItem>
                                    <ComboBoxItem Tag="hu-hu">Magyar</ComboBoxItem>
                                    <ComboBoxItem Tag="it-it">Italiano</ComboBoxItem>
                                    <ComboBoxItem Tag="ja-jp">日本語</ComboBoxItem>
                                    <ComboBoxItem Tag="ko-kr">한국어</ComboBoxItem>
                                    <ComboBoxItem Tag="nl-nl">Nederlands</ComboBoxItem>
                                    <ComboBoxItem Tag="no-no">Norsk</ComboBoxItem>
                                    <ComboBoxItem Tag="pl-pl">Polski</ComboBoxItem>
                                    <ComboBoxItem Tag="pt-pt">Português</ComboBoxItem>
                                    <ComboBoxItem Tag="pt-br">Português (Brazil)</ComboBoxItem>
                                    <ComboBoxItem Tag="ru-ru">Русский</ComboBoxItem>
                                    <ComboBoxItem Tag="sv-se">Svenska</ComboBoxItem>
                                    <ComboBoxItem Tag="th-th">ภาษาไทย</ComboBoxItem>
                                    <ComboBoxItem Tag="zh-cn">简体中文</ComboBoxItem>
                                    <ComboBoxItem Tag="zh-tw">繁體中文</ComboBoxItem>*/
            /*new GameLanguage(
            "English (US)"
            ,
            "en-us"
            ),new GameLanguage(
            "English (GB)"
            ,
            "en-gb"
            ),
            new GameLanguage(
            "Čeština"
            ,
            "cs-cz"
            ),
            new GameLanguage(
            "Dansk"
            ,
            "da-dk"
            ),
            new GameLanguage(
            "Deutsch"
            ,
            "de-de"
            ),
            new GameLanguage(
            "Ελληνικά"
            ,
            "el-gr"
            ),
            new GameLanguage(
            "Español"
            ,
            "es-es"
            ),
            new GameLanguage(
            "Suomi"
            ,
            "fi-fi"
            ),
            new GameLanguage(
            "Français"
            ,
            "fr-fr"
            ),
            new GameLanguage(
            "Magyar"
            ,
            "hu-hu"
            ),
            new GameLanguage(
            "Italiano"
            ,
            "it-it"
            ),
            new GameLanguage(
            "日本語"
            ,
            "ja-jp"
            ),
            new GameLanguage(
            "한국어"
            ,
            "ko-kr"
            ),
            new GameLanguage(
            "Nederlands"
            ,
            "nl-nl"
            ),
            new GameLanguage(
            "Norsk"
            ,
            "no-no"
            ),
            new GameLanguage(
            "Polski"
            ,
            "pl-pl"
            ),
            new GameLanguage(
            "Português"
            ,
            "pt-pt"
            ),
            new GameLanguage(
            "Português (Brazil)"
            ,
            "pt-br"
            ),
            new GameLanguage(
            "Русский"
            ,
            "ru-ru"
            ),
            new GameLanguage(
            "Svenska"
            ,
            "sv-se"
            ),
            new GameLanguage(
            "ภาษาไทย"
            ,
            "th-th"
            ),
            new GameLanguage(
            "简体中文"
            ,
            "zh-cn"
            ),
            new GameLanguage(
            "繁體中文"
            ,
            "zh-tw"
            ),*/
            new GameLanguage("English (US)", "en-us"),
            new GameLanguage("English (GB)", "en-gb"),
            new GameLanguage("Čeština", "cs-cz"),
            new GameLanguage("Dansk", "da-dk"),
            new GameLanguage("Deutsch", "de-de"),
            new GameLanguage("Ελληνικά", "el-gr"),
            new GameLanguage("Español", "es-es"),
            new GameLanguage("Suomi", "fi-fi"),
            new GameLanguage("Français", "fr-fr"),
            new GameLanguage("Magyar", "hu-hu"),
            new GameLanguage("Italiano", "it-it"),
            new GameLanguage("日本語", "ja-jp"),
            new GameLanguage("한국어", "ko-kr"),
            new GameLanguage("Nederlands", "nl-nl"),
            new GameLanguage("Norsk", "no-no"),
            new GameLanguage("Polski", "pl-pl"),
            new GameLanguage("Português", "pt-pt"),
            new GameLanguage("Português (Brazil)", "pt-br"),
            new GameLanguage("Русский", "ru-ru"),
            new GameLanguage("Svenska", "sv-se"),
            new GameLanguage("ภาษาไทย", "th-th"),
            new GameLanguage("简体中文", "zh-cn"),
            new GameLanguage("繁體中文", "zh-tw"),
        };
        
        public ObservableCollection<GameLanguage> GameLanguages
        {
            get => _gameLanguages;
        }

        NOCRespondProperty<GameLanguage> _currentGameLanguageProp;
        public GameLanguage CurrentGameLanguage
        {
            get => _currentGameLanguageProp.Value;
            set => _currentGameLanguageProp.Value = value;
        }

        NOCProperty<bool> _languageManualEntryProp;
        public bool LanguageManualEntry
        {
            get => _languageManualEntryProp.Value;
            set => _languageManualEntryProp.Value = value;
        }

        NOCRespondProperty<bool> _languageManualEntryDropDownOpenProp;
        public bool LanguageManualEntryDropDownOpen
        {
            get => _languageManualEntryDropDownOpenProp.Value;
            set => _languageManualEntryDropDownOpenProp.Value = value;
        }

        NOCRespondProperty<string> _currentManualGameLanguageProp;
        public string CurrentManualGameLanguage
        {
            get => _currentManualGameLanguageProp.Value;
            set => _currentManualGameLanguageProp.Value = value;
        }
        

        public async void ChoosePreferredMonitorCommand(object parameter = null)
        {
            string monitor = await MessageDisplay.ShowModal<string>(new ChooseMonitorViewModel());
            if (!monitor.IsNullOrEmptyOrWhiteSpace())
                Instance.PreferredBorderlessMonitor = monitor;
        }
    }


    public class GameLanguage : NOCObject
    {
        NOCProperty<string> _displayNameProp;
        public string DisplayName
        {
            get => _displayNameProp.Value;
            set => _displayNameProp.Value = value;
        }


        NOCProperty<string> _localeIdentifierProp;
        public string LocaleIdentifier
        {
            get => _localeIdentifierProp.Value;
            set => _localeIdentifierProp.Value = value;
        }

        public GameLanguage(string displayName, string localeIdentifier)
        {
            _displayNameProp = AddProperty(nameof(DisplayName), displayName);
            _localeIdentifierProp = AddProperty(nameof(LocaleIdentifier), localeIdentifier);
        }

        public override string ToString()
            => $"{DisplayName} ({LocaleIdentifier})";
    }
}