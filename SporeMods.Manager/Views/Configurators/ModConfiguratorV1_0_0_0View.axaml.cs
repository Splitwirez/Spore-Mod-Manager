using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SporeMods.Manager.Views.Configurators
{
	/// <summary>
	/// Interaction logic for ModConfiguratorV1_0_0_0.xaml
	/// </summary>
	public partial class ModConfiguratorV1_0_0_0View : UserControl
	{

		public ModConfiguratorV1_0_0_0View()
		{
			InitializeComponent();
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
#if RESTORE_LATER
			DataContext = arg;


			SetBody(new TextBlock()
			{
				Text = arg.Description
			});

			ModNameTextBlock.Text = SporeMods.CommonUI.Localization.LanguageManager.Instance.GetLocalizedText("Mods!Configurator!10xx!Header").Replace("%MODNAME%", arg.DisplayName);
		}

		public void SetBody(params UIElement[] elements)
		{
			CustomInstallerContentStackPanel.Children.Clear();
			foreach (UIElement element in elements)
				CustomInstallerContentStackPanel.Children.Add(element);
		}

		private void HeaderContentControl_MouseEnter(object sender, MouseEventArgs e)
		{
			SetBody(new TextBlock()
			{
				Text = (DataContext as ManagedMod).Description
			});
		}
#endif
	}
}
