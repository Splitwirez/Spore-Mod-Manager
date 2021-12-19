using SporeMods.Core;
using System;
using System.Collections.Generic;
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
		static Func<string, string> GetLocalizedString = SporeMods.CommonUI.Localization.LanguageManager.Instance.GetLocalizedText;

		public MainWindow()
		{
			InitializeComponent();
		}

		string _kitPath = null;
		bool _kitAutoImport = false;
		bool _mandatoryImport = false;

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			SetLanguage();
			IEnumerable<string> args = Environment.GetCommandLineArgs().Skip(1);
			_kitPath = args.FirstOrDefault(x => IsPathValid(x));
			string lkPathFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Spore ModAPI Launcher\path.info");

			if (Environment.GetCommandLineArgs().Contains("--mandate"))
			{
				_mandatoryImport = true;
			}


			if ((!string.IsNullOrEmpty(_kitPath)) && (!string.IsNullOrWhiteSpace(_kitPath)))
			{
				_kitAutoImport = true;
				AutoLauncherKitPathTextBlock.Text = GetLocalizedString("KitImporter!AutoLauncherKitPath").Replace("%KITPATH%", _kitPath);
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
						AutoLauncherKitPathTextBlock.Text = GetLocalizedString("KitImporter!AutoLauncherKitPath").Replace("%KITPATH%", _kitPath);
						VerifyAutoLauncherKitPathPage.Visibility = Visibility.Visible;
					}
					else
					{
						Cmd.WriteLine("1: " + lkPath2);
						SpecifyLauncherKitPathInstructionTextBlock.Text = GetLocalizedString("KitImporter!SpecifyLauncherKitPathInstruction"); //"Please specify the location of the Spore ModAPI Launcher Kit below.";
						SpecifyLauncherKitPathPage.Visibility = Visibility.Visible;
					}
				}
				catch (Exception ex)
				{
					Cmd.WriteLine("2 " + ex.ToString());
					SpecifyLauncherKitPathInstructionTextBlock.Text = GetLocalizedString("SpecifyLauncherKitPathInstruction"); //"Please specify the location of the Spore ModAPI Launcher Kit below.";
					SpecifyLauncherKitPathPage.Visibility = Visibility.Visible;
				}
			}
		}

		public void Import(string path)
		{
			VerifyAutoLauncherKitPathPage.Visibility = Visibility.Collapsed;
			SpecifyLauncherKitPathPage.Visibility = Visibility.Collapsed;
			ImportInProgressPage.Visibility = Visibility.Visible;

			Thread thread = new Thread(() =>
			{
				ImportResult result = LauncherKitImporter.Import(path);
				Dispatcher.BeginInvoke(new Action(() =>
				{
					bool success = false;
					if (
							result.HasInstalledModsRecord &&
							(result.SettingsImportFailedReason == null) ||
							(result.FailedMods.Count <= 0)
						)//(result.SkippedMods.Count > 0)
						success = true;

					if (!success)
						ImportCompleteTextBlock.Text = GetLocalizedString("KitImporter!ImportFailed");

					if (result.FailedMods.Count > 0)
					{
						FailedModsItemsControl.ItemsSource = result.FailedMods;
						FailedModsGroupBox.Visibility = Visibility.Visible;
					}

					if (result.SettingsImportFailedReason != null)
					{
						SettingsImportFailedStackPanel.Visibility = Visibility.Visible;
						SettingsImportFailedReasonTextBlock.Text = result.SettingsImportFailedReason.ToString();
					}

					if (!result.HasInstalledModsRecord)
					{
						NoModsRecordTextBlock.Visibility = Visibility.Visible;
					}

					if (result.SkippedMods.Count > 0)
					{
						SkippedModsItemsControl.ItemsSource = result.SkippedMods;
						SkippedModsGroupBox.Visibility = Visibility.Visible;
					}

					ImportInProgressPage.Visibility = Visibility.Collapsed;
					ImportCompletePage.Visibility = Visibility.Visible;
				}));
			});
			thread.Start();
		}

		private void ProceedWithAutoPathButton_Click(object sender, RoutedEventArgs e)
		{
			Import(_kitPath);
		}

		private void DiscardAutoPathButton_Click(object sender, RoutedEventArgs e)
		{
			VerifyAutoLauncherKitPathPage.Visibility = Visibility.Collapsed;
			SpecifyLauncherKitPathPage.Visibility = Visibility.Visible;
		}

		private void ProceedWithSpecifiedPathButton_Click(object sender, RoutedEventArgs e)
		{
			Import(_kitPath);
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
			if (_mandatoryImport && (ImportCompletePage.Visibility != Visibility.Visible))
				e.Cancel = true;
			if (_kitAutoImport && (ImportCompletePage.Visibility != Visibility.Visible))
				e.Cancel = true;
			else if (ImportInProgressPage.Visibility == Visibility.Visible)
				e.Cancel = true;
			else
				System.Windows.Application.Current.Shutdown(300);
		}

		void SetLanguage()
		{
			ProceedWithAutoPathButton.Content = GetLocalizedString("KitImporter!ProceedWithAutoPath");
			DiscardAutoPathButton.Content = GetLocalizedString("KitImporter!DiscardAutoPath");

			SpecifyLauncherKitPathInstructionTextBlock.Text = GetLocalizedString("KitImporter!LauncherKitNotFoundSpecifyLauncherKitPathInstruction");
			LauncherKitPathBrowseButton.Content = GetLocalizedString("Browse");

			ImportInProgressTextBlock.Text = GetLocalizedString("KitImporter!ImportInProgress");

			ImportCompleteTextBlock.Text = GetLocalizedString("KitImporter!ImportComplete");
			ImportCompleteOkButton.Content = GetLocalizedString("OK");
			SettingsImportFailedTextBlock.Text = GetLocalizedString("KitImporter!SettingsImportFailed");
			NoModsRecordTextBlock.Text = GetLocalizedString("KitImporter!NoModsRecord");
			SkippedModsGroupBox.Header = GetLocalizedString("KitImporter!SkippedMods");
			FailedModsGroupBox.Header = GetLocalizedString("KitImporter!FailedMods");
		}

		/*string GetLanguageString(string identifier)
		{
			return Settings.GetLanguageString(4, identifier);
		}*/

		private void ImportCompleteOkButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
