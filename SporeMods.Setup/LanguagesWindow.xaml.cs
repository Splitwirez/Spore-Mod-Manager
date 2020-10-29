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
using System.Windows.Shapes;

namespace SporeMods.Setup
{
    /// <summary>
    /// Interaction logic for LanguagesWindow.xaml
    /// </summary>
    public partial class LanguagesWindow : Window
    {
        public LanguagesWindow()
        {
            InitializeComponent();
        }

        private void LanguagesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((LanguagesComboBox.SelectedItem is ComboBoxItem item) && (item.Tag is ResourceDictionary language))
            {
                Application.Current.Resources.MergedDictionaries[0] = language;
            }
        }

        bool _allowClose = false;
        private void LanguagesWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_allowClose)
                e.Cancel = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _allowClose = true;
            Close();
        }
    }
}
