﻿using Microsoft.Win32;
using SporeMods.Core;
using Mechanism.Wpf.Core.Windows;
using Mechanism.Wpf.Styles.Shale;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using MessageBox = System.Windows.MessageBox;
using static Mechanism.Wpf.Core.NativeMethods;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using SporeMods.Core.Mods;
using System.Windows.Controls.Primitives;
using static SporeMods.Core.Injection.SporeLauncher;
using SporeMods.Core.Injection;
using System.Runtime.Remoting.Messaging;
using SporeMods.Manager.Configurators;

namespace SporeMods.Manager
{
    /// <summary>
    /// Interaction logic for ManagerContent.xaml
    /// </summary>
    public partial class ManagerContent : UserControl
    {
        public bool HasCustomWindowDecorations
        {
            get => (bool)GetValue(HasCustomWindowDecorationsProperty);
            set => SetValue(HasCustomWindowDecorationsProperty, value);
        }

        public static readonly DependencyProperty HasCustomWindowDecorationsProperty =
        DependencyProperty.Register(nameof(HasCustomWindowDecorations), typeof(bool), typeof(ManagerContent), new PropertyMetadata(false));

        public ObservableCollection<CreditsItem> Credits
        {
            get => (ObservableCollection<CreditsItem>)GetValue(CreditsProperty);
            set => SetValue(CreditsProperty, value);
        }

        public static readonly DependencyProperty CreditsProperty =
        DependencyProperty.Register(nameof(Credits), typeof(ObservableCollection<CreditsItem>), typeof(ManagerContent), new PropertyMetadata(new ObservableCollection<CreditsItem>()
        {
            new CreditsItem("rob55rod", "Designed and built the Spore Mod Manager."),
            new CreditsItem("emd4600", "Started the Spore ModAPI Project, created the original Spore ModAPI Launcher Kit from which the Spore Mod Manager was derived, and helped build the Spore Mod Manager to be as robust as possible.", @"https://github.com/emd4600/"),
            new CreditsItem("reflectronic", "Provided significant guidance and assistance with internal structure and asynchronous behaviour.", @"https://github.com/reflectronic"),
            new CreditsItem("Jaime Olivares", "Created the Zip archive classes used in the Spore Mod Manager.", @"https://github.com/jaime-olivares"),
            new CreditsItem("Newtonsoft", "Made the library to read JSON data.", @"https://www.newtonsoft.com/json"),
            new CreditsItem("cederenescio", "Provided substantial creative influence."),
            new CreditsItem("Huskky", "Assisted substantially with figuring out how to make WINE cooperate."),
            new CreditsItem("Darhagonable", "Provided creative input, helped confirm the feasibility of supporting WINE setups on Linux.", @"http://youtube.com/Darhagonable"),
            new CreditsItem("HEY ROB55ROD, DON'T FORGET TO", "CREDIT THOSE WHO CONTRIBUTE BY TESTING THE SPORE MOD MANAGER!!")
        }));

        FileSystemWatcher _dragWatcher = new FileSystemWatcher(Settings.TempFolderPath)
        {
            EnableRaisingEvents = false,
            IncludeSubdirectories = false
        };

        public ManagerContent()
        {
            InitializeComponent();

            GameInfo.BadGameInstallPath += (sneder, args) =>
            {
                Dispatcher.BeginInvoke(new Action(() => GameInfo_BadGameInstallPath(sneder, args)));
            };
            ModInstallation.ClearQueues();
            //GameInfo.GetRegistryPath(GameInfo.GameDlc.GalacticAdventures);
            MessageDisplay.DebugShowMessageBox("DATA FOLDERS: \n\n" + GameInfo.CoreSporeData + "\n\n" + GameInfo.GalacticAdventuresData);
            ModsManager.InstalledMods.CollectionChanged += (sneder, args) =>
            {
                Dispatcher.BeginInvoke(new Action(() => ModConfigurations_CollectionChanged(sneder, args)));
            };
            ModInstallation.AddModProgress += (sneder, args) =>
            {
                Dispatcher.BeginInvoke(new Action(() => ModInstallation_AddModProgress(sneder, args)));
            };

            MessageDisplay.ErrorOccurred += ModInstallation_ErrorOccurred;
            MessageDisplay.MessageBoxShown += (sneder, args) => Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(args.Content, args.Title)));
            MessageDisplay.DebugMessageSent += (sneder, args) => Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(args.Content, args.Title)));

            Application.Current.Resources.MergedDictionaries[0].MergedDictionaries[1] = ShaleAccents.Sky.Dictionary;
            if (Settings.ShaleDarkTheme)
                DarkShaleToggleSwitch.IsChecked = false;

            DarkShaleToggleSwitch_Checked(DarkShaleToggleSwitch, null);
            if (Settings.UseCustomWindowDecorations)
                StandardWindowDecorationsToggleSwitch.IsChecked = false;

            if (!Settings.AllowVanillaIncompatibleMods)
                BlockVanillaIncompatibleModsToggleSwitch.IsChecked = true;

            DeveloperModeToggleSwitch.IsChecked = Settings.DeveloperMode;
            DeveloperModeToggleSwitch.Checked += (sneder, args) => Settings.DeveloperMode = true;
            DeveloperModeToggleSwitch.Unchecked += (sneder, args) => Settings.DeveloperMode = false;

            if (Settings.DeveloperMode)
                DeveloperTabItem.Visibility = Visibility.Visible;

            Loaded += (sneder, args) =>
            {
                SetLanguage();
                Window.GetWindow(this).Activate();
                System.Timers.Timer sporeOpenTimer = new System.Timers.Timer(100);
                sporeOpenTimer.Elapsed += (snedre, rags) =>
                {
                    /*if ((Process.GetProcessesByName("SporeApp").Count() > 0) || (Process.GetProcessesByName("SporeApp_ModAPIFix").Count() > 0))
                        Dispatcher.BeginInvoke(new Action(() => CloseSporeFirstContentControl.IsOpen = true));
                    else if ((Process.GetProcessesByName("SporeApp").Count() == 0) || (Process.GetProcessesByName("SporeApp_ModAPIFix").Count() == 0))
                        Dispatcher.BeginInvoke(new Action(() => CloseSporeFirstContentControl.IsOpen = false));*/
                    bool isSporeRunning = SporeLauncher.IsSporeRunning();
                    Dispatcher.BeginInvoke(new Action(() => CloseSporeFirstContentControl.IsOpen = isSporeRunning));
                };
                sporeOpenTimer.Start();
            };

            _dragWatcher.Created += (sneder, args) =>
            {
                if (Path.GetFileName(args.FullPath) == "draggedFiles")
                {
                    while (Permissions.IsFileLocked(args.FullPath))
                    { }
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        /*if (Path.GetFileName(args.FullPath) == "draggedFiles")
                        {*/
                        ModInstallation.InstallModsAsync(File.ReadAllLines(args.FullPath));
                        File.Delete(args.FullPath);
                        DropModsDialogContentControl.IsOpen = false;
                        //}
                    }));
                }
            };
        }

        private void ModInstallation_ErrorOccurred(object sender, Core.ErrorEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Exception ex = e.Exception;
                int count = 0;
                while (ex != null)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace, e.Title);
                    ex = ex.InnerException;
                    count++;
                    if (count > 4)
                        break;
                }
                //MessageBox.Show(args.Content, args.Title)));
            }));
        }

        private void GameInfo_BadGameInstallPath(object sender, BadPathEventArgs e)
        {
            bool add = true;
            BadPathEventArgs[] argsList = _badPaths.ToArray();
            foreach (BadPathEventArgs args in argsList)
            {
                if ((args.IsSporebin == e.IsSporebin) && (args.DlcLevel == e.DlcLevel))
                {
                    add = false;
                    break;
                }
            }

            if (add)
                _badPaths.Add(e);
        }

        private void ModConfigurations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IInstalledMod mod in e.NewItems)
                {
                    if ((mod is ManagedMod m) && ModsManager.InstalledMods.Contains(m))
                        m.IsProgressingChanged += InstalledMod_IsProgressingChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (IInstalledMod mod in e.OldItems)
                {
                    if ((mod is ManagedMod m) && !ModsManager.InstalledMods.Contains(m))
                        m.IsProgressingChanged -= InstalledMod_IsProgressingChanged;
                }
            }
            EvaluateShowInstallModsPrompt();
            EvaluateCanLaunch();
        }

        private void EvaluateShowInstallModsPrompt()
        {
            if (ModsManager.InstalledMods.Count == 0)
                InstallModsPromptContentControl.Visibility = Visibility.Visible;
            else
                InstallModsPromptContentControl.Visibility = Visibility.Collapsed;
        }

        private void InstalledMod_IsProgressingChanged(object sender, EventArgs e)
        {
            EvaluateCanLaunch();
        }

        private void EvaluateCanLaunch()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                /*bool canLaunch = true;
                foreach (IInstalledMod m in ManagedMods.Instance.ModConfigurations)
                {
                    if (m.IsProgressing)
                    {
                        canLaunch = false;
                        break;
                    }
                }
                LaunchGameButton.IsEnabled = canLaunch;*/

                LaunchGameButton.IsEnabled = ModsManager.InstalledMods.Where(mod => mod.IsProgressing).Count() == 0;
            }));
        }

        private void ModInstallation_AddModProgress(object sender, ModRegistrationEventArgs e)
        {
            foreach (var m in ModsManager.InstalledMods)
            {
                if (m == e.Mod)
                {
                    e.Mod.Progress++;
                    break;
                    /*if (!e.HasCustomInstaller)
                        ModInstallationProgressBar.Value++;
                    else
                    {
                        //do custom installer things here
                    }*/
                }
            }
        }

        //private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        //public override void EndInit()
        public void MainWindow_OnContentRendered(EventArgs e)
        {
            try
            {
                HasCustomWindowDecorations = (Window.GetWindow(this) is DecoratableWindow);
                if (!HasCustomWindowDecorations)
                {
                    WindowBackdropControl.Visibility = Visibility.Visible;
                    if (Settings.ShaleDarkTheme)
                        DarkShaleToggleSwitch.IsChecked = false;

                    DarkShaleToggleSwitch_Checked(DarkShaleToggleSwitch, null);
                    SetLanguage();
                }
                EvaluateShowInstallModsPrompt();

                DarkShaleToggleSwitch.Checked += DarkShaleToggleSwitch_Checked;
                DarkShaleToggleSwitch.Unchecked += DarkShaleToggleSwitch_Checked;

                StandardWindowDecorationsToggleSwitch.Checked += StandardWindowDecorationsToggleSwitch_Checked;
                StandardWindowDecorationsToggleSwitch.Unchecked += StandardWindowDecorationsToggleSwitch_Checked;

                BlockVanillaIncompatibleModsToggleSwitch.Checked += BlockVanillaIncompatibleModsToggleSwitch_Checked;
                BlockVanillaIncompatibleModsToggleSwitch.Unchecked += BlockVanillaIncompatibleModsToggleSwitch_Checked;

                //Setup game folder Settings and Controls
                AutoGaDataPathCheckBox.Checked += AutoGaDataPathCheckBox_Checked;
                AutoGaDataPathCheckBox.Unchecked += AutoGaDataPathCheckBox_Checked;

                AutoSporebinEp1PathCheckBox.Checked += AutoSporebinEp1PathCheckBox_Checked;
                AutoSporebinEp1PathCheckBox.Unchecked += AutoSporebinEp1PathCheckBox_Checked;

                AutoCoreDataPathCheckBox.Checked += AutoCoreDataPathCheckBox_Checked;
                AutoCoreDataPathCheckBox.Unchecked += AutoCoreDataPathCheckBox_Checked;

                AutoSporebinPathCheckBox.Checked += AutoSporebinPathCheckBox_Checked;
                AutoSporebinPathCheckBox.Unchecked += AutoSporebinPathCheckBox_Checked;

                SetupPathControlStates();

                //Setup game Window mode Settings and Controls
                if (Settings.ForceWindowedMode == 0)
                    WindowedWindowModeRadioButton.IsChecked = true;
                else if (Settings.ForceWindowedMode == 1)
                    FullscreenWindowModeRadioButton.IsChecked = true;

                CustomResolutionWidthTextBox.Text = Settings.ForcedGameWindowWidth.ToString();
                CustomResolutionHeightTextBox.Text = Settings.ForcedGameWindowHeight.ToString();

                OverrideWindowModeCheckBox.Checked += OverrideWindowModeCheckBox_Checked;
                OverrideWindowModeCheckBox.Unchecked += OverrideWindowModeCheckBox_Checked;
                FullscreenWindowModeRadioButton.Checked += OverrideWindowModeCheckBox_Checked;
                FullscreenWindowModeRadioButton.Unchecked += OverrideWindowModeCheckBox_Checked;
                WindowedWindowModeRadioButton.Checked += OverrideWindowModeCheckBox_Checked;
                WindowedWindowModeRadioButton.Unchecked += OverrideWindowModeCheckBox_Checked;

                if (Settings.ForceGameWindowBounds)
                    OverrideGameResolutionCheckBox.IsChecked = true;

                OverrideGameResolutionCheckBox.Checked += OverrideGameResolutionCheckBox_Checked;
                OverrideGameResolutionCheckBox.Unchecked += OverrideGameResolutionCheckBox_Checked;
                AutoResolutionRadioButton.Checked += OverrideGameResolutionCheckBox_Checked;
                AutoResolutionRadioButton.Unchecked += OverrideGameResolutionCheckBox_Checked;
                CustomResolutionRadioButton.Checked += OverrideGameResolutionCheckBox_Checked;
                CustomResolutionRadioButton.Unchecked += OverrideGameResolutionCheckBox_Checked;

                if (Settings.AutoGameWindowBounds)
                    AutoResolutionRadioButton.IsChecked = true;
                else
                    CustomResolutionRadioButton.IsChecked = true;

                if (Settings.ForceGameWindowingMode)
                    OverrideWindowModeCheckBox.IsChecked = true;

                //Setup program updating mode
                if (Settings.UpdatingMode == Settings.UpdatingModeType.Automatic)
                    UpdateAutomaticallyRadioButton.IsChecked = true;
                else if (Settings.UpdatingMode == Settings.UpdatingModeType.AutoCheck)
                    UpdateAutoCheckRadioButton.IsChecked = true;
                else
                    UpdateNeverRadioButton.IsChecked = true;

                UpdateAutomaticallyRadioButton.Checked += UpdateModeRadioButton_Checked;
                UpdateAutomaticallyRadioButton.Unchecked += UpdateModeRadioButton_Checked;
                UpdateAutomaticallyRadioButton.Checked += UpdateModeRadioButton_Checked;
                UpdateAutomaticallyRadioButton.Unchecked += UpdateModeRadioButton_Checked;
                UpdateNeverRadioButton.Checked += UpdateModeRadioButton_Checked;
                UpdateNeverRadioButton.Unchecked += UpdateModeRadioButton_Checked;

                //Setup game startup state Settings and Controls
                if (Settings.UseCustomGameState)
                    LaunchWithStateCheckBox.IsChecked = true;

                LaunchWithStateComboBox.IsEnabled = Settings.UseCustomGameState;
                LaunchWithStateCheckBox.Checked += LaunchWithStateCheckBox_Checked;
                LaunchWithStateCheckBox.Unchecked += LaunchWithStateCheckBox_Checked;

                LaunchWithStateComboBox.Text = Settings.GameState;
                //LaunchWithStateTextBox.TextChanged += LaunchWithStateTextBox_TextChanged;
                LaunchWithStateComboBox.AddHandler(TextBoxBase.TextChangedEvent,
                      new TextChangedEventHandler(LaunchWithStateTextBox_TextChanged));


                LaunchWithLanguageCheckBox.Checked += LaunchWithLanguageCheckBox_Checked;
                LaunchWithLanguageCheckBox.Unchecked += LaunchWithLanguageCheckBox_Checked;

                //Setup game Language override Settings and Controls
                if (Settings.ForceGameLocale)
                    LaunchWithLanguageCheckBox.IsChecked = true;

                if (!string.IsNullOrWhiteSpace(Settings.ForcedGameLocale))
                {
                    if (Settings.ForcedGameLocale.StartsWith("-"))
                        LaunchWithLanguageRegionTextBox.Text = Settings.ForcedGameLocale;
                    else if (Settings.ForcedGameLocale.Contains("-"))
                    {
                        string forcedLangString = Settings.ForcedGameLocale.Substring(0, Settings.ForcedGameLocale.LastIndexOf("-"));
                        string forcedRegionString = Settings.ForcedGameLocale.Substring(forcedLangString.Length + 1);
                        LaunchWithLanguageLangTextBox.Text = forcedLangString;
                        LaunchWithLanguageRegionTextBox.Text = forcedRegionString;
                    }
                    else
                        LaunchWithLanguageLangTextBox.Text = Settings.ForcedGameLocale;
                }

                LaunchWithLanguageLangTextBox.TextChanged += LaunchWithLanguageTextBox_TextChanged;
                LaunchWithLanguageRegionTextBox.TextChanged += LaunchWithLanguageTextBox_TextChanged;
                LanguagesComboBox.SelectionChanged += (sneder, args) =>
                {
                    if (LanguagesComboBox.SelectedItem != null)
                    {
                        if ((LanguagesComboBox.SelectedItem is ComboBoxItem item) && (item.Tag != null) && (item.Tag is string tag) && (tag.Contains("-")))
                        {
                            string[] lang = tag.Split('-');
                            if (lang.Length >= 2)
                            {
                                LaunchWithLanguageLangTextBox.Text = lang[0];
                                LaunchWithLanguageRegionTextBox.Text = lang[1];
                            }
                        }
                        LanguagesComboBox.SelectedItem = null;
                    }
                };

                //Setup additional commandline options Settings and Controls
                AdditionalCommandLineOptionsTextBox.Text = Settings.CommandLineOptions;
                AdditionalCommandLineOptionsTextBox.TextChanged += AdditionalCommandLineOptionsTextBox_TextChanged;

                //Check bad paths, if game could not be automatically detected
                ProceedToNextBadPath(false);

                ModsManager.Instance.ModConfiguratorShown += Instance_ModConfiguratorShown;

                //Install mod passed via commandline, if any
                string[] clArgs = Environment.GetCommandLineArgs();
                if (clArgs.Length > 1)
                {
                    /*string path = Environment.GetCommandLineArgs()[1];
                    path = path.Trim(@"\".ToCharArray());
                    InstallSingleMod(path);
                    FoldersNotFoundContentControl.IsManipulationEnabled = false;*/
                    List<string> modArgs = new List<string>();

                    foreach (string rawArg in clArgs)
                    {
                        string tArg = rawArg.Trim(" ".ToCharArray()).Trim("\"".ToCharArray());
                        if (Uri.IsWellFormedUriString(tArg, UriKind.RelativeOrAbsolute) && File.Exists(tArg))
                        {
                            modArgs.Add(tArg);
                        }
                    }

                    if (modArgs.Count > 0)
                        ModInstallation.InstallModsAsync(modArgs.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
                ModsManager.InstalledMods.Add(new InstallError(ex));
            }
        }

        private void BlockVanillaIncompatibleModsToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (BlockVanillaIncompatibleModsToggleSwitch.IsChecked.Value)
                Settings.AllowVanillaIncompatibleMods = false;
            else
                Settings.AllowVanillaIncompatibleMods = true;
        }

        void SetupPathControlStates()
        {
            AutoGaDataPathCheckBox.IsChecked = string.IsNullOrWhiteSpace(Settings.ForcedGalacticAdventuresDataPath);
            if (!AutoGaDataPathCheckBox.IsChecked.Value)
                GaDataPathTextBox.Text = Settings.ForcedGalacticAdventuresDataPath;
            if (GameInfo.AutoGalacticAdventuresData.IsNullOrEmptyOrWhiteSpace())
                AutoGaDataPathCheckBox.IsEnabled = false;

            AutoSporebinEp1PathCheckBox.IsChecked = string.IsNullOrWhiteSpace(Settings.ForcedGalacticAdventuresSporebinEP1Path);
            if (!AutoSporebinEp1PathCheckBox.IsChecked.Value)
                SporebinEp1PathTextBox.Text = Settings.ForcedGalacticAdventuresSporebinEP1Path;
            if (GameInfo.AutoSporebinEP1.IsNullOrEmptyOrWhiteSpace())
                AutoSporebinEp1PathCheckBox.IsEnabled = false;

            AutoCoreDataPathCheckBox.IsChecked = string.IsNullOrWhiteSpace(Settings.ForcedCoreSporeDataPath);
            if (!AutoCoreDataPathCheckBox.IsChecked.Value)
                CoreDataPathTextBox.Text = Settings.ForcedCoreSporeDataPath;
            if (GameInfo.AutoCoreSporeData.IsNullOrEmptyOrWhiteSpace())
                AutoCoreDataPathCheckBox.IsEnabled = false;

            AutoSporebinPathCheckBox.IsChecked = string.IsNullOrWhiteSpace(Settings.ForcedCoreSporeSporeBinPath);
            if (!AutoSporebinPathCheckBox.IsChecked.Value)
                SporebinPathTextBox.Text = Settings.ForcedCoreSporeSporeBinPath;
            if (GameInfo.AutoSporebin.IsNullOrEmptyOrWhiteSpace())
                AutoSporebinPathCheckBox.IsEnabled = false;
        }

        private void PathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Implement this
            MessageBox.Show("AAAAA");
        }

        private void LaunchWithLanguageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((LaunchWithLanguageLangTextBox.Text.Length > 0) && (LaunchWithLanguageRegionTextBox.Text.Length > 0))
                Settings.ForcedGameLocale = LaunchWithLanguageLangTextBox.Text + "-" + LaunchWithLanguageRegionTextBox.Text;
            else if (LaunchWithLanguageLangTextBox.Text.Length == 0)
                Settings.ForcedGameLocale = "-" + LaunchWithLanguageRegionTextBox.Text;
            else if (LaunchWithLanguageRegionTextBox.Text.Length == 0)
                Settings.ForcedGameLocale = LaunchWithLanguageLangTextBox.Text;

            /*if ((LaunchWithLanguageLangTextBox.Text.Length == 2) && (LaunchWithLanguageRegionTextBox.Text.Length == 2))
            {

            }*/
        }

        private void LaunchWithLanguageCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (LaunchWithLanguageCheckBox.IsChecked.Value)
            {
                Settings.ForceGameLocale = true;
                LaunchWithLanguageLangTextBox.IsEnabled = true;
                LaunchWithLanguageRegionTextBox.IsEnabled = true;
            }
            else
            {
                Settings.ForceGameLocale = false;
                LaunchWithLanguageLangTextBox.IsEnabled = false;
                LaunchWithLanguageRegionTextBox.IsEnabled = false;
            }
        }

        private async Task<bool> Instance_ModConfiguratorShown(ManagedMod arg)
        {
            var tcs = new TaskCompletionSource<bool>();

            void ProceedButton_Click(object sender, RoutedEventArgs e)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ModConfiguratorDialogContentControl.IsOpen = false;
                    ConfiguratorBodyContentControl.Content = null;
                }));
                tcs.TrySetResult(true);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ModConfiguratorModNameTextBlock.Text = Settings.GetLanguageString(2, "ModInstallerHeader").Replace("%MODNAME%", arg.DisplayName);

                if (arg.Identity.InstallerSystemVersion.Major == 1)
                {
                    var configurator = new ModConfiguratorV1_0_0_0(arg);

                    ModConfiguratorHeaderContentControl.MouseEnter += (sneder, args) => configurator.HeaderHover();

                    ConfiguratorBodyContentControl.Content = configurator;
                }

                CustomInstallerInstallButton.Click += ProceedButton_Click;

                ModConfiguratorDialogContentControl.IsOpen = true;
            }));
            bool retVal = await tcs.Task;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CustomInstallerInstallButton.Click -= ProceedButton_Click;
            }));
            return retVal;
        }

        private void AdditionalCommandLineOptionsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.CommandLineOptions = AdditionalCommandLineOptionsTextBox.Text;
        }

        private void LaunchWithStateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool value = LaunchWithStateCheckBox.IsChecked.Value;
            Settings.UseCustomGameState = value;
            LaunchWithStateComboBox.IsEnabled = value;
        }

        private void LaunchWithStateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string value = LaunchWithStateComboBox.Text;

            while (value.StartsWith(" "))
                value = value.Substring(1);
            while (value.EndsWith(" "))
                value = value.Substring(0, value.Length - 1);

            string statePrefix = "-state:";
            if (value.ToLowerInvariant().StartsWith(statePrefix))
                value = value.Substring(statePrefix.Length);

            Settings.GameState = value;
        }

        private void ProceedToNextBadPath(bool removeFirst)
        {
            if (_badPaths.Count > 0)
            {
                if (removeFirst)
                {
                    if (_badPaths.Count > 1)
                        _badPaths.RemoveAt(0);
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            FoldersNotFoundContentControl.IsOpen = false;

                            /*if (string.IsNullOrEmpty(GameInfo.AutoGalacticAdventuresData) || string.IsNullOrWhiteSpace(GameInfo.AutoGalacticAdventuresData))
                                AutoGaDataPathCheckBox.IsChecked = false;
                            if (string.IsNullOrEmpty(GameInfo.AutoCoreSporeData) || string.IsNullOrWhiteSpace(GameInfo.AutoCoreSporeData))
                                AutoCoreDataPathCheckBox.IsChecked = false;
                            if (string.IsNullOrEmpty(GameInfo.AutoSporebin) || string.IsNullOrWhiteSpace(GameInfo.AutoSporebin))
                                AutoSporebinPathCheckBox.IsChecked = false;
                            if (string.IsNullOrEmpty(GameInfo.AutoSporebinEP1) || string.IsNullOrWhiteSpace(GameInfo.AutoSporebinEP1))
                                AutoSporebinEp1PathCheckBox.IsChecked = false;*/
                            //SetupPathControlStates();
                            Permissions.RerunAsAdministrator(true);
                        }));
                        return;
                    }
                }

                BadPathEventArgs[] argsList = _badPaths.ToArray();
                int index = 0;
                while (index < argsList.Length)
                {
                    if (HandleBadPath(argsList[index]))
                        break;
                    else
                        index++;
                }
            }
        }

        public void InstallSingleMod(string path)
        {
            try
            {
                MessageBox.Show("PATH: " + path + "\n\nFILE EXISTS: " + File.Exists(path));
                if (File.Exists(path))
                {
                    string[] paths = new string[] { path };
                    ModInstallation.InstallModsAsync(paths);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("PATH: " + path + "\n\n" + ex.Message + "\n\n" + ex.StackTrace);
                ModsManager.InstalledMods.Add(new InstallError(ex));
            }
        }

        private bool HandleBadPath(BadPathEventArgs args)
        {
            Debug.WriteLine("HANDLING BAD PATH: " + args.DlcLevel.ToString() + ", " + args.IsSporebin);
            List<DetectionFailureGuessFolder> folders = GameInfo.GetFailureGuessFolders(args.DlcLevel, args.IsSporebin);
            if (folders.Count > 0)
            {
                Debug.WriteLine("BAD PATH GUESS COUNT != 1");
                string folderName = string.Empty;
                if (args.DlcLevel == GameInfo.GameDlc.GalacticAdventures)
                {
                    folderName = Settings.GetLanguageString(3, "SporeGAFolder"); //"Galactic Adventures ";

                    if (args.IsSporebin)
                    {
                        if (Settings.ForcedGalacticAdventuresSporebinEP1Path != null)
                            return true;

                        folderName = folderName.Replace("%DIRNAME%", "SporebinEP1");
                    }
                    else
                    {
                        if (Settings.ForcedGalacticAdventuresDataPath != null)
                            return true;

                        folderName = folderName.Replace("%DIRNAME%", "DataEP1/Data");
                    }
                }
                else if (args.DlcLevel == GameInfo.GameDlc.CoreSpore)
                {
                    folderName += Settings.GetLanguageString(3, "SporeCoreFolder"); //"Core Spore ";

                    if (args.IsSporebin)
                    {

                        if (Settings.ForcedCoreSporeSporeBinPath != null)
                            return true;

                        folderName = folderName.Replace("%DIRNAME%", "SporeBin");
                    }
                    else
                    {
                        if (Settings.ForcedCoreSporeDataPath != null)
                            return true;

                        folderName = folderName.Replace("%DIRNAME%", "Data");
                    }
                }


                _notFoundFolderIsSporeBin = args.IsSporebin;
                _notFoundFolderDlcLevel = args.DlcLevel;

                //int index = -1;
                /*while (index == -1)
                {
                    //Thread.Sleep(500);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        index = FolderNotFoundListView.SelectedIndex;
                    }));
                }*/

                /*Dispatcher.BeginInvoke(new Action(() =>
                {
                    FoldersNotFoundContentControl.IsManipulationEnabled = false;
                }));*/
                //Thread.Sleep(125);
                if (folders.Count > 1)
                {
                    FolderNotFoundErrorTextBlock.Text = Settings.GetLanguageString(3, "FolderNotFound").Replace("%FOLDERNAME%", folderName); //"PLACEHOLDER: The " + folderName + " folder could not be automatically uniquely identified. Please select from the list below, or specify manually if needed. (This can be changed later under Settings if needed.)";
                    FolderNotFoundListView.ItemsSource = folders;
                    FolderNotFoundListView.SelectedItem = null;
                    FolderNotFoundListView.SelectionChanged += FolderNotFoundListView_SelectionChanged;
                }
                else
                    FolderNotFoundErrorTextBlock.Text = Settings.GetLanguageString(3, "FolderNotFoundNoGuesses").Replace("%FOLDERNAME%", folderName); //"PLACEHOLDER: The " + folderName + " folder could not be automatically detected. Please specify manually if needed. (This can be changed later under Settings if needed.)";

                FoldersNotFoundContentControl.IsOpen = true;
                Debug.WriteLine("BAD PATH PANEL SHOWN");
                return true;
            }
            else
            {
                return false;
            }
        }

        List<BadPathEventArgs> _badPaths = new List<BadPathEventArgs>();
        bool _notFoundFolderIsSporeBin = false;
        GameInfo.GameDlc _notFoundFolderDlcLevel = GameInfo.GameDlc.None;

        private void ConfirmForcedInstallPath(string GuessPath, bool reset)
        {
            if (_notFoundFolderDlcLevel == GameInfo.GameDlc.GalacticAdventures)
            {
                if (_notFoundFolderIsSporeBin)
                {
                    Settings.ForcedGalacticAdventuresSporebinEP1Path = GuessPath;
                }
                else
                {
                    Settings.ForcedGalacticAdventuresDataPath = GuessPath;
                }
            }
            else if (_notFoundFolderDlcLevel == GameInfo.GameDlc.CoreSpore)
            {

                if (_notFoundFolderIsSporeBin)
                {
                    Settings.ForcedCoreSporeSporeBinPath = GuessPath;
                }
                else
                {
                    Settings.ForcedCoreSporeDataPath = GuessPath;
                }
            }

            if (reset)
            {
                _notFoundFolderIsSporeBin = false;
                _notFoundFolderDlcLevel = GameInfo.GameDlc.None;
            }
        }
        private void FolderNotFoundListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((FolderNotFoundListView.SelectedItem != null) && (FolderNotFoundListView.SelectedItem is DetectionFailureGuessFolder folder))
            {
                ConfirmForcedInstallPath(folder.GuessPath, true);

                /*if (_badPaths.Count > 1)
                {
                    
                    HandleBadPath(_badPaths.ElementAt(0));
                }
                else
                    FoldersNotFoundContentControl.IsManipulationEnabled = false;*/
                ProceedToNextBadPath(true);
            }
        }

        private void LanguageCheckBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                Settings.CurrentLanguageName = (string)e.AddedItems[0];
                if (IsLoaded)
                {
                    SetLanguage();
                }
            }
        }

        void SetLanguage()
        {
            if (IsLoaded && (Window.GetWindow(this) != null) && (Window.GetWindow(this).IsLoaded))
            {
                /*if (Permissions.IsAdministrator())
                    Window.GetWindow(this).Title = GetLanguageString("WindowTitle") + Settings.ModApiManagementKitVersion.ToString() + GetLanguageString("VersionIdentifierSuffix");
                    Title += GetLanguageString("IsAdministrator");*/
                Window.GetWindow(this).Title = Settings.GetLanguageString("WindowTitle").Replace("%VERSION%", Settings.ModManagerVersion.ToString()).Replace("%DLLSBUILD%", Settings.CurrentDllsBuildString);
            }

            LaunchGameButton.Content = Settings.GetLanguageString("LaunchGameButton");
            ModsTabItem.Header = Settings.GetLanguageString("ModsTabItem");
            SettingsTabItem.Header = Settings.GetLanguageString("SettingsTabItem");
            HelpTabItem.Header = Settings.GetLanguageString("HelpTabItem");

            InstallModsButton.Content = Settings.GetLanguageString("InstallModFromDiskButton");
            UninstallModsButton.Content = Settings.GetLanguageString("UninstallModButton");
            ConfigureModButton.Content = Settings.GetLanguageString("ConfigureModButton");

            CopyModsToClipboardButton.Content = Settings.GetLanguageString("CopyModsListToClipboard");

            InstallModsPromptContentControl.Content = Settings.GetLanguageString("DropModsHerePrompt");
            DropModsHereTextBlock.Text = Settings.GetLanguageString("DropModsHereInstruction");

            SearchBox.WatermarkText = Settings.GetLanguageString("SearchWatermark");
            SearchNamesMenuItem.Header = Settings.GetLanguageString("SearchNames");
            SearchDescriptionsMenuItem.Header = Settings.GetLanguageString("SearchDescriptions");
            SearchTagsMenuItem.Header = Settings.GetLanguageString("SearchTags");

            BrowseForModsButton.Content = Settings.GetLanguageString(1, "Browse");

            GaDataPathBrowseButton.Content = Settings.GetLanguageString(1, "Browse");
            SporebinEp1PathBrowseButton.Content = Settings.GetLanguageString(1, "Browse");
            CoreDataPathBrowseButton.Content = Settings.GetLanguageString(1, "Browse");
            SporebinPathBrowseButton.Content = Settings.GetLanguageString(1, "Browse");

            Resources["ModSwitchOnText"] = Settings.GetLanguageString("ModSwitchOn");
            Resources["ModSwitchOffText"] = Settings.GetLanguageString("ModSwitchOff");
            Resources["ModInstallingNowText"] = Settings.GetLanguageString("ModInstallingNow");
            Resources["ModInstalledManuallyText"] = Settings.GetLanguageString("ModInstalledManually");

            //ExitSporeToManageModsTextBlock.Text = Settings.GetLanguageString("ExitSporeToManageMods");

            FoldersGroupBox.Header = Settings.GetLanguageString("FoldersHeader");
            AutoGaDataPathCheckBox.Content = Settings.GetLanguageString("AutoGaDataPath");
            AutoSporebinEp1PathCheckBox.Content = Settings.GetLanguageString("AutoSporebinEp1Path");
            AutoCoreDataPathCheckBox.Content = Settings.GetLanguageString("AutoCoreDataPath");
            AutoSporebinPathCheckBox.Content = Settings.GetLanguageString("AutoSporebinPath");
            Resources["AutoDetectPathText"] = Settings.GetLanguageString("AutoDetectPath");

            WindowGroupBox.Header = Settings.GetLanguageString("WindowHeader");
            OverrideWindowModeCheckBox.Content = Settings.GetLanguageString("OverrideWindowMode");
            FullscreenWindowModeRadioButton.Content = Settings.GetLanguageString("WindowModeFullscreen");
            //BorderlessWindowModeRadioButton.Content = Settings.GetLanguageString("WindowModeBorderlessWindowed");
            WindowedWindowModeRadioButton.Content = Settings.GetLanguageString("WindowModeWindowed");

            OverrideGameResolutionCheckBox.Content = Settings.GetLanguageString("OverrideGameResolution");
            AutoResolutionRadioButton.Content = Settings.GetLanguageString("ResolutionAuto");
            CustomResolutionRadioButton.Content = Settings.GetLanguageString("ResolutionCustom");

            CommandLineGroupBox.Header = Settings.GetLanguageString("GameEntryHeader");
            LaunchWithStateCheckBox.Content = Settings.GetLanguageString("CommandLineState");
            LaunchWithStateNameTextBlock.Text = Settings.GetLanguageString("CommandLineStateName");
            LaunchWithLanguageCheckBox.Content = Settings.GetLanguageString("CommandLineLanguage");
            LocaleNameTextBlock.Text = Settings.GetLanguageString("CommandLineLocaleName");
            AdditionalCommandLineOptionsTextBlock.Text = Settings.GetLanguageString("CommandLineOptions");

            //AppearanceGroupBox.Header = Settings.GetLanguageString("Appearance");
            SkinOptionsGroupBox.Header = Settings.GetLanguageString("AppearanceHeader"); //Settings.GetLanguageString("SkinOptions");
            DarkShaleToggleSwitch.Content = Settings.GetLanguageString("LightSwitchHeader");
            DarkShaleToggleSwitch.TrueText = Settings.GetLanguageString("SwitchOn");
            DarkShaleToggleSwitch.FalseText = Settings.GetLanguageString("SwitchOff");

            ModInstallationGroupBox.Header = Settings.GetLanguageString("ModInstallationHeader");
            BlockVanillaIncompatibleModsToggleSwitch.Content = Settings.GetLanguageString("BlockVanillaIncompatibleMods");
            BlockVanillaIncompatibleModsToggleSwitch.TrueText = Settings.GetLanguageString("SwitchYes");
            BlockVanillaIncompatibleModsToggleSwitch.FalseText = Settings.GetLanguageString("SwitchNo");

            DeveloperModeToggleSwitch.Content = Settings.GetLanguageString("RequiresAppRestart").Replace("%CONTEXT%", Settings.GetLanguageString("UseDeveloperMode"));
            DeveloperModeToggleSwitch.TrueText = Settings.GetLanguageString("SwitchOn");
            DeveloperModeToggleSwitch.FalseText = Settings.GetLanguageString("SwitchOff");

            DeveloperTabItem.Header = Settings.GetLanguageString("DeveloperHeader");
            SideloadCoreDllsGroupBox.Header = Settings.GetLanguageString("SideloadCoreDlls");
            AddSideloadCoreDllsButton.Content = Settings.GetLanguageString("AddSideloadCoreDlls");
            RemoveSideloadCoreDllsButton.Content = Settings.GetLanguageString("RemoveSideloadCoreDlls");
            BuildSideloadCoreDllsTextBlock.Text = Settings.GetLanguageString("BuildSideloadCoreDlls").Replace("%OVERRIDELIBSPATH%", Settings.OverrideLibsPath);

            LanguageTextBlock.Text = Settings.GetLanguageString("LanguageHeader");

            StandardWindowDecorationsToggleSwitch.Content = Settings.GetLanguageString("RequiresAppRestart").Replace("%CONTEXT%", Settings.GetLanguageString("UseStandardWindowDecorations"));

            UpdateGroupBox.Header = Settings.GetLanguageString("UpdateHeader");
            UpdateTextBlock.Text = Settings.GetLanguageString("UpdateQuestion");
            UpdateAutomaticallyRadioButton.Content = Settings.GetLanguageString("UpdateAutomatically");
            UpdateAutoCheckRadioButton.Content = Settings.GetLanguageString("UpdateAutoCheck");
            UpdateNeverRadioButton.Content = Settings.GetLanguageString("UpdateNever");

            HelpGroupBox.Header = Settings.GetLanguageString("HelpHeader");
            HelpThreadButton.Content = Settings.GetLanguageString("GoToForumThread");
            ShowConfigurationFileButton.Content = Settings.GetLanguageString("ShowConfig");

            CloseSporeFirstTextBlock.Text = Settings.GetLanguageString("CloseSporeFirst");
            SporeCantCloseTextBlock.Text = Settings.GetLanguageString("SporeCantClose");
            ForceKillSporeButton.Content = Settings.GetLanguageString("ForceKillSporeButton");

            CreditsGroupBox.Header = Settings.GetLanguageString("CreditsHeader");

            CustomInstallerInstallButton.Content = Settings.GetLanguageString(2, "Install");

            Resources["ProbablyDisksGuessText"] = Settings.GetLanguageString(3, "ProbablyDiskGuess");
            Resources["ProbablyOriginGuessText"] = Settings.GetLanguageString(3, "ProbablyOriginGuess");
            Resources["ProbablyGOGGuessText"] = Settings.GetLanguageString(3, "ProbablyGOGGuess");
        }

        private void LaunchGameButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Launching Spore from within the Spore Mod Manager is temporarily disabled. For the time being, please use the external Spore ModAPI Launcher (SporeMods.Launcher.exe) instead.");
            if (Permissions.IsAtleastWindowsVista() && Permissions.IsAdministrator() && (App.DragServantProcess != null))
                ServantCommands.RunLauncher();
            else// if (!Permissions.IsAdministrator())
                StartLauncher();
        }

        private void InstallModsButton_Click(object sender, RoutedEventArgs e)
        {
            /*DropModsDockPanel.Visibility = Visibility.Visible;
            ModInstallationProgressDockPanel.Visibility = Visibility.Hidden;
            ModsInstalledDockPanel.Visibility = Visibility.Hidden;*/
            if (PrimaryTabControl.SelectedItem != ModsTabItem)
                PrimaryTabControl.SelectedItem = ModsTabItem;
            DropModsDialogContentControl.IsOpen = true;
        }

        public void ShowHideDropModsDialogContentControl(bool isVisible)
        {
            /*DropModsDialogContentControl.IsOpen = isVisible;
            WhenDropModsDialogContentControlVisibilityChanged(IsVisible);*/
        }


        /*private void DropModsHereTextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DropModsHereTextBlock.IsVisible)
            {
                CompositionTarget.Rendering += CompositionTarget_Rendering;
                ShowWindow(App.DragServantProcess.MainWindowHandle, 5);
            }
            else
            {
                MessageBox.Show("Hiding Drag window!");
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                ShowWindow(App.DragServantProcess.MainWindowHandle, 0);
            }
        }*/

        public void MainWindow_IsActiveChanged(object sender, EventArgs e)
        {
            UpdateDragWindow();
        }

        public void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDragWindow();
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            UpdateDragWindow();
        }

        private void UpdateDragWindow()
        {
            if ((App.DragServantProcess != null) && (App.DragServantProcess.MainWindowHandle != IntPtr.Zero))
            {
                if ((IsLoaded && (Window.GetWindow(this) != null) && (Window.GetWindow(this).IsLoaded)) && DropModsHereTextBlockGrid.IsVisible)
                {
                    Window win = Window.GetWindow(this);
                    var basePoint = DropModsHereTextBlockGrid.PointToScreen(new Point(0, 0));
                    IntPtr winHwnd = new WindowInteropHelper(win).Handle;
                    //IntPtr prevWinHwnd = GetWindow(winHwnd, 3);

                    if (GetForegroundWindow() == App.DragServantProcess.MainWindowHandle)
                    {
                        win.Activate();
                    }
                    /*else if (win.IsActive)
                    {
                        SetWindowLong(App.DragServantProcess.MainWindowHandle, GwlExstyle, GetWindowLong(App.DragServantProcess.MainWindowHandle, GwlExstyle).ToInt32() | 0x00000008);
                        //SetWindowPos(App.DragServantProcess.MainWindowHandle, GetWindow(new WindowInteropHelper(win).Handle, 3)/*winHwnd*, (int)basePoint.X, (int)basePoint.Y, (int)DropModsHereTextBlockGrid.ActualWidth, (int)DropModsHereTextBlockGrid.ActualHeight, SwpNoActivate | 0x0040/* | 0x0004*);
                    }
                    else
                    {
                        SetWindowLong(App.DragServantProcess.MainWindowHandle, GwlExstyle, GetWindowLong(App.DragServantProcess.MainWindowHandle, GwlExstyle).ToInt32() | (~0x00000008));
                        //SetWindowPos(App.DragServantProcess.MainWindowHandle, IntPtr.Zero/*winHwnd*, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoActivate | 0x0040/* | 0x0004*);
                        //SetWindowPos(App.DragServantProcess.MainWindowHandle, GetWindow(new WindowInteropHelper(win).Handle, 3)/*winHwnd*, (int)basePoint.X, (int)basePoint.Y, (int)DropModsHereTextBlockGrid.ActualWidth, (int)DropModsHereTextBlockGrid.ActualHeight, SwpNoActivate | 0x0040 | 0x0004);
                    }*/
                    if (/*(win.IsActive | (GetForegroundWindow() == App.DragServantProcess.MainWindowHandle)) && */DropModsDialogContentControl.IsOpen)
                    {
                        SetWindowPos(App.DragServantProcess.MainWindowHandle, winHwnd, (int)basePoint.X, (int)basePoint.Y, (int)DropModsHereTextBlockGrid.ActualWidth, (int)DropModsHereTextBlockGrid.ActualHeight, SwpNoActivate | 0x0040 | 0x0004);
                        SetWindowPos(App.DragServantProcess.MainWindowHandle, GetWindow(winHwnd, 3), 1, 1, 5, 5, SwpNoActivate | SwpNoSize | SwpNoMove | 0x0040/* | 0x0004*/);
                        ShowWindow(App.DragServantProcess.MainWindowHandle, 4);
                    }
                    else
                        ShowWindow(App.DragServantProcess.MainWindowHandle, 0);
                }
                else
                    ShowWindow(App.DragServantProcess.MainWindowHandle, 0);
            }
            /*else
            {
                MessageBox.Show("App.DragServantProcess == null: " + (App.DragServantProcess == null).ToString() + "\nApp.DragServantProcess.MainWindowHandle == IntPtr.Zero: " + (App.DragServantProcess.MainWindowHandle == IntPtr.Zero).ToString());
            }*/
        }

        private void UninstallModsButton_Click(object sender, RoutedEventArgs e)
        {
            var list = GetActiveModsListView();
            IInstalledMod[] mods = new IInstalledMod[list.SelectedItems.Count];
            list.SelectedItems.CopyTo(mods, 0);
            ModInstallation.UninstallModsAsync(mods);
        }

        private void ConfigureModButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetActiveModsListView().SelectedItem is ManagedMod mod)
                mod.ConfigureMod();
            //CustomInstallerInstallButton.Content
        }

        private async void BrowseForModsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Multiselect = true,
                Title = Settings.GetLanguageString("SelectMod"),
                Filter = Settings.GetLanguageString("AllSporeModsFilter").Replace("%EXTENSIONS%", "*.sporemod, *.package") + "|*.sporemod;*.package"
                ///"Spore mod (*.sporemod, *.package)|*.sporemod;*.package|Bundled Spore mod (*.sporemod)|*.sporemod|Legacy Spore Mod (*.package)|*.package"
            };

            try
            {
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (Directory.Exists(downloadsPath))
                    dialog.InitialDirectory = downloadsPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "OpenFileDialog error");
                ModsManager.InstalledMods.Add(new InstallError(ex));
            }

            if (dialog.ShowDialog() == true)
            {
                if (dialog.FileNames.Length > 0)
                {
                    DropModsDialogContentControl.IsOpen = false;
                    await ModInstallation.InstallModsAsync(dialog.FileNames);
                }
            }
        }

        private async void DropModsDialogContentControl_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                /*ModInstallationProgressBar.Maximum = files.Length;

                DropModsDockPanel.Visibility = Visibility.Hidden;
                ModInstallationProgressDockPanel.Visibility = Visibility.Visible;
                ModsInstalledDockPanel.Visibility = Visibility.Hidden;*/

                if (files.Length > 0)
                {
                    DropModsDialogContentControl.IsOpen = false;
                    await ModInstallation.InstallModsAsync(files);

                    //bool proceed = false;
                    /*new Thread(() =>
                    {
                        while (File.Exists(ModInstallation.InstructionPath))
                        { }
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            /*DropModsDockPanel.Visibility = Visibility.Hidden;
                            ModInstallationProgressDockPanel.Visibility = Visibility.Hidden;
                            ModsInstalledDockPanel.Visibility = Visibility.Visible;*
                        }));
                        //proceed = true;
                    }).Start();*/

                    /*if (proceed)
                    {

                        //ShowHideDropModsDialogContentControl(false);
                        //(DropModsDialogContentControl.Content as FrameworkElement).Visibility = Visibility.Visible;
                    }*/
                }
            }
            //BrowseForModsButton_Click
        }

        public void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DropModsDialogContentControl.IsOpen)
                    DropModsDialogContentControl.IsOpen = false;
            }
        }

        private void AutoGaDataPathCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoGaDataPathCheckBox.IsChecked.Value)
            {
                if (GameInfo.AutoGalacticAdventuresData.IsNullOrEmptyOrWhiteSpace())
                    AutoGaDataPathCheckBox.IsChecked = false;
                else
                {
                    GaDataPathTextBox.IsEnabled = false;
                    GaDataPathTextBox.Text = GameInfo.AutoGalacticAdventuresData;
                }
            }
            else
                GaDataPathTextBox.IsEnabled = true;
            //GameInfo.VerifyGamePaths();
        }

        private void AutoSporebinEp1PathCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoSporebinEp1PathCheckBox.IsChecked.Value)
            {
                if (GameInfo.AutoSporebinEP1.IsNullOrEmptyOrWhiteSpace())
                    AutoSporebinEp1PathCheckBox.IsChecked = false;
                else
                {
                    SporebinEp1PathTextBox.IsEnabled = false;
                    SporebinEp1PathTextBox.Text = GameInfo.AutoSporebinEP1;
                }
            }
            else
                SporebinEp1PathTextBox.IsEnabled = true;
            //GameInfo.VerifyGamePaths();
        }

        private void AutoCoreDataPathCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoCoreDataPathCheckBox.IsChecked.Value)
            {
                if (GameInfo.AutoCoreSporeData.IsNullOrEmptyOrWhiteSpace())
                    AutoCoreDataPathCheckBox.IsChecked = false;
                else
                {
                    CoreDataPathTextBox.IsEnabled = false;
                    CoreDataPathTextBox.Text = GameInfo.AutoCoreSporeData;
                }
            }
            else
                CoreDataPathTextBox.IsEnabled = true;
            //GameInfo.VerifyGamePaths();
        }

        private void AutoSporebinPathCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoSporebinPathCheckBox.IsChecked.Value)
            {
                if (GameInfo.AutoSporebin.IsNullOrEmptyOrWhiteSpace())
                    AutoSporebinPathCheckBox.IsChecked = false;
                else
                {
                    SporebinPathTextBox.IsEnabled = false;
                    SporebinPathTextBox.Text = GameInfo.AutoSporebin;
                }
            }
            else
                SporebinPathTextBox.IsEnabled = true;
            //GameInfo.VerifyGamePaths();
        }

        private void GaDataPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GaDataPathTextBox.IsEnabled)
                Settings.ForcedGalacticAdventuresDataPath = GaDataPathTextBox.Text;
            else
                Settings.ForcedGalacticAdventuresDataPath = null;
        }

        private void SporebinEp1PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SporebinEp1PathTextBox.IsEnabled)
                Settings.ForcedGalacticAdventuresSporebinEP1Path = SporebinEp1PathTextBox.Text;
            else
                Settings.ForcedGalacticAdventuresSporebinEP1Path = null;
        }

        private void CoreDataPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CoreDataPathTextBox.IsEnabled)
                Settings.ForcedCoreSporeDataPath = CoreDataPathTextBox.Text;
            else
                Settings.ForcedCoreSporeDataPath = null;
        }

        private void SporebinPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SporebinPathTextBox.IsEnabled)
                Settings.ForcedCoreSporeSporeBinPath = SporebinPathTextBox.Text;
            else
                Settings.ForcedCoreSporeSporeBinPath = null;
        }

        /*private void CurrentSkinComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentSkinComboBox.SelectedIndex == 0)
            {
                if (App.Current.Resources.MergedDictionaries.Contains(darkShaleDictionary))
                    App.Current.Resources.MergedDictionaries.Remove(darkShaleDictionary);
            }
            else if (CurrentSkinComboBox.SelectedIndex == 1)
            {
                if (!App.Current.Resources.MergedDictionaries.Contains(darkShaleDictionary))
                    App.Current.Resources.MergedDictionaries.Add(darkShaleDictionary);
            }
            else
            {
                CurrentSkinComboBox.SelectedIndex = 0;
            }
        }*/
        ResourceDictionary darkShaleDictionary = new ResourceDictionary()
        {
            Source = new Uri(@"DarkShale.xaml", UriKind.RelativeOrAbsolute) ///Mechanism.Wpf.Core;component/Themes/Colors/ShaleBaseDark.xaml
        };

        private void DarkShaleToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (DarkShaleToggleSwitch.IsChecked.Value)
            {
                if (Application.Current.Resources.MergedDictionaries.Contains(darkShaleDictionary))
                    Application.Current.Resources.MergedDictionaries.Remove(darkShaleDictionary);
                Application.Current.Resources.MergedDictionaries[0].MergedDictionaries[2].Source = new Uri(@"/Mechanism.Wpf.Styles.Shale;component/Themes/Colors/BaseLight.xaml", UriKind.RelativeOrAbsolute);

                Settings.ShaleDarkTheme = false;
            }
            else
            {
                if (!Application.Current.Resources.MergedDictionaries.Contains(darkShaleDictionary))
                    Application.Current.Resources.MergedDictionaries.Add(darkShaleDictionary);
                Application.Current.Resources.MergedDictionaries[0].MergedDictionaries[2].Source = new Uri(@"/Mechanism.Wpf.Styles.Shale;component/Themes/Colors/BaseDark.xaml", UriKind.RelativeOrAbsolute);

                Settings.ShaleDarkTheme = true;
            }
        }

        private void StandardWindowDecorationsToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (StandardWindowDecorationsToggleSwitch.IsChecked.Value)
                Settings.UseCustomWindowDecorations = false;
            else
                Settings.UseCustomWindowDecorations = true;
        }

        private void InstalledModsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateActionButtonStates();
        }

        private void PrimaryTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateActionButtonStates();
        }

        ListView GetActiveModsListView()
        {
            ListView list = InstalledModsListView;
            if (InstalledModsListView.Visibility != Visibility.Visible)
                list = ModSearchListView;
            return list;
        }

        void UpdateActionButtonStates()
        {
            if (PrimaryTabControl.SelectedItem == ModsTabItem)
            {
                var list = GetActiveModsListView();
                if (list.SelectedItems.Count > 1)
                {
                    ConfigureModButton.IsEnabled = false;
                    bool areNoneProgressing = true;
                    IInstalledMod[] configurations = new IInstalledMod[list.SelectedItems.Count];
                    list.SelectedItems.CopyTo(configurations, 0);
                    foreach (IInstalledMod mod in configurations/*.Where(x => x is InstalledMod)*/)
                    {
                        if (mod.IsProgressing)
                        {
                            areNoneProgressing = false;
                            break;
                        }
                    }
                    UninstallModsButton.IsEnabled = areNoneProgressing;
                }
                else if (list.SelectedItems.Count == 1)
                {
                    if ((list.SelectedItem is IInstalledMod item) && (!item.IsProgressing))
                    {
                        UninstallModsButton.IsEnabled = true;

                        if ((list.SelectedItem is ManagedMod mod) && mod.HasConfigurator)
                            ConfigureModButton.IsEnabled = true;
                        else
                            ConfigureModButton.IsEnabled = false;
                    }
                    else
                    {
                        UninstallModsButton.IsEnabled = false;
                        ConfigureModButton.IsEnabled = false;
                    }
                }
                else
                {
                    UninstallModsButton.IsEnabled = false;
                    ConfigureModButton.IsEnabled = false;
                }
            }
            else
            {
                ConfigureModButton.IsEnabled = false;
                UninstallModsButton.IsEnabled = false;
            }
        }

        private void CreditsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CreditsListView.SelectedItem != null)
            {
                try
                {
                    Process.Start((CreditsListView.SelectedItem as CreditsItem).Link);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, "WEBPAGE NOT FOUND");
                }

                CreditsListView.SelectedItem = null;
            }
        }

        private void HelpThreadButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"http://davoonline.com/phpBB3/viewtopic.php?f=108&t=6300");
        }

        public void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var win = sender as Window;
            win.SizeChanged -= MainWindow_SizeChanged;
            win.Activated -= MainWindow_IsActiveChanged;
            win.Deactivated -= MainWindow_IsActiveChanged;

            foreach (var mod in ModsManager.InstalledMods)
            {
                if (mod.IsProgressing)
                {
                    e.Cancel = true;
                    break;
                }
            }
        }

        private void FolderNotFoundBrowseByHandButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void FolderNotFoundBrowseByHandActionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (Directory.Exists(FolderNotFoundBrowseByHandActionBox.Text))
                    FolderNotFoundConfirmButton.IsEnabled = true;
                else
                    FolderNotFoundConfirmButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                FolderNotFoundConfirmButton.IsEnabled = false;
            }
        }
        private void FolderNotFoundConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmForcedInstallPath(FolderNotFoundBrowseByHandActionBox.Text, true);

            ProceedToNextBadPath(true);
        }
        private void OverrideWindowModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (OverrideWindowModeCheckBox.IsChecked.Value)
                Settings.ForceGameWindowingMode = true;
            else
                Settings.ForceGameWindowingMode = false;

            if (FullscreenWindowModeRadioButton.IsChecked.Value)
                Settings.ForceWindowedMode = 1;
            else if (WindowedWindowModeRadioButton.IsChecked.Value)
                Settings.ForceWindowedMode = 0;
        }

        private void UpdateModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (UpdateAutomaticallyRadioButton.IsChecked.Value)
                Settings.UpdatingMode = Settings.UpdatingModeType.Automatic;
            else if (UpdateAutoCheckRadioButton.IsChecked.Value)
                Settings.UpdatingMode = Settings.UpdatingModeType.AutoCheck;
            else
                Settings.UpdatingMode = Settings.UpdatingModeType.Disabled;
        }

        private void OverrideGameResolutionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (OverrideGameResolutionCheckBox.IsChecked.Value)
            {
                Settings.ForceGameWindowBounds = true;

                CustomResolutionWidthTextBox.Text = Settings.ForcedGameWindowWidth.ToString();
                CustomResolutionHeightTextBox.Text = Settings.ForcedGameWindowHeight.ToString();
                if (CustomResolutionRadioButton.IsChecked.Value)
                {
                    Settings.AutoGameWindowBounds = false;
                    CustomResolutionWidthTextBox.IsEnabled = true;
                    CustomResolutionHeightTextBox.IsEnabled = true;
                }
                else
                {
                    Settings.AutoGameWindowBounds = true;
                    CustomResolutionWidthTextBox.IsEnabled = false;
                    CustomResolutionHeightTextBox.IsEnabled = false;
                }
            }
            else
            {
                Settings.ForceGameWindowBounds = false;

                CustomResolutionWidthTextBox.IsEnabled = false;
                CustomResolutionHeightTextBox.IsEnabled = false;
            }
        }

        Brush _noRedBrush
        {
            get => new Button().BorderBrush;
        }

        string _redBrushName = "Red";
        Brush _redBrush
        {
            get
            {
                if (Application.Current.Resources[_redBrushName] != null)
                    return (Brush)Application.Current.Resources[_redBrushName];
                else
                    return null;
            }
        }

        private void CustomResolutionWidthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CustomResolutionWidthTextBox.IsEnabled && int.TryParse(CustomResolutionWidthTextBox.Text, out int newSize))
            {
                Settings.ForcedGameWindowWidth = newSize;
                CustomResolutionWidthTextBox.BorderBrush = _noRedBrush;
            }
            else
                CustomResolutionWidthTextBox.BorderBrush = _redBrush;
        }

        private void CustomResolutionHeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CustomResolutionHeightTextBox.IsEnabled && int.TryParse(CustomResolutionHeightTextBox.Text, out int newSize))
            {
                Settings.ForcedGameWindowHeight = newSize;
                CustomResolutionHeightTextBox.BorderBrush = _noRedBrush;
            }
            else
                CustomResolutionHeightTextBox.BorderBrush = _redBrush;
        }

        /*private void LinuxTempModSelectorTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InstallSingleMod(LinuxTempModSelectorTextBox.Text);
                FoldersNotFoundContentControl.IsManipulationEnabled = false;
            }
        }

        private void LinuxTempSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            InstallSingleMod(LinuxTempModSelectorTextBox.Text);
            FoldersNotFoundContentControl.IsManipulationEnabled = false;
        }*/

        private void ShowConfigurationFileButton_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(Settings.ProgramDataPath, "ModManagerSettings.xml");
            if (File.Exists(path))
                SetClipboardTextForTechnicalDetails(File.ReadAllText(path));
        }

        private void SearchBox_ActionSubmitted(object sender, Mechanism.Wpf.Core.ActionSubmittedEventArgs e)
        {
            Search();
        }

        private void SearchBox_ActionCanceled(object sender, RoutedEventArgs e)
        {
            Search(false);
        }

        void Search()
        {
            Search(!(SearchBox.Text.IsNullOrEmptyOrWhiteSpace()));
        }

        void Search(bool start)
        {
            if (IsLoaded)
            {
                if (start)
                {
                    ModSearchListView.Visibility = Visibility.Visible;
                    InstalledModsListView.Visibility = Visibility.Collapsed;
                    //ModSearchListView.ItemsSource = ModSearch.SearchResults;
                    ModSearch.StartSearchAsync(SearchBox.Text, SearchNamesMenuItem.IsChecked, SearchDescriptionsMenuItem.IsChecked, SearchTagsMenuItem.IsChecked);
                }
                else
                {
                    ModSearchListView.Visibility = Visibility.Collapsed;
                    InstalledModsListView.Visibility = Visibility.Visible;
                    //ModSearchListView.ItemsSource = null;
                    ModSearch.CancelSearch();
                }
            }
        }

        private void SearchOptionMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void ModsListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateActionButtonStates();
        }

        private void CopyModsToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            SetClipboardTextForTechnicalDetails(ModsManager.GetModsListForClipboard());
        }

        void SetClipboardTextForTechnicalDetails(string details)
        {
            Clipboard.SetText("```\n[spoiler]\n" +
                details + "\n[/spoiler]\n```");
        }

        private void MenuToggleButton_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender is ToggleButton btn) && (btn.ContextMenu != null))
            {
                if (btn.IsChecked == true)
                {
                    if (btn.Tag is FrameworkElement el)
                    {
                        var pnt = el.PointToScreen(new Point(0, el.ActualHeight));
                        btn.ContextMenu.HorizontalOffset = pnt.X;
                        btn.ContextMenu.VerticalOffset = pnt.Y;
                    }
                    btn.ContextMenu.IsOpen = true;
                }
                else
                    btn.ContextMenu.IsOpen = false;
            }
        }

        private void SearchTargetsContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            SearchTargetsToggleButton.IsChecked = false;
        }

        private void ModsListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((sender is ListView list) && (e.Key == Key.A) && (e.KeyboardDevice.Modifiers == ModifierKeys.Shift))
            {
                list.SelectAll();
                e.Handled = true;
            }
        }

        private void DropModsDialogContentControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = DropModsDialogContentControl.IsVisible;

            if (isVisible)
            {
                UpdateDragWindow();
                CompositionTarget.Rendering += CompositionTarget_Rendering;
                //ShowWindow(App.DragServantProcess.MainWindowHandle, 5);
            }
            else
            {
                UpdateDragWindow();
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                //ShowWindow(App.DragServantProcess.MainWindowHandle, 0);
            }

            _dragWatcher.EnableRaisingEvents = isVisible;
        }

        string _lastDLL = string.Empty;
        private void AddSideloadCoreDllsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = Settings.GetLanguageString("SelectDlls"),
                Filter = Settings.GetLanguageString("DllBundlesFilter").Replace("%EXTENSIONS%", "*.modapic, *.dll, *.lib") + "|*.modapic;*.dll;*.lib"
            };

            try
            {
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (Directory.Exists(downloadsPath))
                    dialog.InitialDirectory = downloadsPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "OpenFileDialog error");
            }

            if (dialog.ShowDialog() == true)
            {
                if (dialog.FileNames.Length > 0)
                {
                    if (dialog.FileNames[0].EndsWith(".modapic", StringComparison.OrdinalIgnoreCase))
                        Core.Injection.CoreDllRetriever.InstallOverrideDlls(dialog.FileNames[0]);
                    else if (dialog.FileNames[0].EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        _lastDLL = dialog.FileNames[0];
                        InstallDLLWhereContentControl.IsOpen = true;
                    }
                    else if (dialog.FileNames[0].EndsWith(".lib", StringComparison.OrdinalIgnoreCase))
                        Core.Injection.CoreDllRetriever.InstallOverrideDll(dialog.FileNames[0], GameInfo.GameExecutableType.None);
                }
            }
        }

        private void RemoveSideloadCoreDllsButton_Click(object sender, RoutedEventArgs e)
        {
            Core.Injection.CoreDllRetriever.PurgeOverrideDlls();
        }

        private void InstallModAPIDllForMarch2017Button_Click(object sender, RoutedEventArgs e)
        {
            InstallDLLWhereContentControl.IsOpen = false;
            Core.Injection.CoreDllRetriever.InstallOverrideDll(_lastDLL, GameInfo.GameExecutableType.GogOrSteam__March2017);
        }

        private void InstallModAPIDllForDisk1_5_1Button_Click(object sender, RoutedEventArgs e)
        {
            InstallDLLWhereContentControl.IsOpen = false;
            Core.Injection.CoreDllRetriever.InstallOverrideDll(_lastDLL, GameInfo.GameExecutableType.Disk__1_5_1);
        }

        private void ForceKillSporeButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Settings.GetLanguageString("ForceKillConfirmDesc"), Settings.GetLanguageString("ForceKillConfirmTitle"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)//(bool)(MessageBox<ForcecloseCancelButtons>.Show(Settings.GetLanguageString("ForceKillConfirmDesc"), Settings.GetLanguageString("ForceKillConfirmTitle"))))
                SporeLauncher.KillSporeProcesses();
        }
    }

    /*public class ForcecloseCancelButtons : IMessageBoxActionSet
    {
        public IEnumerable<object> Actions => new List<object>
        {
            true,
            false
        };

        public string GetDisplayName(object value)
        {
            if ((bool)value)
                return Settings.GetLanguageString("ForceKillSpore");
            else
                return Settings.GetLanguageString("DontForceKillSpore");
        }
    }*/
}
