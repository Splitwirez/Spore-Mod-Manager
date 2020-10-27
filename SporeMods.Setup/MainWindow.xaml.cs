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

namespace SporeMods.Setup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static bool debug = Environment.GetCommandLineArgs().Skip(1).Any(x => x.ToLower() == "--debug");

        static string DEFAULT_INSTALL_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Spore Mod Manager");
        string _installPath = DEFAULT_INSTALL_PATH;
        static string DEFAULT_STORAGE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SporeModManagerStorage");
        string _storagePath = DEFAULT_STORAGE_PATH;

        public MainWindow()
        {
            InitializeComponent();

            if (Permissions.IsAdministrator())
            {
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

                IEnumerable<string> args = Environment.GetCommandLineArgs()/*.Skip(1)*/;
                
                DebugMessageBox("_storagePath: " + _storagePath);

                if (Environment.GetCommandLineArgs().Length > 1)
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

            if (Directory.Exists(text))
            {
                _installPath = text;
                SelectInstallPathBadPathBorder.BorderThickness = new Thickness(0);
                SelectInstallPathNextButton.IsEnabled = true;

            }
            else
            {
                SelectInstallPathBadPathBorder.BorderThickness = new Thickness(1);
                SelectInstallPathNextButton.IsEnabled = false;
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

            if (Directory.Exists(text))
            {
                _storagePath = text;
                SelectStoragePathBadPathBorder.BorderThickness = new Thickness(0);
                SelectStoragePathNextButton.IsEnabled = true;

            }
            else
            {
                SelectStoragePathBadPathBorder.BorderThickness = new Thickness(1);
                SelectStoragePathNextButton.IsEnabled = false;
            }
        }

        private void SelectInstallPathNextButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(SelectStoragePathPage);
            SelectStoragePathTextBox.Text = DEFAULT_STORAGE_PATH;
            UpdateSelectStoragePathText();
        }

        private void SelectStoragePathNextButton_Click(object sender, RoutedEventArgs e)
        {
            InstallSporeModManager();
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Permissions.IsAdministrator())
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
                    File.WriteAllText(SetupInfo.INSTALL_DIR_LOCATOR_PATH, _installPath);
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

                    Hide();
                    Assembly.LoadFrom(Path.Combine(_installPath, "Microsoft.Threading.Tasks.dll"));
                    Assembly.LoadFrom(Path.Combine(_installPath, "Microsoft.Threading.Tasks.Extensions.dll"));
                    Assembly.LoadFrom(Path.Combine(_installPath, "Microsoft.Threading.Tasks.Extensions.Desktop.dll"));
                    var importer = Assembly.LoadFrom(Path.Combine(_installPath, "SporeMods.KitImporter.exe"));
                    Window importerWindow = (Window)Activator.CreateInstance(importer.GetType("SporeMods.KitImporter.MainWindow"));
                    importerWindow.ShowDialog();
                }
            }
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

                    Dispatcher.BeginInvoke(new Action(() => InstallProgressBar.Maximum = resources.Count()));

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
                    Dispatcher.BeginInvoke(new Action(() =>
                        {
                            InstallProgressBar.Value++;
                        }));
                    }
                    Permissions.GrantAccessDirectory(_installPath);

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
    }
}
