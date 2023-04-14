using SporeMods.CommonUI;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        MainViewModel _vm = null;
        public MainViewModel VM
        {
            //get => DataContext as MainViewModel;
            get => _vm;
        }

        public MainView()
        {
            DataContextChanged += MainView_DataContextChanged;
            InitializeComponent();
        }

        private void MainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is MainViewModel vm))
                return;


            _vm = vm;
            
            VM.MinimizeWindowRequested += VM_MinimizeWindowRequested;
            VM.RestoreWindowRequested += VM_RestoreWindowRequested;
            VM.IsSporeRunningChanged += VM_IsSporeRunningChanged;

            DataContextChanged -= MainView_DataContextChanged;
        }

        private void VM_IsSporeRunningChanged(object sender, EventArgs e)
        {
            //VM.IsSporeRunning
        }

        WindowState _previousWindowState = WindowState.Normal;
        bool _shouldRestorePreviousWindowState = false;
        void VM_MinimizeWindowRequested(object sender, EventArgs e)
        {
            _previousWindowState = this.WindowState;
            WindowState = WindowState.Minimized;
            
            _shouldRestorePreviousWindowState = true;
            StateChanged += MainView_StateChanged;
        }
        void VM_RestoreWindowRequested(object sender, EventArgs e)
        {
            MessageBox.Show($"{nameof(VM_RestoreWindowRequested)}, {_shouldRestorePreviousWindowState}");
            if (_shouldRestorePreviousWindowState)
                WindowState = _previousWindowState;
            
            _shouldRestorePreviousWindowState = false;
            StateChanged -= MainView_StateChanged;
        }

        private void MainView_StateChanged(object sender, EventArgs e)
        {
            _shouldRestorePreviousWindowState = false;
            StateChanged -= MainView_StateChanged;
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

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!VM.AllowExitSMM)
                e.Cancel = true;

            base.OnClosing(e);
        }

        static readonly ModifierKeys _CTRL_SHIFT = ModifierKeys.Control | ModifierKeys.Shift;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsConsoleShortcut(e))
            {
                e.Handled = true;
            }
            base.OnKeyUp(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (IsConsoleShortcut(e))
            {
                e.Handled = true;
                Cmd.ShowsConsole = !(Cmd.ShowsConsole);
            }
            base.OnKeyUp(e);
        }

        static bool IsConsoleShortcut(KeyEventArgs e)
        {
            return (e.Key == Key.C)
                &&
                (e.KeyboardDevice.Modifiers == _CTRL_SHIFT)
            ;
        }
    }
}
