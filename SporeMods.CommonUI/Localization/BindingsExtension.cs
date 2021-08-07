using SporeMods.NotifyOnChange;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using System.Collections;

namespace SporeMods.CommonUI.Localization
{
    public class BindingsExtension
    {
        ObservableCollection<IBinding> Values { get; set; } = new ObservableCollection<IBinding>();
        public BindingsExtension(IBinding b1, IBinding b2, IBinding b3, IBinding b4, IBinding b5, IBinding b6)
            : this(b1, b2, b3, b4, b5)
        {
            Values.Add(b6);
        }
        public BindingsExtension(IBinding b1, IBinding b2, IBinding b3, IBinding b4, IBinding b5)
            : this(b1, b2, b3, b4)
        {
            Values.Add(b5);
        }
        public BindingsExtension(IBinding b1, IBinding b2, IBinding b3, IBinding b4)
            : this(b1, b2, b3)
        {
            Values.Add(b4);
        }
        public BindingsExtension(IBinding b1, IBinding b2, IBinding b3)
            : this(b1, b2)
        {
            Values.Add(b3);
        }
        public BindingsExtension(IBinding b1, IBinding b2)
            : this(b1)
        {
            Values.Add(b2);
        }
        public BindingsExtension(IBinding b1)
        {
            Values.Add(b1);
        }
        public BindingsExtension(params IBinding[] bindings)
        {
            foreach (IBinding b in bindings)
                Values.Add(b);
        }

        public ObservableCollection<IBinding> ProvideValue() =>
            Values;
    }
}