using SporeMods.CommonUI;
using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
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
    /// Interaction logic for InstalledModsView.xaml
    /// </summary>
    public partial class InstalledModsView : UserControl
    {
		public InstalledModsView()
        {
            InitializeComponent();
        }

        public void MenuToggleButton_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        ViewModels.InstalledModsViewModel VM => DataContext as ViewModels.InstalledModsViewModel;
        public void ModsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender is ListView listView) && (listView.Visibility == Visibility.Visible))
            {
                var rawSelected = listView.SelectedItems;
                if (rawSelected != null)
                    VM.SelectedMods = rawSelected.OfType<IInstalledMod>();
                else
                    VM.SelectedMods = new List<IInstalledMod>();
            }
        }
	}
}
