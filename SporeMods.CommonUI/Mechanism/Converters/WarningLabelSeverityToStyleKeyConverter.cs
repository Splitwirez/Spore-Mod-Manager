using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace SporeMods.CommonUI
{
    public class WarningLabelSeverityToStyleKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"Mod{((ModWarningLabel.WarningLabelSeverity)value)}LabelZoneStyle";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
