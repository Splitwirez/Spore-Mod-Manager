using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Mechanism.Wpf.Core
{
    [TemplatePart(Name = PartHamburgerToggleButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PartItemsPane, Type = typeof(UIElement))]
    public class NavigationView : ListView
    {
        const String PartHamburgerToggleButton = "PART_HamburgerToggleButton";
        const String PartBackButton = "PART_BackButton";
        const String PartItemsPane = "PART_ItemsPane";

        ToggleButton _hamburgerToggleButton;
        Button _backButton;
        UIElement _itemsPane;

        public enum NavigationViewDisplayMode
        {
            Expanded,
            Compact,
            Minimal
        }

        public NavigationViewDisplayMode DisplayMode
        {
            get => (NavigationViewDisplayMode)GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(NavigationViewDisplayMode), typeof(NavigationView), new PropertyMetadata(NavigationViewDisplayMode.Compact));


        public enum NavigationViewPanePosition
        {
            Left,
            Top,
            LeftCompact,
            LeftMinimal,
            Auto
        }

        public NavigationViewPanePosition PaneDisplayMode
        {
            get => (NavigationViewPanePosition)GetValue(PaneDisplayModeProperty);
            set => SetValue(PaneDisplayModeProperty, value);
        }

        public static readonly DependencyProperty PaneDisplayModeProperty =
            DependencyProperty.Register("PaneDisplayMode", typeof(NavigationViewPanePosition), typeof(NavigationView), new PropertyMetadata(NavigationViewPanePosition.LeftCompact));


        public bool IsBackButtonVisible
        {
            get => (bool)GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.Register("IsBackButtonVisible", typeof(bool), typeof(NavigationView), new PropertyMetadata(true));

        public bool IsPaneOpen
        {
            get => (bool)GetValue(IsPaneOpenProperty);
            set => SetValue(IsPaneOpenProperty, value);
        }

        public static readonly DependencyProperty IsPaneOpenProperty =
            DependencyProperty.Register("IsPaneOpen", typeof(bool), typeof(NavigationView), new PropertyMetadata(false));


        public double PaneSize
        {
            get => (double)GetValue(PaneSizeProperty);
            set => SetValue(PaneSizeProperty, value);
        }

        public static readonly DependencyProperty PaneSizeProperty =
            DependencyProperty.Register("PaneSize", typeof(double), typeof(NavigationView), new PropertyMetadata((double)250));


        public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(UIElement), typeof(UIElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static UIElement GetIcon(UIElement element)
        {
            return (element.GetValue(IconProperty)) as UIElement;
        }

        public static void SetIcon(UIElement element, UIElement value)
        {
            element.SetValue(IconProperty, value);
        }


        public NavigationView()
        {
            
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _hamburgerToggleButton = GetTemplateChild(PartHamburgerToggleButton) as ToggleButton;
            _hamburgerToggleButton.Click += (sneder, args) =>
            {
                if (DisplayMode == NavigationViewDisplayMode.Compact)
                {
                    if (_hamburgerToggleButton.IsChecked == true)
                        IsPaneOpen = true;
                    else
                        IsPaneOpen = false;
                }

                Debug.WriteLine("IsPaneOpen: " + IsPaneOpen.ToString());
            };

            _backButton = GetTemplateChild(PartBackButton) as Button;

            _itemsPane = GetTemplateChild(PartItemsPane) as UIElement;
            _itemsPane.IsVisibleChanged += (sneder, args) =>
            {
                if (DisplayMode != NavigationViewDisplayMode.Compact)
                    IsPaneOpen = _itemsPane.IsVisible;

                Debug.WriteLine("IsPaneOpen: " + IsPaneOpen.ToString());
            };
        }
    }
}
