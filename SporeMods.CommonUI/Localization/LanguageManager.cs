using SporeMods.NotifyOnChange;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia;

namespace SporeMods.CommonUI.Localization
{
    public partial class LanguageManager : NOCSingleInstanceObject<LanguageManager>, IObservable<string>, IResourceHost, IResourceNode
    {
        static Language _canadianEnglish = null;
        public static Language CANADIAN_ENGLISH
        {
            get => _canadianEnglish;
            private set => _canadianEnglish = value;
        }


        const string CANADIAN_ENG_ID = "en-ca";
        const string CANADIAN_ENG_RES_NAME = Language.LANG_RESOURCE_START + CANADIAN_ENG_ID + ".txt";
        List<string> _resNames;
        List<string> _availableLanguageCodes = new List<string>()
        {
            CANADIAN_ENG_ID
        };

        public LanguageManager()
            : base()
        {
            _languages = AddProperty(nameof(Languages), new ObservableCollection<Language>());


            var allResNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            _resNames = allResNames.Where(x => x.StartsWith(Language.LANG_RESOURCE_START)).ToList();
            //string canadianEngRes = Language.LANG_RESOURCE_START + $"{CANADIAN_ENGLISH}.txt";
            _resNames.Remove(CANADIAN_ENG_RES_NAME);

            CANADIAN_ENGLISH = new Language(CANADIAN_ENG_RES_NAME);
            Languages.Add(CANADIAN_ENGLISH);

            _currentLanguage = AddProperty(nameof(CurrentLanguage), CANADIAN_ENGLISH);

            foreach (string resName in _resNames)
            {
                var lang = new Language(resName);
                Languages.Add(lang);
                _availableLanguageCodes.Add(lang.LanguageCode);
            }

            //_resNames.Add(CANADIAN_ENGLISH_RES);


            bool doNormalStuff = true;
            if (false) //Path.GetFileNameWithoutExtension(Process.GetCurrentProcess()/*.GetExecutableName()*/).Equals(CrossProcess.MGR_EXE, StringComparison.OrdinalIgnoreCase))
            {
                string hotReloadFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SMM");
                string hotReloadPath = Path.Combine(hotReloadFolderPath, "te-st.txt");

                if (File.Exists(hotReloadPath))
                {
                    doNormalStuff = false;

                    Language hotReload = new Language(hotReloadPath);
                    Languages.Add(hotReload);
                    CurrentLanguage = hotReload;

                    FileSystemWatcher hotReloadWatcher = new FileSystemWatcher(hotReloadFolderPath, "*.txt");
                    hotReloadWatcher.Changed += (s, e) => Dispatcher.UIThread.Post(() =>
                    {
                        if (e.FullPath.Equals(hotReloadPath, StringComparison.OrdinalIgnoreCase))
                        {
                            Languages.Remove(hotReload);
                            if (File.Exists(hotReloadPath))
                            {
                                hotReload = new Language(hotReloadPath);
                                Languages.Add(hotReload);
                                CurrentLanguage = hotReload;
                            }
                        }
                    });
                    hotReloadWatcher.EnableRaisingEvents = true;
                }
            }


            if (doNormalStuff)
            {
                string ident = GetRoundedSystemLanguageIdentifier();
                var lang = Languages.FirstOrDefault(x => x.LanguageCode.Equals(ident, StringComparison.OrdinalIgnoreCase));
                if (lang == default(Language))
                    lang = CANADIAN_ENGLISH;

                CurrentLanguage = lang;
            }

            _writeCurrentLanguage = true;

            /*if (Process.GetCurrentProcess().MainModule.FileName.Contains("servant", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(Settings.CurrentLanguageCode);
            }*/


            //string currentCode = Settings.CurrentLanguageCode;

            LanguageChanged += (s, e) => NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Empty);
        }


        static readonly string[] LANGUAGE_ROUNDING_ALLOWED_GROUPS =
        {
            "en",
            "es",
            "ca"
        };

        string GetRoundedSystemLanguageIdentifier()
        {
            string langCode = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();

            var target = _availableLanguageCodes.FirstOrDefault(x => x.Equals(langCode, StringComparison.OrdinalIgnoreCase));
            
            if (target != default(string))
            {
                string langGroup = langCode.Split('-')[0];
                if (LANGUAGE_ROUNDING_ALLOWED_GROUPS.Any(x => x.Equals(langGroup, StringComparison.OrdinalIgnoreCase)))
                {
                    langCode = _availableLanguageCodes.FirstOrDefault(x => x.Split('-')[0].Equals(langCode, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (target == default(string))
                target = CANADIAN_ENG_ID;
            return target;
        }


        /*string _osLangIdentifier = null;
        string zGetCurrentLanguageIdentifier()
        {
            if (_osLangIdentifier == null)
            {
                string langCode = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
                if (!Languages.Any(x => x.LanguageCode.Equals(langCode, StringComparison.OrdinalIgnoreCase)))
                {
                    string langGroup = langCode.Split('-')[0];
                    langCode = null;
                    
                    
                    if (LANGUAGE_ROUNDING_ALLOWED_GROUPS.Any(x => x.Equals(langGroup, StringComparison.OrdinalIgnoreCase)))
                    {
                        // Try to get one from the same group. If user has en-us, try to set en-ca, etc
                        langCode = Languages.FirstOrDefault(x => x.LanguageCode.Split('-')[0].Equals(langGroup, StringComparison.OrdinalIgnoreCase)).LanguageCode;
                    }
                }
                
                if (langCode.IsNullOrEmptyOrWhiteSpace())
                    langCode = "en-ca";

                _osLangIdentifier = langCode;
            }

            string ret = Settings.GetElementValue(_currentLanguageCode, _osLangIdentifier);

            return Languages.Any(x => x.LanguageCode.Equals(ret, StringComparison.OrdinalIgnoreCase)) ? ret : "en-ca";
        }*/


        /*bool zTryGetPreferredLanguageForOS(out Language language)
        {
            Language lang = null;
            string langCode = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
            if (!Languages.Any(x =>
                {
                    if (x.LanguageCode.Equals(langCode, StringComparison.OrdinalIgnoreCase))
                    {
                        lang = x;
                        return true;
                    }
                    return false;
                }))
            {
                string langGroup = langCode.Split('-')[0];
                if (LANGUAGE_ROUNDING_ALLOWED_GROUPS.Any(x => x.Equals(langGroup, StringComparison.OrdinalIgnoreCase)))
                {
                    // Try to get one from the same group. If user has en-us, try to set en-ca, etc
                    Languages.FirstOrDefault((x) =>
                    {
                        string code = x.LanguageCode;
                        if (code.Split('-')[0].Equals(langGroup, StringComparison.OrdinalIgnoreCase))
                        {
                            lang = x;
                            return true;
                        }
                        return false;
                    });
                }
            }
            
            if (langCode == null)
                langCode = "en-ca";

            language = lang;
            return language != null;
        }*/

        NOCProperty<ObservableCollection<Language>> _languages;
        public ObservableCollection<Language> Languages
        {
            get => _languages.Value;
            set => _languages.Value = value;
        }

        const string CURRENT_LANGUAGE_CODE = "CurrentLanguageCode";
        NOCProperty<Language> _currentLanguage;
        Language _prevLanguage = null;
        bool _writeCurrentLanguage = false;
        ResourceDictionary _prevLangDictionary = null;
        ResourceDictionary _langDictionary = null;
        
        
        public Language CurrentLanguage
        {
            get => _currentLanguage.Value;
            set
            {
                _currentLanguage.Value = value;

                if ((value != null) && (!value.IsExternalLanguage) && _writeCurrentLanguage)
                    SettingsStore.SetValue(CURRENT_LANGUAGE_CODE, value.LanguageCode);

                NotifyPropertyChanged();
                TryRefreshWpfResources();
                
                LanguageChanged?.Invoke(this, new LanguageEventArgs(_prevLanguage, value));
                _langDictionary = value.Dictionary;
                _prevLanguage = value;
            }
        }

        //ResourceDictionary _prevLangDictionary = null;
        public void TryRefreshWpfResources()
        {
            var currentApp = Application.Current;
            if (currentApp != null)
            {
                var langContents = CurrentLanguage.Dictionary;
                if (currentApp.Resources.MergedDictionaries.Contains(_prevLangDictionary))
                {
                    int prevIndex = currentApp.Resources.MergedDictionaries.IndexOf(_prevLangDictionary);
                    currentApp.Resources.MergedDictionaries.RemoveAt(prevIndex);
                    //currentApp.Resources.MergedDictionaries.Insert(prevIndex, langContents);
                }
                //else
                currentApp.Resources.MergedDictionaries.Add(langContents);

                _prevLangDictionary = langContents;
            }
        }

        internal const string NO_TEXT = "(NO LANGUAGE SELECTED) (NOT LOCALIZED)";
        public string GetLocalizedText(string key)
        {
            //string key = strKey.ToString().Replace('-', '!');
            /*if (Application.Current != null)
            {
                var res = Application.Current.TryFindResource(key);
                if (res != null)
                    value = res.ToString();
            }
            else */
            if (_langDictionary == null)
                return "(NO LANGUAGE SELECTED) (NOT LOCALIZED)";
            var dict = _langDictionary;
            if (dict.ContainsKey(key))
            {
                object res = dict[key];
                if ((res == null) && (dict.MergedDictionaries.Count > 0))
                    res = (dict.MergedDictionaries.ElementAt(0) as ResourceDictionary)[key];

                if (res != null)
                    return res.ToString();
            }
            
            return $"{key} (NOT LOCALIZED)";
        }
    }
}
