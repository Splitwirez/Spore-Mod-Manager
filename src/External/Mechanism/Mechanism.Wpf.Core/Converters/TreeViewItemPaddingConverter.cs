using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class TreeViewItemPaddingConverter : IValueConverter
    {
        public Object Convert(
            Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            Thickness basePadding = ((Thickness)(value));
            var param = Double.Parse(parameter.ToString());
            return new Thickness(basePadding.Left + param, basePadding.Top, basePadding.Right, basePadding.Bottom);
        }

        public Object ConvertBack(
            Object value, Type targetTypes, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
