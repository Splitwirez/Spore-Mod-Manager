using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class BorderPresenceToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brdp = (BorderPresence)value;

            string[] param0 = parameter.ToString().Split(';');
            string[] param1 = param0[0].Split(',');
            string[] param2 = param0[1].Split(',');

            double left = Double.Parse(param1[0]);
            if (!brdp.Left)
                left = Double.Parse(param2[0]);

            double top = Double.Parse(param1[1]);
            if (!brdp.Top)
                top = Double.Parse(param2[1]);

            double right = Double.Parse(param1[2]);
            if (!brdp.Right)
                right = Double.Parse(param2[2]);

            double bottom = Double.Parse(param1[3]);
            if (!brdp.Bottom)
                bottom = Double.Parse(param2[3]);

            return new Thickness(left, top, right, bottom);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
