using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using static SporeMods.CommonUI.Extensions;

namespace SporeMods.CommonUI
{

	public class SysDrawingImageToImageSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			//Image val = value as Image;
			if ((value != null) && (value is Image val))
				return ((Bitmap)val).ToBitmapSource();
			else
				return null;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
