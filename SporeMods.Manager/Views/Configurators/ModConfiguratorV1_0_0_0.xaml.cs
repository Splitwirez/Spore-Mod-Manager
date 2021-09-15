using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SporeMods.Views
{
	/// <summary>
	/// Interaction logic for ModConfiguratorV1_0_0_0.xaml
	/// </summary>
	public partial class ModConfiguratorV1_0_0_0 : UserControl
	{

		public ModConfiguratorV1_0_0_0(ManagedMod arg)
		{
			InitializeComponent();
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
	}
}
