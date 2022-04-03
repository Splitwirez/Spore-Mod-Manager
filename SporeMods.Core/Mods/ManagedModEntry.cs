/*using SporeMods.Core.Mods.ModIdentity;
using SporeMods.Core.ModTransactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
    public class ManagedMod : NotifyPropertyChangedBase, IModEntry
    {
        IModIdentity _identity = null;

        string _displayName = string.Empty;
        public string DisplayName
        {
            get => _identity != null ? _identity.DisplayName : _displayName;
            set
            {
                _displayName = value;
                NotifyPropertyChanged();
            }
        }

        public Version ModVersion
        {
            get => _identity.ModVersion;
        }

        public bool DependsOn(IPartialMod mod)
            => _identity.DependsOn(mod);

        public bool IsSameModAs(IPartialMod mod)
            => _identity.IsSameModAs(mod);

        public bool TryLoadFromRecordDir(string location, bool nameOnly = true)
        {
            string path = nameOnly ? Path.Combine(Settings.ModConfigsPath, location) : location;

            return _identity.TryLoadFromRecordDir(path);
        }

        public async Task<bool> UninstallAsync(ModTransaction transaction)
            => await _identity.UninstallAsync(transaction);


        public List<ModDependency> Dependencies
        {
            get => _identity.Dependencies;
        }
    }
}*/