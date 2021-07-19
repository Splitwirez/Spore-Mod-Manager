using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Converters
{
    public class RevealBrushViewboxConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 5)
            {
                if ((values[0] is FrameworkElement target) && (values[1] is double cursorX) && (values[2] is double cursorY) && (values[3] is double effectWidth) && (values[4] is double effectHeight))
                    return new Rect((cursorX * -1) + (effectWidth / 2), (cursorY * -1) + (effectHeight / 2), target.ActualWidth, target.ActualHeight);
                else
                    throw new Exception("RevealBrushViewboxConverter: values[0] must be a FrameworkElement (" + (values[0] is FrameworkElement) + "), and values[1] (" + (values[1] is double) + "), values[2] (" + (values[2] is double) + "), values[3] (" + (values[3] is double) + "), values[4] (" + (values[4] is double) + ") must be doubles. ");// + values[1].GetType().ToString() + " " + values[2].GetType().ToString());
            }
            else
                throw new Exception("RevealBrushViewboxConverter: Not enough values.");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
