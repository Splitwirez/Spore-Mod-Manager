using SporeMods.Core.Mods;
using SporeMods.CommonUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SporeMods.Manager.Views
{
    /// <summary>
    /// Interaction logic for InstalledModsPageView.xaml
    /// </summary>
    public partial class InstalledModsPageView : UserControl
    {
		public InstalledModsPageView()
		{
			InitializeComponent();
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ModsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender is ListBox box) && (box.IsVisible))
            {
                List<IInstalledMod> mods = new List<IInstalledMod>();
                foreach (var element in box.SelectedItems)
                {
                    if (element is IInstalledMod mod)
                        mods.Add(mod);
                }
                //var selMods = box.SelectedItems.ToList().OfType<IInstalledMod>();
                SelectedModsChanged?.Invoke(mods, null);

                /*ObservableCollection<>
                foreach (IInstalledMod mod in selMods)
                {

                }*/
            }
        }

        public static event EventHandler SelectedModsChanged;
	}
}
