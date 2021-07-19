using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace Mechanism.Wpf.Core.Skinning
{
    public class SkinManager : INotifyPropertyChanged
    {
        ObservableCollection<SkinInfo> _skins = new ObservableCollection<SkinInfo>();
        public ObservableCollection<SkinInfo> Skins
        {
            get => _skins;
            set
            {
                _skins = value;
                NotifyPropertyChanged(nameof(Skins));
            }
        }
        
        public SkinInfo DefaultSkin = null;

        SkinInfo _activeSkin = null;

        public SkinInfo ActiveSkin
        {
            get
            {
                if (_activeSkin == null)
                    return DefaultSkin;
                else
                    return _activeSkin;
            }
            set
            {
                if ((value != null) && Skins.Contains(value) && ((_activeSkin != value) || value.SettingsHaveChanged))
                {
                    _activeSkin = value;
                    ApplySkin(_activeSkin);
                    NotifyPropertyChanged(nameof(ActiveSkin));
                }
            }
        }

        void ApplySkin(SkinInfo targetSkin)
        {
            ResourceDictionary defl = DefaultSkin.GetSkinDictionary();
            ResourceDictionary targ = targetSkin.GetSkinDictionary();
            if (Application.Current.Resources.MergedDictionaries.Count == 0)
            {
                if (targetSkin == DefaultSkin)
                    Application.Current.Resources.MergedDictionaries.Add(defl);
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(defl);
                    Application.Current.Resources.MergedDictionaries.Add(targ);
                }
            }
            else if (Application.Current.Resources.MergedDictionaries.Count == 1)
            {
                if (targetSkin == DefaultSkin)
                    Application.Current.Resources.MergedDictionaries[0] = defl;
                else
                {
                    Application.Current.Resources.MergedDictionaries[0] = defl;
                    Application.Current.Resources.MergedDictionaries.Add(targ);
                }
            }
            else
            {
                if (targetSkin == DefaultSkin)
                {
                    Application.Current.Resources.MergedDictionaries[0] = defl;
                    
                    for (int i = 1; i < Application.Current.Resources.MergedDictionaries.Count; i++)
                        Application.Current.Resources.MergedDictionaries.RemoveAt(i);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries[0] = defl;
                    Application.Current.Resources.MergedDictionaries[1] = targ;

                    for (int i = 2; i < Application.Current.Resources.MergedDictionaries.Count; i++)
                        Application.Current.Resources.MergedDictionaries.RemoveAt(i);
                }
            }

            targetSkin.SettingsHaveChanged = false;
            if (TryGetSettingsPathForSkin(targetSkin, out string savePath))
                targetSkin.SaveSkinSettings(savePath);
        }

        string _allSkinsSettingsPath = null;

        public string AllSkinsSettingsPath
        {
            get => _allSkinsSettingsPath;
            set
            {
                _allSkinsSettingsPath = value;
                NotifyPropertyChanged(nameof(AllSkinsSettingsPath));
            }
        }

        public SkinManager(SkinInfo defaultSkin)
        {
            DefaultSkin = defaultSkin;

            Skins.Add(DefaultSkin);

            ApplySkin(DefaultSkin);
        }

        public SkinManager(string allSkinsSettingsPath, SkinInfo defaultSkin) : this(defaultSkin)
        {
            AllSkinsSettingsPath = allSkinsSettingsPath;
        }

        public void LoadSkinFromFolder(string folderPath)
        {
            Skins.Add(SkinInfo.FromFolder(folderPath));
        }

        /*public void LoadSkinFromFolder(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                throw new DirectoryNotFoundException();

            string dllName = Path.GetFileName(dirPath) + ".dll";
            Debug.WriteLine("DLL NAME: " + dllName);
            string path = Path.Combine(dirPath, dllName);
            if (File.Exists(path))
            {
                foreach (string s in Directory.EnumerateFiles(dirPath, "*.dll"))
                {
                    if (s != path)
                    {
                        string filename = Path.GetFileName(s);
                        string combinedPath = Path.Combine(Directory.GetParent(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).ToString(), filename);
                        if (!File.Exists(combinedPath))
                        {
                            try
                            {
                                Assembly.LoadFile(s);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(ex);
                            }
                        }
                    }
                }
                Assembly skinAssembly = Assembly.LoadFile(path);
                var attributes = skinAssembly.GetCustomAttributes(true); //typeof(SkinAssemblyAttribute)
                foreach (Attribute attr in attributes)
                {
                    if (attr is SkinAssemblyAttribute skinAttr)
                    {
                        ISkinInfo skinInfo = (ISkinInfo)Activator.CreateInstance(skinAttr.InterfaceImplType);
                        Skins.Add(skinInfo);
                        return;
                    }
                }
                throw new Exception("The \"" + dllName + "\" assembly within the provided folder does not possess the " + typeof(SkinAssemblyAttribute).FullName.ToString() + " attribute.");
            }
            else
                throw new FileNotFoundException("Assembly \"" + dllName + "\" was not found within the folder specified.");
        }*/


        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool TryGetSettingsPathForSkin(SkinInfo info, out string path)
        {
            if ((!string.IsNullOrWhiteSpace(AllSkinsSettingsPath)) && (info != null))
            {
                string name = Path.GetFileName(info.DirectoryPath);
                if (name != null)
                {
                    path = Path.Combine(AllSkinsSettingsPath, name);
                    return true;
                }
                else
                {
                    path = null;
                    return false;
                }
            }
            else
            {
                path = null;
                return false;
            }
        }
    }
}
