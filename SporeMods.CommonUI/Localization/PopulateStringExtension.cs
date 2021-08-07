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
    public class PopulateStringExtension : AvaloniaObject, IBinding
    {
        private object _anchor;


        public static readonly StyledProperty<string> FormatProperty =
            AvaloniaProperty.Register<PopulateStringExtension, string>(nameof(Format), string.Empty);
        public string Format
        {
            get => GetValue<string>(FormatProperty);
            set => SetValue(FormatProperty, value);
        }

        public string FieldNames { get; set; }
        public IEnumerable<IBinding> FieldValues { get; set; } = new ObservableCollection<IBinding>();


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

        string GetString(string format, IAvaloniaObject target = null, AvaloniaProperty targetProperty = null, object anchor = null, bool enableDataValidation = false)
        {
            if (FieldNames.IsNullOrEmptyOrWhiteSpace() || format.IsNullOrEmptyOrWhiteSpace() || (format.Equals(BindingValueType.UnsetValue.ToString())))
            {
                var h = FieldValues.Select(x => x.Initiate(target, null));
                return h.Select(x =>
                {
                    string h = null;
                    var obs = x.Observable;
                    obs.Subscribe(w => h = w.ToString());
                    obs.Next();
                    return h;
                }).ToList().FirstOrDefault();
            }
            else
            {
                string output = format;
                string[] names = FieldNames.Split(';');

                var h = FieldValues.Select(x => x.Initiate(target, null));
                var values = h.Select(x =>
                {
                    string h = null;
                    var obs = x.Observable;
                    obs.Subscribe(w => h = w.ToString());
                    obs.Next();
                    return h;
                }).ToList();
                

                int nameCount = names.Length;
                for (int index = 0; index < nameCount; index++)
                {
                    string value = values[index];
                    Console.WriteLine($"value: {value}");
                    output = output.Replace($"%{names[index]}%", value != null ? value : LanguageManager.NO_TEXT);
                }
                return output;
            }
        }



        IList<object> GetValues(IList<object> values)
        {
            for (var i = 0; i < values.Count; ++i)
            {
                if (values[i] is BindingNotification notification)
                {
                    values[i] = notification.Value;
                }
            }

            return new System.Collections.ObjectModel.ReadOnlyCollection<object>(values);
        }

        object GetValue(object inValue)
        {
            if (inValue is BindingNotification notification)
            {
                return notification.Value;
            }
            return inValue;
        }


        public InstancedBinding? Initiate(
            IAvaloniaObject target,
            AvaloniaProperty targetProperty,
            object anchor,
            bool enableDataValidation)
        {
            if (Format is null)
            {
                return null;
            }

            return this.GetObservable<string>(FormatProperty).Select(x => GetString(x)).ToBinding().Initiate(target, targetProperty, anchor, enableDataValidation);
        }
    }
}