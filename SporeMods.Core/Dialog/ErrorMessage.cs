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
	public static partial class ErrorMessage
	{
		public static void Show(Exception exception)
			=> ShowInternal(exception, false);

		public static void ShowFatal(Exception exception)
			=> ShowInternal(exception, true);
	}
}