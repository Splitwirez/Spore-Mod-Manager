using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mechanism.Wpf.Core
{
    [TemplatePart(Name = PartGoStopToggleButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PartWatermarkTextBlock, Type = typeof(TextBlock))]
    public class ActionBox : TextBox
    {
        const string PartGoStopToggleButton = "PART_GoStopToggleButton";
        const string PartWatermarkTextBlock = "PART_WatermarkTextBlock";

        public string WatermarkText
        {
            get => (string)GetValue(WatermarkTextProperty);
            set => SetValue(WatermarkTextProperty, value);
        }

        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register(nameof(WatermarkText), typeof(string), typeof(ActionBox), new PropertyMetadata(string.Empty));

        public ActionType ActionType
        {
            get => (ActionType)GetValue(ActionTypeProperty);
            set => SetValue(ActionTypeProperty, value);
        }

        public static readonly DependencyProperty ActionTypeProperty =
            DependencyProperty.Register(nameof(ActionType), typeof(ActionType), typeof(ActionBox), new PropertyMetadata(ActionType.Regular));

        public bool IsTogglable
        {
            get => (bool)GetValue(IsTogglableProperty);
            set => SetValue(IsTogglableProperty, value);
        }

        public static readonly DependencyProperty IsTogglableProperty =
            DependencyProperty.Register(nameof(IsTogglable), typeof(bool), typeof(ActionBox), new PropertyMetadata(true));

        public bool? IsChecked
        {
            get => (bool?)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool?), typeof(ActionBox), new PropertyMetadata(false));

        static void OnIsCancelablePropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            var box = sender as ActionBox;
            if ((bool)e.NewValue == false)
                box.IsChecked = false;
        }

        public object ButtonContent
        {
            get => (object)GetValue(ButtonContentProperty);
            set => SetValue(ButtonContentProperty, value);
        }

        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register(nameof(ButtonContent), typeof(object), typeof(ActionBox), new PropertyMetadata(null));

        static ActionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ActionBox), new FrameworkPropertyMetadata(typeof(ActionBox)));
        }

        public void CancelAction()
        {
            _goButton.IsChecked = false;
        }

        public static readonly RoutedEvent SubmittedEvent = EventManager.RegisterRoutedEvent(
            nameof(ActionSubmitted), RoutingStrategy.Bubble, typeof(EventHandler<ActionSubmittedEventArgs>), typeof(ActionBox));

        public event EventHandler<ActionSubmittedEventArgs> ActionSubmitted
        {
            add { AddHandler(SubmittedEvent, value); }
            remove { RemoveHandler(SubmittedEvent, value); }
        }

        public static readonly RoutedEvent ActionCanceledEvent = EventManager.RegisterRoutedEvent(
            nameof(ActionCanceled), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ActionBox));

        public event RoutedEventHandler ActionCanceled
        {
            add { AddHandler(ActionCanceledEvent, value); }
            remove { RemoveHandler(ActionCanceledEvent, value); }
        }


        internal ToggleButton _goButton;
        TextBlock _watermarkTextBlock;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _goButton = GetTemplateChild(PartGoStopToggleButton) as ToggleButton;
            if (_goButton != null)
                _goButton.Click += GoStopToggleButton_Click;

            _watermarkTextBlock = GetTemplateChild(PartWatermarkTextBlock) as TextBlock;
        }

        private void GoStopToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsTogglable)
            {
                if (_goButton.IsChecked == true)
                    RaiseEvent(new ActionSubmittedEventArgs(Text, SubmittedEvent));
                else
                    RaiseEvent(new RoutedEventArgs(ActionCanceledEvent));
            }
            else
            {
                RaiseEvent(new ActionSubmittedEventArgs(Text, SubmittedEvent));
                _goButton.IsChecked = false;
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (_watermarkTextBlock != null)
            {
                _watermarkTextBlock.Visibility = String.IsNullOrWhiteSpace(Text) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (ActionType == ActionType.Instant)
            {
                _goButton.IsChecked = !String.IsNullOrWhiteSpace(Text);
                RaiseEvent(new ActionSubmittedEventArgs(Text, ActionCanceledEvent));
                RaiseEvent(new ActionSubmittedEventArgs(Text, SubmittedEvent));
            }
        }
    }

    public enum ActionType
    {
        Regular,
        Instant
    }

    public class ActionSubmittedEventArgs : RoutedEventArgs
    {
        internal ActionSubmittedEventArgs(String text, RoutedEvent @event) : base(@event)
        {
            Query = text;
        }

        public String Query { get; }

        public void CancelAction()
        {
            ((ActionBox)Source)._goButton.IsChecked = false;
        }
    }
}
