using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using SporeMods.BaseTypes;

namespace SporeMods.Core
{
    public class SmmStorage : NOCSingleInstanceObject<SmmStorage>
    {
        const string STORAGE_FOLDER_NAME = "SporeModManagerStorage";
        const string STORAGE_REDIR_NAME = "redirectStorage.txt";


        public SmmStorage()
            : base()
        {
            string defaultStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), STORAGE_FOLDER_NAME);

            if (!Directory.Exists(defaultStoragePath))
            {
                try
                {
                    Directory.CreateDirectory(defaultStoragePath);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The default storage path does not exist. Storage cannot be located. (NOT LOCALIZED)", ex);
                }
            }
            StoragePath = defaultStoragePath;

            string redirectStorageFilePath = Path.Combine(defaultStoragePath, STORAGE_REDIR_NAME);
            if (File.Exists(redirectStorageFilePath))
            {
                string dir = File.ReadAllText(redirectStorageFilePath);
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("The redirected storage path could not be created. (NOT LOCALIZED)", ex);
                    }
                }
                StoragePath = dir;
            }

            TempPath = Path.Combine(StoragePath, "Temp");

            SmmInstallDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();

            _settingsDocPath = Path.Combine(StoragePath, "ModManagerSettings.xml");
            _settingsDocument = XDocument.Load(_settingsDocPath);
        }
        readonly string _settingsDocPath = null;
        readonly XDocument _settingsDocument = null;

        public readonly string StoragePath = null;
        public readonly string TempPath = null;
        public readonly string SmmInstallDirectory = null;



        public T ReadSetting<T>(string name, T defaultValue = default(T))
        {
            var rootElement = _settingsDocument.Root;

            var el = rootElement.Element(name);
            if (el != null)
            {
                string value = el.Value;
                if (value != null)
                {
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)value;
                    }
                    else if (typeof(T).IsEnum)
                    {
                        if (Enum.TryParse(typeof(T), value, out object result))
                            return (T)result;
                    }
                    else if (typeof(T) == typeof(bool))
                    {
                        if (bool.TryParse(value, out bool result))
                            return (T)(object)result;
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        if (int.TryParse(value, out int result))
                            return (T)(object)result;
                    }
                    else if (typeof(T) == typeof(Version))
                    {
                        if (Version.TryParse(value, out Version result))
                            return (T)(object)result;
                    }
                    else if (TryGetParser(out Func<string, T, T> parse))
                    {
                        return parse(value, defaultValue);
                    }
                }
            }
            return defaultValue;
        }

        static bool TryGetParser<T>(out Func<string, T, T> parse)
        {
            parse = null;
            MethodInfo method = typeof(T).GetMethod("TryParse");
            if (method != null)
            {
                if (method.IsStatic && method.IsPublic)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 2)
                    {
                        if (
                                (parameters[0].ParameterType == typeof(string)) &&
                                (parameters[1].ParameterType == typeof(T)) &&
                                parameters[1].IsOut
                            )
                        {
                            parse = (inStr, defaultValue) =>
                            {
                                var parameters = new object[]
                                {
                                    inStr,
                                    null
                                };

                                if ((bool)method.Invoke(null, parameters))
                                    return (T)parameters[1];
                                else
                                    return defaultValue;
                            };
                        }
                    }
                }
            }

            if (parse == null)
            {
                method = typeof(T).GetMethod("Parse");
                if (method != null)
                {
                    if (method.IsStatic && method.IsPublic)
                    {
                        ParameterInfo[] parameters = method.GetParameters();

                        if (parameters.Length == 1)
                        {
                            if (parameters[0].ParameterType == typeof(string))
                            {
                                parse = (inStr, defaultValue) =>
                                {
                                    try
                                    {
                                        return (T)method.Invoke(null, new object[]
                                        {
                                        inStr
                                        });
                                    }
                                    catch
                                    {
                                        return defaultValue;
                                    }
                                };
                            }
                        }
                    }
                }
            }
            return parse != null;
        }

        public void WriteSetting(string name, object value, bool saveNow = true)
        {
            var rootElement = _settingsDocument.Root;
            string val = value != null ? value.ToString() : null;
            
            
            if (val.IsNullOrEmptyOrWhiteSpace())
                val = null;

            rootElement.SetElementValue(name, val);
            
            if (saveNow)
                Save();
        }

        public void Save() =>
            _settingsDocument.Save(_settingsDocPath);


        /*readonly NCProperty<string> _storagePathProp;
        public string StoragePath
        {
            get => _storagePathProp.Value;
            private set => _storagePathProp.Value = value;
        }*/
    }
}
