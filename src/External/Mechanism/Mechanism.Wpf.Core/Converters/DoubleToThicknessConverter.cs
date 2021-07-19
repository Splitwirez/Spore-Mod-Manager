using System;
using System.Globalization;
using System.Windows.Data;

using System.Windows;

namespace Mechanism.Wpf.Core.Converters
{
    public class DoubleToThicknessConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            double val = double.Parse(value.ToString());
            Thickness output = new Thickness(0);
            var param = parameter.ToString().ToLower();

            if (param.Contains("l"))
                output.Left = val;

            if (param.Contains("t"))
                output.Top = val;

            if (param.Contains("r"))
                output.Right = val;

            if (param.Contains("b"))
                output.Bottom = val;

            return output;
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            Thickness val = (Thickness)value;
            var param = parameter.ToString().ToLower();
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
    }
}
