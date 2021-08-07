using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;

namespace SporeMods.CommonUI
{
	/// <summary>
	/// Interaction logic for ProgressDialog.xaml
	/// </summary>
	public partial class ProgressDialog : UserControl
	{
		DoWorkEventHandler _action;
		public Exception Error = null;

		public ProgressDialog()
		{
			InitializeComponent();
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

			_downloadProgress = this.Find<ProgressBar>("DownloadProgress");


			var worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += _action;
			worker.ProgressChanged += worker_ProgressChanged;
			worker.RunWorkerCompleted += worker_RunWorkerCompleted;

			worker.RunWorkerAsync();
        }

		public ProgressDialog Setup(string text, DoWorkEventHandler action)
		{
			//Resources.MergedDictionaries[1] = ShaleAccents.Sky.Dictionary;
			this.Find<TextBlock>("Status").Text = text;
			if (action != null)
			{
				_action = action;
				//Loaded += ProgressDialog_Loaded;
			}

			return this;
		}

		ProgressBar _downloadProgress;

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			Dispatcher.UIThread.Post(() =>
			{
				_downloadProgress.Value = e.ProgressPercentage;
			});
		}

		void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Error = e.Error;
			Close();
		}

		internal void SetProgress(double newValue)
		{
			_downloadProgress.Value = newValue;
		}

		public void Close()
		{
			try
			{
				Avalonia.VisualTree.VisualExtensions.FindAncestorOfType<Window>(this).Close();
			}
			catch { }
		}

		public void Show()
		{
			try
			{
				Avalonia.VisualTree.VisualExtensions.FindAncestorOfType<Window>(this).Show();
				Avalonia.VisualTree.VisualExtensions.FindAncestorOfType<Window>(this).Focus();
				Avalonia.VisualTree.VisualExtensions.FindAncestorOfType<Window>(this).Activate();
			}
			catch { }
		}

		public async Task ShowDialog(Window owner)
		{
			var window = Avalonia.VisualTree.VisualExtensions.FindAncestorOfType<Window>(this);
			await window.ShowDialog(owner);
		}

		public void Hide()
		{
			try
			{
				Avalonia.VisualTree.VisualExtensions.FindAncestorOfType<Window>(this).Hide();
			}
			catch { }
		}
	}
}
