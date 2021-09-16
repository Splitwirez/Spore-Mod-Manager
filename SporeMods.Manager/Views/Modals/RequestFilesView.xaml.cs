using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using RequestFilesViewModel = SporeMods.ViewModels.RequestFilesViewModel;

namespace SporeMods.Views
{
    /// <summary>
    /// Interaction logic for RequestFilesView.xaml
    /// </summary>
    public partial class RequestFilesView : UserControl
    {
        RequestFilesViewModel VM => DataContext as RequestFilesViewModel;

		public RequestFilesView()
        {
            InitializeComponent();
        }


        private void DropHereContentControl_Drop(object sender, DragEventArgs e)
		{
            //MessageBox.Show("Dropped!");
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
                var data = e.Data.GetData(DataFormats.FileDrop);
                //MessageBox.Show($"Dropped data: {data}\nType: '{data.GetType().FullName}'");
                if (data is IEnumerable<string> files)
                    VM.GrantFiles(files);
                else
                    MessageBox.Show("Wrong FileDrop data?? (PLACEHOLDER) (NOT LOCALIZED)");
            }
            else
            {
                MessageBox.Show("Wrong data! (PLACEHOLDER) (NOT LOCALIZED)");
            }
        }
	}
}
