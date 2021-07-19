using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class DoublesLimitToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((parameter is string param) && (param.ToLowerInvariant() == "min"))
            {
                double min = double.MaxValue;
                foreach (double d in values)
                {
                    if (d < min)
                        min = d;
                }
                return min;
            }
            else
            {
                double max = double.MinValue;
                foreach (double d in values)
                {
                    if (d > max)
                        max = d;
                }
                return max;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
