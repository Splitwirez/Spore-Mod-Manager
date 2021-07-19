using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Mechanism.Wpf.Core.Converters
{
    public class SolidColorBrushToColorConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            var color = Color.FromArgb(0xFF, 0xFF, 0x00, 0xFF);

            if ((value != null) && (value is SolidColorBrush))
                color = (value as SolidColorBrush).Color;
            else if ((parameter != null) && (parameter is Color))
                color = (Color)parameter;

            return color;
        }

        public Object ConvertBack(Object value, Type targetTypes, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
