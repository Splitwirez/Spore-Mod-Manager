using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SporeMods.KitImporter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string _kitPath = null;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> args = Environment.GetCommandLineArgs().Skip(1);
            _kitPath = args.FirstOrDefault(x => IsPathValid(x));
            string lkPathFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Spore ModAPI Launcher\path.info");
            if ((!string.IsNullOrEmpty(_kitPath)) && (!string.IsNullOrWhiteSpace(_kitPath)))
            {
                AutoLauncherKitPathTextBlock.Text = _kitPath;
                VerifyAutoLauncherKitPathPage.Visibility = Visibility.Visible;
            }
            else
            {
                try
                {
                    string lkPath2 = null;
                    if (File.Exists(lkPathFilePath))
                        lkPath2 = File.ReadAllText(lkPathFilePath);

                    if ((!string.IsNullOrEmpty(lkPath2)) && (!string.IsNullOrWhiteSpace(lkPath2)) && IsPathValid(lkPath2))
                    {
                        _kitPath = lkPath2;
                        AutoLauncherKitPathTextBlock.Text = _kitPath;
                        VerifyAutoLauncherKitPathPage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Debug.WriteLine("1: " + lkPath2);
                        SpecifyLauncherKitPathInstructionTextBlock.Text = "Please specify the location of the Spore ModAPI Launcher Kit below.";
                        SpecifyLauncherKitPathPage.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("2 " + ex.ToString());
                    SpecifyLauncherKitPathInstructionTextBlock.Text = "Please specify the location of the Spore ModAPI Launcher Kit below.";
                    SpecifyLauncherKitPathPage.Visibility = Visibility.Visible;
                }
            }
        }

        public async Task Import(string path)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                VerifyAutoLauncherKitPathPage.Visibility = Visibility.Collapsed;
                SpecifyLauncherKitPathPage.Visibility = Visibility.Collapsed;
                ImportInProgressPage.Visibility = Visibility.Visible;
            }));

            Task task = new Task(() =>
            {
                LauncherKitImporter.Import(path);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ImportInProgressPage.Visibility = Visibility.Collapsed;
                    ImportCompletePage.Visibility = Visibility.Visible;
                }));
            });
            task.Start();
            await task;
        }

        private async void ProceedWithAutoPathButton_Click(object sender, RoutedEventArgs e)
        {
            await Import(_kitPath);
        }

        private void DiscardAutoPathButton_Click(object sender, RoutedEventArgs e)
        {
            VerifyAutoLauncherKitPathPage.Visibility = Visibility.Collapsed;
            SpecifyLauncherKitPathPage.Visibility = Visibility.Visible;
        }

        private async void ProceedWithSpecifiedPathButton_Click(object sender, RoutedEventArgs e)
        {
            await Import(_kitPath);
        }

        private void LauncherKitPathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                _kitPath = dialog.SelectedPath;
                ProceedWithSpecifiedPathButton.IsEnabled = IsPathValid(_kitPath);
            }
        }

        bool IsPathValid(string lkPath)
        {
            string path = lkPath.Trim('"', ' ');
            if (!Directory.Exists(path))
                return false;

            return File.Exists(Path.Combine(path, "Spore ModAPI Launcher.exe")) &&
                    File.Exists(Path.Combine(path, "Spore ModAPI Easy Installer.exe")) &&
                    File.Exists(Path.Combine(path, "Spore ModAPI Easy Uninstaller.exe")) &&
                    (!File.Exists(Path.Combine(path, "Spore Mod Manager.exe")));
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown(300);
        }
    }
}
