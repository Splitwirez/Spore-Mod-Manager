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
	public static partial class MessageDisplay
	{
		public static void ShowMessageBox(string content, string title = null)
			=> MessageBox.ShowSynchronous(content, title);

		/*public static async Task ShowMessageBox(string content, string title = null)
			=> await MessageBox.Show(content, title);*/

		public static void DebugShowMessageBox(string body, string title = null)
			=> MessageBox.DebugShow(body, title);
	}
}