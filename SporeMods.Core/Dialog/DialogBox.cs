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
	public static partial class DialogBox
	{	
		public static void ShowNative(string content, string title = null)
		{
			Raw(content, title);
		}

		public static async Task ShowAsync(string content, string title = null)
		{
			if (Modal.TotalHandlers > 0)
			{
				await Modal.Show(new MessageBoxViewModel(content, title));
			}
			else
			{
				await Task.Run(() => 
				{
					Raw(content, title);
				});
			}
		}


		public static void DebugShow(string body)
		{
			DebugShow(body, Path.GetFileName(Process.GetCurrentProcess().GetExecutablePath()));
		}

		public static void DebugShow(string body, string title)
		{
			Debug.WriteLine(title + ":\n" + body);
		}
	}
}