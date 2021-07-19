using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mechanism.Wpf.Core
{
    [TemplatePart(Name = PartCloseButton, Type = typeof(Button))]
    public class AnimatableContentControl : ContentControl
    {
        const string PartCloseButton = "PART_CloseButton";

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty = Popup.IsOpenProperty.AddOwner(typeof(AnimatableContentControl));

        public bool HasCloseButton
        {
            get => (bool)GetValue(HasCloseButtonProperty);
            set => SetValue(HasCloseButtonProperty, value);
        }

        public static readonly DependencyProperty HasCloseButtonProperty = DependencyProperty.Register(nameof(HasCloseButton), typeof(bool), typeof(AnimatableContentControl), new PropertyMetadata(false));


        internal Button _closeButton;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _closeButton = GetTemplateChild(PartCloseButton) as Button;
            if (_closeButton != null)
                _closeButton.Click += (sneder, args) => IsOpen = false;
        }
    }
}
