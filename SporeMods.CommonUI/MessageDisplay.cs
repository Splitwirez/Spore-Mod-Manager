using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace SporeMods.CommonUI
{
	public static class MessageDisplay
	{
		static bool EXCEPTION_SHOWN = false;
		public static void ShowException(Exception exception) => ShowException(exception, true);

		public static void ShowException(Exception exception, bool killAfter)
		{
			if (!EXCEPTION_SHOWN)
			{
				EXCEPTION_SHOWN = true;
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

				if (killAfter)
					Process.GetCurrentProcess().Close();
			}
		}

		public static void ShowMessageBox(string messageBoxText, string caption)
		{
			MessageBox.Show(messageBoxText, caption);
		}


		public static void ShowClipboardFallback(string instruction, string content) => ShowClipboardFallback(instruction, content, string.Empty);
		public static void ShowClipboardFallback(string instruction, string content, string title)
		{
			ClipboardFallback fallback = new ClipboardFallback(instruction, content);
			Window window = null;

			/*if (Settings.UseCustomWindowDecorations)
			{
				window = new DecoratableWindow();
			}
			else
			{*/
				window = new Window()
				{
					Title = title,
					Content = fallback,
					SizeToContent = SizeToContent.WidthAndHeight
				};
			//}

			window.ShowDialog();
		}



		public static void EnsureConsole()
		{
			if (Core.MessageDisplay.ShowsConsole)
			{
				IntPtr consoleHwnd = NativeMethods.GetConsoleWindow();
				if (consoleHwnd == IntPtr.Zero)
					NativeMethods.AllocConsole();
				else
					NativeMethods.ShowWindow(consoleHwnd, 1);
			}
		}
	}
}
