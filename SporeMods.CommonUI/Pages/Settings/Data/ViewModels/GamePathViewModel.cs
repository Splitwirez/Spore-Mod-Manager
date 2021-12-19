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


		bool _enableRespondInSetter = false;
		bool _useExplicitPath = true;
		public bool UseExplicitPath
		{
			get => _useExplicitPath;
			set
			{
				bool actuallyChanged = _useExplicitPath != value;
				_useExplicitPath = value;
				NotifyPropertyChanged();
				if (_enableRespondInSetter && actuallyChanged)
				{
					if (value)
						BrowseForExplicitPath();
					else
						EnsurePaths();
				}
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


		FuncCommand<object> _explicitBrowseCommand = null;
		public FuncCommand<object> ExplicitBrowseCommand
		{
			get => _explicitBrowseCommand;
			private set
			{
				if (_explicitBrowseCommand == null)
					_explicitBrowseCommand = value;
				
				NotifyPropertyChanged();
			}
		}
		
		async void BrowseForExplicitPath()
		{
			//TODO: Implement BrowseForExplicitPath properly
			await DialogBox.ShowAsync("Browsing for game install paths manually is not yet re-implemented. (PLACEHOLDER) (NOT LOCALIZED)");
			UseExplicitPath = false;
		}

		Func<IEnumerable<string>> _getAuto = null;

		Action<string> _setExplicit = null;
		Func<string> _getExplicit = null;

		GameInfo.GameDlc _dlc = GameInfo.GameDlc.None;
		bool _data = true;

		static readonly Dictionary<GameInfo.GameDlc, List<string>> DATA_DIR_NAMES = new Dictionary<GameInfo.GameDlc, List<string>>()
		{
			{
				GameInfo.GameDlc.CoreSpore,
				new List<string>()
				{
					"Data"
				}
			},
			{
				GameInfo.GameDlc.GalacticAdventures,
				new List<string>()
				{
					"DataEP1",
					"Data"
				}
			}
		};

		static readonly Dictionary<GameInfo.GameDlc, List<string>> BIN_DIR_NAMES = new Dictionary<GameInfo.GameDlc, List<string>>()
		{
			{
				GameInfo.GameDlc.CoreSpore,
				new List<string>()
				{
					"SporeBin"
				}
			},
			{
				GameInfo.GameDlc.GalacticAdventures,
				new List<string>()
				{
					"SporebinEP1"
				}
			}
		};

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


			ExplicitBrowseCommand = new FuncCommand<object>(o => BrowseForExplicitPath());

			EnsurePaths();
		}

		async void EnsurePaths()
        {
			List<string> dirNames = _data ? DATA_DIR_NAMES[_dlc] : BIN_DIR_NAMES[_dlc];


			IEnumerable<string> rawAutoPaths = _getAuto().Where(x => Directory.Exists(x));
			List<string> autoPaths = new List<string>();
			foreach (string rawPath in rawAutoPaths)
			{
				if (GameInfo.CorrectGameInstallPath(rawPath, _dlc, out string path))
				{
					if (Directory.Exists(path))
					{
						foreach (string s in dirNames)
						{
							string combined = Path.Combine(path, s);
							
							if (Directory.Exists(combined) && (!autoPaths.Any(x => x.Equals(combined, StringComparison.OrdinalIgnoreCase))))
							{
								autoPaths.Add(combined);
								break;
							}
						}
					}
				}
			}

			if (autoPaths.Count() > 1)
			{
				//new AmbiguousGamePathViewModel()
				Cmd.WriteLine("Ambiguous game path");
			}
			else if (autoPaths.Count() < 1)
			{
				IEnumerable<string> finalPath = await Modal.Show<IEnumerable<string>>(new RequestFilesViewModel(FileRequestPurpose.GamePathNotFound, false));
				Cmd.WriteLine("Couldn't detect game path");
			}
			else
			{
				UseExplicitPath = false;
				WorkingPath = autoPaths.First();
				Cmd.WriteLine("Game path OK");
			}

			_enableRespondInSetter = true;
		}
	}
}
