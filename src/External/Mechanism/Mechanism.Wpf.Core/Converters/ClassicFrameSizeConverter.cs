using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    /*public class ClassicFrameSizeConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
#if NET40
            var frame = new Thickness(2.5); // TODO: fix
#else
            var frame = SystemParameters.WindowResizeBorderThickness;
#endif
            return frame;
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }*/

    public class ClassicFrameSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var border = (Thickness)values[0];
            return new Thickness(border.Left, border.Top + (double)values[1], border.Right, border.Bottom);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
