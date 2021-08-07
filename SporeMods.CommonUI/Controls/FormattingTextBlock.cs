/*using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SporeMods.Manager
{
    class FormattingTextBlock : TextBlock
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(FormattingTextBlock), new PropertyMetadata(new PropertyChangedCallback((o, e) =>
        {
            if (o is FormattingTextBlock bl)
                bl.UpdateText(e.NewValue, bl.Format);
        })));

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }


        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(nameof(Format), typeof(string), typeof(FormattingTextBlock), new PropertyMetadata(new PropertyChangedCallback((o, e) =>
        {
            if (o is FormattingTextBlock bl)
                bl.UpdateText(bl.Value, e.NewValue != null ? e.NewValue.ToString() : string.Empty);
        })));

        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set => SetValue(FormatProperty, value);
        }

        void UpdateText(object value, string format)
        {
            if (format != null)
                Text = string.Format(format, value);
        }
    }
}
*/