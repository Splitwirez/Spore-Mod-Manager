using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace SporeMods.CommonUI
{

    public static class ViewLocator
    {
        /*public static string GetViewTypeName(this IViewLocatable vm)
            => vm.ViewTypeName;*/
        public static FrameworkElement GetView(this IViewLocatable vm)
        {
            string viewTypeName = vm.GetViewTypeName();
            Cmd.WriteLine($"VLHelpers.GetView('{vm.GetType().FullName}'), '{viewTypeName}'");
            FrameworkElement view = (FrameworkElement)Activator.CreateInstance(Type.GetType(viewTypeName));
            view.DataContext = vm;
            return view;
        }


        public static readonly IValueConverter Converter = new ViewLocatorConverter();
        private class ViewLocatorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (!(value is IViewLocatable vm))
                    return null;
                return vm.GetView();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotImplementedException();
        }
    }
}
