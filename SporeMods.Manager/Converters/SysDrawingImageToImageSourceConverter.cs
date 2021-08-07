using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*using System.Windows.Data;
using System.Windows.Media;
using static Mechanism.Wpf.Core.Extensions;*/
using Avalonia.Data.Converters;

namespace SporeMods.Manager
{

	public class SysDrawingImageToImageSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
#if RESTORE_LATER
			//Image val = value as Image;
			if ((value != null) && (value is Image val))
				return ((Bitmap)val).ToBitmapSource();
			else
#endif
				return null;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
