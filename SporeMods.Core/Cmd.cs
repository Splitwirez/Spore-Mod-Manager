using System;
using System.Diagnostics;

namespace SporeMods.Core
{
	public static class Cmd
	{
		public static void WriteLine(object? value)
		{
#if DEBUG && !LINUX_DEBUG
			Debug.WriteLine(value);
#endif
			Console.WriteLine(value);
		}
		public static void WriteLine(string? value)
		{
#if DEBUG && !LINUX_DEBUG
			Debug.WriteLine(value);
#endif
			Console.WriteLine(value);
		}


		public static void WriteLine(string? format, params object?[]? args)
		{
#if DEBUG && !LINUX_DEBUG
			Debug.WriteLine(format, args);
#endif
			Console.WriteLine(format, args);
		}
	}
}
