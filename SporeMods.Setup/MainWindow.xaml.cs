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

namespace SporeMods.Setup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _lkPath = null;

        static string DEFAULT_INSTALL_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Spore Mod Manager");
        string _installPath = DEFAULT_INSTALL_PATH;
        static string DEFAULT_STORAGE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SporeModManagerStorage");
        string _storagePath = DEFAULT_STORAGE_PATH;

        public MainWindow()
        {
            InitializeComponent();
            
            var fixedDrives = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed);
            
            DriveInfo mostFreeSpace = fixedDrives.FirstOrDefault();
            foreach (DriveInfo d in fixedDrives)
            {
                if (d.AvailableFreeSpace > mostFreeSpace.AvailableFreeSpace)
                    mostFreeSpace = d;
            }
            //MessageBox.Show(mostFreeSpace.RootDirectory.FullName);
            if (!DEFAULT_STORAGE_PATH.ToLowerInvariant().StartsWith(mostFreeSpace.RootDirectory.FullName.ToLowerInvariant()))
            {
                _storagePath = Path.Combine(mostFreeSpace.RootDirectory.FullName, "SporeModManagerStorage");
            }

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                string lkPath = args[1];
                if (Uri.IsWellFormedUriString(lkPath, UriKind.RelativeOrAbsolute) && Directory.Exists(lkPath))
                {
                    _lkPath = lkPath;
                }
            }

            SetPage(LicensePage);
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
        }

        private void SelectInstallPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = Environment.ExpandEnvironmentVariables(SelectInstallPathTextBox.Text);
            if (!text.EndsWith("Spore Mod Manager"))
            {
                text = Path.Combine(text, "Spore Mod Manager");
            }

            if (Uri.IsWellFormedUriString(text, UriKind.Absolute) && !Directory.Exists(text))
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
            string text = Environment.ExpandEnvironmentVariables(SelectStoragePathTextBox.Text);
            if (!text.EndsWith("SporeModManagerStorage"))
            {
                text = Path.Combine(text, "SporeModManagerStorage");
            }

            if (Uri.IsWellFormedUriString(text, UriKind.Absolute) && !Directory.Exists(text))
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
        }

        private void SelectStoragePathNextButton_Click(object sender, RoutedEventArgs e)
        {
            InstallSporeModManager();
        }


        bool InstallSporeModManager()
        {
            SetPage(InstallProgressPage);
            MessageBox.Show("INSTALL PATH: " + _installPath + "\nSTORAGE PATH: " + _storagePath);
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
                    MessageBox.Show(rresources);
                }));*/

                bool runImporter = false;
                if ((_lkPath != null) && Uri.IsWellFormedUriString(_lkPath, UriKind.RelativeOrAbsolute) && Directory.Exists(_lkPath))
                {
                    if (
                            File.Exists(Path.Combine(_lkPath, "Spore ModAPI Launcher.exe")) &&
                            File.Exists(Path.Combine(_lkPath, "Spore ModAPI Easy Installer.exe")) &&
                            File.Exists(Path.Combine(_lkPath, "Spore ModAPI Easy Uninstaller.exe")) &&
                            (!File.Exists(Path.Combine(_lkPath, "Spore Mod Manager.exe")))
                        )
                    {
                        runImporter = true;
                    }
                }

                if (runImporter)
                    Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("START IMPORTER", "The End")));
                else
                    Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("START MOD MANAGER DIRECTLY", "The End")));
            });
            thread.Start();

            return true;
        }
    }
}
