using Avalonia;
using Avalonia.Controls;
using SporeMods.CommonUI;
using SporeMods.Core;
using SporeMods.Core.Mods;
using SporeMods.NotifyOnChange;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SporeMods.Manager.ViewModels
{
    public class ModManagerViewModel : NOCObject
    {
        public ModManagerViewModel()
            : base()
        {
            _canUninstallMods = AddProperty<bool>(nameof(CanUninstallMods), false);
            _canChangeModSettings  = AddProperty(nameof(CanChangeModSettings), false);

            
            

            _hasModal = AddProperty<bool>(nameof(HasModal), false);
            _currentModal = AddProperty(new NOCRespondProperty<object>(nameof(CurrentModal), null)
            {
                ValueChangeResponse = (obj, oldVal, newVal) =>
                {
                    if ((oldVal == null) && (newVal != null))
                        HasModal = true;
                    else if (newVal == null)
                        HasModal = false;
                }
            });

            MessageDisplay.ModalShown += MessageDisplay_ModalShown;


            SporeMods.Manager.Views.InstalledModsPageView.SelectedModsChanged += (o, _) =>
            {
                if (o is IEnumerable<IInstalledMod> sel)
                {
                    _selectedMods = sel;
                    RefreshSelectedMods(sel);
                }
            };


			EnsureAllPaths();
        }

		
        static readonly string[] GAME_FOLDER_LABELS =
        {
            "Galactic Adventures \'Data\'/\'DataEP1\'",
            "Galactic Adventures \'SporebinEP1\'",
            "Core Spore \'Data\'"
        };


        async void EnsureAllPaths()
        {
            if (Settings.Instance.ForcedGalacticAdventuresDataPath.IsNullOrEmptyOrWhiteSpace())
            {
                Settings.Instance.ForcedGalacticAdventuresDataPath = await EnsurePath(0);
            }


            if (Settings.Instance.ForcedGalacticAdventuresSporebinEP1Path.IsNullOrEmptyOrWhiteSpace())
            {
                Settings.Instance.ForcedGalacticAdventuresSporebinEP1Path = await EnsurePath(1);
            }



            if (Settings.Instance.ForcedCoreSporeDataPath.IsNullOrEmptyOrWhiteSpace())
            {
                Settings.Instance.ForcedCoreSporeDataPath = await EnsurePath(2);
            }
        }

        const string PATH_BROWSE_TITLE_PLACEHOLDER = "SPECIFY PATH TO {0} FOLDER (PLACEHOLDER) (NOT LOCALIZED)";
        async Task EnsureGamePath(int parameter)
        {
            /*string prevPath;
            if (parameter == 0)
                prevPath = Settings.Instance.ForcedCoreSporeDataPath;
            else if (parameter == 1)
                prevPath = Settings.Instance.ForcedGalacticAdventuresDataPath;
            else if (parameter == 2)
                prevPath = Settings.Instance.ForcedGalacticAdventuresSporebinEP1Path;
            else*/
            if ((parameter < 0) || (parameter >= GAME_FOLDER_LABELS.Length))
                throw new Exception("Invalid game path ID, lolwut (NOT LOCALIZED)");
            

            //string newPath = await EnsurePath(GAME_FOLDER_LABELS[parameter], false);
            //if (newp)

            string path = await AskForPath(new OpenFolderDialog()
			{
				Title = string.Format(PATH_BROWSE_TITLE_PLACEHOLDER, GAME_FOLDER_LABELS[parameter])
			});
            if (IsPathValid(path))
            {
                if (parameter == 0)
                    Settings.Instance.ForcedGalacticAdventuresDataPath = path;
                else if (parameter == 1)
                    Settings.Instance.ForcedGalacticAdventuresSporebinEP1Path = path;
                else if (parameter == 2)
                    Settings.Instance.ForcedCoreSporeDataPath = path;
            }
            else
            {
                //TODO: tell the user, not just the terminal
                Console.WriteLine("PATH DIALOG FAILED OR SOMETHING");
            }
        }


        async Task<string> EnsurePath(int gameFolderIndex)
        {
            OpenFolderDialog dialog = new OpenFolderDialog()
			{
				Title = "AUTODETECTION NYI, " + string.Format(PATH_BROWSE_TITLE_PLACEHOLDER, GAME_FOLDER_LABELS[gameFolderIndex])
			};


            
            string path = null;
            while (true)
            {
                path = await AskForPath(dialog);
                if (IsPathValid(path))
                    break;
            }
            return path;
        }

        async Task<string> AskForPath(OpenFolderDialog dialog)
        {
            return await dialog.ShowAsync(App.MainWindow);
        }
        
        
        bool IsPathValid(string path)
            => (!path.IsNullOrEmptyOrWhiteSpace()) && Directory.Exists(path);


        Func<object, IControl> _getView = (viewModel) =>
        {
            Console.WriteLine("VM TYPE: " + viewModel.GetType().FullName);
            if (viewModel is SporeMods.Core.ModConfiguratorModal mod)
            {
                return new Views.Configurators.ModConfiguratorV1_0_0_0View();
            }
            return null;
        };

        public Func<object, IControl> GetView
        {
            get => _getView;
        }



        IEnumerable<IInstalledMod> _selectedMods = null;


        void RefreshSelectedMods(IEnumerable<IInstalledMod> newVal)
        {
            int selCount = newVal.Count();

            if (selCount > 1)
            {
                CanChangeModSettings = false;
                
                //IInstalledMod[] selectedMods = new IInstalledMod[newVal];
                //newVal.CopyTo(selectedMods, 0);
                List<IInstalledMod> selectedMods = newVal.ToList();
                
                var mMods = newVal.OfType<ManagedMod>();
                CanUninstallMods = (mMods.Count() > 0) ? (mMods.All(x => !(x.IsProgressing))) : true;
            }
            else if (selCount == 1)
            {
                CanUninstallMods = true;
                if ((newVal.First() is ManagedMod item) && (!item.IsProgressing))
                {
                    CanChangeModSettings = item.HasConfigurator;
                }
                else
                {
                    CanUninstallMods = true;
                    CanChangeModSettings = false;
                }
            }
            else
            {
                CanUninstallMods = false;
                CanChangeModSettings = false;
            }
        }

        bool IsAnythingHappening(IEnumerable<IInstalledMod> modsInQuestion) => modsInQuestion.Any(x =>
		{
			if (x == null)
				return false;
			if (x is ManagedMod mod)
				return mod.IsProgressing;
			else
				return false;
		});

        async void MessageDisplay_ModalShown(object sender, ModalShownEventArgs args)
        {
            CurrentModal = args.ViewModel;
            await args.Task;
            CurrentModal = null;
        }

        
        NOCProperty<object> _currentModal;
        object CurrentModal
        {
            get => _currentModal.Value;
            set => _currentModal.Value = value;
        }

        NOCProperty<bool> _hasModal;
        bool HasModal
        {
            get => _hasModal.Value;
            set => _hasModal.Value = value;
        }

        public async void InstallModsCommand(Window parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                AllowMultiple = true,
                Title = "CHOOSE MODS (PLACEHOLDER) (NOT LOCALIZED)"
            };

            string[] paths = await dialog.ShowAsync(parameter);

            if ((paths != null) && (paths.Length > 0))
            {
                ModInstallation.InstallModsAsync(paths);
            }
        }

        public void UninstallSelectedModsCommand(object parameter = null)
        {
            if ((CanUninstallMods) && (_selectedMods != null))
                ModInstallation.UninstallModsAsync(_selectedMods.ToArray());
        }

        public async void LaunchGameCommand(object parameter = null)
        {
            /*if (SmmInfo.IsWindowsLike && Permissions.IsAtleastWindowsVista() && Permissions.IsAdministrator() && ((App.DragServantProcess != null) && (!App.DragServantProcess.HasExited)))
				ServantCommands.RunLauncher();
			else*/ // if (!Permissions.IsAdministrator())

            //"/home/splitwirez/wine/spore/smm/debugging"

            #if LINUX_BUILD
            
            string ep1 = Settings.Instance.ForcedGalacticAdventuresSporebinEP1Path;
            if (ep1.Contains("/drive_"))
                ep1 = ep1.Substring(0, ep1.IndexOf("/drive_"));
            else
            {
                OpenFolderDialog dialog = new OpenFolderDialog()
                {
                    Title = "Where is your Spore WINE prefix? (PLACEHOLDER) (NOT LOCALIZED)"
                };
            
                ep1 = await dialog.ShowAsync(App.MainWindow);
            }
            if (!ep1.IsNullOrEmptyOrWhiteSpace())
            {
                CrossProcess.StartLauncher(prefixPath: ep1); //"/home/splitwirez/wine/spore/smm/debugging");
            }
            
            #else
            
            CrossProcess.StartLauncher();
            
            #endif

			//_minimizeOnGameStart = true;
        }

        NOCProperty<bool> _canUninstallMods;
        bool CanUninstallMods
        {
            get => _canUninstallMods.Value;
            set => _canUninstallMods.Value = value;
        }

        NOCProperty<bool> _canChangeModSettings;
        bool CanChangeModSettings
        {
            get => _canChangeModSettings.Value;
            set => _canChangeModSettings.Value = value;
        }

        /*NOCRespondProperty<bool> _isSearching;
        public bool IsSearching
        {
            get => _isSearching.Value;
            set => _isSearching.Value = value;
        }*/
    }
}