using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;


using FClipboard = System.Windows.Forms.Clipboard;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.IO;

namespace SporeMods.ViewModels
{
	public class GamePathViewModel : NotifyPropertyChangedBase
	{
		bool _setExplicitPath = false;
		string _workingPath = string.Empty;
		public string WorkingPath
		{
			get => _workingPath;
			set
			{
				_workingPath = value;
				NotifyPropertyChanged();
				if (UseExplicitPath && _setExplicitPath)
				{
					_setExplicit(_workingPath);
					_setExplicitPath = false;
				}
			}
		}


		bool _useExplicitPath = true; //(!Settings.ForcedGalacticAdventuresDataPath.IsNullOrEmptyOrWhiteSpace()) || GameInfo.AutoGalacticAdventuresData.IsNullOrEmptyOrWhiteSpace()
		public bool UseExplicitPath
		{
			get => _useExplicitPath;
			set
			{
				_useExplicitPath = value;
				NotifyPropertyChanged();
			}
		}

		string _header = string.Empty;
		public string HeaderKey
		{
			get => _header;
			set
			{
				_header = value;
				NotifyPropertyChanged();
			}
		}


		public FuncCommand<object> ExplicitBrowseCommand { get; }
			= new FuncCommand<object>(o =>
			{
				//TODO: Implement ExplicitBrowseCommand
				DialogBox.ShowAsync("Browsing for game install paths manually is not yet implemented. (PLACEHOLDER) (NOT LOCALIZED)");
			});

		Func<IEnumerable<string>> _getAuto = null;

		Action<string> _setExplicit = null;
		Func<string> _getExplicit = null;

		GameInfo.GameDlc _dlc = GameInfo.GameDlc.None;
		bool _data = true;

		public GamePathViewModel(GameInfo.GameDlc dlc, bool data, Func<IEnumerable<string>> getAuto, Func<string> getExplicit, Action<string> setExplicit)
		{
			_getExplicit = getExplicit;
			_setExplicit = setExplicit;

			_getAuto = getAuto;

			_dlc = dlc;
			_data = data;

			if (data)
			{
				if (_dlc == GameInfo.GameDlc.GalacticAdventures)
					HeaderKey = "Settings!Folders!AutoGaData";
				else if (_dlc == GameInfo.GameDlc.CoreSpore)
					HeaderKey = "Settings!Folders!AutoCoreData";
			}
			else if (_dlc == GameInfo.GameDlc.GalacticAdventures)
				HeaderKey = "Settings!Folders!AutoSporebinEp1";

			EnsurePaths();
		}

		async void EnsurePaths()
        {
			IEnumerable<string> autoPaths = _getAuto().Where(x => Directory.Exists(x));

			if (autoPaths.Count() > 1)
			{
				//new AmbiguousGamePathViewModel()
			}
			else if (autoPaths.Count() < 1)
			{
				IEnumerable<string> finalPath = await Modal.Show<IEnumerable<string>>(new RequestFilesViewModel(FileRequestPurpose.GamePathNotFound, false));
			}
			else
			{
				UseExplicitPath = false;
				WorkingPath = autoPaths.First();
			}
		}
	}
}
