using SporeMods.Core;
using SporeMods.Core.Injection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
                    Selected = hotReload;

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
                                    Selected = hotReload;
                                }
                            }
                        });
                    hotReloadWatcher.EnableRaisingEvents = true;
                }
            }

            /*if (Process.GetCurrentProcess().MainModule.FileName.Contains("servant", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(Settings.CurrentLanguageCode);
            }*/
            string currentCode = Settings.CurrentLanguageCode;
            UpdateSelectedLanguage(currentCode);

            SporeLauncher.GetLocalizedString = GetLocalizedText;
            Core.Mods.XmlModIdentityV1.GetLocalizedString = GetLocalizedText;
        }

        void UpdateSelectedLanguage(string langCode)
        {
            foreach (Language lang in _languages)
            {
                if (lang.LanguageCode == langCode)
                {
                    Selected = lang;
                    return;
                }
            }
            /*Exception e = null;
            try
            {
                var newLang = _languages.First(x => x.LanguageCode == langCode);
                if (newLang != null)
                {
                    Selected = newLang;
                    return;
                }
            }
            catch (Exception ex)
            {
                e = ex;
            }*/
            MessageBox.Show($"NO LANGUAGE FOR \'{langCode}\'\n\nLANGUAGE CODES THAT ACTUALLY EXIST:{_langCodes}\n\n(NOT LOCALIZED)");
            /*if (lang.ResPath == langCode)
                Selected = lang;*/
        }

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

        Language _selected = null;
        public Language Selected
        {
            get => _selected;
            set
            {
                _selected = value;

                var currentApp = Application.Current;
                if (currentApp != null)
                {
                    var langContents = _selected.Dictionary;
                    if (currentApp.Resources.MergedDictionaries.Contains(_prevLangDictionary))
                    {
                        int prevIndex = currentApp.Resources.MergedDictionaries.IndexOf(_prevLangDictionary);
                        currentApp.Resources.MergedDictionaries.RemoveAt(prevIndex);
                        currentApp.Resources.MergedDictionaries.Insert(prevIndex, langContents);
                    }
                    else
                    {
                        currentApp.Resources.MergedDictionaries.Add(langContents);

                        if (Process.GetCurrentProcess().MainModule.FileName.Contains("servant", StringComparison.OrdinalIgnoreCase))
                        {
                            Debug.WriteLine(Settings.CurrentLanguageCode);
                        }
                    }

                    _prevLangDictionary = langContents;
                }

                if (_selected.LanguageCode != "te-st")
                    Settings.CurrentLanguageCode = _selected.LanguageCode;

                NotifyPropertyChanged();
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
            if (_selected == null)
                return "(NO LANGUAGE SELECTED) (NOT LOCALIZED)";
            var dict = _selected.Dictionary;
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
