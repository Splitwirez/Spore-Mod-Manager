using SporeMods.CommonUI;
using SporeMods.Core.Mods;
using ModsManager = SporeMods.Core.ModsManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using IValueConverter = System.Windows.Data.IValueConverter;
using CultureInfo = System.Globalization.CultureInfo;

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

            ModsManager.InstalledMods.CollectionChanged += (s, e) => AllUpdateColumnWidths();
			ModsManager.Instance.AllJobsConcluded += (tasks) => AllUpdateColumnWidths();
        }

        public void MenuToggleButton_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        ViewModels.InstalledModsViewModel VM => DataContext as ViewModels.InstalledModsViewModel;
        
        
        void ModsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender is ListView listView) && (listView.Visibility == Visibility.Visible))
            {
                var rawSelected = listView.SelectedItems;
                if (rawSelected != null)
                    VM.SelectedMods = rawSelected.OfType<ISporeMod>().ToList();
                else
                    VM.SelectedMods = new List<ISporeMod>();
            }
        }

        List<Action> _allUpdateColumnWidths = new List<Action>();
        void ModsList_Loaded(object sender, RoutedEventArgs e)
        {
            if ((sender is ListView listView) && (listView.View is GridView gridView))
            {
                var headers = listView.GetDescendantsOfType<GridViewColumnHeader>();
                //headers.FirstOrDefault()
                foreach (GridViewColumnHeader header in headers)
                {
                    header.IsHitTestVisible = false;
                    //Cmd.WriteLine($"{child}: {child.GetType().FullName}");
                }


                /*var presenter = listView.GetDescendantsOfType<ScrollContentPresenter>().FirstOrDefault();
                var firstColumn = gridView.Columns.First();
                var otherColumns = gridView.Columns.Skip(1);
                if (presenter != null)
                {
                    Action updateColumnWidths = () =>
                    {
                        double width = 0;
                        foreach (var current in otherColumns)
                        {
                            width += current.ActualWidth;
                        }
                        firstColumn.Width = (presenter.ActualWidth + presenter.Margin.Left + presenter.Margin.Right) - width;
                    };

                    _allUpdateColumnWidths.Add(updateColumnWidths);
                    listView.SizeChanged += (s, args) => updateColumnWidths();
                    updateColumnWidths();
                    
                }*/
                //listView.Loaded -= ModsList_Loaded;
            }
        }

        

        void AllUpdateColumnWidths()
        {
            /*foreach (Action action in _allUpdateColumnWidths)
            {
                action();
            }*/
        }



        void SearchNames_UnChecked(object sender, RoutedEventArgs e)
		{
            VM.SearchNames = (sender as MenuItem).IsChecked;
        }

        void SearchNames_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as MenuItem).IsChecked = VM.SearchNames;
        }

        void SearchDescriptions_UnChecked(object sender, RoutedEventArgs e)
		{
            VM.SearchDescriptions = (sender as MenuItem).IsChecked;
        }

        void SearchDescriptions_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as MenuItem).IsChecked = VM.SearchDescriptions;
        }
	}

    public class IsManualInstalledFileConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#if MOD_IMPL_RESTORE_LATER
        {
            return value is ManualInstalledFile;
        }
#else
            => false;
#endif

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsThereAProgressSignifierHereConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null) && (value is SporeMods.Core.Mods.ModJob);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
