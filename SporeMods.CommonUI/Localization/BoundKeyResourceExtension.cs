using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;

namespace SporeMods.CommonUI.Localization
{
	public class BoundKeyResourceExtension : AvaloniaObject, IBinding
	{
        public static readonly StyledProperty<string> ResourceKeyProperty =
            AvaloniaProperty.Register<BoundKeyResourceExtension, string>(nameof(ResourceKey), string.Empty);

        public string ResourceKey
        {
            get => (string)GetValue(ResourceKeyProperty);
            set => SetValue(ResourceKeyProperty, value);
        }

        private object? _anchor;

        public BoundKeyResourceExtension()
        {
        }

        public BoundKeyResourceExtension(string resourceKey)
        {
            ResourceKey = resourceKey;
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
                var source = control.GetResourceObservable(ResourceKey);
                return InstancedBinding.OneWay(source);
            }
            else if (_anchor is IResourceProvider resourceProvider)
            {
                var source = resourceProvider.GetResourceObservable(ResourceKey);
                return InstancedBinding.OneWay(source);
            }

            return null;
        }
    }
}
