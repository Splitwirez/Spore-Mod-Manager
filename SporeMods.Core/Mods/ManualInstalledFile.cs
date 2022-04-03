/*using SporeMods.Core.ModTransactions;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

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

        public Task<bool> ApplyAsync(ModTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public bool DependsOn(ISporeMod mod)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExtractAllFilesAsync(Func<string> extractFunc, ModTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public bool IsUpgradeTo(ISporeMod mod)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PurgeAsync(ModTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public bool TryLoadFromRecordDir(string location)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UninstallAsync(ModTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public IModAsyncOperation GetExtractFilesAsyncOp(ModTransaction transaction, string inPath, ZipArchive archive = null)
        {
            throw new NotImplementedException();
        }

        public IModAsyncOperation GetApplyAsyncOp(ModTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public IModAsyncOperation GetPurgeAsyncOp(ModTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public IModAsyncOperation GetUninstallAsyncOp(ModTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<InstallOverviewEntryBase> EnsureCanInstall(InstallOverviewModEntry entry, List<InstallOverviewModEntry> otherEntries)
        {
            throw new NotImplementedException();
        }
    }
}*/