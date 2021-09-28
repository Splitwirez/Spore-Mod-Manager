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

            SporeMods.Core.ModsManager.InstalledMods.CollectionChanged += (s, e) => AllUpdateColumnWidths();
			SporeMods.Core.ModTransactions.ModTransactionManager.Instance.AllTasksConcluded += (tasks) => AllUpdateColumnWidths();
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
                    VM.SelectedMods = rawSelected.OfType<IInstalledMod>().ToList();
                else
                    VM.SelectedMods = new List<IInstalledMod>();
            }
        }

        List<Action> _allUpdateColumnWidths = new List<Action>();
        void ModsList_Loaded(object sender, RoutedEventArgs e)
        {
            if ((sender is ListView listView) && (listView.View is GridView gridView))
            {
                var headers = EnumVisual<GridViewColumnHeader>(listView);
                //headers.FirstOrDefault()
                foreach (GridViewColumnHeader header in headers)
                {
                    header.IsHitTestVisible = false;
                    //Console.WriteLine($"{child}: {child.GetType().FullName}");
                }


                var presenter = EnumVisual<ScrollContentPresenter>(listView).FirstOrDefault();
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
                    
                }
                listView.Loaded -= ModsList_Loaded;
            }
        }

        static List<T> EnumVisual<T>(Visual target) where T : Visual
        {
            List<T> found = new List<T>();
            EnumVisual<T>(target, ref found);
            return found;
        }

        static void EnumVisual<T>(Visual target, ref List<T> found) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(target); i++)
            {
                var child = VisualTreeHelper.GetChild(target, i);
                
                if (child is T find)
                    found.Add(find);
                
                if (child is Visual next)
                    EnumVisual<T>(next, ref found);
            }
        }

        void AllUpdateColumnWidths()
        {
            foreach (Action action in _allUpdateColumnWidths)
            {
                action();
            }
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

    public class IsManualInstalledFileConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is ManualInstalledFile;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
