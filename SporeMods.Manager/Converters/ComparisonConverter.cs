using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace SporeMods.Manager
{
	public class ComparisonConverter : IValueConverter
	{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var retVal = value?.Equals(parameter);
            Console.WriteLine($"CONVERT: {retVal}\n{value}, {targetType}, {parameter}, {culture}\n");
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var retVal = value?.Equals(true) == true ? parameter : BindingValueType.DoNothing;
            Console.WriteLine($"CBACK: {retVal}\n{value}, {targetType}, {parameter}, {culture}\n");
            return retVal;
        }
    }
}