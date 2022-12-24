﻿using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
//using SporeMods.Core.ModTransactions;
using TTimer = System.Timers.Timer;

namespace SporeMods.Core
{
	public partial class ModsManager : NotifyPropertyChangedBase
	{
		private static SynchronizationContext SyncContext = null;

		public static void RunOnMainSyncContext(SendOrPostCallback d)
		{
			SyncContext.Send(d, null);
		}

		private static readonly string[] MOD_EXTENSIONS = { ".package", ".dll" };
		/// <summary>
		/// Returns whether or not a given path or filename corresponds to a mod file which is to be installed.
		/// </summary>
		public static bool IsModFile(string path)
		{
			return MOD_EXTENSIONS.Contains(Path.GetExtension(path).ToLowerInvariant());
		}

		/*public static void AddMod(ISporeMod mod)
		{
			SyncContext.Send(state => InstalledMods.Add(mod), null);
		}

		public static void InsertMod(int index, ISporeMod mod)
		{
			SyncContext.Send(state => InstalledMods.Insert(index, mod), null);
		}

		public static void RemoveMod(ISporeMod mod)
		{
			SyncContext.Send(state => InstalledMods.Remove(mod), null);
		}*/

		/// <summary>
		/// The mods that are currently installed for this user.
		/// </summary>
		// We don't use NotifyPropertyChanged here because the list instance itself should never change, only its contents
		public static ThreadSafeObservableCollection<ISporeMod> InstalledMods { get; } = new ThreadSafeObservableCollection<ISporeMod>();

		ObservableCollection<CommandLineState> _commandLineStates = new ObservableCollection<CommandLineState>()
		{
			new CommandLineState("CellEditor"),
			new CommandLineState("CellToCreatureEditor"),
			new CommandLineState("CreatureEditor"),
			new CommandLineState("VehicleLandEditor"),
			new CommandLineState("VehicleAirEditor"),
			new CommandLineState("VehicleWaterEditor"),
			new CommandLineState("UFOEditor"),
			new CommandLineState("BuildingEditor"),
			new CommandLineState("VehicleEditor"),
		};
		
		public ObservableCollection<CommandLineState> CommandLineStates
		{
			get => _commandLineStates;
			set
			{
				_commandLineStates = value;
				NotifyPropertyChanged();
			}
		}

		string _currentCommandLineStateIdentifier = string.Empty;
		public string CurrentCommandLineSStateIdentifier
		{
			get => _currentCommandLineStateIdentifier;
			set
			{
				_currentCommandLineStateIdentifier = value;
				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Retrieves an existing ManagedMod matching the unique provided if one exists. If not, returns null.
		/// </summary>
#if MOD_IMPL_RESTORE_LATER
		public static ManagedMod GetManagedMod(string unique)
		{
			unique = unique.ToLowerInvariant();
			foreach (ManagedMod m in InstalledMods.Where(x => x is ManagedMod))
			{
				if (m is ManagedMod && m.Unique.ToLowerInvariant() == unique)
				{
					return m;
				}
			}
			return null;
		}
#endif

		/*static List<Func<string, XDocument, ISporeMod>> _MOD_FROM_RECORD_DIR = new List<Func<string, XDocument, ISporeMod>>()
        {
			MI1_0_X_XMod.FromRecordDir,
            PreIdentityMod.FromRecordDir
        };*/
		static IReadOnlyList<Func<ISporeMod>> _MOD_TYPES_FROM_RECORD_DIR = new List<Func<ISporeMod>>()
		{
			() => new MI1_0_1_XMod(),
			() => new MI1_0_0_0Mod(),
			() => new PreIdentityMod(),
		}.AsReadOnly();

		private void PopulateModConfigurations()
		{
#if !MOD_IMPL_RESTORE_LATER
			foreach (string dirPath in Directory.EnumerateDirectories(Settings.ModConfigsPath))
			{
				XDocument identity = null;
				try
				{
					string identityPath  = Path.Combine(dirPath, SporeMods.Core.Mods.ModUtils.ID_XML_FILE_NAME);
					string text = File.ReadAllText(identityPath);
					identity = XDocument.Parse(text);
				}
				catch (Exception ex)
				{
					//TODO: ?????
				}
				if (identity != null)
				{
					ISporeMod mod = null;
					//var mod = new ManagedMod(Path.GetFileName(s), true);
					/*foreach (var fromRecordDir in _MOD_FROM_RECORD_DIR)
					{
						mod = fromRecordDir(dirPath, identity);
						if (mod != null)
							break;
					}*/
					foreach (var createMod in _MOD_TYPES_FROM_RECORD_DIR)
                    {
						mod = createMod();
						if (!mod.TryGetFromRecordDir(dirPath, identity, out Exception exc))
							mod = null;
						else
							break;
                    }

					if (mod != null)
						InstalledMods.Add(mod);
					
					//allModFileNames.AddRange(mod.GetModFileNames().Select(x => x.ToLowerInvariant()));
					//Cmd.WriteLine("MOD: " + s);	
				}
			}
#else
			/*if (Settings.IsFirstRun)
				await GameInfo.VerifyGamePaths();
			Settings.IsFirstRun = false;*/

			Cmd.WriteLine("GETTING MODS");

			// Mod files in lowercase, to know which files don't belong to recognised mods
			List<string> allModFileNames = new List<string>();
			foreach (string s in Directory.EnumerateDirectories(Settings.ModConfigsPath))
			{
				var mod = new ManagedMod(Path.GetFileName(s), true);
				InstalledMods.Add(mod);
				allModFileNames.AddRange(mod.GetModFileNames().Select(x => x.ToLowerInvariant()));
				Cmd.WriteLine("MOD: " + s);
			}

			try
			{
				foreach (string s in Directory.EnumerateFiles(GameInfo.GalacticAdventuresData).Where(x => x.EndsWith(".package", StringComparison.OrdinalIgnoreCase)))
				{
					if (FileWrite.IsUnprotectedFile(s))
					{
						string name = Path.GetFileName(s);
						if (!allModFileNames.Contains(name.ToLowerInvariant()))
							InstalledMods.Add(new ManualInstalledFile(name, ComponentGameDir.GalacticAdventures, false));
					}
				}
			}
			catch (ArgumentNullException) { }

			try
			{
				foreach (string s in Directory.EnumerateFiles(GameInfo.CoreSporeData).Where(x => x.EndsWith(".package", StringComparison.OrdinalIgnoreCase)))
				{
					if (FileWrite.IsUnprotectedFile(s))
					{
						string name = Path.GetFileName(s);
						if (!allModFileNames.Contains(name.ToLowerInvariant()))
							InstalledMods.Add(new ManualInstalledFile(name, ComponentGameDir.Spore, false));
					}
				}
			}
			catch (ArgumentNullException) { }

			OrderInstalledMods();
			InstalledMods.CollectionChanged += (sneder, args) =>
			{
				if ((args.NewItems != null) && (args.NewItems.Count > 0))
					OrderInstalledMods();
			};
			NotifyPropertyChanged();
#endif
		}

		public static 
#if MOD_IMPL_RESTORE_LATER
		ManualInstalledFile 

#else
		ISporeMod
#endif
		GetManuallyInstalledFile(string fileName, ComponentGameDir targetLocation)
        {
#if MOD_IMPL_RESTORE_LATER
			foreach (var mod in InstalledMods)
            {
				if (mod is ManualInstalledFile)
                {
					var file = (ManualInstalledFile)mod;
					if (file.Location == targetLocation && file.RealName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
						return file;
                    }
                }
            }
#endif
			return null;
        }

		public static string GetModsListForClipboard()
		{
			string modsText = string.Empty;
#if MOD_IMPL_RESTORE_LATER
			foreach (IModEntry mod in InstalledMods)
			{
				string modText = mod.DisplayName + " (UNIQUE: " + mod.Unique + ", DIR: " + mod.RealName + ")";
				if (mod.ModVersion != ModIdentity.UNKNOWN_MOD_VERSION)
					modText += ", version " + mod.ModVersion;
				if (mod is ManualInstalledFile)
					modText += ", INSTALLED MANUALLY";
				modsText += modText + "\r\n\r\n";
			}
			//modsText = modsText.TrimEnd('\n');
#endif
			return modsText;
		}

		bool _updatingModsOrder = false;
		private void OrderInstalledMods()
		{
#if MOD_IMPL_RESTORE_LATER
			if (!_updatingModsOrder)
			{
				_updatingModsOrder = true;
				InstalledMods.OrderBy(x => x.DisplayName);
				_updatingModsOrder = false;
			}
#endif
		}

		/*private void Watcher_Created(object sender, FileSystemEventArgs e)
		{

			while (!File.Exists(Path.Combine(e.FullPath, "ModInfo.xml"))) { }

			_context.Send(state =>
			{
				ModConfigurations.Add(new InstalledMod(Path.GetFileNameWithoutExtension(e.FullPath))
				{
					IsProgressing = true
				});
			}, null);
		}*/

		public event PropertyChangedEventHandler PropertyChanged;

		/*public async Task<bool> ShowModConfigurator(ManagedMod mod)
		{
			MessageDisplay.DebugShowMessageBox("ShowModConfigurator\nHasConfigurator: " + mod.HasConfigurator);

			if (mod.HasConfigurator)
				return await ModConfiguratorShown(mod);
			else
				return false;
		}*/

		//public event Func<ManagedMod, Task<bool>> ModConfiguratorShown;


		bool _anyTasksRunning = false;
		public bool AnyTasksRunning
		{
			get => _anyTasksRunning;
			set
			{
				_anyTasksRunning = value;
				NotifyPropertyChanged();
			}
		}


		double _overallProgress = 0.0;
		public double OverallProgress
		{
			get => _overallProgress;
			set
			{
				_overallProgress = value;
				NotifyPropertyChanged();
			}
		}

		double _overallProgressTotal = 0.0;
		public double OverallProgressTotal
		{
			get => _overallProgressTotal;
			set
			{
				if (
						(value > _overallProgressTotal)
						|| (value == 0)
					)
				{
					_overallProgressTotal = value;
					NotifyPropertyChanged();
				}
			}
		}

		/*int _taskCount = 0;
		public int TaskCount
		{
			get => _taskCount;
			private set
			{
				_taskCount = value;
				NotifyPropertyChanged(nameof(TaskCount));
			}
		}
		
		public void AddToTaskCount(int add)
		{
			if (add > 0)
			{
				OverallProgressTotal += (add * 100.0);
				TaskCount += add;
			}
		}*/

		public static event EventHandler<ModTasksCompletedEventArgs> TasksCompleted;


		// This must come last so it can initialize correctly
		static ModsManager _instance = null;
		public static ModsManager Instance
		{
			get => _instance;
		}

		public static void Ensure()
		{
			if (_instance == null)
				_instance = new ModsManager();
		}


		private ModsManager()
		{

			SyncContext = SynchronizationContext.Current;


			PopulateModConfigurations();
			/*
			ManagedMod.AnyModIsProgressingChanged += (sneder, e) =>
			{
				SyncContext.Send(state =>
				{
					/*if (e.IsNowProgressing)
					{
						AnyTasksRunning = true;
					}
					else
					{
						TaskCount--;

						if (TaskCount == 0)
						{
							OverallProgress = 0;
							OverallProgressTotal = 0;
							AnyTasksRunning = false;
						}
					}* /
					if (e.IsNowProgressing)
					{
						AnyTasksRunning = true;
						OverallProgressTotal += 100;
						ModInstallation.InstallActivitiesCounter++;
					}
					else
					{
						AnyTasksRunning = InstalledMods.OfType<ManagedMod>().Any(x => x.IsProgressing);
						if (!AnyTasksRunning)
						{
							OverallProgress = 0;
							OverallProgressTotal = 0;
							ModInstallation.InstallActivitiesCounter = 0;
							TasksCompleted?.Invoke(this, new ModTasksCompletedEventArgs()
							{
								InstalledAnyMods = ModTransactionManager.IS_INSTALLING_MODS,
								UninstalledAnyMods = ModTransactionManager.IS_UNINSTALLING_MODS,
								ReconfiguredAnyMods = ModTransactionManager.IS_RECONFIGURING_MODS,
								Failures = ModTransactionManager.INSTALL_FAILURES
							});
							ModTransactionManager.IS_INSTALLING_MODS = false;
							ModTransactionManager.IS_UNINSTALLING_MODS = false;
							ModTransactionManager.IS_RECONFIGURING_MODS = false;
							ModTransactionManager.INSTALL_FAILURES.Clear();
						}
					}
					/*else if (!InstalledMods.OfType<ManagedMod>().Any(x => x.IsProgressing))
					{
						bool anyRunning = false;
						int time = 0;
						TTimer timer = new TTimer(10);
						timer.Elapsed += (s, a) =>
						{
							time++;
							if (anyRunning)
							{
								timer.Stop();
							}
							else
							{
								SyncContext.Send(state2 => anyRunning = InstalledMods.OfType<ManagedMod>().Any(x => x.IsProgressing), null);
								if (time >= 10)
								{
									SyncContext.Send(state2 =>
									{
										if (!InstalledMods.OfType<ManagedMod>().Any(x => x.IsProgressing))
										{
											AnyTasksRunning = false;
											OverallProgress = 0;
											OverallProgressTotal = 0;
										}
									}, null);
									timer.Stop();
								}
							}
						};
						timer.Start();
						//MessageDisplay.ShowMessageBox("!AnyTasksRunning");
					}* /
					//MessageDisplay.ShowMessageBox("Progress", OverallProgress + " --> " + OverallProgressTotal);
				}, null);
			};

			ManagedMod.AnyModProgressChanged += (sneder, e) =>
			{
				SyncContext.Send(state =>
				{
					if (e.Change > 0)
						OverallProgress += e.Change;
				}, null);
			};
			*/
		}
	}

	public class ModTasksCompletedEventArgs : EventArgs
	{
		public bool InstalledAnyMods { get; internal set; } = false;
		public bool UninstalledAnyMods { get; internal set; } = false;
		public bool ReconfiguredAnyMods { get; internal set; } = false;
		public Dictionary<string, Exception> Failures { get; internal set; } = new Dictionary<string, Exception>();
	}

	public class CommandLineState : NotifyPropertyChangedBase
    {
		string _displayName = string.Empty;
		public string DisplayNameKey
        {
			get => _displayName;
			set
            {
				_displayName = value;
				NotifyPropertyChanged();
			}
		}

		string _stateIdentifier = string.Empty;
		public string StateIdentifier
		{
			get => _stateIdentifier;
			set
			{
				_stateIdentifier = value;
				NotifyPropertyChanged();
			}
		}

		internal CommandLineState(string stateIdentifier)
        {
			StateIdentifier = stateIdentifier;
			DisplayNameKey = "Settings!GameEntry!StartupEditor!Editors!" + stateIdentifier;
		}

        public override string ToString()
        {
            return _stateIdentifier;
        }
    }
}
