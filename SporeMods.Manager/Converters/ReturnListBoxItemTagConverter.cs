using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace SporeMods.Manager
{
	public class ReturnListBoxItemTagConverter : IValueConverter
	{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Control ctrl)
                return ctrl.Tag == value;
            else
                throw new Exception("what");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingValueType.DoNothing;
        }
    }
}