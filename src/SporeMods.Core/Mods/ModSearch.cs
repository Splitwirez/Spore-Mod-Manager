using SporeMods.Mods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SporeMods.Mods
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
			ModsManager.InstalledMods.CollectionChanged += (sender, args) =>
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


		public static void CancelSearch()
		{
			if (_searching)
				_cancel = true;
			else
				Instance.SearchResults.Clear();
		}

		static bool _searching = false;
		static bool _cancel = false;

		private static void StartSearch(string query, bool searchNames, bool searchDescriptions, bool searchTags)
		{
			var lowerQuery = query.ToLowerInvariant();

			_searching = true;
			var mods = new ObservableCollection<IInstalledMod>();
			ModsManager.RunOnMainSyncContext(state => mods = ModsManager.InstalledMods);
			for (int i = 0; i < mods.Count; i++)
			{
				if (_cancel)
				{
					ModsManager.RunOnMainSyncContext(state => Instance.SearchResults.Clear());
					_cancel = false;
					break;
				}
				else
				{
						
					IInstalledMod mod = mods[i];
					if (
					(searchNames && mod.DisplayName.ToLowerInvariant().Contains(lowerQuery))
					|| (searchDescriptions && mod.Description != null && mod.Description.ToLowerInvariant().Contains(lowerQuery))
					|| (searchTags && false/*temp*/)
					)
					{
						ModsManager.RunOnMainSyncContext(state => Instance.SearchResults.Add(mod));
					}
				}
			}
			_searching = false;
		}

		public static void StartSearchAsync(string query, bool searchNames, bool searchDescriptions, bool searchTags)
		{
			var task = new Task(() => StartSearch(query, searchNames, searchDescriptions, searchTags));
			task.Start();
		}

		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
