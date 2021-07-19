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
//using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using System.Runtime.InteropServices.ComTypes;

namespace SporeMods.Setup
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

		public void Start()
        {
			Show();

			//MessageBox.Show(SetupInformation.IsUpdatingModManager.ToString(), "IsUpdatingModManager");

			if (SetupInformation.IsAutoUpdatingModManager)
				SetupSteps.InstallSporeModManager(this);
			else if (SetupInformation.IsUpgradingFromLauncherKit)
				SetPage(WelcomeToUpgradePathPage);
			else
				SetPage(LicensePage);
		}

		public void SetPage(int index)
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
				/*string reso = string.Empty;
				foreach (string s in Application.ResourceAssembly.GetManifestResourceNames())
				{
					reso += s + "\n";
				}

				Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(reso, "RESOURCES")));*/
				SetPage(InstallModePage);
			}
		}

		private void SimpleInstallButton_Click(object sender, RoutedEventArgs e)
		{
			SetupSteps.InstallSporeModManager(this);
		}

		private void AdvancedInstallButton_Click(object sender, RoutedEventArgs e)
		{
			SetPage(SelectInstallPathPage);
			SelectInstallPathTextBox.Text = SetupInformation.DEFAULT_INSTALL_PATH;
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

			if (dirDoesntExistOrIsExistingModMgr && (!text.ToLowerInvariant().StartsWith(SetupInformation.UsersDir)))
			{
				SetupInformation.InstallPath = text;
				SelectInstallPathBadPathBorder.BorderThickness = new Thickness(0);
				SelectInstallPathNextButton.IsEnabled = true;
				SelectInstallPathError.Text = string.Empty;
			}
			else
			{
				SelectInstallPathBadPathBorder.BorderThickness = new Thickness(1);
				SelectInstallPathNextButton.IsEnabled = false;
				if (text.ToLowerInvariant().StartsWith(SetupInformation.UsersDir))
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

			bool inUserDir = text.ToLowerInvariant().StartsWith(SetupInformation.UsersDir);
			bool inProgramFiles = text.ToLowerInvariant().StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToLowerInvariant() + Path.DirectorySeparatorChar) ||
							(Environment.Is64BitOperatingSystem && text.ToLowerInvariant().StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLowerInvariant() + Path.DirectorySeparatorChar));

			if (storageDoesntExistOrIsAlreadyMgrStoragePath && (!inUserDir) && (!inProgramFiles))
			{
				SetupInformation.StoragePath = text;
				SelectStoragePathBadPathBorder.BorderThickness = new Thickness(0);
				SelectStoragePathNextButton.IsEnabled = true;
				SelectStoragePathError.Text = string.Empty;
			}
			else
			{
				SelectStoragePathBadPathBorder.BorderThickness = new Thickness(1);
				SelectStoragePathNextButton.IsEnabled = false;
				if (inUserDir)
					SelectStoragePathError.SetResourceReference(TextBlock.TextProperty, "CannotStoreConfigInUserSpecificLocation"); //.Text = "The Spore Mod Manager cannot store additional information in a user-specific location.";
				else if (inProgramFiles)
					SelectStoragePathError.SetResourceReference(TextBlock.TextProperty, "CannotStoreConfigInProtectedLocation");
				else
					SelectStoragePathError.SetResourceReference(TextBlock.TextProperty, "CannotStoreConfigInExistingFolder"); //.Text = "The Spore Mod Manager cannot store additional information in a pre-existing folder.";
			}
		}

		private void SelectInstallPathNextButton_Click(object sender, RoutedEventArgs e)
		{
			if (!SetupInformation.InstallPath.EndsWith("Spore Mod Manager"))
				SetupInformation.InstallPath = Path.Combine(SetupInformation.InstallPath, "Spore Mod Manager");
			SetPage(SelectStoragePathPage);
			SelectStoragePathTextBox.Text = SetupInformation.DEFAULT_STORAGE_PATH;
			UpdateSelectStoragePathText();
		}

		private void SelectStoragePathNextButton_Click(object sender, RoutedEventArgs e)
		{
			if (!SetupInformation.StoragePath.EndsWith("SporeModManagerStorage"))
				SetupInformation.StoragePath = Path.Combine(SetupInformation.StoragePath, "SporeModManagerStorage");
			SetupSteps.InstallSporeModManager(this);
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
				if (!Directory.Exists(SetupInformation.DEFAULT_STORAGE_PATH))
					Directory.CreateDirectory(SetupInformation.DEFAULT_STORAGE_PATH);

				Permissions.GrantAccessDirectory(SetupInformation.DEFAULT_STORAGE_PATH);

				if (File.Exists(SetupInformation.INSTALL_DIR_LOCATOR_PATH))
					Permissions.GrantAccessFile(SetupInformation.INSTALL_DIR_LOCATOR_PATH);

				File.WriteAllText(SetupInformation.INSTALL_DIR_LOCATOR_PATH, SetupInformation.InstallPath);

				Permissions.GrantAccessFile(SetupInformation.INSTALL_DIR_LOCATOR_PATH);


				if (SetupInformation.IsAutoUpdatingModManager && (SetupInformation.MgrExePath != null))
				{
					if (File.Exists(SetupInformation.LAST_EXE_PATH))
						Permissions.GrantAccessFile(SetupInformation.LAST_EXE_PATH);

					File.WriteAllText(SetupInformation.LAST_EXE_PATH, SetupInformation.MgrExePath);

					Permissions.GrantAccessFile(SetupInformation.LAST_EXE_PATH);
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

				/*if (false) //dodging this UAC prompt seems to be a bridge too far
				{
					Hide();
					AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

					//Assembly.LoadFrom(Path.Combine(_installPath, "Microsoft.Threading.Tasks.dll"));
					//Assembly.LoadFrom(Path.Combine(_installPath, "Microsoft.Threading.Tasks.Extensions.dll"));
					//Assembly.LoadFrom(Path.Combine(_installPath, "Microsoft.Threading.Tasks.Extensions.Desktop.dll"));

					var importer = Assembly.LoadFrom(Path.Combine(_installPath, "SporeMods.KitImporter.exe"));
					Window importerWindow = (Window)Activator.CreateInstance(importer.GetType("SporeMods.KitImporter.MainWindow"));
					importerWindow.ShowDialog();
				}*/

				if ((!SetupInformation.IsAutoUpdatingModManager) && SetupInformation.IsUpgradingFromLauncherKit)
				{
					//var importerPath = Path.Combine(SetupInformation.InstallPath, "SporeMods.KitImporter.exe");

					try
					{
						Process process = SetupSteps.StartLauncherKitImporterAsAdministrator("--mandate"); /*Process.Start(new ProcessStartInfo(importerPath)
						{
							UseShellExecute = true,
							Arguments = "--mandate"
						});*/
						process.WaitForExit();
					}
					catch (Win32Exception w32ex)
					{
						string forceLkImportPath = Path.Combine(SetupInformation.StoragePath, "ForceLkImport.info");
						File.WriteAllText(forceLkImportPath, string.Empty);
						Permissions.GrantAccessFile(forceLkImportPath);
						SetupSteps.DebugMessageBox("forceLkImportPath: " + forceLkImportPath);
					}
				}
				Application.Current.Shutdown(300);
			}
		}

		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string path = Path.Combine(SetupInformation.InstallPath, args.Name);

			if (File.Exists(path))
				return Assembly.LoadFrom(path);
			else
				return null;
		}

		

		

		private void UpgradeOkButton_Click(object sender, RoutedEventArgs e)
		{
			SetPage(LicensePage);
		}

		private void SuccessCloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
			Application.Current.Shutdown(300);
		}

		private void SelectInstallPathBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("NYI (NOT LOCALIZED) (do I really need to explain why?");
#if RESTORE_LATER
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
			{
				string path = dialog.SelectedPath;
				if (!path.EndsWith("Spore Mod Manager"))
					path = Path.Combine(path, "Spore Mod Manager");
				SelectInstallPathTextBox.Text = path;
			}
#endif
		}

		private void SelectStoragePathBrowseButton_Click(object sender, RoutedEventArgs e)
		{
#if RESTORE_LATER
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
			{
				string path = dialog.SelectedPath;
				if (!path.EndsWith("SporeModManagerStorage"))
					path = Path.Combine(path, "SporeModManagerStorage");
				SelectStoragePathTextBox.Text = path;
			}
#endif
		}

		private void SelectInstallPathDefaultButton_Click(object sender, RoutedEventArgs e)
		{
			SelectInstallPathTextBox.Text = SetupInformation.DEFAULT_INSTALL_PATH;
			SetupInformation.StoragePath = SetupInformation.DEFAULT_INSTALL_PATH;
		}

		private void SelectStoragePathDefaultButton_Click(object sender, RoutedEventArgs e)
		{
			SelectStoragePathTextBox.Text = SetupInformation.AutoStoragePath;
			SetupInformation.StoragePath = SetupInformation.AutoStoragePath;
		}

		public void SetProgressBarMax(double maxVal)
		{
			Dispatcher.BeginInvoke(new Action(() => InstallProgressBar.Maximum = maxVal));
		}

		public void IncrementProgress()
		{
			Dispatcher.BeginInvoke(new Action(() => InstallProgressBar.Value++));
		}

		private void CreateDesktopShortcutsCheckBox_IsCheckedChanged(object sender, RoutedEventArgs e)
		{
			SetupInformation.CreateShortcuts = CreateDesktopShortcutsCheckBox.IsChecked.Value;
		}

		public void SwitchToInstallProgressPage()
		{
			Dispatcher.BeginInvoke(new Action(() => SetPage(InstallProgressPage)));
		}

		public void SwitchToInstallCompletedPage()
		{
			Dispatcher.BeginInvoke(new Action(() => SetPage(InstallCompletedPage)));
		}
	}
}
