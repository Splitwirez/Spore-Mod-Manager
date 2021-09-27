using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

#nullable enable

namespace SporeMods.CommonUI
{
	public class NullableBoolToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool? nulBool = (bool?)value;
			
			return nulBool.HasValue && nulBool.Value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool? retVal = null;
			if ((bool)value)
				retVal = true;
			else
				retVal = false;

			return retVal;
		}
	}
}
