using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SporeMods.Core;
using SporeMods.Core.Mods;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
using System.IO;

namespace SporeMods.ViewModels
{
	public class GamePathSettingsViewModel : NotifyPropertyChangedBase
	{
		GamePathViewModel _gaData = null;
		public GamePathViewModel GaData
		{
			get => _gaData;
			private set
			{
				_gaData = value;
				NotifyPropertyChanged();
			}
		}


		GamePathViewModel _sporebinEp1 = null;
		public GamePathViewModel SporebinEp1
		{
			get => _sporebinEp1;
			private set
			{
				_sporebinEp1 = value;
				NotifyPropertyChanged();
			}
		}


		GamePathViewModel _coreData = null;
		public GamePathViewModel CoreData
		{
			get => _coreData;
			private set
			{
				_coreData = value;
				NotifyPropertyChanged();
			}
		}

		public GamePathSettingsViewModel()
		{
			GaData = new GamePathViewModel(GameInfo.GameDlc.GalacticAdventures, true, () => GameInfo.GetAllGameInstallPathsFromRegistry(GameInfo.GameDlc.GalacticAdventures)/*.Where(x => Directory.Exists(Path.Combine(x, "DataEP1")) || Directory.Exists(Path.Combine(x, "Data")))*/,
											() => Settings.ForcedGalacticAdventuresDataPath,
											newPath => Settings.ForcedGalacticAdventuresDataPath = newPath
										);

			SporebinEp1 = new GamePathViewModel(GameInfo.GameDlc.GalacticAdventures, false, () => GameInfo.GetAllGameInstallPathsFromRegistry(GameInfo.GameDlc.GalacticAdventures)/*.Where(x => Directory.Exists(Path.Combine(x, "SporebinEP1")))*/,
											() => Settings.ForcedGalacticAdventuresDataPath,
											newPath => Settings.ForcedGalacticAdventuresDataPath = newPath
										);

			CoreData = new GamePathViewModel(GameInfo.GameDlc.CoreSpore, true, () => GameInfo.GetAllGameInstallPathsFromRegistry(GameInfo.GameDlc.CoreSpore)/*.Where(x => Directory.Exists(Path.Combine(x, "DataEP1")) || Directory.Exists(Path.Combine(x, "Data")))*/,
											() => Settings.ForcedGalacticAdventuresDataPath,
											newPath => Settings.ForcedGalacticAdventuresDataPath = newPath
										);
		}
	}
}
