using SporeMods.Core;
using SporeMods.CommonUI.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;

namespace SporeMods.CommonUI
{
	public static class WineHelper
	{
		public static void SetClipboardContent(string newContent)
		{
			try
			{
				//Clipboard.SetText(newContent);
			}
			catch (Exception ex)
			{
				ShowClipboardFallback(LanguageManager.Instance.GetLocalizedText("CopypasteToTechSupport"), newContent);
			}
		}

		public static void OpenUrl(string url, Process dragServant)
		{
			string servantNoticePath = Path.Combine(SmmInfo.TempFolderPath, "OpenUrl");
			if (Permissions.IsAdministrator() && (dragServant != null) && (!dragServant.HasExited))
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
					ShowClipboardFallback(LanguageManager.Instance.GetLocalizedText("CopyUrlIntoBrowser"), url);
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


		static void ShowClipboardFallback(string instruction, string content) => ShowClipboardFallback(instruction, content, string.Empty);
		static void ShowClipboardFallback(string instruction, string content, string title)
		{
			ClipboardFallback fallback = new ClipboardFallback().Setup(instruction, content);
			Window window = new Window();

			window.Title = title;
			window.Content = fallback;
			window.SizeToContent = SizeToContent.WidthAndHeight;
			window.Show(); //.ShowDialog();
		}
	}
}
