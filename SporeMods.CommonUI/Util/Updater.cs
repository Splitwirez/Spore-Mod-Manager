using SporeMods.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using SporeMods.CommonUI.Localization;

namespace SporeMods.CommonUI
{
	public static class Updater
	{
		private static string GetLocalizedString(string key) =>
			LanguageManager.Instance.GetLocalizedText(key);

		public static void CheckForUpdates()
		{
			CheckForUpdates(false);
		}

		public static void CheckForUpdates(bool forceInstallUpdate)
		{
			try
			{
				if (File.Exists(UpdaterService.UpdaterPath))
					File.Delete(UpdaterService.UpdaterPath);

				bool ignoreUpdates = Environment.GetCommandLineArgs().Contains(UpdaterService.IGNORE_UPDATES_ARG);
				if (!ignoreUpdates)
				{
					if ((!forceInstallUpdate) && (Settings.UpdatingMode == Settings.UpdatingModeType.Disabled))
						return;

					// Necessary to stablish SSL connection with Github API
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

					// We will only show one error even if it cannot check the two updates
					WebException webException = null;

					UpdaterService.GithubRelease release = null;
					bool hasProgramUpdate = false;
					try
					{
						hasProgramUpdate = UpdaterService.HasProgramUpdate(out release);
					}
					catch (WebException ex)
					{
						webException = ex;
					}
					catch (Exception ex)
					{
						MessageDisplay.ShowException(ex);
						return;
					}

					if (hasProgramUpdate)
					{
						bool update = true;
						if ((!forceInstallUpdate) && (Settings.UpdatingMode == Settings.UpdatingModeType.AutoCheck))
						{
							update = MessageBox.Show(GetLocalizedString("Update!Notify!App!Content"),
								GetLocalizedString("Update!Notify!App!Header"), MessageBoxButton.YesNo) == MessageBoxResult.Yes;
						}

						if (update)
						{
							
							bool updateDownloadFinished = false;

							/*while ((!File.Exists(UpdaterService.UpdaterPath)) || Permissions.IsFileLocked(UpdaterService.UpdaterPath) || (!updateDownloadFinished))
							{ }*/

							var progressDialog = GetProgressDialog(GetLocalizedString("Update!Notify!App!ProgressContent"), (s, e) =>
							{
							/*Thread prgThread = new Thread(() =>
							{*/
								updateDownloadFinished = UpdaterService.UpdateProgram(release, (s_, e_) =>
							{
										(s as BackgroundWorker).ReportProgress(e_.ProgressPercentage);
									});

							/*});
							prgThread.Start();*/
							});
							progressDialog.ShowDialog();

							if (progressDialog.Error != null)
							{
								MessageDisplay.ShowException(progressDialog.Error);
								return;
							}

							while (Permissions.IsFileLocked(UpdaterService.UpdaterPath))
							{ }

							string argsPath = Path.Combine(Settings.TempFolderPath, "postUpdateCmdArgs.info");
							File.WriteAllText(argsPath, Permissions.GetProcessCommandLineArgs());
							Permissions.GrantAccessFile(argsPath);

							var updaterInfo = new ProcessStartInfo(UpdaterService.UpdaterPath, "--update \"" + Path.GetDirectoryName(Process.GetCurrentProcess().GetExecutablePath()) + "\" \"" + Process.GetCurrentProcess().GetExecutablePath() + "\" --lang:" + LanguageManager.Instance.CurrentLanguage.LanguageCode + " " + Permissions.GetProcessCommandLineArgs())
							{
								UseShellExecute = true,
								WorkingDirectory = Settings.TempFolderPath
							};
							//CrossProcess.PropagateDotnetEnvironmentVariables(ref updaterInfo);
							Process.Start(updaterInfo);
							
							Process.GetCurrentProcess().Kill();
						}
					}

					//TODO remember to restore this
					webException = null;

					bool hasDllsUpdate = false;
					try
					{
						hasDllsUpdate = UpdaterService.HasDllsUpdate(out release);
					}
					catch (WebException ex)
					{
						webException = ex;
					}
					catch (Exception ex)
					{
						MessageDisplay.ShowException(ex);
						return;
					}

					if (webException != null)
					{
						MessageBox.Show(GetLocalizedString("Update!Error!Other!Content") + "\n" + webException.ToString(), GetLocalizedString("Update!Error!Other!Header"));
						return;
					}

					if (hasDllsUpdate)
					{
						// If we reach this point with a program update available, it means it didn't update
						// (as the update restarts the program), so we cannot continue
						if (hasProgramUpdate)
						{
							MessageBox.Show(GetLocalizedString("Update!Error!CantUpdateDllsYet!Content"),
								GetLocalizedString("Update!Error!CantUpdateDllsYet!Header"));
						}
						else
						{
							bool update = true;
							if (Settings.UpdatingMode == Settings.UpdatingModeType.AutoCheck)
							{
								update = MessageBox.Show(GetLocalizedString("Update!Notify!ModApiDlls!Content"),
									GetLocalizedString("Update!Notify!ModApiDlls!Content"), MessageBoxButton.YesNo) == MessageBoxResult.Yes;
							}

							if (update)
							{
								var progressDialog = GetProgressDialog(GetLocalizedString("Update!Notify!ModApiDlls!ProgressContent"), (s, e) =>
								{
									UpdaterService.UpdateDlls(release, (s_, e_) =>
									{
										(s as BackgroundWorker).ReportProgress(e_.ProgressPercentage);
									});
								});
								progressDialog.ShowDialog();

								if (progressDialog.Error != null)
								{
									MessageDisplay.ShowException(progressDialog.Error);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ShowExceptionNoExit(ex);
			}
		}

		static bool exceptionShown = false;
		static void ShowExceptionNoExit(Exception exception)
		{
			if (!exceptionShown)
			{
				exceptionShown = true;
				Exception current = exception;
				int count = 0;
				string errorText = "\n\nPlease send the contents this MessageBox and all which follow it to rob55rod\\Splitwirez, along with a description of what you were doing at the time.\n\nThe Spore Mod Manager will exit after the last Inner exception has been reported.";
				string errorTitle = "Something is very wrong here. Layer ";
				while (current != null)
				{
					MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, errorTitle + count);
					count++;
					current = current.InnerException;
					if (count > 4)
						break;
				}
				if (current != null)
				{
					MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, errorTitle + count);
				}
			}
		}

		public static ProgressDialog GetProgressDialog(string text, DoWorkEventHandler action)
		{
			return GetProgressDialog(text, action, false);
		}

		public static ProgressDialog GetProgressDialog(string text, DoWorkEventHandler action, bool testUI)
		{
			ProgressDialog dialog = new ProgressDialog(text, action);

			Window window = new Window()
			{
				Content = dialog,
				SizeToContent = SizeToContent.WidthAndHeight,
				ResizeMode = ResizeMode.CanMinimize,
				Resources = dialog.Resources //.MergedDictionaries.Add() //Application.Current.Resources.MergedDictionaries[0].MergedDictionaries[1] = ShaleAccents.Sky.Dictionary;
			};


			if (testUI)
			{
				dialog.SetProgress(50);
				window.Show();
			}

			return dialog;
		}
	}
}
