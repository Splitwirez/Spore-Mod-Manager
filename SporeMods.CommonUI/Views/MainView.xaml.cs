using SporeMods.CommonUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using MainViewModel = SporeMods.ViewModels.MainViewModel;

namespace SporeMods.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainViewModel VM
        {
            get => DataContext as MainViewModel;
        }

        public MainView()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (UACPartnerCommands.TryGetPartnerDragWindowHwnd(out IntPtr hWnd) && UACPartnerCommands.TryShowHidePartnerDragWindow(true))
            {
                NativeMethods.SetParent(hWnd, new WindowInteropHelper(this).EnsureHandle());
                //UACPartnerCommands.TryShowHideDragServantWindow(false);
            }
        }
    }
}
