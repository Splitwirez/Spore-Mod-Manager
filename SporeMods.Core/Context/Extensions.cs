using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
//using System.Windows;

namespace SporeMods.Core
{
	public static class Extensions
	{

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr OpenProcess(int flags, bool inherit, int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, out int lpdwSize);

		public static string GetExecutablePath(this Process process)
		{
			//if (SmmInfo.IsWindowsLike)
			#if !LINUX_BUILD
			try
			{
				string raw = WinRawQueryExecutablePath(process.Id);
				if (!raw.IsNullOrEmptyOrWhiteSpace())
				{
					return raw;
				}
			}
			catch
			{ }
			#endif


			return process.MainModule.FileName;
		}

		static string WinRawQueryExecutablePath(int pid)
		{
			string returnValue = null;
			StringBuilder stringBuilder = new StringBuilder(1024);
			IntPtr hprocess = OpenProcess(0x1000, false, pid);

			if (hprocess != IntPtr.Zero)
			{
				int size = stringBuilder.Capacity;

				if (QueryFullProcessImageName(hprocess, 0, stringBuilder, out size))
					returnValue = stringBuilder.ToString();
			}

			return returnValue;
		}


		public static bool IsNullOrEmptyOrWhiteSpace(this string value)
		{
			return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
		}


		public static bool TryGetEntry(this ZipArchive archive, string entryName, out ZipArchiveEntry entry)
		{
			entry = archive.Entries.FirstOrDefault(x => x.FullName == entryName);
			return entry != null;
		}

		public static bool IsDirectory(this ZipArchiveEntry entry)
		{
			string eName = entry.Name;
			string eFull = entry.FullName;
			bool backSlash = eFull.EndsWith(@"\");
			bool foreSlash = eFull.EndsWith(@"/");
			bool blankName = string.IsNullOrEmpty(eName);

			return (backSlash || foreSlash) && blankName;
		}
	}
}
