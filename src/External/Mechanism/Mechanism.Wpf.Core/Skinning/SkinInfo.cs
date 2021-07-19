using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Mechanism.Wpf.Core.Skinning
{
    public class SkinInfo : INotifyPropertyChanged
    {

        static string displayNameXmlTag = "displayName";
        static string authorNameXmlTag = "authorName";
        static string guidXmlTag = "globalUniqueIdentifier";
        static string dllNameXmlTag = "mainAssemblyName";
        static string skinVersionXmlTag = "skinVersion";
        static string appVersionXmlTag = "appVersion";


        SkinAssemblyInfo _skinAssemblyInfo = null;
        public string DirectoryPath = null;
        public bool AreAssembliesLoaded = false;
        
        string _mainDllName = string.Empty;


        string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                NotifyPropertyChanged(nameof(DisplayName));
            }
        }

        Guid _skinGuid = Guid.Empty;
        public Guid SkinGuid
        {
            get => _skinGuid;
            set
            {
                _skinGuid = value;
                NotifyPropertyChanged(nameof(SkinGuid));
            }
        }

        string _author = string.Empty;
        public string Author
        {
            get => _author;
            set
            {
                _author = value;
                NotifyPropertyChanged(nameof(Author));
            }
        }

        Version _skinVersion = new Version(0, 0, 0, 0);
        public Version SkinVersion
        {
            get => _skinVersion;
            set
            {
                _skinVersion = value;
                NotifyPropertyChanged(nameof(SkinVersion));
            }
        }

        Version _appVersion = new Version(0, 0, 0, 0);
        public Version AppVersion
        {
            get => _appVersion;
            set
            {
                _appVersion = value;
                NotifyPropertyChanged(nameof(AppVersion));
            }
        }

        internal SkinInfo(string name, string author, Guid guid, Version skinVersion, Version appVersion)
        {
            DisplayName = name;
            Author = author;
            SkinGuid = guid;
            SkinVersion = skinVersion;
            AppVersion = appVersion;
        }

        internal SkinInfo(string name, string author, string directoryPath, string dllName, Guid guid, Version skinVersion, Version appVersion) : this(name, author, guid, skinVersion, appVersion)
        {
            DirectoryPath = directoryPath;
            _mainDllName = dllName;
        }

        public SkinInfo(SkinAssemblyInfo info, string name, string author, Guid guid, Version skinVersion, Version appVersion) : this(name, author, guid, skinVersion, appVersion)
        {
            _skinAssemblyInfo = info;
            AreAssembliesLoaded = true;
        }

        public static bool IsXmlValidForSkin(XDocument doc, out string displayName, out string authorName, out string dllName, out Guid skinGuid, out Version skinVersion, out Version appVersion)
        {
            bool displayValid = false;
            bool authorValid = false;
            bool dllValid = false;
            bool guidValid = false;
            bool skinVerValid = false;
            bool appVerValid = false;

            displayName = null;
            authorName = null;
            skinGuid = Guid.Empty;
            skinVersion = new Version(0, 0, 0, 0);
            appVersion = new Version(0, 0, 0, 0);

            XElement root = doc.Root;


            displayName = LocalizedGetNodeValue(root, doc.Root, displayNameXmlTag);
            displayValid = displayName != null;
                
            dllName = LocalizedGetNodeValue(root, doc.Root, dllNameXmlTag);
            dllValid = dllName != null;

            authorName = LocalizedGetNodeValue(root, doc.Root, authorNameXmlTag);
            authorValid = authorName != null;

            guidValid = TryGetSkinXmlGuid(doc, out skinGuid);


            if (Version.TryParse(LocalizedGetNodeValue(root, doc.Root, skinVersionXmlTag), out Version skinVal))
            {
                skinVersion = skinVal;
                skinVerValid = true;
            }

            if (Version.TryParse(LocalizedGetNodeValue(root, doc.Root, appVersionXmlTag), out Version appVal))
            {
                appVersion = appVal;
                appVerValid = true;
            }

            return displayValid && authorValid && dllValid && guidValid && skinVerValid && appVerValid;
        }

        public static bool TryGetSkinXmlGuid(XDocument doc, out Guid skinGuid)
        {
            if (Guid.TryParse(GetRelativeNodeValue(doc.Root, guidXmlTag), out Guid skinGuidVal))
            {
                skinGuid = skinGuidVal;
                return true;
            }
            else
            {
                skinGuid = Guid.Empty;
                return false;
            }
        }


        public void LoadAssemblies()
        {
            string dllPath = Path.Combine(DirectoryPath, _mainDllName + ".dll");

            if (File.Exists(dllPath))
            {
                List<Assembly> dependencies = new List<Assembly>();
                AssemblyName[] refs = Assembly.GetEntryAssembly().GetReferencedAssemblies();
                foreach (string s in Directory.EnumerateFiles(DirectoryPath, "*.dll").Where(x => (x.Equals(dllPath, StringComparison.OrdinalIgnoreCase) == false)))
                {
                    //string filename = Path.GetFileName(s);
                    //string combinedPath = Path.Combine(Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).ToString(), filename);
                    /*if (!File.Exists(combinedPath))
                    {*/
                    /*try
                    {*/
                    /*bool conflictFound = false;
                    foreach (AssemblyName name in refs)
                    {
                        //MessageBox.Show(name.Name);
                        if (Path.GetFileNameWithoutExtension(s).Equals(name.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            conflictFound = true;
                            break;
                        }
                    }*/
                    if ((refs.FirstOrDefault(x => Path.GetFileNameWithoutExtension(s).Equals(x.Name, StringComparison.OrdinalIgnoreCase)) == null) && File.Exists(s))//!conflictFound
                        dependencies.Add(Assembly.LoadFrom(s));
                    /*}
                    catch (Exception ex)
                    {
                        throw new Exception();
                        //Debug.WriteLine(ex);
                    }*/
                    //}
                }
                Assembly skinAssembly = Assembly.LoadFrom(dllPath);
                /*AssemblyName[] names = skinAssembly.GetReferencedAssemblies();
                foreach (AssemblyName n in names)
                {
                    MessageBox.Show(n.FullName);
                    string dependencyPath = Path.Combine(DirectoryPath, n.FullName);
                    if (File.Exists(dependencyPath))
                    {
                        try
                        {
                            dependencies.Add(Assembly.LoadFile(dependencyPath));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception();
                        }
                    }
                }*/
                var attributes = skinAssembly.GetCustomAttributes(true); //typeof(SkinAssemblyAttribute)
                foreach (Attribute attr in attributes)
                {
                    if (attr is SkinAssemblyAttribute skinAttr)
                    {
                        _skinAssemblyInfo = (SkinAssemblyInfo)Activator.CreateInstance(skinAttr.InterfaceImplType);
                    }
                }
                if (_skinAssemblyInfo == null)
                    throw new Exception(); // "The \"" + dllName + "\" assembly within the provided folder does not possess the " + typeof(SkinAssemblyAttribute).FullName.ToString() + " attribute.");
                else
                    AreAssembliesLoaded = true;
            }
            else
                throw new FileNotFoundException(); // "Assembly \"" + dllName + "\" was not found within the folder specified.");
        }

        internal static SkinInfo FromFolder(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                throw new DirectoryNotFoundException();

            string xmlPath = Path.Combine(dirPath, "SkinInfo.xml");
            if (!File.Exists(xmlPath))
                throw new FileNotFoundException();

            XDocument xml = XDocument.Load(xmlPath);

            if (IsXmlValidForSkin(xml, out string displayName, out string authorName, out string dllName, out Guid skinGuid, out Version skinVersion, out Version appVersion))
            {
                //string path = Path.Combine(dirPath, dllName);
                return new SkinInfo(displayName, authorName, dirPath, dllName, skinGuid, skinVersion, appVersion);
            }
            else
                return null;
        }

        static string LocalizedGetNodeValue(XElement localizedElement, XElement fallbackElement, string tagName)
        {
            string ret = GetRelativeNodeValue(localizedElement, tagName);

            if (ret == null)
                ret = GetRelativeNodeValue(fallbackElement, tagName);

            return ret;
        }

        static string GetRelativeNodeValue(XElement element, string tagName)
        {
            IEnumerable<XElement> names = element.Descendants();
            foreach (XElement nameNode in names)
            {
                if (nameNode.Name.LocalName.Equals(tagName))
                    return nameNode.Value;
            }

            return null;
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SettingsHaveChanged
        {
            get => _skinAssemblyInfo.SettingsHaveChanged;
            set => _skinAssemblyInfo.SettingsHaveChanged = value;
        }

        public Page GetSettingsPage()
        {
            if (!AreAssembliesLoaded)
                LoadAssemblies();
            return _skinAssemblyInfo.SettingsPage;
        }

        public ResourceDictionary GetSkinDictionary()
        {
            if (!AreAssembliesLoaded)
                LoadAssemblies();
            return _skinAssemblyInfo.GetSkinDictionary();
        }

        public bool LoadSkinSettings(string inputPath)
        {
            if (AreAssembliesLoaded)
            {
                _skinAssemblyInfo.LoadSkinSettings(inputPath);
                return true;
            }
            else
                return false;
        }

        public bool SaveSkinSettings(string outputPath)
        {
            if (AreAssembliesLoaded)
            {
                _skinAssemblyInfo.SaveSkinSettings(outputPath);
                return true;
            }
            else
                return false;
        }

        public bool ResetSkinSettings()
        {
            if (AreAssembliesLoaded)
            {
                _skinAssemblyInfo.ResetSkinSettings();
                return true;
            }
            else
                return false;
        }
    }
}