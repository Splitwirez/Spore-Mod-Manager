using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core
{
	public static partial class DialogBox
	{
		[DllImport("user32.dll", SetLastError = true, CharSet= CharSet.Auto)]
		static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

		const string CONSOLE_SEPARATOR = "_____________________________________________________________";
		
        internal static void Raw(string content, string title)
		{
			string realContent = EnsureContent(content);
			EnsureTitle(title, out string realTitle);
			
			MessageBox(IntPtr.Zero, realContent, realTitle, 0);

			Console.WriteLine($"\n\n{CONSOLE_SEPARATOR}\n{realTitle}\n{CONSOLE_SEPARATOR}\n{realContent}\n{CONSOLE_SEPARATOR}\n\n");
			//TODO: Figure out what (if anything) can be shown on-screen
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
    }
}