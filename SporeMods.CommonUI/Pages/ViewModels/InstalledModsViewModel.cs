using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SporeMods.Core;
using SporeMods.Core.Mods;
using SporeMods.CommonUI;
using SporeMods.Core.ModTransactions;

namespace SporeMods.ViewModels
{
	public class InstalledModsViewModel : NotifyPropertyChangedBase
	{
		bool _isSearching = false;
		public bool IsSearching
		{
			get => _isSearching;
			set
			{
				Console.WriteLine($"IsSearching: {value}");
				_isSearching = Search(value, SearchQuery) ? value : false;
				NotifyPropertyChanged();

				if (!value)
					SearchQuery = string.Empty;
			}
		}

		
		string _searchQuery = string.Empty;
		public string SearchQuery
		{
			get => _searchQuery;
			set
			{
				if (IsSearching)
					Search(true, value);
				
				_searchQuery = value;
				NotifyPropertyChanged();
			}
		}


		bool _searchNames = true;
		public bool SearchNames
		{
			get => _searchNames;
			set
			{
				_searchNames = value;
				NotifyPropertyChanged();

				if (IsSearching)
					Search(true, SearchQuery);
			}
		}


		bool _searchDescriptions = false;
		public bool SearchDescriptions
		{
			get => _searchDescriptions;
			set
			{
				_searchDescriptions = value;
				NotifyPropertyChanged();

				if (IsSearching)
					Search(true, SearchQuery);
			}
		}


		bool Search(bool isSearching, string searchQuery)
		{
			bool search = isSearching && (!searchQuery.IsNullOrEmptyOrWhiteSpace());
			ModSearch.CancelSearch();
			
			if (search)
				ModSearch.StartSearchAsync(searchQuery, SearchNames, SearchDescriptions, false/*tags*/);
			
			return search;
		}


		List<IInstalledMod> _selectedMods = new List<IInstalledMod>();
		public List<IInstalledMod> SelectedMods
		{
			get => _selectedMods;
			set
			{
				_selectedMods = value.ToList();
				NotifyPropertyChanged();
				SelectedModsChanged?.Invoke(_selectedMods, null);
			}
		}



		public InstalledModsViewModel()
			: base()
		{
			ModsManager.InstalledMods.CollectionChanged += (s, e) => RefreshColumns();
			ModTransactionManager.Instance.AllTasksConcluded += (tasks) => RefreshColumns();
			
			RefreshColumns();
		}

		void RefreshColumns()
		{
			AnyExperimental = ModsManager.InstalledMods.OfType<ManagedMod>().Any(x => x.Identity.IsExperimental);
			AnyCausesSaveDataDependency = ModsManager.InstalledMods.OfType<ManagedMod>().Any(x => x.Identity.CausesSaveDataDependency);
		}


		bool _anyExperimental = false;
		public bool AnyExperimental
		{
			get => _anyExperimental;
			set
			{
				_anyExperimental = value;
				NotifyPropertyChanged();
			}
		}


		bool _anyCausesSaveDataDependency = false;
		public bool AnyCausesSaveDataDependency
		{
			get => _anyCausesSaveDataDependency;
			set
			{
				_anyCausesSaveDataDependency = value;
				NotifyPropertyChanged();
			}
		}

		public static event EventHandler<EventArgs> SelectedModsChanged;
	}
}
