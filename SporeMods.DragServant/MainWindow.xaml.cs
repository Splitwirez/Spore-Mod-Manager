using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static Mechanism.Wpf.Core.NativeMethods;
using SporeMods.Core;
using System.Diagnostics;

namespace SporeMods.DragServant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Hide();
            /*ShowInTaskbar = false;
            ShowActivated = false;*/
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GwlExstyle, (Int32)(GetWindowLong(helper.Handle, GwlExstyle)) | WsExToolwindow);
            //SetWindowLong(helper.Handle, GwlExstyle, (Int32)GetWindowLong(helper.Handle, GwlExstyle) | ~0x00040000); //WS_EX_APPWINDOW
            //Hide();
            //Path.Combine(Settings.TempFolderPath, "LaunchGame")
        }

        /*protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            ShowInTaskbar = false;
            ShowActivated = false;
        }*/

        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (Settings.DebugMode)
                {
                    foreach (string f in files)
                    {
                        MessageBox.Show(f);
                    }
                }

                File.WriteAllLines(Path.Combine(Settings.TempFolderPath, "draggedFiles"), files);
            }
        }
    }
}
