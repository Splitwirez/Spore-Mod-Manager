using SporeMods.Core.InstalledMods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SporeMods.Core
{
    //[DebuggerDisplay("Path = {Path} Query = {Query}")]
    public class ModSearch : INotifyPropertyChanged
    {
        ObservableCollection<IInstalledMod> _searchResults = new ObservableCollection<IInstalledMod>();
        public ObservableCollection<IInstalledMod> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                NotifyPropertyChanged(nameof(SearchResults));
            }
        }

        public static ModSearch Instance = new ModSearch();
        private ModSearch()
        {
            ManagedMods.Instance.ModConfigurations.CollectionChanged += (sneder, args) =>
            {
                if ((args.OldItems != null) && (args.OldItems.Count > 0))
                {
                    foreach (IInstalledMod mod in args.OldItems)
                    {
                        if (SearchResults.Contains(mod))
                            SearchResults.Remove(mod);
                    }
                }
            };
        }


        public void CancelSearch()
        {
            if (_searching)
                _cancel = true;
            else
                SearchResults.Clear();
        }

        bool _searching = false;
        bool _cancel = false;
        public async Task StartSearch(string query, bool searchNames, bool searchDescriptions, bool searchTags)
        {
            Task searchTask = new Task(() =>
            {
                _searching = true;
                ObservableCollection<IInstalledMod> mods = new ObservableCollection<IInstalledMod>();
                ManagedMods.SyncContext.Send(state => mods = ManagedMods.Instance.ModConfigurations, null);
                for (int i = 0; i < mods.Count; i++)
                {
                    if (_cancel)
                    {
                        ManagedMods.SyncContext.Send(state => SearchResults.Clear(), null);
                        _cancel = false;
                        break;
                    }
                    else
                    {
                        
                        IInstalledMod mod = mods[i];
                        if (
                        (searchNames && mod.DisplayName.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                        || (searchDescriptions && (mod is InstalledMod imd) && imd.HasDescription && imd.Description.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                        || (searchTags && false/*temp*/)
                        )
                        {
                            ManagedMods.SyncContext.Send(state => {
                                SearchResults.Add(mod);
                            }, null);
                        }
                    }
                }
                _searching = false;
            });
            searchTask.Start();
            await searchTask;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
