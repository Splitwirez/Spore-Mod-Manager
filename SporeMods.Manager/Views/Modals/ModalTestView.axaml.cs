using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using System.Xml.Linq;
using SporeMods.Core;
using MessageDisplay = SporeMods.Core.MessageDisplay;
//using System.Windows.Interop;
using System.Runtime.InteropServices;
using SporeMods.Core.Mods;
using Avalonia.Controls.Primitives;
using SporeMods.Core.Injection;
using static SporeMods.Core.Injection.SporeLauncher;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
using Avalonia.Markup.Xaml;

namespace SporeMods.Manager.Views
{
	/// <summary>
	/// Interaction logic for ManagerContent.xaml
	/// </summary>
	public partial class ModalTestView : UserControl
	{
		public ModalTestView()
		{
			InitializeComponent();
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
