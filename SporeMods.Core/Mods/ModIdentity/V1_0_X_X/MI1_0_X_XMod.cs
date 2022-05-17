using SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents;
using System;
using System.Collections.Generic;
using System.IO.Compression;
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

        public bool IsExperimental => false;

        public bool CausesSaveDataDependency => false;

        public bool RequiresGalaxyReset => false;

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

        public bool GuaranteedVanillaCompatible => false;


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


        public string GetViewTypeName()
        {
            string typeName = this.GetType().FullName;
            var seg = typeName.Split('.');
            typeName = seg[seg.Length - 1];
            typeName = "SporeMods.CommonUI.Configurators." + typeName + "ConfiguratorView";
            return typeName;
        }

        public Task<ModJobBatchEntryBase> EnsureCanInstall(ModJobBatchModEntry entry, List<ModJobBatchModEntry> otherEntries)
        {
            throw new NotImplementedException();
        }
    }
}
