using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using CMessageBox = Mechanism.Wpf.Core.Windows.MessageBox;
using DecoratableWindow = Mechanism.Wpf.Core.Windows.DecoratableWindow;

namespace SporeMods.CommonUI
{
	public static class MessageDisplay
	{
		static bool EXCEPTION_SHOWN = false;
		public static void ShowException(Exception exception) => ShowException(exception, true);

		

		private const string ERROR_TITLE = "Something is very wrong here. Layer ";
		
		private const string ERROR_TEXT = "\n\nPlease send the contents this MessageBox and all which follow it to rob55rod\\Splitwirez, along with a description of what you were doing at the time.\n\n";
		private static readonly string FATAL_ERROR_TEXT = ERROR_TEXT + "The Spore Mod Manager will exit after the last Inner exception has been reported.";
		
		
		public static void ShowException(Exception exception, bool killAfter)
		{
			if (!EXCEPTION_SHOWN)
			{
				EXCEPTION_SHOWN = true;
				Exception current = exception;
				int count = 0;
				string errorText = killAfter ? FATAL_ERROR_TEXT : ERROR_TEXT;

				
				while (current != null)
				{
					ShowExceptionLayer(exception, errorText, count);
					count++;
					current = current.InnerException;
					if (count > 4)
						break;
				}
				if (current != null)
				{
					ShowExceptionLayer(exception, errorText, count);
				}

				if (killAfter)
					Process.GetCurrentProcess().Close();
			}
		}

		private static void ShowExceptionLayer(Exception current, string errorText, int count)
        {
			MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, ERROR_TITLE + count);
		}


		public static void ShowMessageBox(string messageBoxText, string caption)
		{
			if (false) // && VersionValidation.IsConfigValidationCompleted && Settings.UseCustomWindowDecorations)
				CMessageBox.Show(messageBoxText, caption);
			else
				MessageBox.Show(messageBoxText, caption);
		}


		public static void ShowClipboardFallback(string instruction, string content) => ShowClipboardFallback(instruction, content, string.Empty);
		public static void ShowClipboardFallback(string instruction, string content, string title)
		{
			ClipboardFallback fallback = new ClipboardFallback(instruction, content);
			Window window = null;

			if (Settings.UseCustomWindowDecorations)
			{
				window = new DecoratableWindow();
			}
			else
			{
				window = new Window();
			}

			window.Title = title;
			window.Content = fallback;
			window.SizeToContent = SizeToContent.WidthAndHeight;
			window.ShowDialog();
		}
	}
}
