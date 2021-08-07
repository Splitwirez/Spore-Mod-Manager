using SporeMods.CommonUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SporeMods.Manager.Views
{
    /// <summary>
    /// Interaction logic for InstalledModsPageView.xaml
    /// </summary>
    public partial class SettingsPageView : UserControl
    {
		public SettingsPageView()
		{
			InitializeComponent();
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
	}
}
