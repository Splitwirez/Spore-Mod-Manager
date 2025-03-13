
using SporeMods.Core.Transactions;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public class ManualInstalledFile : NotifyPropertyChangedBase, ISporeMod
    {
        string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasExplicitUnique => throw new NotImplementedException();

        public string Unique => throw new NotImplementedException();

        public bool HasInlineDescription => throw new NotImplementedException();

        public string InlineDescription => throw new NotImplementedException();

        public bool HasExplicitVersion => throw new NotImplementedException();

        public Version ModVersion => throw new NotImplementedException();

        public List<ModDependency> Dependencies => throw new NotImplementedException();

        public List<string> UpgradeTargets => throw new NotImplementedException();

        public bool IsExperimental => throw new NotImplementedException();

        public bool CausesSaveDataDependency => throw new NotImplementedException();

        public bool RequiresGalaxyReset => throw new NotImplementedException();

        IModText ISporeMod.DisplayName => throw new NotImplementedException();

        IModText ISporeMod.InlineDescription => throw new NotImplementedException();

        bool ISporeMod.HasExplicitUnique => throw new NotImplementedException();

        string ISporeMod.Unique => throw new NotImplementedException();

        bool ISporeMod.HasInlineDescription => throw new NotImplementedException();

        bool ISporeMod.HasExplicitVersion => throw new NotImplementedException();

        Version ISporeMod.ModVersion => throw new NotImplementedException();

        List<ModDependency> ISporeMod.Dependencies => throw new NotImplementedException();

        List<string> ISporeMod.UpgradeTargets => throw new NotImplementedException();

        ModWarningLabels ISporeMod.WarningLabels => throw new NotImplementedException();

        Task<Exception> ISporeMod.ApplyAsync(ModTransaction transaction)
        {
            throw new NotImplementedException();
        }

        bool ISporeMod.DependsOn(ISporeMod mod)
        {
            throw new NotImplementedException();
        }

        Task<ModJobBatchEntryBase> ISporeMod.EnsureCanInstall(ModJobBatchModEntry entry, List<ModJobBatchModEntry> otherEntries)
        {
            throw new NotImplementedException();
        }

        Task<Exception> ISporeMod.ExtractRecordFilesAsync(ModTransaction transaction, string inPath, ZipArchive archive)
        {
            throw new NotImplementedException();
        }

        bool ISporeMod.IsUpgradeTo(ISporeMod mod)
        {
            throw new NotImplementedException();
        }

        Task<Exception> ISporeMod.PurgeAsync(ModTransaction transaction)
        {
            throw new NotImplementedException();
        }

        Task<Exception> ISporeMod.RemoveRecordFilesAsync(ModTransaction transaction, bool removeConfig)
        {
            throw new NotImplementedException();
        }

        bool ISporeMod.TryGetFromRecordDir(string subdirName, XDocument doc, out Exception error)
        {
            throw new NotImplementedException();
        }
    }
}