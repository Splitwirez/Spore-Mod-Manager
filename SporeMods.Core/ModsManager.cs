using SporeMods.Core.Mods;
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
using TTimer = System.Timers.Timer;

namespace SporeMods.Core
{
    public class ModsManager : INotifyPropertyChanged
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

        public static void AddMod(IInstalledMod mod)
        {
            SyncContext.Send(state => InstalledMods.Add(mod), null);
        }

        public static void InsertMod(int index, IInstalledMod mod)
        {
            SyncContext.Send(state => InstalledMods.Insert(index, mod), null);
        }

        public static void RemoveMod(IInstalledMod mod)
        {
            SyncContext.Send(state => InstalledMods.Remove(mod), null);
        }

        /// <summary>
        /// The mods that are currently installed for this user.
        /// </summary>
        // We don't use NotifyPropertyChanged here because the list instance itself should never change, only its contents
        public static ObservableCollection<IInstalledMod> InstalledMods { get; } = new ObservableCollection<IInstalledMod>();

        ObservableCollection<string> _commandLineStates = new ObservableCollection<string>()
        {
            "CellEditor",
            "CellToCreatureEditor",
            "CreatureEditor",
            "BuildingEditor",
            "VehicleEditor",
            "VehicleLandEditor",
            "VehicleAirEditor",
            "VehicleWaterEditor",
            "UFOEditor"
        };
        public static ObservableCollection<string> CommandLineStates
        {
            get => Instance._commandLineStates;
            set
            {
                Instance._commandLineStates = value;
                Instance.NotifyPropertyChanged(nameof(CommandLineStates));
            }
        }

        /// <summary>
        /// Retrieves an existing ManagedMod matching the name provided if one exists. If not, returns null.
        /// </summary>
        public static ManagedMod GetManagedMod(string name)
        {
            name = name.ToLowerInvariant();
            foreach (ManagedMod m in InstalledMods.Where(x => x is ManagedMod))
            {
                if (m is ManagedMod && m.RealName.ToLowerInvariant() == name)
                {
                    return m;
                }
            }
            return null;
        }

        private void PopulateModConfigurations()
        {
            /*if (Settings.IsFirstRun)
                await GameInfo.VerifyGamePaths();
            Settings.IsFirstRun = false;*/

            Debug.WriteLine("GETTING MODS");

            // Mod files in lowercase, to know which files don't belong to recognised mods
            List<string> allModFileNames = new List<string>();
            foreach (string s in Directory.EnumerateDirectories(Settings.ModConfigsPath))
            {
                var mod = new ManagedMod(Path.GetFileName(s), true);
                InstalledMods.Add(mod);
                allModFileNames.AddRange(mod.GetModFileNames().Select(x => x.ToLowerInvariant()));
                Debug.WriteLine("MOD: " + s);
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
            NotifyPropertyChanged(nameof(InstalledMods));
        }

        public static void RemoveMatchingManuallyInstalledFile(string fileName, ComponentGameDir targetLocation)
        {
            SyncContext.Send(state =>
            {
                foreach (ManualInstalledFile file in ModsManager.InstalledMods.Where(
                    x => x is ManualInstalledFile && ((ManualInstalledFile)x).Location == targetLocation))
                {
                    if (file.RealName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        ModsManager.InstalledMods.Remove(file);
                        break;
                    }
                }
            }, null);
        }

        public static string GetModsListForClipboard()
        {
            string modsText = string.Empty;
            foreach (IInstalledMod mod in InstalledMods)
            {
                string modText = mod.DisplayName + " (UNIQUE: " + mod.Unique + ", DIR: " + mod.RealName + ")";
                if (mod.ModVersion != ModIdentity.UNKNOWN_MOD_VERSION)
                    modText += ", version " + mod.ModVersion;
                if (mod is ManualInstalledFile)
                    modText += ", INSTALLED MANUALLY";
                modsText += modText + "\r\n\r\n";
            }
            //modsText = modsText.TrimEnd('\n');
            return modsText;
        }

        bool _updatingModsOrder = false;
        private void OrderInstalledMods()
        {
            if (!_updatingModsOrder)
            {
                _updatingModsOrder = true;
                InstalledMods.OrderBy(x => x.DisplayName);
                _updatingModsOrder = false;
            }
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

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task<bool> ShowModConfigurator(ManagedMod mod)
        {
            MessageDisplay.DebugShowMessageBox("ShowModConfigurator\nHasConfigurator: " + mod.HasConfigurator);

            if (mod.HasConfigurator)
                return await ModConfiguratorShown(mod);
            else
                return false;
        }

        public event Func<ManagedMod, Task<bool>> ModConfiguratorShown;

        
        bool _anyTasksRunning = false;
        public bool AnyTasksRunning
        {
            get => _anyTasksRunning;
            set
            {
                _anyTasksRunning = value;
                NotifyPropertyChanged(nameof(AnyTasksRunning));
            }
        }


        double _overallProgress = 0.0;
        public double OverallProgress
        {
            get => _overallProgress;
            set
            {
                _overallProgress = value;
                NotifyPropertyChanged(nameof(OverallProgress));
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
                    NotifyPropertyChanged(nameof(OverallProgressTotal));
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


        // This must come last so it can initialize correctly
        public static ModsManager Instance = new ModsManager();
        private ModsManager()
        {

            SyncContext = SynchronizationContext.Current;


            PopulateModConfigurations();

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
                    }*/
                    if (e.IsNowProgressing)
                    {
                        AnyTasksRunning = true;
                        OverallProgressTotal += 100;
                    }
                    else
                    {
                        AnyTasksRunning = InstalledMods.OfType<ManagedMod>().Any(x => x.IsProgressing);
                        if (!AnyTasksRunning)
                        {
                            OverallProgress = 0;
                            OverallProgressTotal = 0;
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
                    }*/
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
        }
    }
}
