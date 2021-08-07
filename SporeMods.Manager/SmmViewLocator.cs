/*using SporeMods.Core;
using SporeMods.CommonUI.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using SporeMods.CommonUI;
using SporeMods.Manager.Views;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;

namespace SporeMods.Manager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class SmmViewLocator : ViewLocator
	{
		protected override bool DirectGetView(object viewModel, out IControl view)
        {
			if (viewModel is SporeMods.Core.Mods.ManagedMod mod)
            {
                view = new Views.Configurators.ModConfiguratorV1_0_0_0View();
				return true;
            }
			return base.DirectGetView(viewModel, out view);
		}
	}
}
*/