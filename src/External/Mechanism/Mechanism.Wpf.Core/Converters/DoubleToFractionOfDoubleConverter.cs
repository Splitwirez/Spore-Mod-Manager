using System;
using System.Globalization;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class DoubleToFractionOfDoubleConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            double val = 10;

            if (value is double)
                val = (double)value;
            else if (value is int)
                val = (int)value;
            else if (value is string)
                val = Double.Parse(value.ToString());

            double fraction = 2;

            if (parameter is double)
                fraction = (double)value;
            else if (parameter is int)
                fraction = (int)value;
            else if (parameter is string)
                fraction = Double.Parse(parameter.ToString());

            /*if (parameter == null)
            {
                return val / 2;
            }
            else
            {
                var paramString = (String)parameter;
                return val / System.Convert.ToDouble(paramString);
            }*/
            return val / fraction;
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                return ((Double)value) * 2;
            }
            else
            {
                var paramString = (String)parameter;
                return ((Double)value) * System.Convert.ToDouble(paramString);
            }
        }
    }
}
