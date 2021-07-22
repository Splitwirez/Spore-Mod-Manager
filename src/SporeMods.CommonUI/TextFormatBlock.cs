using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SporeMods.CommonUI.Localization;

namespace SporeMods.CommonUI
{
    public class TextFormatBlock : TextBlock
    {
        public TextFormatBlock()
            : base()
        {
            TextFormatInput.AnyFormatterChanged += TextFormatter_AnyFormatterChanged;
        }

        private void TextFormatter_AnyFormatterChanged(object sender, DependencyPropertyChangedEventArgs e) =>
            UpdateText();

        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register(nameof(Format), typeof(string), typeof(TextFormatBlock), new PropertyMetadata(new PropertyChangedCallback((o, e) =>
        {
            if (o is TextFormatBlock bl)
                bl.UpdateText();
        })));

        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set => SetValue(FormatProperty, value);
        }

        static ObservableCollection<TextFormatInput> DefaultInputs()
        {
            return new ObservableCollection<TextFormatInput>();
        }

        public static readonly DependencyProperty InputsProperty =
            DependencyProperty.Register(nameof(Inputs), typeof(ObservableCollection<TextFormatInput>), typeof(TextFormatBlock), new PropertyMetadata(DefaultInputs(), new PropertyChangedCallback((o, e) =>
        {
            if (o is TextFormatBlock bl)
                bl.UpdateText();
        })));

        public ObservableCollection<TextFormatInput> Inputs
        {
            get => (ObservableCollection<TextFormatInput>)GetValue(InputsProperty);
            set => SetValue(InputsProperty, value);
        }

        void UpdateText()
        {
            var inputs = Inputs;
            if (inputs != null)
            {
                string output = Format;
                if (!output.IsNullOrEmptyOrWhiteSpace())
                {
                    foreach (TextFormatInput input in inputs)
                    {
                        var inValue = (!input.LocalizedTextKey.IsNullOrEmptyOrWhiteSpace()) ? TryFindResource(input.LocalizedTextKey) : input.Value;
                        output.Replace($"%{input.ReplaceTarget}%", inValue != null ? inValue.ToString() : string.Empty);
                    }
                }
            }

            /*

            foreach (var )*/
        }
    }

    public class TextFormatInput : DependencyObject
    {
        public static readonly DependencyProperty ReplaceTargetProperty =
            DependencyProperty.Register(nameof(ReplaceTarget), typeof(string), typeof(TextFormatInput), new PropertyMetadata(new PropertyChangedCallback((o, e) => OnStringFormattingPropertyChanged(o, e))));

        public string ReplaceTarget
        {
            get => (string)GetValue(ReplaceTargetProperty);
            set => SetValue(ReplaceTargetProperty, value);
        }

        public static readonly DependencyProperty LocalizedValueProperty =
            DependencyProperty.Register(nameof(LocalizedTextKey), typeof(string), typeof(TextFormatInput), new PropertyMetadata(new PropertyChangedCallback((o, e) => OnStringFormattingPropertyChanged(o, e))));

        public string LocalizedTextKey
        {
            get => (string)GetValue(LocalizedValueProperty);
            set => SetValue(LocalizedValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(TextFormatInput), new PropertyMetadata(new PropertyChangedCallback((o, e) => OnStringFormattingPropertyChanged(o, e))));

        internal static void OnStringFormattingPropertyChanged(DependencyObject formatter, DependencyPropertyChangedEventArgs propChangedArgs)
        {
            if (formatter is TextFormatInput fmtr)
                AnyFormatterChanged?.Invoke(fmtr, propChangedArgs);
        }

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        internal static event EventHandler<DependencyPropertyChangedEventArgs> AnyFormatterChanged;
    }
}
