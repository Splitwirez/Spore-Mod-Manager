using Mechanism.Wpf.Core;
using Mechanism.Wpf.Core.Windows;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class GetMaximizeBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            Debug.WriteLine("CONVERTING");
            //var win = value as CompositingWindow;
            //Debug.WriteLine("e");
            //NativeMethods.DwmGetWindowAttribute(win.Handle, NativeMethods.DwmWindowAttribute.ExtendedFrameBounds, out NativeMethods.RECT dwmRect, Marshal.SizeOf(typeof(NativeMethods.RECT))); //sizeof(NativeMethods.RECT)
            //DwmGetWindowAttribute
            //Debug.WriteLine("winRect: " + winRect.Left.ToString() + ", " + winRect.Top.ToString() + ", " + winRect.Right.ToString() + ", " + winRect.Bottom.ToString());
            //Debug.WriteLine("dwmRect: " + dwmRect.Left.ToString() + ", " + dwmRect.Top.ToString() + ", " + dwmRect.Right.ToString() + ", " + dwmRect.Bottom.ToString());
            //double verticalWidth = SystemScaling.RealPixelsToWpfUnits(NativeMethods.GetSystemMetrics(46)); //+ NativeMethods.GetSystemMetrics(32)); //32
            //double horizontalHeight = SystemScaling.RealPixelsToWpfUnits(/*NativeMethods.GetSystemMetrics(45) + */NativeMethods.GetSystemMetrics(33)); //33
            //double borderSize = SystemParameters.ResizeFrameVerticalBorderWidth + SystemParameters.FixedFrameVerticalBorderWidth - SystemParameters.BorderWidth;
            //Mechanism.Wpf.Core.MonitorInfo.AllMonitors

            NativeMethods.Rect monBounds = NativeMethods.MonitorFromHandle((value as CompositingWindow).Handle).rcMonitor;

            //var screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)SystemScaling.WpfUnitsToRealPixels((value as CompositingWindow).Left), (int)SystemScaling.WpfUnitsToRealPixels((value as CompositingWindow).Top)));

            double verticalWidth = SystemScaling.RealPixelsToWpfUnits(monBounds.Left);// - winRect.Left;
            double horizontalHeight = SystemScaling.RealPixelsToWpfUnits(monBounds.Top);// - winRect.Top;
            return new Thickness(verticalWidth, horizontalHeight, (verticalWidth) * -1, (horizontalHeight) * -1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
