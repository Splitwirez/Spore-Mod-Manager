using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class DoubleComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            //double val = double.Parse(value.ToString());
            double val = ConversionInputHelper.GetDouble(value);
            int paramFirstNumerical = 0;
            for (int i = 0; i < parameter.ToString().Length; i++)
            {
                if (char.IsNumber(parameter.ToString().ElementAt(i)))
                {
                    paramFirstNumerical = i;
                    break;
                }
            }

            double param = double.Parse(parameter.ToString().Substring(paramFirstNumerical));

            string opr = parameter.ToString().Substring(0, paramFirstNumerical).ToLowerInvariant();

            if (opr == "g")
                return val > param;
            else if (opr == "l")
            return val < param;
            else if (opr == "e")
                return val == param;
            else if (opr == "ge")
                return val >= param;
            else if (opr == "le")
                return val <= param;
            else
                return val >= param;
        }

        public object ConvertBack(object value, Type targetType,
        object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
