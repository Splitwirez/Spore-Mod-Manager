using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace SporeMods.CommonUI
{
    public class EqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            /*bool ret = */
            return values[0] == values[1];
            /*return (parameter == null)
                ? ret
                : !ret
            ;*/
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}