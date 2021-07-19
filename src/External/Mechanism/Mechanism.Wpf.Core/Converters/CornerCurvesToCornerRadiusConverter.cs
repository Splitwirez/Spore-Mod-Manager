using Mechanism.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class CornerCurvesToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var curves = (CornerCurves)value;
            //Debug.WriteLine("curves.TopLeft: " + curves.TopLeft.ToString());

            string[] param0 = parameter.ToString().Split(';');
            string[] param1 = param0[0].Split(',');
            string[] param2 = param0[1].Split(',');

            double topLeft = Double.Parse(param1[0]);
            if (!curves.TopLeft)
                topLeft = Double.Parse(param2[0]);

            double topRight = Double.Parse(param1[1]);
            if (!curves.TopRight)
                topRight = Double.Parse(param2[1]);

            double bottomRight = Double.Parse(param1[2]);
            if (!curves.BottomRight)
                bottomRight = Double.Parse(param2[2]);

            double bottomLeft = Double.Parse(param1[3]);
            if (!curves.BottomLeft)
                bottomLeft = Double.Parse(param2[3]);

            /*Debug.WriteLine("curves: " + curves.TopLeft + ", " + curves.TopRight + ", " + curves.BottomRight + ", " + curves.BottomLeft);
            Debug.WriteLine("param1: ");
            for (int i = 0; i < param1.Count(); i++)
            {
                Debug.WriteLine(i.ToString() + ": " + param1[i]);
            }
            Debug.WriteLine("param2: ");
            for (int i = 0; i < param2.Count(); i++)
            {
                Debug.WriteLine(i.ToString() + ": " + param2[i]);
            }
            Debug.WriteLine("output: " + topLeft.ToString() + ", " + topRight.ToString() + ", " + bottomRight.ToString() + ", " + bottomLeft.ToString());*/

            return new CornerRadius(topLeft, topRight, bottomRight, bottomLeft);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
