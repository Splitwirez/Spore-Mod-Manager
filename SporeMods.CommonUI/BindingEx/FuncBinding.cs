using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;


namespace SporeMods.CommonUI
{
    public partial class FuncBinding : BindingExBase
    {
        readonly string _funcName = string.Empty;
        public FuncBinding(string funcName)
        {
            _funcName = funcName;
        }
        public FuncBinding(string funcName, string path)
            : this(funcName)
        {
            if (path == null)
                return;

            Path = new PropertyPath(path, (object[])null);
        }


        static readonly IValueConverter _CONVERTER = new GetFuncConverter();
        protected override bool PrepareBinding(in IProvideValueTarget pvt, ref Binding binding, in FrameworkElement target, in DependencyProperty prop)
        {
            binding.Converter = _CONVERTER;
            binding.ConverterParameter = _funcName;

            return true;
        }
    }
}