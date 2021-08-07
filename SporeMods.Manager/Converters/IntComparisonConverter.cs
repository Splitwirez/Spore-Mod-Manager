using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace SporeMods.Manager
{
	public class IntComparisonConverter : IValueConverter
	{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == int.Parse(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((bool?)value).Value)
                return int.Parse(parameter.ToString());
            else
                return BindingValueType.UnsetValue;
        }
    }
}