using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Mechanism.Wpf.Core.Converters
{
    public class ThicknessFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness val = (Thickness)value;
            var param = parameter.ToString().ToLower().ToCharArray();
            if (param[0] == 'i')
                val.Left = val.Left * -1;
            else if (param[0] == 'z')
                val.Left = 0;

            if (param[1] == 'i')
                val.Top = val.Top * -1;
            else if (param[1] == 'z')
                val.Top = 0;

            if (param[2] == 'i')
                val.Right = val.Right * -1;
            else if (param[2] == 'z')
                val.Right = 0;

            if (param[3] == 'i')
                val.Bottom = val.Bottom * -1;
            else if (param[3] == 'z')
                val.Bottom = 0;

            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}