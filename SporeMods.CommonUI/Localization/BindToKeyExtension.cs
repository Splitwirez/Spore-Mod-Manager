using SporeMods.NotifyOnChange;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Reactive;
using Avalonia.ReactiveUI;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
//using Avalonia.Controls.ResourceNodeExtensions;
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
    public class BindToKeyExtension : AvaloniaObject, IBinding
    {
        private object? _anchor;

        public BindToKeyExtension()
        {
        }

        public BindToKeyExtension(object resourceKey)
        {
            ResourceKey = resourceKey;
        }

        public static readonly StyledProperty<object> ResourceKeyProperty =
            AvaloniaProperty.Register<BindToKeyExtension, object>(nameof(ResourceKey));
        public object ResourceKey
        {
            get => GetValue<object>(ResourceKeyProperty);
            set => SetValue(ResourceKeyProperty, value);
        }

        public IBinding ProvideValue(IServiceProvider serviceProvider)
        {
            var provideTarget = serviceProvider.GetService<IProvideValueTarget>();

            if (!(provideTarget.TargetObject is IStyledElement))
            {
                _anchor = serviceProvider.GetFirstParent<IStyledElement>() ??
                    serviceProvider.GetFirstParent<IResourceProvider>() ??
                    (object?)serviceProvider.GetFirstParent<IResourceHost>();
            }

            return this;
        }

        InstancedBinding? IBinding.Initiate(
            IAvaloniaObject target,
            AvaloniaProperty targetProperty,
            object anchor,
            bool enableDataValidation)
        {
            if (ResourceKey is null)
            {
                return null;
            }

            var control = target as IResourceHost ?? _anchor as IResourceHost;

            if (control != null)
            {
                var source = control.GetResourceObservable(ResourceKey, GetConverter(targetProperty));
                return InstancedBinding.OneWay(source);
            }
            else if (_anchor is IResourceProvider resourceProvider)
            {
                var source = resourceProvider.GetResourceObservable(ResourceKey, GetConverter(targetProperty));
                return InstancedBinding.OneWay(source);
            }

            return null;
        }

        private Func<object?, object?>? GetConverter(AvaloniaProperty targetProperty)
        {
            if (targetProperty?.PropertyType == typeof(IBrush))
            {
                return x => ColorToBrushConverter.Convert(x, typeof(IBrush));
            }

            return null;
        }
    }
}
