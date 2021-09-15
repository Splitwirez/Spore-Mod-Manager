using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using Colors = System.Windows.Media.Colors;
using Screen = System.Windows.Forms.Screen;
using Graphics = System.Drawing.Graphics;
using Point = System.Windows.Point;
using System.Runtime.InteropServices;
using Mechanism.Wpf.Core;
using System.Diagnostics;

namespace SporeMods.CommonUI
{
    public class WindowChromeHelper : DependencyObject
    {
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




        public WindowChromeHelper()
        {
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

        public static readonly DependencyProperty UseReRenderHackProperty =
            DependencyProperty.RegisterAttached("UseReRenderHack", typeof(bool), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(false, UseReRenderHackChanged));

        public static bool GetUseReRenderHack(Window element)
        {
            return (bool)element.GetValue(UseReRenderHackProperty);
        }

        public static void SetUseReRenderHack(Window element, bool value)
        {
            element.SetValue(UseReRenderHackProperty, value);
        }

        static void UseReRenderHackChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender is Window window) && (e.NewValue is bool useHack))
            {
                if (useHack)
                {
                    window.SizeChanged += Window_SizeChangedHack;
                    window.StateChanged += Window_StateChangedHack;
                }
                else
                {
                    window.SizeChanged -= Window_SizeChangedHack;
                    window.StateChanged -= Window_StateChangedHack;
                }
            }
        }

        private static void Window_SizeChangedHack(object sender, SizeChangedEventArgs e)
            => ReRenderHack(sender);

        private static void Window_StateChangedHack(object sender, EventArgs e)
            => ReRenderHack(sender);

        private static void ReRenderHack(object sender)
        {
            if (sender is Window window)
            {
                window.InvalidateMeasure();
                window.InvalidateArrange();
                window.InvalidateVisual();
                window.UpdateLayout();
            }
        }

        public static readonly DependencyProperty Win7UseCornerRadiusProperty =
            DependencyProperty.RegisterAttached("Win7UseCornerRadius", typeof(bool), typeof(WindowChromeHelper), new FrameworkPropertyMetadata(false));

        public static bool GetWin7UseCornerRadius(Window element)
        {
            return (bool)element.GetValue(Win7UseCornerRadiusProperty);
        }

        public static void SetWin7UseCornerRadius(Window element, bool value)
        {
            element.SetValue(Win7UseCornerRadiusProperty, value);
        }


        static void UseCustomDecorationsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender is Window window) && (e.NewValue is bool useCustomDecorations))
            {
                var interopHelper = new WindowInteropHelper(window);
                IntPtr hwnd = interopHelper.EnsureHandle();
                HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = useCustomDecorations ? Colors.Transparent : Colors.Black;

                if (!SafeSetWindowCompositionAttribute(hwnd, useCustomDecorations))
                {
                    SetWin7UseCornerRadius(window, useCustomDecorations);
                }

                if (useCustomDecorations)
                    window.StateChanged += Window_StateChangedCustomDeco;
                else
                    window.StateChanged -= Window_StateChangedCustomDeco;

                if (window.IsInitialized && window.IsLoaded)
                    StateChanged(window);
            }
        }

        private static void Window_StateChangedCustomDeco(object sender, EventArgs e)
        {
            if (sender is Window window)
                StateChanged(window);
        }

        static void StateChanged(Window window)
        {
            if (window.WindowState == WindowState.Maximized)
            {
                window.SizeChanged -= Window_SizeChangedNonMaximized;
                window.LocationChanged -= Window_LocationChangedNonMaximized;
                window.InvalidateVisual();
                window.UpdateLayout();

                window.Dispatcher.BeginInvoke(new Action(() => RefreshEdges(window)), System.Windows.Threading.DispatcherPriority.Render, null);

            }
            else
            {
                RefreshEdges(window);

                window.SizeChanged += Window_SizeChangedNonMaximized;
                window.LocationChanged += Window_LocationChangedNonMaximized;
            }
        }

        private static void Window_LocationChangedNonMaximized(object sender, EventArgs e)
        {
            if (sender is Window window)
                RefreshEdges(window);
        }

        private static void Window_SizeChangedNonMaximized(object sender, SizeChangedEventArgs e)
        {
            if (sender is Window window)
                RefreshEdges(window);
        }

        private static void Window_LayoutUpdatedAfterMaximize(object sender, EventArgs e)
        {
            if (sender is Window window)
            {
                RefreshEdges(window);
                window.LayoutUpdated -= Window_LayoutUpdatedAfterMaximize;
            }
        }

        static readonly Thickness ZERO_THICKNESS = new Thickness(0);
        static void RefreshEdges(Window window)
        {
            Screen screen = Screen.FromHandle(new WindowInteropHelper(window).EnsureHandle());
            var work = screen.WorkingArea;
            
            Point targetTL = window.PointToScreen(new Point(0, 0));
            Point targetBR = window.PointToScreen(new Point(window.ActualWidth, window.ActualHeight));
            
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

            if (window.WindowState == WindowState.Maximized)
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
            SetMaximizeInset(window, new Thickness(leftInset, topInset, rightInset, bottomInset));
            AttachedProperties.SetBorderPresence(window, new BorderPresence(leftPresence, topPresence, rightPresence, bottomPresence));
            AttachedProperties.SetCornerCurves(window, new CornerCurves(tl, tr, br, bl));
        }

        static bool SafeSetWindowCompositionAttribute(IntPtr hwnd, bool allowTransparency)
        {
            bool setAttr = false;
            if (
                    (
                        (Environment.OSVersion.Version.Major == 6) &&
                        (Environment.OSVersion.Version.Minor >= 2)
                    ) ||
                    (Environment.OSVersion.Version.Major > 6)
                )
            {
                try
                {
                    var accent = new NativeMethods.AccentPolicy();
                    var accentStructSize = Marshal.SizeOf(accent);
                    accent.AccentState = allowTransparency ? NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND : NativeMethods.AccentState.ACCENT_DISABLED;

                    var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                    Marshal.StructureToPtr(accent, accentPtr, false);

                    var data = new NativeMethods.WindowCompositionAttributeData
                    {
                        Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                        SizeOfData = accentStructSize,
                        Data = accentPtr
                    };

                    NativeMethods.SetWindowCompositionAttribute(hwnd, ref data);
                    setAttr = true;
                }
                catch (Exception ex)
                {
                }
            }
            return setAttr;
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

#if NO
    public class WindowInsetConverter : IMultiValueConverter
    {
        public static WindowInsetConverter Instance { get; } = new WindowInsetConverter();
        private WindowInsetConverter()
        { }


        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            //Control target = values[0] as Control;
            Window window = values[1] as Window;
            Screen screen = Screen.FromHandle(new WindowInteropHelper(window).EnsureHandle());
            var work = screen.WorkingArea;
            //Rect work = new Rect(SystemScaling.RealPixelsToWpfUnits(workRaw.X), SystemScaling.RealPixelsToWpfUnits(workRaw.Y), SystemScaling.RealPixelsToWpfUnits(workRaw.Width), SystemScaling.RealPixelsToWpfUnits(workRaw.Height));
            Point targetTL = target.PointToScreen(new Point(0, 0));
            Point targetBR = target.PointToScreen(new Point(target.ActualWidth, target.ActualHeight));
            //Rect target = new Rect(targetTL.X, targetTL.Y, )
            double l = work.X - targetTL.X;
            double t = work.Y - targetTL.Y;
            double r = targetBR.X - work.Right;
            double b = targetBR.Y - work.Bottom;
            return new Thickness(l, t, r, b);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
#endif

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("dwmapi.dll")]
        static extern Int32 DwmIsCompositionEnabled(out Boolean enabled);

        public static bool DwmIsCompositionEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                DwmIsCompositionEnabled(out bool returnValue);
                return returnValue;
            }
            else
                return false;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }
    }
}
