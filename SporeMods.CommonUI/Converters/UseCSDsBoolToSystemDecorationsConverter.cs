using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace SporeMods.CommonUI
{
	public class UseCSDsBoolToSystemDecorationsConverter : IValueConverter
	{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? SystemDecorations.BorderOnly : SystemDecorations.Full;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}