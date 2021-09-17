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

namespace SporeMods.ViewModels
{
	public class MainViewModel : NotifyPropertyChangedBase
	{
		int _selectedTabIndex = 0;

		public int SelectedTabIndex
		{
			get => _selectedTabIndex;
			set
			{
				_selectedTabIndex = value;
				NotifyPropertyChanged();
			}
		}


		string _title = string.Empty;
		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				NotifyPropertyChanged();
			}
		}


		bool _canChangeModSettings = false;
		public bool CanChangeModSettings
		{
			get => _canChangeModSettings;
			set
			{
				_canChangeModSettings = value;
				NotifyPropertyChanged();
			}
		}


		bool _canUninstallMods = false;
		public bool CanUninstallMods
		{
			get => _canUninstallMods;
			set
			{
				_canUninstallMods = value;
				NotifyPropertyChanged();
			}
		}


		bool _canLaunchSpore = true;
		public bool CanLaunchSpore
		{
			get => _canLaunchSpore;
			set
			{
				_canLaunchSpore = value;
				NotifyPropertyChanged();
			}
		}

		public FuncCommand<object> ShowTestDialogCommand { get; }
			= new FuncCommand<object>(o =>
		{
			DialogBox.ShowAsync("Hello world!", "Yeehaw");
		});


		FuncCommand<object> _installMods = null;
		public FuncCommand<object> InstallModsCommand
		{
			get => _installMods;
		}

		FuncCommand<object> _uninstallMods = null;
		public FuncCommand<object> UninstallModsCommand
		{
			get => _uninstallMods;
		}
		
		FuncCommand<object> _changeModSettings = null;
		public FuncCommand<object> ChangeModSettingsCommand
		{
			get => _changeModSettings;
		}

		FuncCommand<object> _launchSpore = null;
		public FuncCommand<object> LaunchSporeCommand
		{
			get => _launchSpore;
		}


		/*static Process _dragServantProcess = null;
		public static Process DragServantProcess
		{
			get => _dragServantProcess;
			private set => _dragServantProcess = value;
		}
		public static bool HasDragServant
		{
			get => (DragServantProcess != null) && (!DragServantProcess.HasExited);
		}*/

		public MainViewModel()
			: base()
		{
			LanguageManager.LanguageChanged += (s, e) => RefreshTitle();
			RefreshTitle();

			_installMods = new FuncCommand<object>(o => InstallMods());
			_uninstallMods = new FuncCommand<object>(o => UninstallMods());
			_changeModSettings = new FuncCommand<object>(o => ChangeModSettings());

			_launchSpore = new FuncCommand<object>(o =>
			{
				if (CanLaunchSpore)
				{
					if (Permissions.IsAtleastWindowsVista() && Permissions.IsAdministrator() && ServantCommands.HasDragServant)
						ServantCommands.RunLauncher();
					else// if (!Permissions.IsAdministrator())
						CrossProcess.StartLauncher();

					_minimizeOnGameStart = true;
				}
			});

			InstalledModsViewModel.SelectedModsChanged += (s, e) =>
			{
				if (s is IEnumerable<IInstalledMod> mods)
				{
					RefreshCanDoesThings(mods);
					_selectedMods = mods;
				}
			};

			ManagedMod.AnyModIsProgressingChanged += (s, e) => CanLaunchSpore = !AreAnyProgressing(ModsManager.InstalledMods);
		}

		void RefreshCanDoesThings(IEnumerable<IInstalledMod> mods)
		{
			CanUninstallMods = false;
			CanChangeModSettings = false;
			
			int count = mods.Count();
			
			if (count >= 1)
			{
				CanUninstallMods = !AreAnyProgressing(mods);
				CanChangeModSettings = (mods.Count() == 1) && (mods.First() is ManagedMod mmod) && (!mmod.IsProgressing) && mmod.HasConfigurator;
			}
		}

		bool AreAnyProgressing(IEnumerable<IInstalledMod> mods)
			=> mods.Any(x => (x is ManagedMod mmod) ? mmod.IsProgressing : false);

		void RefreshTitle()
		{
			Title = LanguageManager.Instance.GetLocalizedText("Header!" +
#if SET_BUILD_CHANNEL || DEBUG
				"WithBuildChannel"
#else
				"NoBuildChannel"
#endif
				)
				.Replace("%VERSION%", Settings.ModManagerVersion.ToString())
				.Replace("%DLLSBUILD%", Settings.CurrentDllsBuildString)
				.Replace("%DOTNETRUNTIME%", Settings.TargetFramework)
#if SET_BUILD_CHANNEL || DEBUG
				.Replace("%BUILDCHANNEL%", Settings.BuildChannel)
#endif
				;
		}

		async Task InstallMods()
		{
			var files = await Modal.Show(new RequestFilesViewModel(FileRequestPurpose.InstallMods, true));
			if (files != null)
				ModInstallation.InstallModsAsync(files.ToArray());
		}

		
		IEnumerable<IInstalledMod> _selectedMods = null;
		async Task UninstallMods()
		{
			if (
					(_selectedMods != null) &&
					(_selectedMods.Count() > 0) &&
					(!_selectedMods.Any(x => (x is ManagedMod mmod) ? mmod.IsProgressing : false))
				)
				ModInstallation.UninstallModsAsync(_selectedMods.ToArray());
			else
			{
				CanUninstallMods = false;
				await DialogBox.ShowAsync("Can't uninstall mods that are presently doing other stuff (PLACEHOLDER) (NOT LOCALIZED)");
			}
		}


		async Task ChangeModSettings()
		{
			bool error = true;
			if (
					(_selectedMods != null) &&
					(_selectedMods.Count() == 1)
				)
			{
				if (
					(_selectedMods.First() is ManagedMod mmod) &&
					(!mmod.IsProgressing) &&
					(mmod.HasConfigurator)
				)
				{
					await mmod.ShowSettings();
					error = false;
				}
			}
			
			if (error)
			{
				CanChangeModSettings = false;
				await DialogBox.ShowAsync("Can't change settings for the specified mod or lack thereof (PLACEHOLDER) (NOT LOCALIZED)");
			}
		}

		bool _minimizeOnGameStart = false;
	}
}
