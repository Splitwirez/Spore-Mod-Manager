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
using SporeMods.Core.Transactions;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
//using ModTaskStatus = SporeMods.Core.ModTransactions.TaskStatus;

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

		
		
		InstalledModsViewModel _installedModsVM = new InstalledModsViewModel();
		public InstalledModsViewModel InstalledModsVM
        {
			get => _installedModsVM;
        }

		HelpViewModel _helpVM = new HelpViewModel();
		public HelpViewModel HelpVM
		{
			get => _helpVM;
		}

		SettingsViewModel _settingsVM = new SettingsViewModel();
		public SettingsViewModel SettingsVM
		{
			get => _settingsVM;
		}

		ModalDisplayViewModel _modalDisplayVM = new ModalDisplayViewModel();
		public ModalDisplayViewModel ModalDisplayVM
		{
			get => _modalDisplayVM;
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
					if (Permissions.IsAtleastWindowsVista() && Permissions.IsAdministrator() && UACPartnerCommands.HasUACPartnership)
						UACPartnerCommands.RunLauncher();
					else// if (!Permissions.IsAdministrator())
						CrossProcess.StartLauncher();

					_minimizeOnGameStart = true;
				}
			});

			InstalledModsViewModel.SelectedModsChanged += (s, e) =>
			{
				if (s is List<ISporeMod> mods)
				{
					RefreshCanDoesThings(mods);
					_selectedMods = mods;
				}
			};

			//ManagedMod.AnyModIsProgressingChanged += (s, e) => CanLaunchSpore = !AreAnyProgressing(ModsManager.InstalledMods);
			
			ModsManager.Instance.AllJobsConcluded += async (conclusion) =>
			{
				await Modal.Show(conclusion);
				/*
				int succeeded = await Task<int>.Run(() => tasks.Count(x => x.ProgressSignifier.Status == ModTaskStatus.Succeeded));
				int skipped = await Task<int>.Run(() => tasks.Count(x => x.ProgressSignifier.Status == ModTaskStatus.Skipped));
				int failed = await Task<int>.Run(() => tasks.Count(x => x.ProgressSignifier.Status == ModTaskStatus.Failed));

				if (
						   (succeeded > 0)
						&& (skipped == 0)
						&& (failed == 0)
					)
				{
					await Modal.Show(new AllTasksSucceededNoticeViewModel());
				}
				else
				{
					string tempMsgText =
					await Task<string>.Run(() =>
					{
						string tempText = "All tasks have concluded (PLACEHOLDER) (NOT LOCALIZED)";


						bool hasRunningTasks = ModTransactionManager.Instance.HasRunningTasks;
						int howManyNotConcluded = tasks.Count(x => !x.ProgressSignifier.IsConcluded);
						bool anyNotConcluded = howManyNotConcluded > 0;
						if (hasRunningTasks || anyNotConcluded)
						{
							tempText = "All tasks have completed, but something weird happened: ";
							if (hasRunningTasks)
								tempText += "[ANY HASRUNNINGTASKS] ";
							if (anyNotConcluded)
								tempText += $"[{howManyNotConcluded} NOTCONCLUDED] ";

							tempText += "(PLACEHOLDER) (NOT LOCALIZED)\nIf you see this text, inform Splitwirez immediately, and try not to close the SMM in the meantime if possible.";
						}


						//if (succeeded > 0)
						tempText += $"\n\tSucceeded: {succeeded} (PLACEHOLDER) (NOT LOCALIZED)\n" +

						//if (skipped > 0)
						/*tempText += * /$"\n\tSkipped: {skipped} (PLACEHOLDER) (NOT LOCALIZED)\n" +

						//if (failed > 0)
						/*tempText += * /$"\n\tFailed: {failed} (PLACEHOLDER) (NOT LOCALIZED)\n";


						/*var failedTasks = tasks.Where(x => x.ProgressSignifier.Status == ModTaskStatus.Failed);
						if (failedTasks.Count() > 0)
						{* /
						foreach (var fail in tasks)
						{
							if (fail.Exception != null)
								tempText += fail.Exception.ToString();
						}
						//}
						return tempText;
					});


					await DialogBox.ShowAsync(tempMsgText, "Temporary task conclusion notification (PLACEHOLDER) (NOT LOCALIZED)");
				}*/
			};
		}

		void RefreshCanDoesThings(IEnumerable<ISporeMod> mods)
		{
			CanUninstallMods = false;
			CanChangeModSettings = false;
			
			int count = mods.Count();
			
			if (count >= 1)
			{
				CanUninstallMods = !AreAnyProgressing(mods);
#if MOD_IMPL_RESTORE_LATER
				CanChangeModSettings = (mods.Count() == 1) && (mods.First() is ManagedMod mmod) && (!mmod.HasProgressSignifier()) && mmod.HasConfigurator;
#endif
			}
		}

		bool AreAnyProgressing(IEnumerable<ISporeMod> mods)
#if MOD_IMPL_RESTORE_LATER
			=> mods.Any(x => (x is ManagedMod mmod) ? mmod.HasProgressSignifier() : false);
#else
			=> false;
#endif

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
				//.Replace("%DLLSBUILD%", Settings.CurrentDllsBuildString)
				//.Replace("%DOTNETRUNTIME%", Settings.TargetFramework)
#if SET_BUILD_CHANNEL || DEBUG
				.Replace("%BUILDCHANNEL%", Settings.BuildChannel)
#endif
				;
		}

		async Task InstallMods()
		{
			var files = await Modal.Show(new RequestFilesViewModel(FileRequestPurpose.InstallMods, true));
			if (files != null)
				ModsManager.Instance.InstallModsAsync(files.ToArray());
		}

		
		List<ISporeMod> _selectedMods = new List<ISporeMod>();
		async Task UninstallMods()
		{
#if MOD_IMPL_RESTORE_LATER
			bool allCanUninstall = _selectedMods.All(x => x.CanUninstall);
			Cmd.WriteLine($"UninstallMods() called\n\tcount: {_selectedMods.Count}\n\tall can uninstall: {allCanUninstall}");
			if (
					(_selectedMods.Count > 0) &&
					allCanUninstall
				)
			{
				Cmd.WriteLine("Uninstalling...");
				await ModTransactionManager.UninstallModsAsync(_selectedMods.ToArray());
			}
			else
			{
				CanUninstallMods = false;
				await DialogBox.ShowAsync("Can't uninstall mods that are presently doing other stuff (PLACEHOLDER) (NOT LOCALIZED)");
			}
#else
			var modsToUninstall = _selectedMods.ToArray();
			//await DialogBox.ShowAsync($"{modsToUninstall.Length}");
			ModsManager.Instance.UninstallModsAsync(modsToUninstall);
#endif
		}


		async Task ChangeModSettings()
		{
			ISporeMod mod = _selectedMods?.FirstOrDefault();
			bool error = true;
#if MOD_IMPL_RESTORE_LATER
			if (
					(mod != null) &&
					(mod is ManagedMod mmod) &&
					mmod.CanReconfigure)
			{
				mmod.ShowSettings();
				error = false;
			}
#endif
			
			if (error)
			{
				CanChangeModSettings = false;
				await DialogBox.ShowAsync("Can't change settings for the specified mod or lack thereof (PLACEHOLDER) (NOT LOCALIZED)");
			}
		}

		bool _minimizeOnGameStart = false;
	}
}
