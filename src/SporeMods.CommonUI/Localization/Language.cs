using SporeMods;
using SporeMods.BaseTypes;
using SporeMods.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace SporeMods.CommonUI.Localization
{
    public class Language : NOCObject
    {
        internal const string LANG_RESOURCE_START = "SporeMods.CommonUI.Localization.Languages.";


        /*static readonly Dictionary<string, string> EXE_SPECIFIC_TEXT = new Dictionary<string, string>()
        {
            {
                MgrProcesses.MGR_EXE,
                "Manager"
            },
            {
                MgrProcesses.LAUNCHER_EXE,
                "Launcher"
            },
            {
                MgrProcesses.IMPORTER_EXE,
                "KitImporter"
            },
            {
                MgrProcesses.DRAG_EXE,
                "UacMessenger"
            },
        };*/

        bool _isEnCa = false;
        public Language(string langRes)
            : this(langRes, null)
        { }

        public Language(string langRes, string langCode)
        {
            _displayName = AddProperty(nameof(DisplayName), string.Empty);
            _languageCode = AddProperty(nameof(LanguageCode), string.Empty);
            _completeness = AddProperty(nameof(Completeness), 0.0);
            _isExternalLanguage = AddProperty(nameof(IsExternalLanguage), false);
            _dictionary = AddProperty<ResourceDictionary>(nameof(Dictionary), null);

            string path = langRes;
            IEnumerable<string> lines = null;


            try
            {
                IsExternalLanguage = File.Exists(langRes);
            }
            catch (Exception ex)
            { }


            if (path.StartsWith(LANG_RESOURCE_START))
            {
                path = path.Substring(LANG_RESOURCE_START.Length);

                if (path.Contains('.'))
                    path = path.Substring(0, path.IndexOf('.'));

                
                List<string> allLines = new List<string>();
                
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(langRes))
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        allLines.Add(line);
                    }
                }

                lines = allLines;

                
                if (langCode == null)
                    LanguageCode = path;
                else
                    LanguageCode = langCode;
            }
            else if (IsExternalLanguage)
            {
                lines = File.ReadAllLines(langRes);
                
                
                string languageCode = Path.GetFileNameWithoutExtension(langRes);
                if (!languageCode.Contains('-'))
                    languageCode = $"test={languageCode}";
                LanguageCode = languageCode;
            }

            _isEnCa = LanguageCode == "en-ca";
            ResourceDictionary lang = new ResourceDictionary();


            DisplayName = lines.First();

            lines = lines.Skip(1);
            /*bool exeSpecific = false;
            bool exeMatched = false;*/

            List<string> prefixes = new List<string>();

            int startCount = 0;
            int endCount = 0;


            foreach (string l in lines)
            {
                string line = l;

                if (line.Contains('#'))
                    line = line.Substring(0, line.IndexOf('#'));

                line = line.TrimStart();

                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line) || (line.Length <= 0))
                    continue;

                /*if (line.StartsWith('$'))
                {
                    string rest = line.Substring(1).Trim();
                    if (rest.Equals("end", StringComparison.OrdinalIgnoreCase))
                    {
                        exeSpecific = false;
                    }
                    else
                    {
                        exeSpecific = true;
                        string thisExe = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
                        exeMatched = EXE_SPECIFIC_TEXT.ContainsKey(thisExe) && (EXE_SPECIFIC_TEXT[thisExe] == rest);
                    }
                }
                else */
                if (line.EndsWith('{'))
                {
                    prefixes.Add(line.Substring(0, line.Length - 1).TrimEnd());
                    startCount++;
                }
                else if (line == "}")
                {
                    if (prefixes.Count > 0)
                        prefixes.RemoveAt(prefixes.Count - 1);

                    endCount++;
                }
                else if (line.Contains(' '))
                {
                    int firstSpace = line.IndexOf(' ');
                    string key = string.Empty;

                    foreach (string s in prefixes)
                    {
                        key += $"{s}!";
                    }
                    key += line.Substring(0, firstSpace);

                    //if ((exeSpecific && exeMatched) || (!exeSpecific))
                    lang.Add(key, line.Substring(firstSpace + 1).Replace("<br>", "\n"));
                }
            }

            if (startCount != endCount)
            {
                string pref = string.Empty;
                foreach (string s in prefixes)
                {
                    pref += $"{s}!";
                }
                pref = pref.TrimEnd('!');
                MessageBox.Show($"LANGUAGE PARSE FAIL:\nThe language files contains {startCount} \'{{\' and {endCount} \'}}\'. These should be equal, but they are not.\nThe problem may lie somewhere near \'{pref}\'. (NOT LOCALIZED)");
            }

            if (!_isEnCa)
            {
                //Debug.WriteLine("a");
                ResourceDictionary enCaD = LanguageManager.CanadianEnglish.Dictionary;
                lang.MergedDictionaries.Add(enCaD);

                Completeness = (lang.Keys.Count / enCaD.Keys.Count) * 100;
            }
            else
                Completeness = 100;

            Dictionary = lang;

            /*foreach (string key in lang.Keys)
            {
                output += $"\nK\"{key}\" = V\"{lang[key]}\"";
            }

            MessageBox.Show(output);*/
        }


        NOCProperty<string> _displayName;
        public string DisplayName
        {
            get => _displayName.Value;
            private set => _displayName.Value = value;
        }

        NOCProperty<string> _languageCode;
        public string LanguageCode
        {
            get => _languageCode.Value;
            private set => _languageCode.Value = value;
        }

        NOCProperty<double> _completeness;
        public double Completeness
        {
            get => _completeness.Value;
            private set => _completeness.Value = value;
        }

        NOCProperty<bool> _isExternalLanguage;
        public bool IsExternalLanguage
        {
            get => _isExternalLanguage.Value;
            private set => _isExternalLanguage.Value = value;
        }

        NOCProperty<ResourceDictionary> _dictionary = null;
        public ResourceDictionary Dictionary
        {
            get => _dictionary.Value;
            private set => _dictionary.Value = value;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
