using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class ListViewItemIndexToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values[0] != null) && (values[1] != null))
            {
                FrameworkElement item = values[0] as FrameworkElement;
                Panel panel = values[1] as Panel;
                string position = (string)parameter;// (values[2]);

                bool returnValue = false;

                if (panel != null)
                {
                    int index = panel.Children.IndexOf(item);

                    if (position.ToLowerInvariant() == "first")
                        returnValue = index == 0;
                    else if (position.ToLowerInvariant() == "last")
                        returnValue = index == (panel.Children.Count - 1);
                    else if (position.ToLowerInvariant() == "only")
                        returnValue = (panel.Children.Count == 1) && (index == 0);
                }

                //Debug.WriteLine("conditions: " + (position.ToLowerInvariant() == "first").ToString() + ", " + (position.ToLowerInvariant() == "last").ToString() + ", " + (position.ToLowerInvariant() == "only").ToString() + "; values: " + (item != null).ToString() + ", " + (panel != null).ToString() + "; parameter: " + position.ToLowerInvariant() + "; returnValue: " + returnValue.ToString());
                //conditions: False, True; values: True, True; parameter:last; returnValue: False
                return returnValue;
            }
            else
                return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}