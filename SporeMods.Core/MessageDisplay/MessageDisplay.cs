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
		/*[DllImport("user32.dll", SetLastError = true, CharSet= CharSet.Auto)]
		static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

		const string CONSOLE_SEPARATOR = "_____________________________________________________________";
		static void MessageBoxRaw(string content, string title)
		{
			string realCaption = content.IsNullOrEmptyOrWhiteSpace()
#if !LINUX_BUILD
			MessageBox(IntPtr.Zero, , caption)
#endif
			Console.WriteLine($"\n\n{CONSOLE_SEPARATOR}\n{title}\n{CONSOLE_SEPARATOR}\n{body}\n{CONSOLE_SEPARATOR}\n\n");
		}



		public static string ErrorSeparator = "?\n?\n?\n?\n";
		//public static string ErrorsSubdirectory = "Exceptions";
		

		/*public static event EventHandler<MessageBoxEventArgs> DebugMessageSent = (e) =>
		{

		};


		public static readonly Action<string, string, > ErrorOccurred = (e) =>
		{
			MessageBox(IntPtr.Zero, e.Content, e.Title, 0);
		};


		public static readonly Action<string, string> MessageBoxShown = (body, title) =>
		{
			try
			{
				MessageBox(IntPtr.Zero, body, title, 0);
			}
			catch
			{
				
			}
		};
		
		public static void ShowMessageBox(string content, string title = null)
		{
			MessageBoxShown(content, title.IsNullOrEmptyOrWhiteSpace() ? Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location) : title);
			ShowMessageBox
		}*/



		
		//public delegate Task<T> ShowModalEventHandler<T>(IModalViewModel<T> vm);



		static List<ModalShownEventArgs> _modals = new List<ModalShownEventArgs>();
		
		public static async Task<T> ShowModal<T>(IModalViewModel<T> vm)
		{
			Console.WriteLine("MODAL OF TYPE \'" + vm.GetType().FullName + "\' SHOWN: " + vm.ToString());
			//int count = 0;//_modals.Count;
			var task = vm.GetCompletionSource().Task;
			AddToQueue(new ModalShownEventArgs(vm, task));
			return await task;
		}

		/*public static async Task ShowMessageBox(string content, string title)
		{
			if (TotalModalShownHandlers > 0)
			{
				await ShowModal(new MessageBoxViewModel(content, title));
			}
			else
			{
				MessageBoxRaw(content, title);
			}
		}*/




		static void AddToQueue(ModalShownEventArgs args)
		{
			_modals.Add(args);
			StartRollingModals();
		}

		static bool _rolling = false;
		static async void StartRollingModals()
		{
			if (!_rolling)
			{
				_rolling = true;
				while (_modals.Count > 0)
				{
					var args = _modals[0];

					_modalShown?.Invoke(null, args);
					await args.Task;
					_modals.Remove(args);
				}
				_rolling = false;
			}
		}
		
		
		static int _totalModalShownHandlers = 0;
		internal static int TotalModalShownHandlers
		{
			get => _totalModalShownHandlers;
			set => _totalModalShownHandlers = Math.Max(0, value);
		}

		
		static event EventHandler<ModalShownEventArgs> _modalShown;
		public static event EventHandler<ModalShownEventArgs> ModalShown
		{
			add
			{
				_modalShown += value;
				if (_rolling && (_modals.Count > 0))
					_modalShown?.Invoke(null, _modals[0]);
				
				TotalModalShownHandlers++;
			}
			remove
			{
				_modalShown -= value;
				if (TotalModalShownHandlers > 0)
					TotalModalShownHandlers--;
			}
		}
		
		
		/*const string FALLBACK_MSG_CONTENT = "No content was provided for this MessageBox (PLACEHOLDER) (NOT LOCALIZED)";
		
		static string EnsureMessageContent(string content)
			=> MessageBox.EnsureContent(content);

		static bool EnsureMessageTitle(string title, out string guaranteedTitle)
			=> MessageBox.EnsureTitle(title, out guaranteedTitle);*/

		//public static event EventHandler LastModalDismissed;


		/*public static void DebugShowMessageBox(string body)
		{
			DebugShowMessageBox(body, Path.GetFileName(Process.GetCurrentProcess().GetExecutablePath()));
		}

		public static void DebugShowMessageBox(string body, string title)
		{
			Debug.WriteLine(title + ":\n" + body);
			/*if (Settings.DebugMode)
				DebugMessageSent?.Invoke(null, new MessageBoxEventArgs(title, body));* /
		}*/
	}

	public class ModalShownEventArgs
	{
		Task _task = null;
		public Task Task
			=> _task;

		IModalViewModel _vm = null;
		public IModalViewModel ViewModel
			=> _vm;

		public ModalShownEventArgs(IModalViewModel vm, Task task)
		{
			_vm = vm;
			_task = task;
		}
		
		private ModalShownEventArgs()
		{ }
	}

	
}