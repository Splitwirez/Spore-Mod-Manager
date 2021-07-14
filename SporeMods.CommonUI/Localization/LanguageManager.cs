using SporeMods.Core;
using SporeMods.Core.Injection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace SporeMods.CommonUI.Localization
{
    public class LanguageManager : NotifyPropertyChangedBase
    {
        public static LanguageManager Instance { get; private set; }

        static LanguageManager()
        {
            Instance = new LanguageManager();
            Instance.FinishInit();
        }

        string _langCodes = string.Empty;

        static Language _canadianEnglish = null;
        public static Language CanadianEnglish
        {
            get => _canadianEnglish;
            private set => _canadianEnglish = value;
        }


        List<string> _resNames;
        private LanguageManager()
        {
            var allResNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            _resNames = allResNames.Where(x => x.StartsWith(Language.LANG_RESOURCE_START)).ToList();
            string canadianEngRes = Language.LANG_RESOURCE_START + "en-ca.txt";
            _resNames.Remove(canadianEngRes);
            _langCodes += "\'en-ca\'";

            CanadianEnglish = new Language(canadianEngRes);
            Languages.Add(CanadianEnglish);

            /*string output = string.Empty;
            foreach (string resName in allResNames)
            {
                if (resNames.Contains(resName))
                    output += $"\nMATCHED: {resName}";
                else
                    output += $"\n{resName}";
            }

            MessageBox.Show(output, "RES NAMES");*/
        }

        void FinishInit()
        {
            foreach (string resName in _resNames)
            {
                var lang = new Language(resName);
                Languages.Add(lang);
                _langCodes += $"\n\'{lang.LanguageCode}\'";
            }


            if (Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName).Equals(CrossProcess.MGR_EXE, StringComparison.OrdinalIgnoreCase))
            {


                string hotReloadFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SMM");
                string hotReloadPath = Path.Combine(hotReloadFolderPath, "te-st.txt");

                if (File.Exists(hotReloadPath))
                {
                    Language hotReload = new Language(hotReloadPath);
                    Languages.Add(hotReload);
                    CurrentLanguage = hotReload;

                    FileSystemWatcher hotReloadWatcher = new FileSystemWatcher(hotReloadFolderPath, "*.txt");
                    hotReloadWatcher.Changed += (s, e) => Application.Current.Dispatcher.Invoke(() =>
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
                else
                    CurrentLanguage = CurrentLanguage;
            }

            /*if (Process.GetCurrentProcess().MainModule.FileName.Contains("servant", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(Settings.CurrentLanguageCode);
            }*/
            

            //string currentCode = Settings.CurrentLanguageCode;
            
            SporeLauncher.GetLocalizedString = GetLocalizedText;
            Core.Mods.XmlModIdentityV1.GetLocalizedString = GetLocalizedText;
        }


        static readonly string[] LANGUAGE_ROUNDING_ALLOWED_GROUPS =
        {
            "en",
            "es",
            "ca"
        };

        string _osLangIdentifier = null;
        string GetCurrentLanguageIdentifier()
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
                
                if (langCode == null)
                    langCode = "en-ca";

                _osLangIdentifier = langCode;
            }

            string ret = Settings.GetElementValue(_currentLanguageCode, _osLangIdentifier);

            return Languages.Any(x => x.LanguageCode.Equals(ret, StringComparison.OrdinalIgnoreCase)) ? ret : "en-ca";
        }
        
        
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

        ResourceDictionary _prevLangDictionary = null;
        ObservableCollection<Language> _languages = new ObservableCollection<Language>();
        public ObservableCollection<Language> Languages
        {
            get => _languages;
            set
            {
                _languages = value;
                NotifyPropertyChanged();
            }
        }

        static readonly string _currentLanguageCode = "CurrentLanguageCode";
        Language _currentLanguage = null;
        public Language CurrentLanguage
        {
            get
            {
                if (_currentLanguage != null)
                    return _currentLanguage;

                string ident = GetCurrentLanguageIdentifier();
                return Languages.FirstOrDefault(x => x.LanguageCode.Equals(ident, StringComparison.OrdinalIgnoreCase));
            }
            set
            {
                _currentLanguage = value;

                if (!value.IsExternalLanguage)
                    Settings.SetElementValue(_currentLanguageCode, value.LanguageCode);

                NotifyPropertyChanged();
                Console.Write("a");

                var currentApp = Application.Current;
                if (currentApp != null)
                {
                    var langContents = value.Dictionary;
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
        }

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
            if (_currentLanguage == null)
                return "(NO LANGUAGE SELECTED) (NOT LOCALIZED)";
            var dict = _currentLanguage.Dictionary;
            if (dict.Contains(key))
            {
                object res = dict[key];
                if ((res == null) && (dict.MergedDictionaries.Count > 0))
                    res = dict.MergedDictionaries[0][key];

                if (res != null)
                    return res.ToString();
            }
            
            return $"{key} (NOT LOCALIZED)";
        }
    }
}
