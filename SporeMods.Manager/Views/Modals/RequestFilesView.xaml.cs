using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace SporeMods.Views
{
    /// <summary>
    /// Interaction logic for RequestFilesView.xaml
    /// </summary>
    public partial class RequestFilesView : UserControl
    {
		public RequestFilesView()
        {
            InitializeComponent();
        }


        private void DropHereContentControl_Drop(object sender, DragEventArgs e)
		{
            MessageBox.Show("Dropped!");
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
                (DataContext as SporeMods.ViewModels.RequestFilesViewModel).GrantFiles((IEnumerable<string>)(e.Data.GetData(DataFormats.FileDrop)));
            }
            else
            {
                MessageBox.Show("Wrong data!");
            }
        }
	}
}
