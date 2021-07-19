using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Mechanism.Wpf.Styles.Shale
{
    public enum ShaleBorderStyle
    {
        None,
        Button,
        CheckBox,
        RadioButton,
        Sheet,
        MenuBar,
        ContextMenu,
        MenuItem,
        ProgressBarFill,
        TitleBar,
        WindowBody,
        CaptionButton,
        ScrollBarTrack,
        TabBody,
        TabItem,
        LineTabBody,
        LineTabItem
    }

    //[ContentProperty(nameof(Content))]
    public class ShaleBorderDecorator : ContentControl
    {
        static FrameworkPropertyMetadataOptions _options = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange;

        public ShaleBorderStyle BorderStyle
        {
            get => (ShaleBorderStyle)GetValue(BorderStyleProperty);
            set => SetValue(BorderStyleProperty, value);
        }

        public static readonly DependencyProperty BorderStyleProperty =
                    DependencyProperty.Register(nameof(BorderStyle), typeof(ShaleBorderStyle), typeof(ShaleBorderDecorator), new FrameworkPropertyMetadata(ShaleBorderStyle.None, _options));

        public bool IsVisuallyMouseOver
        {
            get => (bool)GetValue(IsVisuallyMouseOverProperty);
            set => SetValue(IsVisuallyMouseOverProperty, value);
        }

        public static readonly DependencyProperty IsVisuallyMouseOverProperty =
                    DependencyProperty.Register(nameof(IsVisuallyMouseOver), typeof(bool), typeof(ShaleBorderDecorator), new FrameworkPropertyMetadata(false, _options));

        public bool? IsVisuallyPressed
        {
            get => (bool?)GetValue(IsVisuallyPressedProperty);
            set => SetValue(IsVisuallyPressedProperty, value);
        }

        public static readonly DependencyProperty IsVisuallyPressedProperty =
                    DependencyProperty.Register(nameof(IsVisuallyPressed), typeof(bool?), typeof(ShaleBorderDecorator), new FrameworkPropertyMetadata(false, _options));

        public bool IsVisuallyEnabled
        {
            get => (bool)GetValue(IsVisuallyEnabledProperty);
            set => SetValue(IsVisuallyEnabledProperty, value);
        }

        public static readonly DependencyProperty IsVisuallyEnabledProperty =
                    DependencyProperty.Register(nameof(IsVisuallyEnabled), typeof(bool), typeof(ShaleBorderDecorator), new FrameworkPropertyMetadata(false, _options));

        public bool IsVisuallyActive
        {
            get => (bool)GetValue(IsVisuallyActiveProperty);
            set => SetValue(IsVisuallyActiveProperty, value);
        }

        public static readonly DependencyProperty IsVisuallyActiveProperty =
                    DependencyProperty.Register(nameof(IsVisuallyActive), typeof(bool), typeof(ShaleBorderDecorator), new FrameworkPropertyMetadata(false, _options));
    }
}
