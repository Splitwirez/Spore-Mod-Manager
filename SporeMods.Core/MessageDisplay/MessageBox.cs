using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core
{
	internal static partial class MessageBox
	{
        /*public static readonly Action<string, string> Shown = (body, title) =>
		{
			try
			{
				MessageBox(IntPtr.Zero, body, title, 0);
			}
			catch
			{
				
			}
		};*/
        

		/*public static void ShowMessageBox(string content)
		{
			ShowMessageBox(content, null);
		}*/
		
		public static void ShowSynchronous(string content, string title = null)
		{
			//Shown(content, title.IsNullOrEmptyOrWhiteSpace() ? Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location) : title);
			Raw(content, title);
		}

		public static async Task Show(string content, string title)
		{
			if (MessageDisplay.TotalModalShownHandlers > 0)
			{
				await MessageDisplay.ShowModal(new MessageBoxViewModel(content, title));
			}
			else
			{
				Raw(content, title);
			}
		}
		
		
		const string FALLBACK_CONTENT = "No content was provided for this MessageBox (PLACEHOLDER) (NOT LOCALIZED)";
		
		internal static string EnsureContent(string content)
			=> content.IsNullOrEmptyOrWhiteSpace()
					? FALLBACK_CONTENT
					: content;

		internal static bool EnsureTitle(string title, out string guaranteedTitle)
        {
			if (title.IsNullOrEmptyOrWhiteSpace())
            {
                guaranteedTitle = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location);
                return false;
            }
            else
            {
                guaranteedTitle = title;
                return true;
            }
        }

		//public static event EventHandler LastModalDismissed;


		public static void DebugShow(string body)
		{
			DebugShow(body, Path.GetFileName(Process.GetCurrentProcess().GetExecutablePath()));
		}

		public static void DebugShow(string body, string title)
		{
			Debug.WriteLine(title + ":\n" + body);
			/*if (Settings.DebugMode)
				DebugMessageSent?.Invoke(null, new MessageBoxEventArgs(title, body));*/
		}
	}
}