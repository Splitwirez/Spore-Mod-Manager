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

        string _installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Spore Mod Manager");
        static string DEFAULT_STORAGE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SporeModManagerStorage");
        string _storagePath = DEFAULT_STORAGE_PATH;
        public MainWindow()
        {
            InitializeComponent();

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
                SetPage(InstallModePage);
        }

        private void SimpleInstallButton_Click(object sender, RoutedEventArgs e)
        {
            InstallSporeModManager();
        }

        private void AdvancedInstallButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(SelectInstallPathPage);
        }

        bool InstallSporeModManager()
        {
            SetPage(InstallProgressPage);
            string[] resources = Application.ResourceAssembly.GetManifestResourceNames();
            InstallProgressBar.Maximum = resources.Length;
            Thread thread = new Thread(() =>
            {
                string rresources = string.Empty;
                foreach (string s in resources)
                {
                    rresources += s + "\n";
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        InstallProgressBar.Value++;
                    }));
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(rresources);
                }));

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
