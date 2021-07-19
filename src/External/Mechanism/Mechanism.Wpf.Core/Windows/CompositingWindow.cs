using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Timers;
using System.ComponentModel;
using Mechanism.Wpf.Core;
using System.Windows.Media;

namespace Mechanism.Wpf.Core.Windows
{
    public abstract partial class CompositingWindow : Window
    {
        static CompositingWindow()
        {
            AllowsTransparencyProperty.OverrideMetadata(typeof(CompositingWindow), new FrameworkPropertyMetadata(true));
        }

        public IntPtr Handle;
        

        NativeMethods.DWM_BLURBEHIND _blurInfo;
        NativeMethods.DWM_BLURBEHIND _unblurInfo;
        bool _realHide = false;

        internal static bool IsDesktopCompositionEnabled()
        {
            if (Environment.OSVersion.Version.Major < 6)
                return false;
            else
                return UnsafeIsCompositionEnabled();
        }

        static bool UnsafeIsCompositionEnabled()
        {
            return NativeMethods.DwmIsCompositionEnabled();
        }

        /// <summary>
        /// Gets whether blurred aero glass is available on the current machine
        /// </summary>
        public bool IsGlassAvailable
        {
            get => (bool)GetValue(IsGlassAvailableProperty);
            private set => SetValue(IsGlassAvailableProperty, value);
        }

        public static readonly DependencyProperty IsGlassAvailableProperty =
            DependencyProperty.Register(nameof(IsGlassAvailable), typeof(bool), typeof(CompositingWindow), new FrameworkPropertyMetadata(false, OnCompositionStatePropertyChangedCallback));


        /// <summary>
        /// Gets (or sets) whether or not this Window should be excluded from Aero peek
        /// </summary>
        public bool IgnorePeek
        {
            get => (bool)GetValue(IgnorePeekProperty);
            set => SetValue(IgnorePeekProperty, value);
        }

        public static readonly DependencyProperty IgnorePeekProperty =
            DependencyProperty.Register(nameof(IgnorePeek), typeof(bool), typeof(CompositingWindow), new FrameworkPropertyMetadata(false, OnIgnorePeekChangedCallback));

        internal static void OnIgnorePeekChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CompositingWindow).SetPeekState();
        }

        internal virtual void SetPeekState()
        {
            if (IsDesktopCompositionEnabled())
            {
                int peekValue = 0;

                if (IgnorePeek)
                    peekValue = 1;

                UnsafeSetPeekState(peekValue);
            }
        }

        protected void UnsafeSetPeekState(int peekValue)
        {
            NativeMethods.DwmSetWindowAttribute(Handle, 12, ref peekValue, sizeof(int));
        }

        public enum WindowCompositionState
        {
            Alpha,
            Glass,
            Accent,
            Acrylic
        }

        /// <summary>
        /// Gets (or sets) whether this Window should use pure unaltered transparency (Alpha), subtly blurred transparency (Glass), translucent accent colour (Accent), or heavily blurred transparency (Acrylic)
        /// </summary>
        public WindowCompositionState CompositionState
        {
            get => (WindowCompositionState)GetValue(CompositionStateProperty);
            set => SetValue(CompositionStateProperty, value);
        }

        public static readonly DependencyProperty CompositionStateProperty =
            DependencyProperty.Register(nameof(CompositionState), typeof(WindowCompositionState), typeof(CompositingWindow), new FrameworkPropertyMetadata(WindowCompositionState.Alpha, OnCompositionStatePropertyChangedCallback));


        /// <summary>
        /// Gets (or sets) whether or not this Window should be able to receive focus from the user
        /// </summary>
        public bool CanActivate
        {
            get => (bool)GetValue(CanActivateProperty);
            set => SetValue(CanActivateProperty, value);
        }

        public static readonly DependencyProperty CanActivateProperty =
            DependencyProperty.Register(nameof(CanActivate), typeof(bool), typeof(CompositingWindow), new FrameworkPropertyMetadata(true, OnCanActivatePropertyChangedCallback));

        static void OnCanActivatePropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            var win = sender as CompositingWindow;
            if ((bool)e.NewValue)
                NativeMethods.SetWindowLong(win.Handle, NativeMethods.GwlExstyle, NativeMethods.GetWindowLong(win.Handle, NativeMethods.GwlExstyle).ToInt32() & ~NativeMethods.WsExNoActivate);
            else
                NativeMethods.SetWindowLong(win.Handle, NativeMethods.GwlExstyle, NativeMethods.GetWindowLong(win.Handle, NativeMethods.GwlExstyle).ToInt32() & NativeMethods.WsExNoActivate);
        }

        public bool IsVisuallyActive
        {
            get => (bool)GetValue(IsVisuallyActiveProperty);
            set => SetValue(IsVisuallyActiveProperty, value);
        }

        public static readonly DependencyProperty IsVisuallyActiveProperty =
            DependencyProperty.Register(nameof(IsVisuallyActive), typeof(bool), typeof(CompositingWindow), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Gets (or sets) whether or not this Window should be listed in the ALT+TAB dialog box and other similar listings
        /// </summary>
        public bool ShowInAltTab
        {
            get => (bool)GetValue(ShowInAltTabProperty);
            set => SetValue(ShowInAltTabProperty, value);
        }

        public static readonly DependencyProperty ShowInAltTabProperty =
            DependencyProperty.Register(nameof(ShowInAltTab), typeof(bool), typeof(CompositingWindow), new FrameworkPropertyMetadata(true, OnShowInAltTabPropertyChangedCallback));

        internal static void OnShowInAltTabPropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateShowInAltTabPropertyValue(sender as CompositingWindow);
        }

        static void UpdateShowInAltTabPropertyValue(CompositingWindow win)
        {
            int exStyle = NativeMethods.GetWindowLong(win.Handle, NativeMethods.GwlExstyle).ToInt32();

            if (win.ShowInAltTab)
                exStyle |= ~NativeMethods.WsExToolwindow;
            //NativeMethods.SetWindowLong(win._handle, NativeMethods.GwlExstyle, NativeMethods.GetWindowLong(win._handle, NativeMethods.GwlExstyle).ToInt32() & ~NativeMethods.WsExToolwindow);
            else
                exStyle |= NativeMethods.WsExToolwindow;
            //NativeMethods.SetWindowLong(win._handle, NativeMethods.GwlExstyle, NativeMethods.GetWindowLong(win._handle, NativeMethods.GwlExstyle).ToInt32() & NativeMethods.WsExToolwindow);

            NativeMethods.SetWindowLong(win.Handle, NativeMethods.GwlExstyle, exStyle);

            //Debug.WriteLine("win.ShowInAltTab: " + win.ShowInAltTab.ToString());
        }

        /// <summary>
        /// Gets (or sets) whether or not this Window is being animated ifjsodig
        /// </summary>
        public bool IsWindowVisible
        {
            get => (bool)GetValue(IsWindowVisibleProperty);
            set => SetValue(IsWindowVisibleProperty, value);
        }

        public static readonly DependencyProperty IsWindowVisibleProperty =
            DependencyProperty.Register(nameof(IsWindowVisible), typeof(bool), typeof(CompositingWindow), new FrameworkPropertyMetadata(true));


        /// <summary>
        /// Gets (or sets) the number of milliseconds until the Window should hide or close after its closing animation begins
        /// </summary>
        public int HideTransitionDuration
        {
            get => (int)GetValue(HideTransitionDurationProperty);
            set => SetValue(HideTransitionDurationProperty, value);
        }

        public static readonly DependencyProperty HideTransitionDurationProperty =
            DependencyProperty.Register(nameof(HideTransitionDuration), typeof(int), typeof(CompositingWindow), new FrameworkPropertyMetadata(0));

        static void OnCompositionStatePropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            var win = sender as CompositingWindow;
            win.SetCompositionState(win.CompositionState);
        }

        //https://stackoverflow.com/questions/22841575/wpf-is-there-an-event-triggered-on-resource-change
        /*internal object DefaultStyleToMonitor
        {
            get { return GetValue(DefaultStyleToMonitorProperty); }
            set { SetValue(DefaultStyleToMonitorProperty, value); }
        }

        internal static readonly DependencyProperty DefaultStyleToMonitorProperty
            = DependencyProperty.Register(nameof(DefaultStyleToMonitor),
                   typeof(object),
                   typeof(CompositingWindow),
                   new FrameworkPropertyMetadata(new DynamicResourceExtension(typeof(CompositingWindow)), FrameworkPropertyMetadataOptions.AffectsRender, OnDefaultStyleToMonitorPropertyChangedCallback));

        public static void OnDefaultStyleToMonitorPropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender is CompositingWindow win)/* && (e.OldValue is Style oldStyle) && (e.NewValue is Style newStyle)*)
            {
                Style newStyle = null;
                /*{
                    Application.Current.TryFindResource
                }*
                //System.Linq.Enumerable.FirstOrDefault
                if (Application.Current != null)
                {
                    /*var newDefaultStyle = win.Style;//Application.Current.TryFindResource(win.DefaultStyleToMonitor);
                    if (newDefaultStyle != null)
                    {*
                    foreach (object key in Application.Current.Resources.Keys)
                    {
                        if ((key == win.DefaultStyleToMonitor)/* && (Application.Current.Resources[key] == newDefaultStyle)*)
                        {
                            newStyle = (Style)Application.Current.Resources[key];
                            break;
                        }
                        //win.TryFindResource
                        /*System.Windows.ResourceKey
                        win.Style.*
                    }
                    //}
                }
            }
        }*/


        public CompositingWindow()
        {
            //System.Reflection.type
            /*System.Reflection.MethodInfo method = null;//GetType().GetMethod("OverrideStyleMetadata");//.Invoke(this, null);
            Type type = GetType();
            int iteration = 0;
            do
            {
                iteration++;
                type = type.BaseType;
                method = type.GetMethod("OverrideStyleMetadata");
            }
            while (type != typeof(CompositingWindow));
            System.Reflection.MethodInfo method2 = typeof(CompositingWindow).GetMethod("OverrideStyleMetadata", System.Reflection.BindingFlags.NonPublic);
            method.Invoke(this, null);*/
            OverrideStyleMetadata();
            /*try
            {
                object style = TryFindResource(DefaultStyleKey);
                if ((style == null) && (Application.Current != null))
                    style = Application.Current.TryFindResource(DefaultStyleKey);
                if (style != null)
                    Style = (Style)style;
                    //SetCurrentValue(StyleProperty, style);
                //System.Windows.MessageBox.Show("AAA", "AAA");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "BBB");
            }*/
            base.WindowStyle = WindowStyle.None;
            //AllowsTransparency = true;
            if (SystemState.FakeCompositionOff)
                IsGlassAvailable = false;
            else
            {
                if (Environment.OSVersion.Version >= new Version(6, 2, 8400, 0))
                    IsGlassAvailable = IsDesktopCompositionEnabled() && ((System.IO.File.Exists(@"C:\AeroGlass\aerohost.exe")) || (System.IO.File.Exists(Environment.ExpandEnvironmentVariables(@"%systemdrive%\AeroGlass\aerohost.exe"))));
                else if (Environment.OSVersion.Version >= new Version(6, 0, 5112, 0))
                    IsGlassAvailable = IsDesktopCompositionEnabled();
                else
                    IsGlassAvailable = false;
            }

            if (IsDesktopCompositionEnabled())
            {
                SetBlurBehinds();
            }

            Loaded += CompositingWindow_Loaded;
        }

        protected static List<Type> OverriddenTypes = new List<Type>()
        {
            typeof(CompositingWindow)
        };

        /*public override void BeginInit()
        {
            OverrideStyleMetadata();
            base.BeginInit();
        }*/

        protected virtual void OverrideStyleMetadata()
        {
            SetStyle();
        }

        void SetStyle()
        {
            try
            {
                if (OverriddenTypes.Contains(GetType()))
                {
                    //System.Windows.MessageBox.Show(DefaultStyleKey.ToString() + "\n\n" + DefaultStyleKey.GetType().FullName);
                    object key = Type.GetType(DefaultStyleKey.ToString());
                    //System.Windows.MessageBox.Show(key.ToString(), "key");
                    object style = TryFindResource(key);
                    if (Application.Current != null) //((style == null) && (Application.Current != null))
                    {
                        //System.Windows.MessageBox.Show("style was not found in window resources");
                        style = Application.Current.FindResource(key);
                    }
                    if (style != null)
                    {
                        //if (getresou)
                        if (GetBindingExpression(StyleProperty) == null)
                            Style = (Style)style;

                        //System.Windows.MessageBox.Show("style was applied");
                    }
                    /*else
                        System.Windows.MessageBox.Show("style was null");*/
                    //SetCurrentValue(StyleProperty, style);
                    //System.Windows.MessageBox.Show("AAA", "AAA");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("STYLE FAIL: \n\n" + ex.ToString());
            }
        }

        void SetBlurBehinds()
        {
            _blurInfo = new NativeMethods.DWM_BLURBEHIND()
            {
                dwFlags = NativeMethods.DWM_BB.Enable | NativeMethods.DWM_BB.BlurRegion | NativeMethods.DWM_BB.TransitionMaximized,
                fEnable = true,
                //hRgnBlur = IntPtr.Zero,
                fTransitionOnMaximized = true
            };

            _unblurInfo = new NativeMethods.DWM_BLURBEHIND()
            {
                dwFlags = NativeMethods.DWM_BB.Enable | NativeMethods.DWM_BB.BlurRegion | NativeMethods.DWM_BB.TransitionMaximized,
                fEnable = false,
                hRgnBlur = IntPtr.Zero,
                fTransitionOnMaximized = true
            };
        }

        public IntPtr CompositingWindowWndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled)
        {
            if (msg == 0x0084)
            {
                /*var x = (uint)lParam & (uint)0x0000FFFF;
                var y = (uint)lParam >> 16;*/
                NativeMethods.POINT cursor = new NativeMethods.POINT();
                if (NativeMethods.ScreenToClient(Handle, ref cursor))
                {
                    if (InputHitTest(new Point(SystemScaling.RealPixelsToWpfUnits(cursor.X), SystemScaling.RealPixelsToWpfUnits(cursor.Y))) != null)
                        return new IntPtr(1);
                }
            }
            else if (msg == 0x0083)
            {
                //wParam = new IntPtr(1);
                handled = true;
            }

            return IntPtr.Zero;
        }

        /*private static object CoerceAllowsTransparency(DependencyObject d, object value)
        {
            return (d as Window).GetValue(AllowsTransparencyProperty);
        }*/

            /*protected override void OnStateChanged(EventArgs e) //private void CompositingWindow_StateChanged(object sender, EventArgs e)
            {
                base.OnStateChanged(e);

                if (WindowState == WindowState.Maximized)
                {
                    _maxWidth = MaxWidth;
                    _maxHeight = MaxHeight;
                    System.Windows.Forms.Screen s = System.Windows.Forms.Screen.FromHandle(Handle); //System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)SystemScaling.WpfUnitsToRealPixels(Left), (int)SystemScaling.WpfUnitsToRealPixels(Top)));
                    MaxWidth = s.WorkingArea.Width;
                    MaxHeight = s.WorkingArea.Height;
                }
                else
                {
                    MaxWidth = _maxWidth;
                    MaxHeight = _maxHeight;
                }
            }*/

        /*protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //Handle = new WindowInteropHelper(this).EnsureHandle();
            //NativeMethods.SetWindowLong(Handle, NativeMethods.GwlExstyle, NativeMethods.GetWindowLong(Handle, NativeMethods.GwlExstyle).ToInt32() & 0x00000020 & 0x00080000);
            //System.Windows.Media.CompositionTarget = 
            //System.Windows.Media.RenderOptions.com
            //var source = HwndSource.FromHwnd(Handle); //PresentationSource.FromVisual(this) as HwndSource;
            //source.ContentRendered += Source_ContentRendered;
            //source.CompositionTarget.BackgroundColor = System.Windows.Media.Colors.Transparent;
            //source.CompositionTarget.RenderMode = RenderMode.Default;
            //source.CompositionTarget
            //System.Windows.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;
            //source.CompositionTarget.UsesPerPixelOpacity = true;
            //source.UsesPerPixelOpacity = true;
        }*/

        /*private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Source_ContentRendered(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }*/

        /*HwndSourceParameters CreateHwndSourceParameters()
        {
            new HwndSourceParameters
        }*/

        private void CompositingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= CompositingWindow_Loaded;
            SetCurrentValue(StyleProperty, Style);

            if (IsVisible)
            {
                IsWindowVisible = false;
                Show();
                
            }

            if (!ShowInAltTab)
                UpdateShowInAltTabPropertyValue(this);

            SetPeekState();
        }

        new public void Show()
        {
            IsWindowVisible = true;
            base.Show();
            _realHide = false;
            /*int interval = 0;
            Timer timer = new Timer(1);
            timer.Elapsed += (sneder, args) =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (interval < )
                        interval++;
                    else
                    {
                        base.Show();
                        timer.Stop();
                    }
                }));
            };*/
        }


        
        new public void Hide()
        {
            IsWindowVisible = false;
            _realHide = true;
            int interval = 0;
            Timer timer = new Timer(1);
            timer.Elapsed += (sneder, args) =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (interval >= HideTransitionDuration)
                    {
                        timer.Stop();
                        if (_realHide)
                            base.Hide();
                    }
                    else
                    {
                        interval++;
                    }
                }));
            };
            timer.Start();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (!_isClosingNow)
            {
                Handle = new WindowInteropHelper(this).EnsureHandle();
                //NativeMethods.SetWindowLong(Handle, NativeMethods.GwlStyle, NativeMethods.GetWindowLong(Handle, NativeMethods.GwlStyle).ToInt32() & ~(0x00000000 | 0x00C00000 | 0x00800000 | 0x00080000 | 0x00040000));
                HwndSource.FromHwnd(Handle).CompositionTarget.BackgroundColor = Colors.Transparent;
                HwndSource.FromHwnd(Handle).AddHook(new HwndSourceHook(CompositingWindowWndProc));
                SetCompositionState(CompositionState);
            }
        }

        bool _isClosingNow = false;

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            bool animate = false;

            if (!(e.Cancel) & !_isClosingNow)
            {
                e.Cancel = true;
                _isClosingNow = true;
                animate = true;
                //Debug.WriteLine("doing the thing");
            }

            if (animate == true)
            {
                IsWindowVisible = false;
                int interval = 0;
                Timer timer = new Timer(1);
                timer.Elapsed += (sneder, args) =>
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (interval >= HideTransitionDuration)
                        {
                            base.Close();
                            timer.Stop();
                        }
                        else
                        {
                            interval++;
                            //Debug.WriteLine("interval: " + interval);
                        }
                    }));
                };
                timer.Start();
            }
        }

        void ClearCompositionState()
        {
            if (Environment.OSVersion.Version >= new Version(6, 2, 5112, 0))
                UnsafeClearCompositionState();
        }

        void UnsafeClearCompositionState()
        {
            if (IsDesktopCompositionEnabled())
            {
                NativeMethods.DwmEnableBlurBehindWindow(Handle, ref _unblurInfo);

                if (Environment.OSVersion.Version >= new Version(10, 0, 17134, 0))
                {
                    var accent = new NativeMethods.AccentPolicy();
                    var accentStructSize = Marshal.SizeOf(accent);
                    accent.GradientColor = (0x990000 << 24) | (0x990000 /* BGR */ & 0xFFFFFF);
                    accent.AccentState = NativeMethods.AccentState.ACCENT_DISABLED;

                    var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                    Marshal.StructureToPtr(accent, accentPtr, false);

                    var data = new NativeMethods.WindowCompositionAttributeData
                    {
                        Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                        SizeOfData = accentStructSize,
                        Data = accentPtr
                    };

                    NativeMethods.SetWindowCompositionAttribute(Handle, ref data);

                    Marshal.FreeHGlobal(accentPtr);
                }
                else if (Environment.OSVersion.Version >= new Version(6, 2, 9200, 0))
                {
                    var accent = new NativeMethods.AccentPolicy();
                    var accentStructSize = Marshal.SizeOf(accent);
                    accent.AccentState = NativeMethods.AccentState.ACCENT_DISABLED;
                    accent.AccentFlags = 0;
                    accent.GradientColor = 0;
                    accent.AnimationId = 0;

                    var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                    Marshal.StructureToPtr(accent, accentPtr, false);

                    var data = new NativeMethods.WindowCompositionAttributeData
                    {
                        Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                        SizeOfData = accentStructSize,
                        Data = accentPtr
                    };

                    NativeMethods.SetWindowCompositionAttribute(Handle, ref data);

                    Marshal.FreeHGlobal(accentPtr);
                }
            }
        }

        void SetCompositionState()
        {
            UnsafeSetCompositionState(CompositionState);
        }

        void SetCompositionState(WindowCompositionState targetState)
        {
            if (CompositionState != WindowCompositionState.Alpha)
                UnsafeSetCompositionState(targetState);
            else
                ClearCompositionState();
        }

        void UnsafeSetCompositionState(WindowCompositionState targetState)
        {
            if (IsDesktopCompositionEnabled())
            {
                ClearCompositionState();

                if (targetState == WindowCompositionState.Glass)
                {
                    if (new Version(10, 0, 16299, 0) <= Environment.OSVersion.Version)
                    {
                        var accent = new NativeMethods.AccentPolicy();
                        var accentStructSize = Marshal.SizeOf(accent);
                        accent.AccentState = NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND;

                        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                        Marshal.StructureToPtr(accent, accentPtr, false);

                        var data = new NativeMethods.WindowCompositionAttributeData
                        {
                            Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                            SizeOfData = accentStructSize,
                            Data = accentPtr
                        };

                        NativeMethods.SetWindowCompositionAttribute(Handle, ref data);

                        Marshal.FreeHGlobal(accentPtr);
                    }
                    else if (Environment.OSVersion.Version.Major >= 10)
                    {
                        var accent = new NativeMethods.AccentPolicy();
                        var accentStructSize = Marshal.SizeOf(accent);
                        accent.GradientColor = (0x990000 << 24) | (0x990000 /* BGR */ & 0xFFFFFF);
                        accent.AccentState = NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND;

                        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                        Marshal.StructureToPtr(accent, accentPtr, false);

                        var data = new NativeMethods.WindowCompositionAttributeData
                        {
                            Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                            SizeOfData = accentStructSize,
                            Data = accentPtr
                        };

                        NativeMethods.SetWindowCompositionAttribute(Handle, ref data);

                        Marshal.FreeHGlobal(accentPtr);
                    }
                    else //TODO: Figure something out for unmodified Windows 8.x
                    {
                        //HwndSource.FromHwnd(_handle).CompositionTarget.BackgroundColor = System.Windows.Media.Colors.Transparent;
                        IntPtr windowRegion = IntPtr.Zero;
                        if (NativeMethods.GetWindowRect(Handle, out NativeMethods.RECT rect))
                        {
                            //windowRegion = NativeMethods.CreateRectRgn(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
                            windowRegion = NativeMethods.CreateRectRgn(0, 0, (int)(SystemScaling.WpfUnitsToRealPixels(ActualWidth)), (int)(SystemScaling.WpfUnitsToRealPixels(ActualHeight)));
                            _blurInfo.hRgnBlur = windowRegion;
                            NativeMethods.DwmEnableBlurBehindWindow(Handle, ref _blurInfo);
                        }

                        /*Int32 refValue = (Int32)NativeMethods.DwmNCRenderingPolicy.Enabled;
                        NativeMethods.DwmSetWindowAttribute(_handle, (Int32)NativeMethods.DwmWindowAttribute.NCRenderingPolicy, ref refValue, sizeof(Int32));
                        NativeMethods.MARGINS margins = new NativeMethods.MARGINS()
                        {
                            leftWidth = -1,
                            rightWidth = -1,
                            topHeight = -1,
                            bottomHeight = -1
                        };
                        NativeMethods.DwmExtendFrameIntoClientArea(_handle, ref margins);*/
                    }
                }
                else if (targetState == WindowCompositionState.Accent)
                {
                    if (Environment.OSVersion.Version >= new Version(10, 0, 17134, 0))
                    {
                        var accent = new NativeMethods.AccentPolicy();
                        var accentStructSize = Marshal.SizeOf(accent);
                        accent.GradientColor = (0x990000 << 24) | (0x990000 /* BGR */ & 0xFFFFFF);
                        accent.AccentState = NativeMethods.AccentState.ACCENT_ENABLE_GRADIENT;

                        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                        Marshal.StructureToPtr(accent, accentPtr, false);

                        var data = new NativeMethods.WindowCompositionAttributeData
                        {
                            Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                            SizeOfData = accentStructSize,
                            Data = accentPtr
                        };

                        NativeMethods.SetWindowCompositionAttribute(Handle, ref data);

                        Marshal.FreeHGlobal(accentPtr);
                    }
                    else if (Environment.OSVersion.Version >= new Version(6, 2, 9200, 0))
                    {
                        var accent = new NativeMethods.AccentPolicy();
                        var accentStructSize = Marshal.SizeOf(accent);
                        accent.AccentState = NativeMethods.AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
                        accent.AccentFlags = 0;// 0x20 | 0x40 | 0x80| 0x100;
                        accent.GradientColor = 0;
                        accent.AnimationId = 1;

                        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                        Marshal.StructureToPtr(accent, accentPtr, false);

                        var data = new NativeMethods.WindowCompositionAttributeData
                        {
                            Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                            SizeOfData = accentStructSize,
                            Data = accentPtr
                        };

                        NativeMethods.SetWindowCompositionAttribute(Handle, ref data);

                        Marshal.FreeHGlobal(accentPtr);
                    }
                    else //TODO: Figure something out for Windows 7
                    {

                    }
                }
                else if (targetState == WindowCompositionState.Acrylic)
                {
                    if (Environment.OSVersion.Version >= new Version(10, 0, 17134, 0))
                    {
                        var accent = new NativeMethods.AccentPolicy();
                        var accentStructSize = Marshal.SizeOf(accent);
                        accent.GradientColor = (0x990000 << 24) | (0x990000 /* BGR */ & 0xFFFFFF);
                        accent.AccentState = NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND;

                        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                        Marshal.StructureToPtr(accent, accentPtr, false);

                        var data = new NativeMethods.WindowCompositionAttributeData
                        {
                            Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                            SizeOfData = accentStructSize,
                            Data = accentPtr
                        };

                        NativeMethods.SetWindowCompositionAttribute(Handle, ref data);

                        Marshal.FreeHGlobal(accentPtr);
                    }
                    else //TODO: Figure something out for older versions of Windows
                    {

                    }
                }
                else
                    NativeMethods.DwmEnableBlurBehindWindow(Handle, ref _unblurInfo);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (CompositionState != WindowCompositionState.Alpha)
                SetCompositionState();
        }
    }
}