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
        readonly DoWorkEventHandler action;
        public Exception Error = null;

        public ProgressDialog(string text, DoWorkEventHandler action)
        {
            InitializeComponent();

            Status.Text = text;
            this.action = action;
        }

        void OnLoad(object sender, RoutedEventArgs args)
        {
            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += action;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            worker.RunWorkerAsync();
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DownloadProgress.Value = e.ProgressPercentage;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Error = e.Error;
            Close();
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
