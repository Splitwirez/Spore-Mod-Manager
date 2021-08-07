using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SporeMods.NotifyOnChange;
using static SporeMods.Core.GameInfo;

namespace SporeMods.Core
{
	public static partial class SmmInfo// : NOCSingleInstanceObject<SmmInfo>
	{
		[DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern string GetWineVersion();

		private static bool TryGetIsRunningUnderWine(out bool wineVersionFound, out Version wineVersion)
		{
			wineVersionFound = false;
			wineVersion = new Version(0, 0, 0, 0);
			try
			{
				string wineVerStr = GetWineVersion();
				if (Version.TryParse(wineVerStr, out Version wineVer))
				{
					wineVersion = wineVer;
					wineVersionFound = true;
				}
				
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public static readonly bool IsExplicitWineFromLinux = Cmd.HasArg(Cmd.RunUnderWineFromLinux);
	}
}