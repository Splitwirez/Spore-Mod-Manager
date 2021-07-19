using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class RectangleToAspectRatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Rectangle rect = (Rectangle)value;
            return ((double)rect.Width) / ((double)rect.Height);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
