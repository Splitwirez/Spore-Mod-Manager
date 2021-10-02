using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace SporeMods.CommonUI
{
    public class BoolToDoubleOrNothingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool bVal))
                throw new InvalidOperationException();
            else if (!bVal)
                return 0;
            else if (parameter is double param)
                return param;
            else if ((parameter != null) && double.TryParse(parameter.ToString(), out param))
                return param;
            else
                return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
