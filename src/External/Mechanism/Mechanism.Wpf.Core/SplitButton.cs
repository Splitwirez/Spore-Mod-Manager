using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Mechanism.Wpf.Core
{
    [TemplatePart(Name = PartFlyoutButton, Type = typeof(ToggleButton))]
    public class SplitButton : Button
    {
        const String PartFlyoutButton = "PART_FlyoutButton";

        Binding _flyoutOpenBinding = new Binding()
        {
            Path = new PropertyPath("IsFlyoutOpen"),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        public ContextMenu FlyoutMenu
        {
            get => (ContextMenu)GetValue(FlyoutMenuProperty);
            set => SetValue(FlyoutMenuProperty, value);
        }

        public static readonly DependencyProperty FlyoutMenuProperty =
            DependencyProperty.Register(nameof(FlyoutMenu), typeof(ContextMenu), typeof(SplitButton), new PropertyMetadata(null, OnFlyoutMenuPropertyChangedCallback));

        static void OnFlyoutMenuPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((e.NewValue != null) && (e.NewValue is ContextMenu flyout))
            {
                BindingOperations.SetBinding(flyout, Popup.IsOpenProperty, (sender as SplitButton)._flyoutOpenBinding);
            }


            if ((e.OldValue != null) && (e.OldValue is ContextMenu oldFlyout))
            {
                //oldFlyout.PlacementTarget = null;
                if (oldFlyout.GetBindingExpression(Popup.IsOpenProperty).ParentBinding == (sender as SplitButton)._flyoutOpenBinding)
                {
                    BindingOperations.ClearBinding(oldFlyout, Popup.IsOpenProperty);
                }
            }
        }

        public bool IsFlyoutOpen
        {
            get => (bool)GetValue(IsFlyoutOpenProperty);
            set => SetValue(IsFlyoutOpenProperty, value);
        }

        public static readonly DependencyProperty IsFlyoutOpenProperty =
            DependencyProperty.Register(nameof(IsFlyoutOpen), typeof(bool), typeof(SplitButton), new PropertyMetadata(false, OnIsFlyoutOpenPropertyChangedCallback));

        static void OnIsFlyoutOpenPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            var button = sender as SplitButton;
            if ((bool)e.NewValue && (button.FlyoutMenu != null) && (button.FlyoutMenu is ContextMenu flyout))
            {
                flyout.PlacementTarget = (sender as SplitButton)._flyoutButton;
                flyout.Placement = PlacementMode.Right;
                System.Diagnostics.Debug.WriteLine("flyout.PlacementTarget == null: " + (flyout.PlacementTarget == null));
            }
        }


            ToggleButton _flyoutButton;

        public SplitButton()
        {
            _flyoutOpenBinding.Source = this;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _flyoutButton = GetTemplateChild(PartFlyoutButton) as ToggleButton;
            if (_flyoutButton != null)
            {
                /*Binding flyoutBinding = new Binding()
                {
                    Source = this,
                    Path = new PropertyPath("FlyoutMenu"),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(_flyoutButton, ContextMenuProperty, flyoutBinding);*/

                _flyoutButton.MouseRightButtonUp += FlyoutButton_MouseRightButtonUp;
            }
        }

        private void FlyoutButton_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((ContextMenu != null) && (!ContextMenu.IsOpen))
                ContextMenu.IsOpen = true;

            e.Handled = true;
        }
    }
}
