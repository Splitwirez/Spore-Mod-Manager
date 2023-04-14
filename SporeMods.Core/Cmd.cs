using System;
using System.Diagnostics;
using System.Linq;
using SporeMods.Core.Injection;

namespace SporeMods.Core
{
	public static class Cmd
	{
		static IntPtr _consoleHWnd = IntPtr.Zero;
		static bool _hasInitializedConsole = false;
		static bool _hasShownConsole = false;

		internal const string SHOW_CONSOLE_CMD = "--console";
		static bool _showsConsole = false;

		public static bool ShowsConsole
		{
			get => _showsConsole;
			set
			{
				_showsConsole = value;
				RefreshConsole();
			}
		}

		static void RefreshConsole()
		{
			int showCmd = 0;
			if (ShowsConsole)
            {
				if (_hasShownConsole)
					showCmd = 5;
				else
				{
					showCmd = 1;
					_hasShownConsole = true;
				}
            }
			NativeMethods.ShowWindow(_consoleHWnd, showCmd);
		}

		public static void EnsureConsole()
        {
			if (!_hasInitializedConsole)
			{
				ShowsConsole = Environment
					.GetCommandLineArgs()
					.Any(x => x
						.Trim('"')
						.Equals(SHOW_CONSOLE_CMD, StringComparison.OrdinalIgnoreCase)
					)
				;

				IntPtr consoleHWnd = NativeMethods.GetConsoleWindow();
				if (consoleHWnd == IntPtr.Zero)
					NativeMethods.AllocConsole();
				_consoleHWnd = NativeMethods.GetConsoleWindow();
				if (!_showsConsole)
                {
					NativeMethods.ShowWindow(_consoleHWnd, 0);
				}

				_hasInitializedConsole = true;
			}
		}


		public static void WriteLine(object value)
		{
#if DEBUG && !LINUX_DEBUG
			Debug.WriteLine(value);
#endif
			Console.WriteLine(value);
		}
		public static void WriteLine(string value)
		{
#if DEBUG && !LINUX_DEBUG
			Debug.WriteLine(value);
#endif
			Console.WriteLine(value);
		}


		public static void WriteLine(string format, params object[] args)
		{
#if DEBUG && !LINUX_DEBUG
			Debug.WriteLine(format, args);
#endif
			Console.WriteLine(format, args);
		}
	}
}
