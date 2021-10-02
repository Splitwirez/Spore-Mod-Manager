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
			ModsManager.InstalledMods.CollectionChanged += (_, __) => RefreshColumns();
			IInstalledMod.AnyModStatusChanged += (_, __) =>
			{
				Console.WriteLine("Status changed!");
				RefreshColumns();
				Console.WriteLine($"AnyHasProgressSignifier: {AnyHasProgressSignifier}");
			};
			ModTransactionManager.Instance.AllTasksConcluded += _ => RefreshColumns();
			
			RefreshColumns();
		}

		void RefreshColumns()
		{
			var mmods = ModsManager.InstalledMods.OfType<ManagedMod>();
			bool anyExperimental = false;
			bool anyCausesSaveDataDependency = false;
			bool anyHasProgressSignifier = false;
			foreach (ManagedMod mod in mmods)
			{
				if ((!anyExperimental) && mod.Identity.IsExperimental)
					anyExperimental = true;

				if ((!anyCausesSaveDataDependency) && mod.Identity.CausesSaveDataDependency)
					anyCausesSaveDataDependency = true;

				if ((!anyHasProgressSignifier) && mod.HasProgressSignifier())
					anyHasProgressSignifier = true;
				
				if (anyExperimental && anyCausesSaveDataDependency && anyHasProgressSignifier)
					break;
			}
			AnyExperimental = anyExperimental; //mmods.Any(x => x.Identity.IsExperimental);
			AnyCausesSaveDataDependency = anyCausesSaveDataDependency; //mmods.Any(x => x.Identity.CausesSaveDataDependency);
			AnyHasProgressSignifier = anyHasProgressSignifier; //mmods.Any(x => x.HasProgressSignifier());
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


		bool _anyHasProgressSignifier = false;
		public bool AnyHasProgressSignifier
		{
			get => _anyHasProgressSignifier;
			set
			{
				_anyHasProgressSignifier = value;
				NotifyPropertyChanged();
			}
		}

		public static event EventHandler<EventArgs> SelectedModsChanged;
	}
}
