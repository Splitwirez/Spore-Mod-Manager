using SporeMods.CommonUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SporeMods.CommonUI
{
    public class CornerCurvesToCornerRadiusConverter : IValueConverter
    {
        static readonly CornerRadiusConverter _RAD_CONV = new CornerRadiusConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var curves = (CornerCurves)value;
            //Debug.WriteLine("curves.TopLeft: " + curves.TopLeft.ToString());

            string[] param0 = parameter.ToString().Split(';');

            CornerRadius trueRadius = (CornerRadius)_RAD_CONV.ConvertFrom(null, CultureInfo.InvariantCulture, param0[0]);
            CornerRadius falseRadius = (CornerRadius)_RAD_CONV.ConvertFrom(null, CultureInfo.InvariantCulture, param0[1]);
            
            //string[] param1 = param0[0].Split(',');
            //string[] param2 = param0[1].Split(',');


            double topLeft = trueRadius.TopLeft;
            if (!curves.TopLeft)
                topLeft = falseRadius.TopLeft;

            double topRight = trueRadius.TopRight;
            if (!curves.TopRight)
                topRight = falseRadius.TopRight;

            double bottomRight = trueRadius.BottomRight;
            if (!curves.BottomRight)
                bottomRight = falseRadius.BottomRight;

            double bottomLeft = trueRadius.BottomLeft;
            if (!curves.BottomLeft)
                bottomLeft = falseRadius.BottomLeft;

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
