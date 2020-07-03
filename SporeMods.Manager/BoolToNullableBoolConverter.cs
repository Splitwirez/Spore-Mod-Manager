using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SporeMods.Manager
{
    public class BoolToNullableBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? retVal = null;
            if ((bool)value)
                retVal = true;
            else
                retVal = false;

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((bool?)value).Value)
                return true;
            else
                return false;
        }
    }
}
