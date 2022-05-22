using SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SporeMods.CommonUI
{
    public class DescriptionImageToImageSourcConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            DescriptionImage img = (DescriptionImage)values[0];
            ComponentBase cmp = (ComponentBase)values[1];
            MemoryStream stream = cmp.Mod.GetImageStream(img.FileName);
            //var img = new BitmapSource(stream);
            //cmp.Mod.
            //img.FileName
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}