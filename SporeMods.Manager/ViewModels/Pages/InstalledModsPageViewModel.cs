using Avalonia;
using SporeMods.CommonUI;
using SporeMods.Core;
using SporeMods.Core.Mods;
using SporeMods.NotifyOnChange;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace SporeMods.Manager.ViewModels
{
    public class InstalledModsPageViewModel : NOCObject
    {
		/*NOCRespondProperty<ObservableCollection<IInstalledMod>> _selectedMods;
        ObservableCollection<IInstalledMod> SelectedMods
        {
            get => _selectedMods.Value;
            set => _selectedMods.Value = value;
        }*/

        public static event EventHandler SelectionChanged;

        public InstalledModsPageViewModel()
            : base()
        {
            /*_selectedMods = AddProperty(new NOCRespondProperty<ObservableCollection<IInstalledMod>>(nameof(SelectedMods), new ObservableCollection<IInstalledMod>())
            {
                ValueChangeResponse = (obj, oldVal, newVal) =>
                {
                    Console.WriteLine("SelectedMods changed!");
                    if (newVal != null)
                        SelectionChanged?.Invoke(newVal, null);
                }
            });*/

            //_searchQuery = AddProperty(new NOCRespondProperty<string>(string.Empty)

            _isSearching = AddProperty(new NOCRespondProperty<bool>(nameof(IsSearching), false)
            {
                ValueChangeResponse = ((x, o, n) => RefreshSearch())
            });

            _searchQuery = AddProperty(new NOCRespondProperty<string>(nameof(SearchQuery), string.Empty)
            {
                ValueChangeResponse = ((x, o, n) => RefreshSearch())
            });
        }

        void RefreshSearch()
        {
            if (IsSearching && (!SearchQuery.IsNullOrEmptyOrWhiteSpace()))
                ModSearch.StartSearchAsync(SearchQuery, true, false, false); //TODO: toggle names/desc/etc
            else
            {
                ModSearch.CancelSearch();
                SearchQuery = string.Empty;
                IsSearching = false;
            }
        }

        NOCRespondProperty<string> _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery.Value;
            set => _searchQuery.Value = value;
        }

        NOCRespondProperty<bool> _isSearching;
        public bool IsSearching
        {
            get => _isSearching.Value;
            set => _isSearching.Value = value;
        }
    }
}