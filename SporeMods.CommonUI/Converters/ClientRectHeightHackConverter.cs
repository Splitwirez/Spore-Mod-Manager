/*using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace SporeMods.CommonUI
{
	public class ClientRectHeightHackConverter : IValueConverter
	{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var win = (Window)value;
            UnmanagedMethods.RECT clientRect;
            UnmanagedMethods.GetClientRect(win.PlatformImpl.Handle.Handle, out clientRect);
            return (clientRect.Bottom - clientRect.Top) * Avalonia.VisualTree.VisualExtensions.GetVisualRoot(win).RenderScaling;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}*/