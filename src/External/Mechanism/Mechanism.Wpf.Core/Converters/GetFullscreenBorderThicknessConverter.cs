using Mechanism.Wpf.Core;
using Mechanism.Wpf.Core.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class GetFullscreenBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var win = value as CompositingWindow;


            NativeMethods.GetWindowRect(win.Handle, out NativeMethods.RECT winRect);

            Debug.WriteLine("WINRECT: " + winRect.Top.ToString() + ", " + winRect.Left.ToString());

            double horizontalHeight = winRect.Left * -1;
            double verticalWidth = winRect.Top * -1;
            return new Thickness(horizontalHeight, verticalWidth, horizontalHeight, verticalWidth); //(winRect.Right - winRect.Left) * -1, (winRect.Bottom - winRect.Top) * -1
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
