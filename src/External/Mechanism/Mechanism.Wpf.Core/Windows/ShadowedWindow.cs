using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Mechanism.Wpf.Core;

namespace Mechanism.Wpf.Core.Windows
{
    public partial class ShadowedWindow : CompositingWindow
    {
        /// <summary>
        /// Gets or sets the <see cref="System.Windows.Style"/> that is applied to the shadow <see cref="System.Windows.Window"/> surrounding this <see cref="Mechanism.Wpf.Core.Windows.ShadowedWindow"/>.
        /// </summary>
        public Style ShadowStyle
        {
            get => (Style)GetValue(ShadowStyleProperty);
            set => SetValue(ShadowStyleProperty, value);
        }

        public static readonly DependencyProperty ShadowStyleProperty =
            DependencyProperty.Register(nameof(ShadowStyle), typeof(Style), typeof(ShadowedWindow), new PropertyMetadata(OnShadowChangedCallback));


        /*
        /// <summary>
        /// Gets or sets the distance between the bounds of this Window and its shadow <see cref="System.Windows.Window"/>.
        /// </summary>
        public Thickness ShadowOffsetThickness
        {
            get => (Thickness)GetValue(ShadowOffsetThicknessProperty);
            set => SetValue(ShadowOffsetThicknessProperty, value);
        }

        public static readonly DependencyProperty ShadowOffsetThicknessProperty =
            DependencyProperty.Register(nameof(ShadowOffsetThickness), typeof(Thickness), typeof(ShadowedWindow), new PropertyMetadata(new Thickness(50), OnShadowChangedCallback));*/

        public static void OnShadowChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender != null) && (sender is ShadowedWindow win) && (win._shadowWindow != null))
                win._shadowWindow.SyncShadowToWindow();
        }

        ShadowWindow _shadowWindow = null;
        readonly TimeSpan _noDuration = TimeSpan.FromMilliseconds(0);

        /*static ShadowedWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ShadowedWindow), new FrameworkPropertyMetadata(typeof(ShadowedWindow)));
            //IgnorePeekProperty.OverrideMetadata(typeof(ShadowedWindow), new FrameworkPropertyMetadata(false, ShadowedWindow.OnIgnorePeekChangedCallback));
        }*/

        WindowInteropHelper _helper = null;

        public ShadowedWindow() : base()
        {
            _shadowWindow = new ShadowWindow(this);
            /*_shadowWindow = new Window()
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                ShowInTaskbar = false,
                ShowActivated = false,
                Tag = this
            };*/
            //_shadowWindow.Tag = this;
            _helper = new WindowInteropHelper(_shadowWindow);

            /*_shadowWindow.SourceInitialized += (sneder, args) =>
            {
                

                SyncShadowToWindow();

                if (!IsWindowVisible)
                    _shadowWindow.Hide();
            };*/

            /*Loaded += ShadowedWindow_Loaded;
            _shadowWindow.Loaded += ShadowWindow_Loaded;*/

            /*Binding shadowStyleBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("ShadowStyle"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_shadowWindow, Window.StyleProperty, shadowStyleBinding);

            Binding shadowTopmostBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("Topmost"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_shadowWindow, Window.TopmostProperty, shadowTopmostBinding);

            Binding shadowVisibilityBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("Visibility"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_shadowWindow, Window.VisibilityProperty, shadowVisibilityBinding);

            Binding shadowRenderTransformBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("RenderTransform"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_shadowWindow, Window.RenderTransformProperty, shadowRenderTransformBinding);

            Binding shadowOpacityBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("Opacity"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_shadowWindow, Window.OpacityProperty, shadowOpacityBinding);

            Binding shadowIsFocusedBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("IsActive"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_shadowWindow, Window.IsManipulationEnabledProperty, shadowIsFocusedBinding);

            Binding shadowIsEnabledBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("IsWindowVisible"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_shadowWindow, Window.IsEnabledProperty, shadowIsEnabledBinding);

            Binding shadowIsHitTestVisibleBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("WindowState"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new WindowStateIsMaximizedToBoolConverter()
            };
            BindingOperations.SetBinding(_shadowWindow, Window.IsHitTestVisibleProperty, shadowIsHitTestVisibleBinding);

            Binding shadowBorderThicknessBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("ShadowOffsetThickness"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_shadowWindow, Window.BorderThicknessProperty, shadowBorderThicknessBinding);*/

            Initialized += (sneder, args) =>
            {
                if (WindowState == WindowState.Normal && IsVisible)
                {
                    _shadowWindow.Show();
                    //SyncShadowToWindow();
                }
            };

            /*Activated += ShadowedWindow_Activated;
            Deactivated += ShadowedWindow_Activated;*/

            //HwndSource.FromHwnd(Handle).AddHook(new HwndSourceHook(ShadowedWindowWndProc));
        }

        protected override void OverrideStyleMetadata()
        {
            if (!OverriddenTypes.Contains(GetType()))
            {
                DefaultStyleKeyProperty.OverrideMetadata(GetType(), new FrameworkPropertyMetadata(typeof(ShadowedWindow)));
                OverriddenTypes.Add(GetType());
            }
            base.OverrideStyleMetadata();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _shadowWindow.Close();
            _shadowWindow = null;
        }

        /*private void ShadowedWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SyncShadowToWindow();
            Loaded -= ShadowedWindow_Loaded;
        }

        private void ShadowWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SyncShadowToWindow();
            _shadowWindow.Loaded -= ShadowWindow_Loaded;
        }*/

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Normal)
                _shadowWindow.Show();
            else
                _shadowWindow.Hide();
        }

        /*protected IntPtr ShadowedWindowWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WmWindowPosChanging)
                SyncShadowToWindow();

            return IntPtr.Zero;
        }

        private void ShadowedWindow_Activated(object sender, EventArgs e)
        {
            SyncShadowToWindow();
        }*/

        /*new internal static void OnIgnorePeekChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ShadowedWindow).SetPeekState();
        }

        internal override void SetPeekState()
        {
            base.SetPeekState();

            if (CompositingWindow.IsDesktopCompositionEnabled())
            {
                int peekValue = 0;

                if (IgnorePeek)
                    peekValue = 1;

                NativeMethods.DwmSetWindowAttribute(_helper.Handle, 12, ref peekValue, sizeof(int));
            }
        }*/

        public void SyncShadowToWindow()
        {
            if ((_shadowWindow != null) && (_helper != null)/* && _shadowWindow.IsVisible*/)
            {
                /*int x = (int)SystemScaling.WpfUnitsToRealPixels(Left - ShadowOffsetThickness.Left);
                int y = (int)SystemScaling.WpfUnitsToRealPixels(Top - ShadowOffsetThickness.Top);
                int cx = (int)SystemScaling.WpfUnitsToRealPixels(ActualWidth + ShadowOffsetThickness.Left + ShadowOffsetThickness.Right);
                int cy = (int)SystemScaling.WpfUnitsToRealPixels(ActualHeight + ShadowOffsetThickness.Top + ShadowOffsetThickness.Bottom);
                NativeMethods.SetWindowPos(_helper.Handle, Handle, x, y, cx, cy, NativeMethods.SwpNoActivate);*/
                //_shadowWindow.SyncShadowToWindow();
            }
        }

        /*protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            SyncShadowToWindow();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            SyncShadowToWindow();
        }*/
    }
}
