using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace SporeMods.CommonUI
{
    public class BorderPresenceToThicknessConverter : IValueConverter
    {
        static readonly ThicknessConverter _THC_CONV = new ThicknessConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brdp = (BorderPresence)value;

            string[] param0 = parameter.ToString().Split(';');

            Thickness trueThickness = (Thickness)_THC_CONV.ConvertFrom(null, CultureInfo.InvariantCulture, param0[0]);
            Thickness falseThickness = (Thickness)_THC_CONV.ConvertFrom(null, CultureInfo.InvariantCulture, param0[1]);
            /*string[] param1 = param0[0].Split(',');
            string[] param2 = param0[1].Split(',');*/

            double left = trueThickness.Left;
            if (!brdp.Left)
                left = falseThickness.Left;

            double top = trueThickness.Top;
            if (!brdp.Top)
                top = falseThickness.Top;

            double right = trueThickness.Right;
            if (!brdp.Right)
                right = falseThickness.Right;

            double bottom = trueThickness.Bottom;
            if (!brdp.Bottom)
                bottom = falseThickness.Bottom;

            return new Thickness(left, top, right, bottom);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
