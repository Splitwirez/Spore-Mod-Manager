using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;


using FClipboard = System.Windows.Forms.Clipboard;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SporeMods.ViewModels
{
	public class AmbiguousGamePathViewModel : RequestFilesViewModel
	{
		public AmbiguousGamePathViewModel(FileRequestPurpose purpose, bool acceptMultiple)
			: base(FileRequestPurpose.AmbiguousGamePath, false)
		{
			
		}
	}
}
