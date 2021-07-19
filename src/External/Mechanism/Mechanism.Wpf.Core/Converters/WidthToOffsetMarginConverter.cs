using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Mechanism.Wpf.Core.Converters
{
    public class WidthToOffsetMarginConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            var param = parameter.ToString();
            if (param == "Top")
            {
                return new Thickness(0, (Double)value, 0, (Double)value * -1);
            }
            else if (param == "Right")
            {
                return new Thickness((Double)value * -1, 0, (Double)value, 0);
            }
            else if (param == "Bottom")
            {
                return new Thickness(0, (Double)value * -1, 0, (Double)value);
            }
            else
            {
                return new Thickness((Double)value, 0, (Double)value * -1, 0);
            }
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            var param = parameter.ToString();
            if (param == "Top")
            {
                return (Double)(((Thickness)(value)).Top);
            }
            else if (param == "Right")
            {
                return (Double)(((Thickness)(value)).Right);
            }
            else if (param == "Bottom")
            {
                return (Double)(((Thickness)(value)).Bottom);
            }
            else
            {
                return (Double)(((Thickness)(value)).Left); //return new Thickness((double)value, 0, (double)value * -1, 0);
            }
        }
    }
}
