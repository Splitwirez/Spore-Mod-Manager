using System;
using System.Globalization;
using System.Windows.Data;

using System.Windows;
using System.Diagnostics;

namespace Mechanism.Wpf.Core.Converters
{
    public class ThicknessToDoubleConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            Thickness val = (Thickness)value;
            var param = parameter.ToString().ToLower();
            //Debug.WriteLine("val: " + val);
            if (param == "t")
            {
                return val.Top;
            }
            else if (param == "r")
            {
                return val.Right;
            }
            else if (param == "b")
            {
                return val.Bottom;
            }
            else
            {
                return val.Left;
            }
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            var param = parameter.ToString().ToLower();
            if (param == "t")
            {
                return (Double)(((Thickness)(value)).Top);
            }
            else if (param == "r")
            {
                return (Double)(((Thickness)(value)).Right);
            }
            else if (param == "b")
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
