using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using static Mechanism.Wpf.Core.Extensions;

namespace Mechanism.Wpf.Core.Converters
{
    public class SysDrawingImageToImageBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            Image val = value as Image;

            if (val != null)
                return new ImageBrush(((Bitmap)val).ToBitmapSource());
            else
                return new ImageBrush();
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
