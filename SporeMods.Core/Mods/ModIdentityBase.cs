using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public abstract class ModIdentityBase : NotifyPropertyChangedBase, ISporeMod, IConfigurableMod
    {
#region Metadata
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

        public bool HasInlineDescription
        {
            get => InlineDescription != null;
        }


        IModText _inlineDescription = null;
        public IModText InlineDescription
        {
            get => _inlineDescription;
            protected set
            {
                _inlineDescription = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(HasInlineDescription));
            }
        }
        #endregion

        #region Warnings
#if DISABLE_DEFAULT_WARNING_IMPL
        bool _isExperimental = false;
        public bool IsExperimental
        {
            get => _isExperimental;
            protected set
            {
                _isExperimental = value;
                NotifyPropertyChanged();
                this.WhenWarningPropertyChanged(value, ref _warningLabels);
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
                this.WhenWarningPropertyChanged(value, ref _warningLabels);
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
                this.WhenWarningPropertyChanged(value, ref _warningLabels);
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
                this.WhenWarningPropertyChanged(value, ref _warningLabels);
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
                this.WhenWarningPropertyChanged(value, ref _warningLabels);
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
                this.WhenWarningPropertyChanged(value, ref _warningLabels);
            }
        }

        

        ObservableCollection<ModWarningLabel> _warningLabels = new ObservableCollection<ModWarningLabel>();
        public ObservableCollection<ModWarningLabel> WarningLabels
        {
            get => _warningLabels;
        }
#else
        ModWarningLabels _warningLabels = new ModWarningLabels();
        public ModWarningLabels WarningLabels
        {
            get => _warningLabels;
        }
#endif
#endregion

        public abstract bool HasExplicitVersion
        {
            get;
        }

        public abstract Version ModVersion
        {
            get;
        }

        public abstract List<ModDependency> Dependencies
        {
            get;
        }

        public abstract List<string> UpgradeTargets
        {
            get;
        }

        bool _hasSettings = false;
        public virtual bool HasSettings
        {
            get => _hasSettings;
            protected set
            {
                _hasSettings = value;
                NotifyPropertyChanged();
            }
        }

        public abstract bool IsUpgradeTo(ISporeMod mod);

        public abstract bool DependsOn(ISporeMod mod);

        public abstract bool TryGetFromRecordDir(string subdirName, XDocument doc, out Exception error);

        public abstract Task<ModJobBatchEntryBase> EnsureCanInstall(ModJobBatchModEntry entry, List<ModJobBatchModEntry> otherEntries);

        public abstract Task<Exception> ExtractRecordFilesAsync(ModTransaction transaction, string inPath, ZipArchive archive = null);

        public abstract Task<Exception> ApplyAsync(ModTransaction transaction);

        public abstract Task<Exception> PurgeAsync(ModTransaction transaction);

        public abstract Task<Exception> RemoveRecordFilesAsync(ModTransaction transaction, bool removeConfig);

        public abstract IViewLocatable GetSettingsViewModel(bool postInstall);
    }
}
