using System;
using System.Globalization;
using System.Windows.Data;

using System.Windows;
using System.Diagnostics;

namespace Mechanism.Wpf.Core.Converters
{
    public class ThicknessInvertFilterConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            Thickness val = (Thickness)value;
            var param = parameter.ToString().ToLower();
            //Debug.WriteLine("val: " + val);
            if (param.Contains("t"))
            {
                val.Top = val.Top * -1;
            }
            if (param.Contains("r"))
            {
                val.Right = val.Right * -1;
            }
            if (param.Contains("b"))
            {
                val.Bottom = val.Bottom * -1;
            }
            if (param.Contains("l"))
            {
                val.Left = val.Left * -1;
            }
            return val;
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
