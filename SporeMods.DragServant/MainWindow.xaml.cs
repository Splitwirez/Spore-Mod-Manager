﻿using System;
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
//using CompositingWindow = Mechanism.Wpf.Core.Windows.CompositingWindow;
using SporeMods.Core;
using System.Diagnostics;

namespace SporeMods.DragServant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IntPtr _winHandle = IntPtr.Zero;
        public MainWindow()
        {
            InitializeComponent();
            //Hide();
            /*ShowInTaskbar = false;
            ShowActivated = false;*/
            IsVisibleChanged += (sneder, args) =>
            {
                if (args.NewValue is bool val) 
                {
                    if (val)
                    {
                        DropModsHereTextBlock.Text = Settings.GetLanguageString("DropModsHereInstruction");
                        RootGrid.Visibility = Visibility.Visible;
                    }
                    else
                        RootGrid.Visibility = Visibility.Collapsed;
                }
            };

            Activated += (sneder, args) => SetStyles();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _winHandle = new WindowInteropHelper(this).EnsureHandle();
            SetStyles();
            //SetWindowLong(_winHandle, GwlExstyle, (Int32)(GetWindowLong(_winHandle, GwlExstyle)) | WsExToolwindow | WsExNoActivate);
            //Hide();
            //Path.Combine(Settings.TempFolderPath, "LaunchGame")
        }

        void SetStyles()
        {
            //SetWindowLong(_winHandle, GwlExstyle, (Int32)(GetWindowLong(_winHandle, GwlExstyle)) | ~WsExToolwindow);
            SetWindowLong(_winHandle, GwlExstyle, ((Int32)(GetWindowLong(_winHandle, GwlExstyle)) | WsExToolwindow) & ~0x00040000);
            //SetWindowLong(helper.Handle, GwlExstyle, (Int32)GetWindowLong(helper.Handle, GwlExstyle) | ~0x00040000); //WS_EX_APPWINDOW
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

                string draggedFilesPath = Path.Combine(Settings.TempFolderPath, "draggedFiles");
                File.WriteAllLines(draggedFilesPath, files);
                Permissions.GrantAccessFile(draggedFilesPath);
            }
        }
    }
}
