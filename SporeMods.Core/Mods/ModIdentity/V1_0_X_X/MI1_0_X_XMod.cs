﻿using SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public abstract partial class MI1_0_X_XMod : NotifyPropertyChangedBase, ISporeMod, IConfigurableMod
    {
        IModText _fallbackDisplayName = new FixedModText(string.Empty);
        IModText _displayName = null;
        public IModText DisplayName
        {
            get => (_displayName != null) ? _displayName : _fallbackDisplayName;
            protected set
            {
                _displayName = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasExplicitUnique => true;


        string _unique = string.Empty;
        public string Unique
        {
            get => _unique;
            protected set
            {
                _unique = value;
                _fallbackDisplayName = new FixedModText(_unique);
                NotifyPropertyChanged();
            }
        }

        string _recordDirName = string.Empty;
        public string RecordDirName
        {
            get => _recordDirName;
            protected set
            {
                _recordDirName = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasInlineDescription => false;


        IModText _inlineDescription = null;
        public IModText InlineDescription
        {
            get => _inlineDescription;
            protected set
            {
                _inlineDescription = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasExplicitVersion => false;

        public Version ModVersion => new Version(0, 0, 0, 0);

        public List<ModDependency> Dependencies => new List<ModDependency>();

        public List<string> UpgradeTargets => new List<string>();



        bool _isExperimental = false;
        public bool IsExperimental
        {
            get => _isExperimental;
            protected set
            {
                _isExperimental = value;
                NotifyPropertyChanged();
            }
        }

        bool _causesSaveDataDependency = false;
        public bool CausesSaveDataDependency
        {
            get => _causesSaveDataDependency;
            protected set
            {
                _causesSaveDataDependency = value;
                NotifyPropertyChanged();
            }
        }

        bool _requiresGalaxyReset = false;
        public bool RequiresGalaxyReset
        {
            get => _requiresGalaxyReset;
            protected set
            {
                _requiresGalaxyReset = value;
                NotifyPropertyChanged();
            }
        }


        bool _usesCodeInjection = false;
        public bool UsesCodeInjection
        {
            get => _usesCodeInjection;
            protected set
            {
                _usesCodeInjection = value;
                NotifyPropertyChanged();
            }
        }

        bool _guaranteedVanillaCompatible = false;
        public bool GuaranteedVanillaCompatible
        {
            get => _guaranteedVanillaCompatible;
            protected set
            {
                _guaranteedVanillaCompatible = value;
                NotifyPropertyChanged();
            }
        }

        bool _knownHazardousMod = false;
        public bool KnownHazardousMod
        {
            get => _knownHazardousMod;
            protected set
            {
                _knownHazardousMod = value;
                NotifyPropertyChanged();
            }
        }


        public bool IsUpgradeTo(ISporeMod mod)
            => Unique == mod.Unique;

        public bool DependsOn(ISporeMod mod)
            => false;

        bool _hasSettings = false;
        public bool HasSettings
        {
            get => _hasSettings;
            protected set
            {
                _hasSettings = value;
                NotifyPropertyChanged();
            }
        }
        /*get => (this is IConfigurableMod cm) ? cm.GetHasSettings() : false;
        public bool GetHasSettings()
            => _hasSettings;
        protected void SetHasSettings(bool value)
        {
            _hasSettings = value;
            NotifyPropertyChanged(nameof(((ISporeMod)this).HasSettings));
        }*/

        internal bool IsIncoming = false;


        ThreadSafeObservableCollection<ComponentBase> _allComponents = new ThreadSafeObservableCollection<ComponentBase>();
        public ThreadSafeObservableCollection<ComponentBase> AllComponents
        {
            get => _allComponents;
        }


        ThreadSafeObservableCollection<ComponentBase> _featureComponents = new ThreadSafeObservableCollection<ComponentBase>();
        public ThreadSafeObservableCollection<ComponentBase> FeatureComponents
        {
            get => _featureComponents;
        }

        public Task<ModJobBatchEntryBase> EnsureCanInstall(ModJobBatchModEntry entry, List<ModJobBatchModEntry> otherEntries)
        {
            throw new NotImplementedException();
        }


        public object GetSettingsViewModel(bool postInstall)
            => new MI1_0_X_XModSettingsViewModel(this);
        public string GetSettingsViewTypeName(bool postInstall)
        {
            string typeName = typeof(MI1_0_X_XMod).FullName; //this.GetType().FullName;
            var seg = typeName.Split('.');
            typeName = seg[seg.Length - 1];
            typeName = $"SporeMods.Views.{typeName}SettingsView";
            //typeName = $"SporeMods.Views.{typeof(MI1_0_X_XMod).FullName.Split('.', StringSplitOptions.RemoveEmptyEntries).Last()}SettingsView";
            return typeName;
        }

        public static ISporeMod FromRecordDir(string location, XDocument doc)
        {
            MI1_0_X_XMod mod = null;
            Version identityVersion = EnsureIdentityVersion(doc);
            string identityPath = Path.Combine(location, SporeMods.Core.Mods.ModConstants.ID_XML_FILE_NAME);
            var xmlRoot = doc.Root;
            if (xmlRoot.TryGetAttributeValue("unique", out string unique))
            {
                List<string> fileNames = new List<string>();
                foreach (string f in Directory.EnumerateFiles(location))
                {
                    fileNames.Add(Path.GetFileName(f));
                }


                if (identityVersion == ModConstants.ID_VER_1_0_0_0)
                    mod = new MI1_0_0_0Mod(location, unique, fileNames);
                else
                    mod = new MI1_0_1_1Mod(location, unique, fileNames);

                mod.ReadIdentityRoot(xmlRoot);
            }
            return mod;
        }

        private static Version EnsureIdentityVersion(XDocument doc)
        {
            var versionAttr = doc.Root.Attribute("installerSystemVersion");
            Version identityVersion = new Version(0, 0, 0, 0);
            if (versionAttr == null)
                throw new FormatException(Externals.GetLocalizedText("Mods!Error!Identity!MissingSysVersion"));
            else if (Version.TryParse(versionAttr.Value, out identityVersion))
            {
                if (
                           (identityVersion != ModConstants.ID_VER_1_0_0_0)
                        && (identityVersion != ModConstants.ID_VER_1_0_1_0)
                        && (identityVersion != ModConstants.ID_VER_1_0_1_1)
                    )
                    throw new FormatException(Externals.GetLocalizedText("Mods!Error!Identity!UnsupportedSysVersion").Replace("%VERSION%", identityVersion.ToString()));
            }
            else
                throw new FormatException(Externals.GetLocalizedText("Mods!Error!Identity!InvalidAttributeValue").Replace("%ATTRIBUTE%", "installerSystemVersion").Replace("%VALUE%", versionAttr.Value).Replace("%TYPE%", "Version"));
            
            return identityVersion;
        }
    }
}
