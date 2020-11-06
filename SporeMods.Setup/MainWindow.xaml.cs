using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using System.Runtime.InteropServices.ComTypes;

namespace SporeMods.Setup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string usersDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)).Parent.ToString().ToLowerInvariant();
        static bool debug = Environment.GetCommandLineArgs().Skip(1).Any(x => x.ToLower() == "--debug");
        static bool isUpdatingModManager = false;
        static bool isUpdatingFromLauncherKit = false;

        static string DEFAULT_INSTALL_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Spore Mod Manager");
        string _installPath = DEFAULT_INSTALL_PATH;
        static string DEFAULT_STORAGE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SporeModManagerStorage");
        string _storagePath = DEFAULT_STORAGE_PATH;
        string _autoStoragePath = DEFAULT_STORAGE_PATH;

        public MainWindow()
        {
            InitializeComponent();

            if (Permissions.IsAdministrator())
            {
                IEnumerable<string> args = Environment.GetCommandLineArgs()/*.Skip(1)*/;
                
                if (args.Count() >= 5)
                {
                    if (args.ElementAt(1).Contains("--update"))
                    {
                        isUpdatingModManager = true;
                        Height /= 2;

                        string mgrPath = args.ElementAt(2).Trim('"', ' ');
                        if (Directory.Exists(mgrPath))
                            _installPath = mgrPath;

                        string mgrExePath = args.ElementAt(3).Trim('"', ' ');
                        if (File.Exists(mgrExePath))
                            App.MgrExePath = mgrExePath;

                        string langArg = args.ElementAt(4).Trim('"', ' ');
                        if (langArg.Contains("--lang:"))
                            App.Language = langArg.Substring(langArg.IndexOf("--lang:") + 7, 5);
                    }
                }


                var fixedDrives = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed);

                DriveInfo mostFreeSpace = fixedDrives.FirstOrDefault();
                foreach (DriveInfo d in fixedDrives)
                {
                    if (d.AvailableFreeSpace > mostFreeSpace.AvailableFreeSpace)
                        mostFreeSpace = d;
                }
                //DebugMessageBox(mostFreeSpace.RootDirectory.FullName);
                if (!DEFAULT_STORAGE_PATH.ToLowerInvariant().StartsWith(mostFreeSpace.RootDirectory.FullName.ToLowerInvariant()))
                {
                    _storagePath = Path.Combine(mostFreeSpace.RootDirectory.FullName, "SporeModManagerStorage");
                }
                _autoStoragePath = _storagePath;

                
                DebugMessageBox("_storagePath: " + _storagePath);
                isUpdatingFromLauncherKit = (App.LkPath != null) && (args.Count() > 1);

                if (App.MgrExePath != null)
                    InstallSporeModManager();
                else if (isUpdatingFromLauncherKit)
                    SetPage(WelcomeToUpgradePathPage);
                else
                    SetPage(LicensePage);
            }
            else
                Close();
        }

        void SetPage(int index)
        {
            if ((index >= 0) && (index < PagesGrid.Children.Count))
            {
                for (int i = 0; i < PagesGrid.Children.Count; i++)
                {
                    if (i == index)
                        PagesGrid.Children[i].Visibility = Visibility.Visible;
                    else
                        PagesGrid.Children[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        void SetPage(UIElement el)
        {
            if (PagesGrid.Children.Contains(el))
            {
                foreach (UIElement e in PagesGrid.Children)
                {
                    if (e == el)
                        e.Visibility = Visibility.Visible;
                    else
                        e.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void LicenseNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (PleaseReadTheTermsBeforeCheckingThis.IsChecked == true)
            {
                SetPage(InstallModePage);
                //InstallSporeModManager();
            }
        }

        private void SimpleInstallButton_Click(object sender, RoutedEventArgs e)
        {
            InstallSporeModManager();
        }

        private void AdvancedInstallButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(SelectInstallPathPage);
            SelectInstallPathTextBox.Text = DEFAULT_INSTALL_PATH;
            UpdateSelectInstallPathText();
        }

        private void SelectInstallPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSelectInstallPathText();
        }

        void UpdateSelectInstallPathText()
        {
            string text = Environment.ExpandEnvironmentVariables(SelectInstallPathTextBox.Text);
            if (!text.EndsWith("Spore Mod Manager"))
            {
                text = Path.Combine(text, "Spore Mod Manager");
            }

            bool dirDoesntExistOrIsExistingModMgr = !Directory.Exists(text);

            if (!dirDoesntExistOrIsExistingModMgr)
                dirDoesntExistOrIsExistingModMgr = File.Exists(Path.Combine(text, "Spore Mod Manager.exe"));

            if (dirDoesntExistOrIsExistingModMgr && (!text.ToLowerInvariant().StartsWith(usersDir)))
            {
                _installPath = text;
                SelectInstallPathBadPathBorder.BorderThickness = new Thickness(0);
                SelectInstallPathNextButton.IsEnabled = true;
                SelectInstallPathError.Text = string.Empty;
            }
            else
            {
                SelectInstallPathBadPathBorder.BorderThickness = new Thickness(1);
                SelectInstallPathNextButton.IsEnabled = false;
                if (text.ToLowerInvariant().StartsWith(usersDir))
                    SelectInstallPathError.SetResourceReference(TextBlock.TextProperty, "CannotInstallToUserSpecificLocation"); //"Cannot install the Spore Mod Manager to a user-specific location.";
                else
                    SelectInstallPathError.SetResourceReference(TextBlock.TextProperty, "CannotInstallToExistingFolder"); //.Text = "Cannot install the Spore Mod Manager to a pre-existing folder.";
            }
        }

        private void SelectStoragePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSelectStoragePathText();
        }

        void UpdateSelectStoragePathText()
        {
            string text = Environment.ExpandEnvironmentVariables(SelectStoragePathTextBox.Text);
            if (!text.EndsWith("SporeModManagerStorage"))
            {
                text = Path.Combine(text, "SporeModManagerStorage");
            }

            bool storageDoesntExistOrIsAlreadyMgrStoragePath = !Directory.Exists(text);
            if (!storageDoesntExistOrIsAlreadyMgrStoragePath)
                storageDoesntExistOrIsAlreadyMgrStoragePath = File.Exists(Path.Combine(text, "ModManagerSettings.xml"));

            if (storageDoesntExistOrIsAlreadyMgrStoragePath && (!text.ToLowerInvariant().StartsWith(usersDir)))
            {
                _storagePath = text;
                SelectStoragePathBadPathBorder.BorderThickness = new Thickness(0);
                SelectStoragePathNextButton.IsEnabled = true;
                SelectStoragePathError.Text = string.Empty;
            }
            else
            {
                SelectStoragePathBadPathBorder.BorderThickness = new Thickness(1);
                SelectStoragePathNextButton.IsEnabled = false;
                if (text.ToLowerInvariant().StartsWith(usersDir))
                    SelectStoragePathError.SetResourceReference(TextBlock.TextProperty, "CannotStoreConfigInUserSpecificLocation"); //.Text = "The Spore Mod Manager cannot store additional information in a user-specific location.";
                else
                    SelectStoragePathError.SetResourceReference(TextBlock.TextProperty, "CannotStoreConfigInExistingFolder"); //.Text = "The Spore Mod Manager cannot store additional information in a pre-existing folder.";
            }
        }

        private void SelectInstallPathNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_installPath.EndsWith("Spore Mod Manager"))
                _installPath = Path.Combine(_installPath, "Spore Mod Manager");
            SetPage(SelectStoragePathPage);
            SelectStoragePathTextBox.Text = DEFAULT_STORAGE_PATH;
            UpdateSelectStoragePathText();
        }

        private void SelectStoragePathNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_storagePath.EndsWith("SporeModManagerStorage"))
                _storagePath = Path.Combine(_storagePath, "SporeModManagerStorage");
            InstallSporeModManager();
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (InstallProgressPage.IsVisible)
                e.Cancel = true;
            else if (InstallCompletedPage.IsVisible)
            {
                /*if (_lkPath != null)
                {
                    File.WriteAllLines(SetupInfo.INSTALL_DIR_LOCATOR_PATH, new string[]
                        {
                            _installPath,
                            _lkPath
                        });
                }
                else
                {*/
                if (!Directory.Exists(DEFAULT_STORAGE_PATH))
                    Directory.CreateDirectory(DEFAULT_STORAGE_PATH);

                Permissions.GrantAccessDirectory(DEFAULT_STORAGE_PATH);

                if (File.Exists(SetupInfo.INSTALL_DIR_LOCATOR_PATH))
                    Permissions.GrantAccessFile(SetupInfo.INSTALL_DIR_LOCATOR_PATH);

                File.WriteAllText(SetupInfo.INSTALL_DIR_LOCATOR_PATH, _installPath);

                Permissions.GrantAccessFile(SetupInfo.INSTALL_DIR_LOCATOR_PATH);


                if (isUpdatingModManager && (App.MgrExePath != null))
                {
                    if (File.Exists(SetupInfo.LAST_EXE_PATH))
                        Permissions.GrantAccessFile(SetupInfo.LAST_EXE_PATH);

                    File.WriteAllText(SetupInfo.LAST_EXE_PATH, App.MgrExePath);

                    Permissions.GrantAccessFile(SetupInfo.LAST_EXE_PATH);
                }
                /*}
                Permissions.GrantAccessFile(SetupInfo.INSTALL_DIR_LOCATOR_PATH);

                if (_lkPath != null)
                {
                    DebugMessageBox("START IMPORTER");
                    Application.Current.Shutdown(SetupInfo.EXIT_RUN_LK_IMPORTER);
                }
                else
                {
                    DebugMessageBox("START MOD MANAGER DIRECTLY");
                    Application.Current.Shutdown(SetupInfo.EXIT_RUN_MOD_MGR);
                }*/

                if (false) //dodging this UAC prompt seems to be a bridge too far
                {
                    Hide();
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                    //Assembly.LoadFrom(Path.Combine(_installPath, "Microsoft.Threading.Tasks.dll"));
                    //Assembly.LoadFrom(Path.Combine(_installPath, "Microsoft.Threading.Tasks.Extensions.dll"));
                    //Assembly.LoadFrom(Path.Combine(_installPath, "Microsoft.Threading.Tasks.Extensions.Desktop.dll"));

                    var importer = Assembly.LoadFrom(Path.Combine(_installPath, "SporeMods.KitImporter.exe"));
                    Window importerWindow = (Window)Activator.CreateInstance(importer.GetType("SporeMods.KitImporter.MainWindow"));
                    importerWindow.ShowDialog();
                }

                if ((!isUpdatingModManager) && isUpdatingFromLauncherKit)
                {
                    var importerPath = Path.Combine(_installPath, "SporeMods.KitImporter.exe");

                    try
                    {
                        Process process = Process.Start(importerPath);
                        process.WaitForExit();
                    }
                    catch (Win32Exception w32ex)
                    {
                        string forceLkImportPath = Path.Combine(_storagePath, "ForceLkImport.info");
                        File.WriteAllText(forceLkImportPath, string.Empty);
                        Permissions.GrantAccessFile(forceLkImportPath);
                        DebugMessageBox(forceLkImportPath, "forceLkImportPath");
                    }
                }
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string path = Path.Combine(_installPath, args.Name);

            if (File.Exists(path))
                return Assembly.LoadFrom(path);
            else
                return null;
        }

        void InstallSporeModManager()
        {
            try
            {
                SetPage(InstallProgressPage);
                DebugMessageBox("INSTALL PATH: " + _installPath + "\nSTORAGE PATH: " + _storagePath);
                Thread thread = new Thread(() =>
                {
                    if (!Directory.Exists(_installPath))
                        Directory.CreateDirectory(_installPath);

                //string rresources = string.Empty;
                IEnumerable<string> resources = Application.ResourceAssembly.GetManifestResourceNames().Where(x => !x.ToLowerInvariant().EndsWith(".resources"));

                    Dispatcher.BeginInvoke(new Action(() => InstallProgressBar.Maximum = resources.Count() + 4));

                    foreach (string s in resources)
                    {
                        string fileOutPath = Path.Combine(_installPath, s.Replace("SporeMods.Setup.", string.Empty));
                        using (var resource = Application.ResourceAssembly.GetManifestResourceStream(s))
                        {
                            using (var file = new FileStream(fileOutPath, FileMode.Create, FileAccess.Write))
                            {
                                resource.CopyTo(file);
                            }
                        }

                        Permissions.GrantAccessFile(fileOutPath);

                        //rresources += s + "\n";
                        Dispatcher.BeginInvoke(new Action(() => InstallProgressBar.Value++));
                    }
                    Permissions.GrantAccessDirectory(_installPath);

                    if (App.MgrExePath == null)
                    {
                        if (_storagePath != DEFAULT_STORAGE_PATH)
                        {
                            if (!Directory.Exists(DEFAULT_STORAGE_PATH))
                                Directory.CreateDirectory(DEFAULT_STORAGE_PATH);

                            string redirect = Path.Combine(DEFAULT_STORAGE_PATH, "redirectStorage.txt");
                            File.WriteAllText(redirect, _storagePath);

                            Permissions.GrantAccessFile(redirect);
                        }
                        Permissions.GrantAccessDirectory(_storagePath);
                        /*Dispatcher.BeginInvoke(new Action(() =>
                        {
                            DebugMessageBox(rresources);
                        }));*/
                        string menuShortcutDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Spore Mod Manager");
                        if (!Directory.Exists(menuShortcutDir))
                            Directory.CreateDirectory(menuShortcutDir);


                        string mgrPath = Path.Combine(_installPath, "Spore Mod Manager.exe");
                        string launcherPath = Path.Combine(_installPath, "Spore Mod Launcher.exe");


                        Shortcut.IShellLinkW mgrMenuShortcut = Shortcut.GetShortcut();
                        mgrMenuShortcut.SetPath(mgrPath);
                        string mgrMenuOutPath = Path.Combine(menuShortcutDir, "Spore Mod Manager.lnk");
                        if (File.Exists(mgrMenuOutPath))
                            File.Delete(mgrMenuOutPath);
                        ((IPersistFile)mgrMenuShortcut).Save(mgrMenuOutPath, false);
                        Permissions.GrantAccessFile(mgrMenuOutPath);
                        Dispatcher.BeginInvoke(new Action(() => InstallProgressBar.Value++));

                        Shortcut.IShellLinkW launcherMenuShortcut = Shortcut.GetShortcut();
                        launcherMenuShortcut.SetPath(launcherPath);
                        string launcherMenuOutPath = Path.Combine(menuShortcutDir, "Launch Spore.lnk");
                        if (File.Exists(launcherMenuOutPath))
                            File.Delete(launcherMenuOutPath);
                        ((IPersistFile)launcherMenuShortcut).Save(launcherMenuOutPath, false);
                        Permissions.GrantAccessFile(launcherMenuOutPath);
                        Dispatcher.BeginInvoke(new Action(() => InstallProgressBar.Value++));


                        Shortcut.IShellLinkW mgrDesktopShortcut = Shortcut.GetShortcut();
                        mgrDesktopShortcut.SetPath(mgrPath);
                        string mgrDesktopOutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Spore Mod Manager.lnk");
                        if (File.Exists(mgrDesktopOutPath))
                            File.Delete(mgrDesktopOutPath);
                        ((IPersistFile)mgrDesktopShortcut).Save(mgrDesktopOutPath, false);
                        Permissions.GrantAccessFile(mgrDesktopOutPath);
                        Dispatcher.BeginInvoke(new Action(() => InstallProgressBar.Value++));

                        Shortcut.IShellLinkW launcherDesktopShortcut = Shortcut.GetShortcut();
                        launcherDesktopShortcut.SetPath(launcherPath);
                        string launcherDesktopOutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Launch Spore.lnk");
                        if (File.Exists(launcherDesktopOutPath))
                            File.Delete(launcherDesktopOutPath);
                        ((IPersistFile)launcherDesktopShortcut).Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Launch Spore.lnk"), false);
                        Permissions.GrantAccessFile(launcherDesktopOutPath);
                        Dispatcher.BeginInvoke(new Action(() => InstallProgressBar.Value++));

                        Permissions.GrantAccessDirectory(menuShortcutDir);

                        //purge Launcher Kit shortcuts
                        try
                        {
                            string[] shortcutFilePaths = Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)).ToArray();
                            foreach (string s in shortcutFilePaths)
                            {
                                try
                                {
                                    string shortcutName = Path.GetFileName(s).ToLowerInvariant();
                                    if (shortcutName.Contains("spore modapi") &&
                                    (
                                    shortcutName.EndsWith(".lnk") ||
                                    shortcutName.EndsWith(".bat")
                                    ))
                                    {
                                        if (File.Exists(s))
                                            File.Delete(s);
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }


                    Dispatcher.BeginInvoke(new Action(() => SetPage(InstallCompletedPage)));
                });
                thread.Start();

            }
            catch (Exception ex)
            {

            }
        }

        public void DebugMessageBox(string text)
        {
            if (debug)
                MessageBox.Show(text);
        }

        private void UpgradeOkButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(LicensePage);
        }

        private void SuccessCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelectInstallPathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                string path = dialog.SelectedPath;
                if (!path.EndsWith("Spore Mod Manager"))
                    path = Path.Combine(path, "Spore Mod Manager");
                SelectInstallPathTextBox.Text = path;
            }
        }

        private void SelectStoragePathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                string path = dialog.SelectedPath;
                if (!path.EndsWith("SporeModManagerStorage"))
                    path = Path.Combine(path, "SporeModManagerStorage");
                SelectStoragePathTextBox.Text = path;
            }
        }

        private void SelectInstallPathDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            SelectInstallPathTextBox.Text = DEFAULT_INSTALL_PATH;
            _storagePath = DEFAULT_INSTALL_PATH;
        }

        private void SelectStoragePathDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            SelectStoragePathTextBox.Text = _autoStoragePath;
            _storagePath = _autoStoragePath;
        }
    }
}
