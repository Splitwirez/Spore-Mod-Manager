using SporeMods.Core.InstalledMods;
using SporeMods.Core.ModIdentity;
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

namespace SporeMods.Core
{
    public class ManagedMods : INotifyPropertyChanged
    {
        public static SynchronizationContext SyncContext = null;

        public static ManagedMods Instance = new ManagedMods();
        private ManagedMods()
        {

            SyncContext = SynchronizationContext.Current;

            /*FileSystemWatcher watcher = new FileSystemWatcher(Settings.ModConfigsPath)
            {
                IncludeSubdirectories = false
            };

            watcher.Created += Watcher_Created;

            watcher.EnableRaisingEvents = true;*/
            PopulateModConfigurations();
        }

        public void AddMod(IInstalledMod mod)
        {
            SyncContext.Send(state => ModConfigurations.Add(mod/*new InstalledMod(Path.GetFileNameWithoutExtension(e.FullPath)*/), null);
        }

        ObservableCollection<IInstalledMod> _modConfigurations = new ObservableCollection<IInstalledMod>();
        public ObservableCollection<IInstalledMod> ModConfigurations
        {
            get => _modConfigurations;
            set
            {
                _modConfigurations = value;
                NotifyPropertyChanged(nameof(ModConfigurations));
            }
        }

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
        public ObservableCollection<string> CommandLineStates
        {
            get => _commandLineStates;
            set
            {
                _commandLineStates = value;
                NotifyPropertyChanged(nameof(CommandLineStates));
            }
        }

        /// <summary>
        /// Asynchronously retrieves an existing InstalledMod matching the name provided if one exists. If not, waits until it does exist.
        /// </summary>
        public async Task<InstalledMod> GetModConfigurationAsync(string name)
        {
            MessageDisplay.DebugShowMessageBox(name);
            Task<InstalledMod> task = new Task<InstalledMod>(() =>
            {
                InstalledMod mod = GetModConfiguration(name);
                MessageDisplay.DebugShowMessageBox(mod.RealName + ", " + mod.Unique, mod.DisplayName);
                return mod;
            });

            task.Start();
            return await task;
        }

        /// <summary>
        /// Retrieves an existing InstalledMod matching the name provided if one exists. If not, waits until it does exist.
        /// </summary>
        public InstalledMod GetModConfiguration(string name)
        {
            InstalledMod mod = null;
            while (mod == null)
            {
                IInstalledMod[] configs = Instance.ModConfigurations.ToArray();
                foreach (InstalledMod m in configs.Where(x => x is InstalledMod))
                {
                    if ((m != null) && (m.RealName.ToLowerInvariant() == name.ToLowerInvariant()))
                    {
                        mod = m;
                        break;
                    }
                }
            }
            return mod;
        }

        private async Task PopulateModConfigurations()
        {
            /*if (Settings.IsFirstRun)
                await GameInfo.VerifyGamePaths();
            Settings.IsFirstRun = false;*/

            _modConfigurations = new ObservableCollection<IInstalledMod>();

            Debug.WriteLine("GETTING MODS");

            List<string> allModFileNames = new List<string>();
            foreach (string s in Directory.EnumerateDirectories(Settings.ModConfigsPath))
            {
                var mod = new InstalledMod(Path.GetFileName(s));
                _modConfigurations.Add(mod);
                allModFileNames.AddRange(mod.GetModFileNames());
                Debug.WriteLine("MOD: " + s);
            }

            foreach (string s in Directory.EnumerateFiles(GameInfo.GalacticAdventuresData).Where(x => x.EndsWith(".package", StringComparison.OrdinalIgnoreCase)))
            {
                if (FileWrite.IsUnprotectedFile(s))
                {
                    string name = Path.GetFileName(s).ToLowerInvariant();
                    if (allModFileNames.Where(x => x.ToLowerInvariant() == name.ToLowerInvariant()).Count() == 0)
                        _modConfigurations.Add(new ManualInstalledFile(name, ComponentGameDir.galacticadventures, false));
                }
            }
            foreach (string s in Directory.EnumerateFiles(GameInfo.CoreSporeData).Where(x => x.EndsWith(".package", StringComparison.OrdinalIgnoreCase)))
            {
                if (FileWrite.IsUnprotectedFile(s))
                {
                    string name = Path.GetFileName(s).ToLowerInvariant();
                    if (allModFileNames.Where(x => x.ToLowerInvariant() == name.ToLowerInvariant()).Count() == 0)
                        _modConfigurations.Add(new ManualInstalledFile(name, ComponentGameDir.spore, false));
                }
            }
            //List<IInstalledMod> mods = _modConfigurations.ToList().Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName));
            OrderModConfigurations();
            ModConfigurations.CollectionChanged += (sneder, args) =>
            {
                if ((args.NewItems != null) && (args.NewItems.Count > 0))
                    OrderModConfigurations();
            };
            NotifyPropertyChanged(nameof(ModConfigurations));
        }

        public static void RemoveMatchingManuallyInstalledFile(string fileName, ComponentGameDir targetLocation)
        {
            ManagedMods.SyncContext.Send(state =>
            {
                foreach (ManualInstalledFile file in ManagedMods.Instance.ModConfigurations.Where(x => x is ManualInstalledFile).Where(x => ((ManualInstalledFile)x).Location == targetLocation))
                {
                    if (file.RealName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        ManagedMods.Instance.ModConfigurations.Remove(file);
                        break;
                    }
                }
            }, null);
        }

        public string GetModsListForClipboard()
        {
            string modsText = string.Empty;
            foreach (IInstalledMod mod in ModConfigurations)
            {
                string modText = mod.DisplayName + " (UNIQUE: " + mod.Unique + ", DIR: " + mod.RealName + ")";
                if ((mod is InstalledMod imd) && imd.ModHasVersion)
                    modText += ", version " + imd.ModVersion;
                else if (mod is ManualInstalledFile)
                    modText += ", INSTALLED MANUALLY";
                modsText += modText + "\n\n";
            }
            modsText = modsText.TrimEnd('\n');
            return modsText;
        }

        bool _updatingModsOrder = false;
        private void OrderModConfigurations()
        {
            if (!_updatingModsOrder)
            {
                _updatingModsOrder = true;
                ModConfigurations = new ObservableCollection<IInstalledMod>(ModConfigurations.OrderBy(x => x.DisplayName));
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

        public async Task<bool> ShowModConfigurator(InstalledMod mod)
        {
            MessageDisplay.DebugShowMessageBox("ShowModConfigurator\nHasConfigurator: " + mod.HasConfigurator + "\nConfigurator != null: " + (mod.Configurator != null));

            if (mod.HasConfigurator)
                return await ModConfiguratorShown(mod.Configurator);
            else
                return false;
        }

        public event Func<ModConfiguration, Task<bool>> ModConfiguratorShown;
    }
}
