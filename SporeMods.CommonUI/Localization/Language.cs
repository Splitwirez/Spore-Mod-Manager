using SporeMods.Core;
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
    public class Language : NotifyPropertyChangedBase
    {
        internal const string LANG_RESOURCE_START = "SporeMods.CommonUI.Localization.Languages.";


        static readonly Dictionary<string, string> EXE_SPECIFIC_TEXT = new Dictionary<string, string>()
        {
            {
                CrossProcess.MGR_EXE,
                "Manager"
            },
            {
                CrossProcess.LAUNCHER_EXE,
                "Launcher"
            },
            {
                CrossProcess.IMPORTER_EXE,
                "KitImporter"
            },
            {
                CrossProcess.DRAG_EXE,
                "DragServant"
            },
        };

        bool _isEnCa = false;
        public Language(string langRes)
            : this(langRes, null)
        { }

        public Language(string langRes, string langCode)
        {
            string path = langRes;
            IEnumerable<string> lines = null;


            try
            {
                IsExternalLanguage = File.Exists(langRes);
            }
            catch (Exception ex)
            {
                Cmd.WriteLine(ex);
            }


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
                //Cmd.WriteLine("a");
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


        string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            private set
            {
                _displayName = value;
                NotifyPropertyChanged();
            }
        }

        string _resPath = string.Empty;
        public string LanguageCode
        {
            get => _resPath;
            private set
            {
                _resPath = value;
                NotifyPropertyChanged();
            }
        }

        double _completeness = 0;
        public double Completeness
        {
            get => _completeness;
            private set
            {
                _completeness = value;
                NotifyPropertyChanged();
            }
        }

        bool _isExternalLanguage = false;
        public bool IsExternalLanguage
        {
            get => _isExternalLanguage;
            private set
            {
                _isExternalLanguage = value;
                NotifyPropertyChanged();
            }
        }

        ResourceDictionary _contents = null;
        public ResourceDictionary Dictionary
        {
            get => _contents;
            private set
            {
                _contents = value;
                NotifyPropertyChanged();
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
