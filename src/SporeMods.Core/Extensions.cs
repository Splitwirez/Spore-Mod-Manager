using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json;

namespace SporeMods
{
	/// <summary>
	/// This class manages updates for the program and the core ModAPI DLLs.
	/// Updates are checked from repositories in GitHub.
	/// </summary>
	public static class Extensions
	{
		public static bool IsNullOrEmptyOrWhiteSpace(this string value)
		{
			return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
		}


		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr OpenProcess(int flags, bool inherit, int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, out int lpdwSize);

		public static string GetExecutablePath(this Process process)
		{
			string returnValue = string.Empty;
			StringBuilder stringBuilder = new StringBuilder(1024);
			IntPtr hprocess = OpenProcess(0x1000, false, process.Id);

			if (hprocess != IntPtr.Zero)
			{
				int size = stringBuilder.Capacity;

				if (QueryFullProcessImageName(hprocess, 0, stringBuilder, out size))
					returnValue = stringBuilder.ToString();
			}

			return returnValue;
		}
	}
}
