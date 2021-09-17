using SporeMods.CommonUI.Themes.Shale;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace SporeMods.CommonUI
{
	/// <summary>
	/// Interaction logic for ProgressDialog.xaml
	/// </summary>
	public partial class ProgressDialog : UserControl
	{
		readonly DoWorkEventHandler _action;
		public Exception Error = null;

		public ProgressDialog(string text, DoWorkEventHandler action)
		{
			InitializeComponent();
			//Resources.MergedDictionaries[1] = ShaleAccents.Sky.Dictionary;
			Status.Text = text;
			if (action != null)
			{
				_action = action;
				Loaded += ProgressDialog_Loaded;
			}
		}

		private void ProgressDialog_Loaded(object sender, RoutedEventArgs e)
		{
			var worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += _action;
			worker.ProgressChanged += worker_ProgressChanged;
			worker.RunWorkerCompleted += worker_RunWorkerCompleted;

			worker.RunWorkerAsync();
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			Dispatcher.Invoke(new Action(() =>
			{
				DownloadProgress.Value = e.ProgressPercentage;
			}));
		}

		void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Error = e.Error;
			Close();
		}

		internal void SetProgress(double newValue)
		{
			DownloadProgress.Value = newValue;
		}

		public void Close()
		{
			try
			{
				Window.GetWindow(this).Close();
			}
			catch { }
		}

		public void Show()
		{
			try
			{
				Window.GetWindow(this).Show();
				Window.GetWindow(this).Focus();
				Window.GetWindow(this).Activate();
			}
			catch { }
		}

		public bool? ShowDialog()
		{
			try
			{
				return Window.GetWindow(this).ShowDialog();
			}
			catch { return null; }
		}

		public void Hide()
		{
			try
			{
				Window.GetWindow(this).Hide();
			}
			catch { }
		}
	}
}
