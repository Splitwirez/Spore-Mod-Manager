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
		const string ERROR_TITLE = "ERROR (PLACEHOLDER) (NOT LOCALIZED)";
		const string ERROR_TEXT = "{0}";
		
		const string FATAL_ERROR_TITLE = "FATAL " + ERROR_TITLE;
		const string FATAL_ERROR_TEXT = "{0}\n\n\nSMM WILL CLOSE AFTER THIS (PLACEHOLDER) (NOT LOCALIZED)";
		
		static void ShowInternal(Exception exception, bool fatal)
		{
			DialogBox.Raw(string.Format(
				fatal
					? FATAL_ERROR_TEXT
					: ERROR_TEXT
			, exception),
				fatal
					? FATAL_ERROR_TITLE
					: ERROR_TITLE
			);

			if (fatal)
				Environment.Exit(-1);
		}
	}
}