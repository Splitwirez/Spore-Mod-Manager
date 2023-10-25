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
using System.Windows.Automation;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Documents;
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


		bool _isSporeRunning = false;
		public bool IsSporeRunning
		{
			get => _isSporeRunning;
			protected set
			{
				_isSporeRunning = value;
				NotifyPropertyChanged();
				IsSporeRunningChanged?.Invoke(this, new EventArgs());
			}
		}
		public event EventHandler<EventArgs> IsSporeRunningChanged;

		bool _allowExitSMM = true;
		public bool AllowExitSMM
		{
			get => _allowExitSMM;
			protected set
            {
				_allowExitSMM = value;
				NotifyPropertyChanged();
			}
		}

		public async Task InstallModsCommand()
			=> await InstallMods();

		public async Task UninstallModsCommand()
			=> await UninstallMods();

		public async Task ChangeModSettingsCommand()
			=> await ChangeModSettings();

		public async Task LaunchSporeCommand()
			=> await LaunchSpore();

		public async Task LaunchSpore()
		{
			if (!CanLaunchSpore)
				return;

			MinimizeWindowRequested?.Invoke(this, new EventArgs());
			await UACPartnerCommands.LaunchSporeAsync();
			//TODO: Line below this comment never occurs?
			RestoreWindowRequested?.Invoke(this, new EventArgs());
		}

		public event EventHandler MinimizeWindowRequested;
		public event EventHandler RestoreWindowRequested;

		
		
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
		//readonly Dictionary<int, Process> _sporeProcesses = new Dictionary<int, Process>();
		ObservableCollection<Process> _sporeProcesses = new ObservableCollection<Process>();
		/*uint _sporeProcessCount = 0;
		uint SporeProcessCount
        {
			get => _sporeProcessCount;
			set
            {
				_sporeProcessCount = value;
				SmmApp.Current.Dispatcher.Invoke(() =>
				{
					IsSporeRunning = _sporeProcessCount > 0;
				});
			}
		}*/

		public MainViewModel()
			: base()
		{
			LanguageManager.LanguageChanged += (s, e) => RefreshTitle();
			RefreshTitle();

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
				AllowExitSMM = true;
			};

			
			_sporeProcesses.CollectionChanged += (s, e) =>
			{
				IsSporeRunning = _sporeProcesses.Count > 0;
			};

			try
			{
				Automation.AddAutomationEventHandler(
					WindowPattern.WindowOpenedEvent,
					AutomationElement.RootElement,
					TreeScope.Subtree,
					Automation_WindowOpened);
			}
			catch (TypeInitializationException typeInitExc)
			{
				if (typeInitExc.InnerException is SEHException sehExc)
					CommonUI.MessageDisplay.ShowException(sehExc);
				else
					CommonUI.MessageDisplay.ShowFatalException(typeInitExc);
			}
		}

		void Automation_WindowOpened(object sender, AutomationEventArgs e)
        {
			/*SmmApp.Current.Dispatcher.Invoke(() =>
			{*/
				if (!(sender is AutomationElement autoEl))
					return;
				var winHandle = new IntPtr(autoEl.Current.NativeWindowHandle);
				NativeMethods.GetWindowThreadProcessId(winHandle, out var pidRaw);
				int pid = (int)pidRaw;

				var sporeProcess = Process.GetProcessById(pid);
				
				if (!GameInfo.IsSporeProcess(sporeProcess))
					return;
				if (_sporeProcesses.Contains(sporeProcess))
					return;

				sporeProcess.EnableRaisingEvents = true;
				if (sporeProcess.HasExited)
					return;


				_sporeProcesses.Add(sporeProcess);
				sporeProcess.Exited += SporeProcess_Exited;
			//});
		}

		void SporeProcess_Exited(object sender, EventArgs e)
		{
			_sporeProcesses.Remove((Process)sender);

			((Process)sender).Exited -= SporeProcess_Exited;
		}

		void RefreshCanDoesThings(IEnumerable<ISporeMod> mods)
		{
			CanUninstallMods = false;
			CanChangeModSettings = false;

			int count = mods.Count();

			if (count > 0)
			{
				if (ModsManager.Instance.Jobs.HasRunningTasks)
					return;

				CanUninstallMods = true;

				if (count == 1)
					CanChangeModSettings = mods.First().HasSettings(out _);
			}
		}

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
			using (TemporarilyPreventExitSMM())
			{
				var files = await Modal.Show(new RequestFilesViewModel(FileRequestPurpose.InstallMods, true));
				if (files != null)
					await ModsManager.Instance.InstallModsAsync(files.ToArray());
			}
		}
		IDisposable TemporarilyPreventExitSMM()
		{
			AllowExitSMM = false;
			InlineDisposable disp = new InlineDisposable(() =>
			{
				AllowExitSMM = (!ModsManager.Instance.AnyTasksRunning);
			});
			return disp;
		}

		
		List<ISporeMod> _selectedMods = new List<ISporeMod>();
		async Task UninstallMods()
		{
			using (TemporarilyPreventExitSMM())
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
				await ModsManager.Instance.UninstallModsAsync(modsToUninstall);
			}
#endif
		}


		async Task ChangeModSettings()
		{
			ISporeMod mod = _selectedMods?.FirstOrDefault();
			if (mod.HasSettings(out IConfigurableMod cMod))
			{
				using (TemporarilyPreventExitSMM())
				{
					await ModsManager.Instance.ChangeSettingsForModAsync(cMod);
					return;
				}
			}

			CanChangeModSettings = false;
			await DialogBox.ShowAsync("Can't change settings for the specified mod or lack thereof (PLACEHOLDER) (NOT LOCALIZED)");
		}
	}
}
