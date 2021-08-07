using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SporeMods.NotifyOnChange;

namespace SporeMods.Core
{
	internal static partial class MessageBox
	{
#if !LINUX_BUILD
		[DllImport("user32.dll", SetLastError = true, CharSet= CharSet.Auto)]
		static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
#endif

		const string CONSOLE_SEPARATOR = "_____________________________________________________________";
		
        internal static void Raw(string content, string title)
		{
			string realContent = EnsureContent(content);
			EnsureTitle(title, out string realTitle);
#if !LINUX_BUILD
			MessageBox(IntPtr.Zero, realContent, realTitle, 0);
#endif
			Console.WriteLine($"\n\n{CONSOLE_SEPARATOR}\n{realTitle}\n{CONSOLE_SEPARATOR}\n{realContent}\n{CONSOLE_SEPARATOR}\n\n");
			//TODO: Figure out what (if anything) can be shown on-screen
		}
    }
    
    
    
    internal class MessageBoxViewModel : NOCObject, IModalViewModel<object>
	{
		TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();

		
		public TaskCompletionSource<object> GetCompletionSource()
			=> _tcs;
		
		public void AcknowledgeCommand(object parameter = null)
		{
			_tcs.TrySetResult(null);
		}


		NOCProperty<string> _contentProp;
		public string Content
		{
			get => _contentProp.Value;
			protected set => _contentProp.Value = value;
		}

		NOCProperty<string> _titleProp;
		public string Title
		{
			get => _titleProp.Value;
			protected set => _titleProp.Value = value;
		}

		NOCProperty<bool> _hasTitleProp;
		public bool HasTitle
		{
			get => _hasTitleProp.Value;
			protected set => _hasTitleProp.Value = value;
		}


		public MessageBoxViewModel(string content, string title = null)
		{
			_contentProp = AddProperty(nameof(Content), MessageBox.EnsureContent(content));
			
			
			_hasTitleProp = AddProperty(nameof(HasTitle), MessageBox.EnsureTitle(title, out string guaranteedTitle));
			_titleProp = AddProperty(nameof(Title), guaranteedTitle);
		}
	}
}