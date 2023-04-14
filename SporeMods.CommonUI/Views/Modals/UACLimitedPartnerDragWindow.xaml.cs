using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using static SporeMods.Core.NativeMethods;
//using CompositingWindow = SporeMods.CommonUI.Windows.CompositingWindow;
using SporeMods.Core;
using static SporeMods.CommonUI.UACPartnerCommands;
using SporeMods.CommonUI.Localization;

namespace SporeMods.Views
{
    /// <summary>
    /// Interaction logic for UACLimitedPartnerDragWindow.xaml
    /// </summary>
    public partial class UACLimitedPartnerDragWindow : Window
    {
        IntPtr _winHandle = IntPtr.Zero;
        public UACLimitedPartnerDragWindow()
        {
            InitializeComponent();
            Activated += (sneder, args) => SetStyles();
        }

        public void RefreshText(string text)
        {
            DropHereZone.Content = text;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            //var _ = LanguageManager.Instance.CurrentLanguage;
            _winHandle = new WindowInteropHelper(this).EnsureHandle();
            SetStyles();
        }

        void SetStyles()
        {
            SetWindowLong(_winHandle, GwlStyle, ((Int32)(GetWindowLong(_winHandle, GwlStyle)) & ~WsSizeBox));

            //SetWindowLong(_winHandle, GwlExstyle, (Int32)(GetWindowLong(_winHandle, GwlExstyle)) | ~WsExToolwindow);
            SetWindowLong(_winHandle, GwlExstyle, ((Int32)(GetWindowLong(_winHandle, GwlExstyle)) | WsExToolwindow)/* & ~WsExAppWindow*/);
            //SetWindowLong(helper.Handle, GwlExstyle, (Int32)GetWindowLong(helper.Handle, GwlExstyle) | ~0x00040000); //WS_EX_APPWINDOW
        }

        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            Cmd.WriteLine("Dropped!");


            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string x in files)
                    Cmd.WriteLine(x);

                SendDroppedFiles(files);
            }
        }
    }
}
