using SporeMods.Core;
using SporeMods.CommonUI.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace SporeMods.CommonUI
{
	public static class WineHelper
	{
		public static void SetClipboardContent(string newContent)
		{
			try
			{
				Clipboard.SetText(newContent);
			}
			catch (Exception ex)
			{
				MessageDisplay.ShowClipboardFallback(LanguageManager.Instance.GetLocalizedText("CopypasteToTechSupport"), newContent);
			}
		}

		public static void OpenUrl(string url)
		{
			string servantNoticePath = Path.Combine(Settings.TempFolderPath, "OpenUrl");
			if (Permissions.IsAdministrator() && (ServantCommands.HasDragServant))
			{
				try
				{
					if (File.Exists(servantNoticePath))
						File.Delete(servantNoticePath);
				}
				catch { }

				File.WriteAllText(servantNoticePath, url);
			}
			else
			{
				bool showFallback = false;
				/*if (Settings.NonEssentialIsRunningUnderWine)
				{*/
				try
				{
					Process process = Process.Start(new ProcessStartInfo(url)
					{
						UseShellExecute = true
					});

					if (process == null)
						showFallback = true;
					else if (process.HasExited)
						showFallback = true;
				}
				catch (Exception ex)
				{
					showFallback = true;
				}
				//}

				if (showFallback)
					MessageDisplay.ShowClipboardFallback(LanguageManager.Instance.GetLocalizedText("CopyUrlIntoBrowser"), url);
			}
			/*bool processCreationFailed = false;
			try
			{
				Process process = Process.Start(new ProcessStartInfo(url)
				{
					UseShellExecute = true
				});
				processCreationFailed = process == null;
				if (!processCreationFailed)
					processCreationFailed = process.HasExited;
			}
			catch
			{
				processCreationFailed = true;
			}

			if (processCreationFailed)
				MessageDisplay.ShowClipboardFallback(Settings.GetLanguageString("CopyUrlIntoBrowser"), url);*/
		}
	}
}
