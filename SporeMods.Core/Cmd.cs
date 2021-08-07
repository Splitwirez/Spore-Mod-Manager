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
	public static class Cmd
	{
		public static readonly string LaunchSporeWithoutManagerOptions = "--NoManagerArgs";
		public static readonly string RunUnderWineFromLinux = "--wine";

		public static bool HasArg(string arg) =>
			Environment.GetCommandLineArgs().Any(x => x.Trim('"').Equals(arg, StringComparison.OrdinalIgnoreCase));
	}
}