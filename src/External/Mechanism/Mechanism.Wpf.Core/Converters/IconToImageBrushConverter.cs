using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Mechanism.Wpf.Core.SystemScaling;

namespace Mechanism.Wpf.Core.Converters
{
    public class IconToImageBrushConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            var icon = (value as Icon);
            if (icon != null)
            {
                int param = 32;

                /*if (parameter != null)
                    Debug.WriteLine("parameter type: " + parameter.GetType().ToString());*/

                int validateParam = param;

                if (parameter != null)
                    /*Debug.WriteLine("param parse outcome: " + */int.TryParse((string)parameter, out validateParam)/* + " " + param.ToString())*/;

                if (validateParam > 0)
                    param = validateParam;

                int targetSize = param;

                if (param >= 256)//Mechanism.Wpf.Core.Statics.SystemScaling.ScalingFactor > 1)
                    targetSize = WpfUnitsToRealPixels(param);

                try
                {
                    return new ImageBrush(Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(targetSize, targetSize)));
                }
                catch (COMException ex)
                {
                    Debug.WriteLine("IconToImageBrushConverter.Convert machine broke: \n" + ex);
                    try
                    {
                        return new ImageBrush(Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, 16, 16), BitmapSizeOptions.FromEmptyOptions()));
                    }
                    catch (COMException exc)
                    {
                        Debug.WriteLine("IconToImageBrushConverter.Convert machine broke again: \n" + exc);
                        return new ImageBrush();
                    }
                }
                //BitmapSizeOptions.FromWidthAndHeight(WpfUnitsToRealPixels(param), WpfUnitsToRealPixels(param))
            }
            else return new ImageBrush();
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class ConversionInputHelper
    {
        public static double GetDouble(object val)
        {
            if (val is double)
                return (double)val;
            else if (val is int)
                return (int)val;
            else if (val is string)
                return Double.Parse(val.ToString());
            else return 0.0;
        }
    }
}