using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Interactivity;
using System.Windows.Shell;
using Colors = System.Windows.Media.Colors;
using Screen = System.Windows.Forms.Screen;
using Graphics = System.Drawing.Graphics;
using Point = System.Windows.Point;
using SporeMods.CommonUI;

using Cmd = SporeMods.Core.Cmd;

namespace SporeMods.CommonUI
{
    public class WindowChromeHelper : Behavior<Window>
    {
        static readonly Thickness ZERO_THICKNESS = new Thickness(0);

        public static readonly DependencyProperty UseCustomDecorationsProperty =
            DependencyProperty.RegisterAttached("UseCustomDecorations", typeof(bool), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(false, UseCustomDecorationsChanged));

        public static bool GetUseCustomDecorations(Window element)
        {
            return (bool)element.GetValue(UseCustomDecorationsProperty);
        }

        public static void SetUseCustomDecorations(Window element, bool value)
        {
            element.SetValue(UseCustomDecorationsProperty, value);
        }

        static void UseCustomDecorationsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Window window)
            {
                var helper = GetWindowChromeHelper(window);
                
                if (helper == null)
                    SetWindowChromeHelper(window, new WindowChromeHelper());
                else if (e.NewValue is bool useCustomDecorations)
                    helper.CustomDecorationsChanged(useCustomDecorations);
            }
        }

        public static readonly DependencyProperty WindowChromeHelperProperty =
            DependencyProperty.RegisterAttached("WindowChromeHelper", typeof(WindowChromeHelper), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(null, WindowChromeHelperChanged));

        public static WindowChromeHelper GetWindowChromeHelper(Window element)
        {
            return (WindowChromeHelper)element.GetValue(WindowChromeHelperProperty);
        }

        static void WindowChromeHelperChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender is Window window) && (e.NewValue != null) && (e.NewValue is WindowChromeHelper chromeHelper))
                Interaction.GetBehaviors(window).Add(chromeHelper);
        }

        public static void SetWindowChromeHelper(Window element, WindowChromeHelper value)
        {
            element.SetValue(WindowChromeHelperProperty, value);
        }
        
        
        static void ButtonWindowPropertiesChanged(object sender, DependencyPropertyChangedEventArgs e)
            => SetButtonStuff(sender, e);

        public static readonly DependencyProperty ClosesWindowProperty =
            DependencyProperty.RegisterAttached("ClosesWindow", typeof(Window), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(null, propertyChangedCallback: ButtonWindowPropertiesChanged));

        public static Window GetClosesWindow(Button element)
        {
            return (Window)element.GetValue(ClosesWindowProperty);
        }

        public static void SetClosesWindow(Button element, Window value)
        {
            element.SetValue(ClosesWindowProperty, value);
        }

        internal static readonly DependencyProperty IsCloseButtonForProperty =
            DependencyProperty.RegisterAttached("IsCloseButtonFor", typeof(Button), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(null));



        public static readonly DependencyProperty MaximizesWindowProperty =
            DependencyProperty.RegisterAttached("MaximizesWindow", typeof(Window), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(null, propertyChangedCallback: ButtonWindowPropertiesChanged));

        public static Window GetMaximizesWindow(Button element)
        {
            return (Window)element.GetValue(MaximizesWindowProperty);
        }

        public static void SetMaximizesWindow(Button element, Window value)
        {
            element.SetValue(MaximizesWindowProperty, value);
        }

        internal static readonly DependencyProperty IsMaximizeButtonForProperty =
            DependencyProperty.RegisterAttached("IsMaximizeButtonFor", typeof(Button), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(null));



        public static readonly DependencyProperty MinimizesWindowProperty =
            DependencyProperty.RegisterAttached("MinimizesWindow", typeof(Window), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(null, propertyChangedCallback: ButtonWindowPropertiesChanged));

        public static Window GetMinimizesWindow(Button element)
        {
            return (Window)element.GetValue(MinimizesWindowProperty);
        }

        public static void SetMinimizesWindow(Button element, Window value)
        {
            element.SetValue(MinimizesWindowProperty, value);
        }

        internal static readonly DependencyProperty IsMinimizeButtonForProperty =
            DependencyProperty.RegisterAttached("IsMinimizeButtonFor", typeof(Button), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(null));



        public static readonly DependencyProperty UseReRenderHackProperty =
            DependencyProperty.RegisterAttached("UseReRenderHack", typeof(bool), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(false, UseReRenderHackPropertyChangedCallback));

        public static bool GetUseReRenderHack(Window element)
        {
            return (bool)element.GetValue(UseReRenderHackProperty);
        }

        public static void SetUseReRenderHack(Window element, bool value)
        {
            element.SetValue(UseReRenderHackProperty, value);
        }

        static void UseReRenderHackPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender is Window window) && (e.NewValue is bool useHack))
                GetWindowChromeHelper(window).UseReRenderHackChanged(useHack);
        }



        public static readonly DependencyProperty Win7UseCornerRadiusProperty =
            DependencyProperty.RegisterAttached("Win7UseCornerRadius", typeof(bool), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(false, Win7UseCornerRadiusPropertyChangedCallback));

        public static bool GetWin7UseCornerRadius(Window element)
        {
            return (bool)element.GetValue(Win7UseCornerRadiusProperty);
        }

        public static void SetWin7UseCornerRadius(Window element, bool value)
        {
            element.SetValue(Win7UseCornerRadiusProperty, value);
        }

        static void Win7UseCornerRadiusPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender is Window window) && (e.NewValue is bool useRadius))
                GetWindowChromeHelper(window).Win7UseCornerRadiusChanged(useRadius);
        }

        public static readonly DependencyProperty Win7CornerRadiusProperty
            = DependencyProperty.RegisterAttached("Win7CornerRadius", typeof(CornerRadius), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(new CornerRadius(), Win7CornerRadiusPropertyChangedCallback));

        public static CornerRadius GetWin7CornerRadius(Window element)
        {
            return (CornerRadius)element.GetValue(Win7CornerRadiusProperty);
        }

        public static void SetWin7CornerRadius(Window element, CornerRadius value)
        {
            element.SetValue(Win7CornerRadiusProperty, value);
        }

        static void Win7CornerRadiusPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Window window)
                GetWindowChromeHelper(window).UpdateCornerRadius();
        }



        public static readonly DependencyProperty ExtendedTitleBarHeightProperty =
            DependencyProperty.RegisterAttached("ExtendedTitleBarHeight", typeof(double), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(0.0));

        public static double GetExtendedTitleBarHeight(Window element)
        {
            return (double)element.GetValue(ExtendedTitleBarHeightProperty);
        }

        public static void SetExtendedTitleBarHeight(Window element, double value)
        {
            element.SetValue(ExtendedTitleBarHeightProperty, value);
        }


        public static readonly DependencyProperty MaximizeInsetProperty =
            DependencyProperty.RegisterAttached("MaximizeInset", typeof(Thickness), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(ZERO_THICKNESS, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static Thickness GetMaximizeInset(Window element)
        {
            return (Thickness)element.GetValue(MaximizeInsetProperty);
        }

        public static void SetMaximizeInset(Window element, Thickness value)
        {
            element.SetValue(MaximizeInsetProperty, value);
        }



        static void SetButtonStuff(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Button btn)
            {
                var inProp = e.Property;

                DependencyProperty outProp = null;
                RoutedEventHandler handler = null;

                if (inProp == ClosesWindowProperty)
                {
                    outProp = IsCloseButtonForProperty;
                    handler = CloseButton_Click;
                }
                else if (inProp == MaximizesWindowProperty)
                {
                    outProp = IsMaximizeButtonForProperty;
                    handler = MaximizeButton_Click;
                }
                else if (inProp == MinimizesWindowProperty)
                {
                    outProp = IsMinimizeButtonForProperty;
                    handler = MinimizeButton_Click;
                }

                if ((e.NewValue is Window newWin) && (newWin != null))
                {
                    btn.Click += handler;
                    newWin.SetValue(outProp, btn);
                }

                if ((e.OldValue is Window oldWin) && (oldWin != null))
                {
                    btn.Click -= handler;
                    if (oldWin.GetValue(outProp) == btn)
                        oldWin.SetValue(outProp, null);
                }
            }
        }




        static void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                GetClosesWindow(btn).Close();
        }

        static void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var win = GetMaximizesWindow(btn);
                win.WindowState = (win.WindowState != WindowState.Maximized) ? WindowState.Maximized : WindowState.Normal;
            }
        }

        static void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var win = GetMinimizesWindow(btn);
                win.WindowState = (win.WindowState != WindowState.Minimized) ? WindowState.Minimized : WindowState.Normal;
            }
        }




        IntPtr _hwnd = IntPtr.Zero;

        protected override void OnAttached()
        {
            base.OnAttached();

            var interopHelper = new WindowInteropHelper(AssociatedObject);
            _hwnd = interopHelper.EnsureHandle();
            
            CustomDecorationsChanged(GetUseCustomDecorations(AssociatedObject));
            UseReRenderHackChanged(GetUseReRenderHack(AssociatedObject));

            if (AssociatedObject.IsInitialized && AssociatedObject.IsLoaded)
                StateChanged();
            

            HwndSource.FromHwnd(_hwnd).AddHook(new HwndSourceHook(WndProc));
            /*var closesDesc = DependencyPropertyDescriptor.FromProperty(ClosesWindowProperty, typeof(Button));
            closesDesc.AddValueChanged(this, (o, e) =>
            {
                if (o is Button btn)
                    SetButtonStuff(btn, GetClosesWindow(btn), IsCloseButtonForProperty, CloseButton_Click);
            });


            var maximizesDesc = DependencyPropertyDescriptor.FromProperty(MaximizesWindowProperty, typeof(Button));
            maximizesDesc.AddValueChanged(this, (o, e) =>
            {
                if (o is Button btn)
                    SetButtonStuff(btn, GetMaximizesWindow(btn), IsMaximizeButtonForProperty, MaximizeButton_Click);
            });


            var minimizesDesc = DependencyPropertyDescriptor.FromProperty(MinimizesWindowProperty, typeof(Button));
            minimizesDesc.AddValueChanged(this, (o, e) =>
            {
                if (o is Button btn)
                    SetButtonStuff(btn, GetMinimizesWindow(btn), IsMinimizeButtonForProperty, MinimizeButton_Click);
            });*/
        }

        
        void CustomDecorationsChanged(bool useCustomDecorations)
        {
            if (_hwnd != IntPtr.Zero)
            {
#if TRANSPARENT_WINDOW
                HwndSource.FromHwnd(_hwnd).CompositionTarget.BackgroundColor = useCustomDecorations ? Colors.Transparent : Colors.Black;

                if (!SafeSetWindowCompositionAttribute(useCustomDecorations))
                {
                    SetWin7UseCornerRadius(AssociatedObject, useCustomDecorations);
                }
#else
                NativeMethods.SetWindowRgn(_hwnd, IntPtr.Zero, true);
#endif

                if (useCustomDecorations)
                    AssociatedObject.StateChanged += Window_StateChangedCustomDeco;
                else
                    AssociatedObject.StateChanged -= Window_StateChangedCustomDeco;
            }
        }

        void UseReRenderHackChanged(bool useReRenderHack)
        {
            if (useReRenderHack)
            {
                AssociatedObject.SizeChanged += Window_SizeChangedHack;
                AssociatedObject.StateChanged += Window_StateChangedHack;
            }
            else
            {
                AssociatedObject.SizeChanged -= Window_SizeChangedHack;
                AssociatedObject.StateChanged -= Window_StateChangedHack;
            }
        }

        bool _win7useCornerRadius = false;
        void Win7UseCornerRadiusChanged(bool win7useCornerRadius)
        {
            //NativeMethods.SetWindowLong(GetWindowLong)
            _win7useCornerRadius = win7useCornerRadius;
            
            if (!_win7useCornerRadius)
            {
                NativeMethods.SetWindowRgn(_hwnd, IntPtr.Zero, true);
            }
        }

        const NativeMethods.CombineRgnStyles COMBINE = NativeMethods.CombineRgnStyles.RgnOr;
        public IntPtr WndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled)
        {
            if (msg == NativeMethods.WmSize)
            {
                if (_win7useCornerRadius)
                {
                    if (wParam.ToInt32() != NativeMethods.SizeMaximized)
                    {
                        int lP = lParam.ToInt32();
                        
                        //IntPtr rgn = NativeMethods.CreateRoundRectRgn(10, 10, (lP & 0xFFFF) - 20, (lP >> 16) - 20, 20, 20);
                        int width = (lP & 0xFFFF);
                        int height = (lP >> 16);

                        //CornerCurves curv = AttachedProperties.GetCornerCurves(AssociatedObject);
                        UpdateCornerRadius(width, height, false);
                    }
                    else
                        UpdateCornerRadius();
                }
            }

            return IntPtr.Zero;
        }

        void UpdateCornerRadius()
        {
            if (AssociatedObject.WindowState == WindowState.Maximized)
                UpdateCornerRadius(0, 0, true);
            else
                UpdateCornerRadius(SystemScaling.WpfUnitsToRealPixels(AssociatedObject.Width), SystemScaling.WpfUnitsToRealPixels(AssociatedObject.Height), false);
        }

        void UpdateCornerRadius(int width, int height, bool maximized)
        {
            IntPtr completeRegion = IntPtr.Zero;

            if ((!maximized) && GetWin7UseCornerRadius(AssociatedObject))
            {
                int topLeft = 0;
                int topRight = 0;
                int bottomRight = 0;
                int bottomLeft = 0;

                CornerRadius radius = GetWin7CornerRadius(AssociatedObject);
                
                if (radius != null)
                {
                    topLeft = SystemScaling.WpfUnitsToRealPixels(radius.TopLeft);
                    topRight = SystemScaling.WpfUnitsToRealPixels(radius.TopRight);
                    bottomRight = SystemScaling.WpfUnitsToRealPixels(radius.BottomRight);
                    bottomLeft = SystemScaling.WpfUnitsToRealPixels(radius.BottomLeft);
                }
                
                /*CornerRadius radiusRaw = (chrome != null) ? chrome.CornerRadius : new CornerRadius(0);
                int topLeft = SystemScaling.WpfUnitsToRealPixels(curv.TopLeft ? radiusRaw.TopLeft : 0);
                int topRight = SystemScaling.WpfUnitsToRealPixels(curv.TopRight ? radiusRaw.TopRight : 0);
                int bottomRight = SystemScaling.WpfUnitsToRealPixels(curv.BottomRight ? radiusRaw.BottomRight : 0);
                int bottomLeft = SystemScaling.WpfUnitsToRealPixels(curv.BottomLeft ? radiusRaw.BottomLeft : 0);*/
                
                int leftEdgeRight = Math.Max(topLeft, bottomLeft);
                int topEdgeBottom = Math.Max(topLeft, topRight);

                int rightEdgeMax = Math.Max(topRight, bottomRight);
                int rightEdgeLeft = width - rightEdgeMax;
                
                int bottomEdgeMax = Math.Max(bottomLeft, bottomRight);
                int bottomEdgeTop = height - bottomEdgeMax;

                
                completeRegion = NativeMethods.CreateRectRgn(leftEdgeRight, topEdgeBottom, rightEdgeLeft, bottomEdgeTop);


                if (leftEdgeRight > 0)
                {
                    IntPtr leftEdgeRect = NativeMethods.CreateRectRgn(0, topLeft, leftEdgeRight, height - bottomLeft);
                    NativeMethods.CombineRgn(completeRegion, leftEdgeRect, completeRegion, COMBINE);
                }

                if (topEdgeBottom > 0)
                {
                    IntPtr topEdgeRect = NativeMethods.CreateRectRgn(topLeft, 0, width - topRight, topEdgeBottom);
                    NativeMethods.CombineRgn(completeRegion, topEdgeRect, completeRegion, COMBINE);
                }

                if (rightEdgeMax > 0)
                {
                    IntPtr rightEdgeRect = NativeMethods.CreateRectRgn(rightEdgeLeft, topRight, width, height - bottomRight);
                    NativeMethods.CombineRgn(completeRegion, rightEdgeRect, completeRegion, COMBINE);
                }

                if (bottomEdgeMax > 0)
                {
                    IntPtr bottomEdgeRect = NativeMethods.CreateRectRgn(bottomLeft, bottomEdgeTop, width - bottomRight, height);
                    NativeMethods.CombineRgn(completeRegion, bottomEdgeRect, completeRegion, COMBINE);
                }


                width++;
                height++;
                
                int topLeftD = topLeft * 2;
                int topRightD = topRight * 2;
                int bottomRightD = bottomRight * 2;
                int bottomLeftD = bottomLeft * 2;

                IntPtr topLeftEllipse = NativeMethods.CreateEllipticRgn(0, 0, topLeftD, topLeftD);
                IntPtr topRightEllipse = NativeMethods.CreateEllipticRgn(width - topRightD, 0, width, topRightD);
                IntPtr bottomRightEllipse = NativeMethods.CreateEllipticRgn(width - bottomRightD, height - bottomRightD, width, height);
                IntPtr bottomLeftEllipse = NativeMethods.CreateEllipticRgn(0, height - bottomLeftD, bottomLeftD, height);

                if (topLeft > 0)
                    NativeMethods.CombineRgn(completeRegion, topLeftEllipse, completeRegion, COMBINE);
                
                if (topRight > 0)
                    NativeMethods.CombineRgn(completeRegion, topRightEllipse, completeRegion, COMBINE);
                
                if (bottomRight > 0)
                    NativeMethods.CombineRgn(completeRegion, bottomRightEllipse, completeRegion, COMBINE);
                
                if (bottomLeft > 0)
                    NativeMethods.CombineRgn(completeRegion, bottomLeftEllipse, completeRegion, COMBINE);
            }
            
            NativeMethods.SetWindowRgn(_hwnd, completeRegion, true);
        }



        private void Window_SizeChangedHack(object sender, SizeChangedEventArgs e)
            => ReRenderHack();

        private void Window_StateChangedHack(object sender, EventArgs e)
            => ReRenderHack();

        private void ReRenderHack()
        {
            
            AssociatedObject.InvalidateMeasure();
            AssociatedObject.InvalidateArrange();
            AssociatedObject.InvalidateVisual();
            AssociatedObject.UpdateLayout();
        }
        


        private void Window_StateChangedCustomDeco(object sender, EventArgs e)
        {
            StateChanged();
        }

        
        double _prevWidth = -1;
        double _prevHeight = -1;
        int _wasMaximized = 0;
        
        void StateChanged()
        {
            if (AssociatedObject.WindowState == WindowState.Maximized)
            {
                _prevWidth = AssociatedObject.Width;
                _prevHeight = AssociatedObject.Height;
                _wasMaximized = 1;

                AssociatedObject.SizeChanged -= Window_SizeChangedNonMaximized;
                AssociatedObject.LocationChanged -= Window_LocationChangedNonMaximized;
                AssociatedObject.InvalidateVisual();
                AssociatedObject.UpdateLayout();

                AssociatedObject.Dispatcher.BeginInvoke(new Action(() =>
                {
                    AssociatedObject.InvalidateVisual();
                    AssociatedObject.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RefreshEdges();
                    }), System.Windows.Threading.DispatcherPriority.Render, null);

                }), System.Windows.Threading.DispatcherPriority.Render, null);
            }
            else
            {
                RefreshEdges();

                AssociatedObject.SizeChanged += Window_SizeChangedNonMaximized;
                AssociatedObject.LocationChanged += Window_LocationChangedNonMaximized;
            }
        }

        void Window_LocationChangedNonMaximized(object sender, EventArgs e)
            => RefreshEdges();

        bool IsFloating
        {
            get
            {
                BorderPresence presence = AttachedProperties.GetBorderPresence(AssociatedObject);
                return presence.Left && presence.Top && presence.Right && presence.Bottom;
            }
        }

        private void Window_SizeChangedNonMaximized(object sender, SizeChangedEventArgs e)
        {
            RefreshEdges();

            if (_wasMaximized != 0)
            {
            /*window.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (window.WindowState == WindowState.Normal)
                {*/
                    if (IsFloating)
                    {
                        if (_wasMaximized == 2)
                        {
                            if (AssociatedObject.Width != _prevWidth)
                            {
                                AssociatedObject.Width = _prevWidth;
                                _wasMaximized = 0;
                            }
                            
                            
                            if (AssociatedObject.Height != _prevHeight)
                            {
                                AssociatedObject.Height = _prevHeight;
                                _wasMaximized = 0;
                            }
                        }
                        else if (_wasMaximized == 1)
                        {
                            if ((AssociatedObject.Width == _prevWidth) && (AssociatedObject.Height != _prevHeight))
                                _wasMaximized = 2;
                        }
                    }
                //}
            //}), System.Windows.Threading.DispatcherPriority.Render, null);
            }
        }

        private void Window_LayoutUpdatedAfterMaximize(object sender, EventArgs e)
        {
            RefreshEdges();
            AssociatedObject.LayoutUpdated -= Window_LayoutUpdatedAfterMaximize;
        }

        void RefreshEdges()
        {
            Screen screen = Screen.FromHandle(new WindowInteropHelper(AssociatedObject).EnsureHandle());
            var work = screen.WorkingArea;
            
            Point targetTL = AssociatedObject.PointToScreen(new Point(0, 0));
            Point targetBR = AssociatedObject.PointToScreen(new Point(AssociatedObject.ActualWidth, AssociatedObject.ActualHeight));
            
            double leftInset = 0;
            double topInset = 0;
            double rightInset = 0;
            double bottomInset = 0;

            bool leftPresence = true;
            bool topPresence = true;
            bool rightPresence = true;
            bool bottomPresence = true;

            bool tl = true;
            bool tr = true;
            bool br = true;
            bool bl = true;

            if (AssociatedObject.WindowState == WindowState.Maximized)
            {
                leftInset = Math.Max(0, work.X - targetTL.X);
                topInset = Math.Max(0, work.Y - targetTL.Y);
                rightInset = Math.Max(0, -(work.Right - targetBR.X));
                bottomInset = Math.Max(0, -(work.Bottom - targetBR.Y));

                tl = false;
                tr = false;
                br = false;
                bl = false;
            }
            else
            {
                leftPresence = targetTL.X != work.X;
                topPresence = targetTL.Y != work.Y;
                rightPresence = targetBR.X != work.Right;
                bottomPresence = targetBR.Y != work.Bottom;

                tl = topPresence && leftPresence;
                tr = topPresence && rightPresence;
                br = bottomPresence && rightPresence;
                bl = bottomPresence && leftPresence;
            }
            SetMaximizeInset(AssociatedObject, new Thickness(leftInset, topInset, rightInset, bottomInset));
            AttachedProperties.SetBorderPresence(AssociatedObject, new BorderPresence(leftPresence, topPresence, rightPresence, bottomPresence));
            AttachedProperties.SetCornerCurves(AssociatedObject, new CornerCurves(tl, tr, br, bl));
        }

#if TRANSPARENT_WINDOW
        static Version _winVersion = new Func<Version>(() =>
        {
            NativeMethods.OSVERSIONINFOEXW info = new NativeMethods.OSVERSIONINFOEXW();
			NativeMethods.RtlGetVersion(ref info);
            return new Version(info.dwMajorVersion, info.dwMinorVersion, info.dwBuildNumber); //>= new Version(10, 0, 10240); //15063);
        })();


        bool SafeSetWindowCompositionAttribute(bool allowTransparency)
        {
            bool setAttr = false;
            if (
                    //(
                        (_winVersion.Major == 6) &&
                        (
                            (_winVersion.Minor >= 2) ||
                            (_winVersion.Minor == 3)
                        )
                    
                    
                    //Only on Windows 8 - SetWindowCompositionAttribute doesn't exist in 7, and Windows 10 is way too ever-evolving for calling it to be even remotely safe
                    
                    /*) ||
                    (_winVersion.Major > 6)*/
                )
            {
                try
                {
                    var accent = new NativeMethods.AccentPolicy();
                    var accentStructSize = Marshal.SizeOf(accent);

                    if (allowTransparency)
                    {
                        accent.AccentState = /*_useWin10AlphaComposition ? NativeMethods.AccentState.ACCENT_ENABLE_PER_PIXEL_ALPHA_I_GUESS : */
                        NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND_BUT_ITS_PER_PIXEL_ALPHA_ON_WINDOWS_8;
                        
                        /*if (_useWin10AlphaComposition)
                        {
                            accent.GradientColor = 0x00000000;
                        }*/
                    }
                    else
                        accent.AccentState = NativeMethods.AccentState.ACCENT_DISABLED;

                    var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                    Marshal.StructureToPtr(accent, accentPtr, false);

                    var data = new NativeMethods.WindowCompositionAttributeData
                    {
                        Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                        SizeOfData = accentStructSize,
                        Data = accentPtr
                    };

                    NativeMethods.SetWindowCompositionAttribute(_hwnd, ref data);
                    //NativeMethods.SetWindowLong(_hwnd, NativeMethods.GwlExstyle, NativeMethods.GetWindowLong(_hwnd, NativeMethods.GwlExstyle).ToInt32() & NativeMethods.WsExNoRedirectionBitmap);
                    setAttr = true;
                }
                catch (Exception ex)
                {
                    Cmd.WriteLine($"SetWindowCompositionAttribute failed!\n{ex}");
                }
            }
            return setAttr;
        }
#endif
    }


    public class IsAllTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;
            if (value is BorderPresence bp)
                retVal = bp.Left && bp.Top && bp.Right && bp.Bottom;
            else if (value is CornerCurves cc)
                retVal = cc.TopLeft && cc.TopRight && cc.BottomRight && cc.BottomLeft;
            else
                throw new NotImplementedException();

            return (parameter != null) ? !retVal : retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
