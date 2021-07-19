using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mechanism.Wpf.Core.Flair
{
    public class ConnectedSelector : ContentControl
    {
        public Panel TargetPanel
        {
            get => (Panel)GetValue(TargetPanelProperty);
            set => SetValue(TargetPanelProperty, value);
        }

        public static readonly DependencyProperty TargetPanelProperty =
                    DependencyProperty.Register(nameof(TargetPanel), typeof(Panel), typeof(ConnectedSelector),
                        new PropertyMetadata(null, OnPropertiesChangedCallback));

        public int SelectedObjectIndex
        {
            get => (int)GetValue(SelectedObjectIndexProperty);
            set => SetValue(SelectedObjectIndexProperty, value);
        }

        public static readonly DependencyProperty SelectedObjectIndexProperty =
                    DependencyProperty.Register(nameof(SelectedObjectIndex), typeof(int), typeof(ConnectedSelector),
                        new PropertyMetadata(-1, OnPropertiesChangedCallback));

        public int CollectionSize
        {
            get => (int)GetValue(CollectionSizeProperty);
            set => SetValue(CollectionSizeProperty, value);
        }

        public static readonly DependencyProperty CollectionSizeProperty =
                    DependencyProperty.Register(nameof(CollectionSize), typeof(int), typeof(ConnectedSelector),
                        new PropertyMetadata(-1, OnPropertiesChangedCallback));

        public double SelectionLeft
        {
            get => (double)GetValue(SelectionLeftProperty);
            set => SetValue(SelectionLeftProperty, value);
        }

        public static readonly DependencyProperty SelectionLeftProperty =
            DependencyProperty.Register(nameof(SelectionLeft), typeof(double), typeof(ConnectedSelector), new PropertyMetadata(0.0));

        public double SelectionTop
        {
            get => (double)GetValue(SelectionTopProperty);
            set => SetValue(SelectionTopProperty, value);
        }

        public static readonly DependencyProperty SelectionTopProperty =
            DependencyProperty.Register(nameof(SelectionTop), typeof(double), typeof(ConnectedSelector), new PropertyMetadata(0.0)); 
        
        public double SelectionRight
        {
            get => (double)GetValue(SelectionRightProperty);
            set => SetValue(SelectionRightProperty, value);
        }

        public static readonly DependencyProperty SelectionRightProperty =
            DependencyProperty.Register(nameof(SelectionRight), typeof(double), typeof(ConnectedSelector), new PropertyMetadata(0.0));

        public double SelectionBottom
        {
            get => (double)GetValue(SelectionBottomProperty);
            set => SetValue(SelectionBottomProperty, value);
        }

        public double SelectionWidth
        {
            get => (double)GetValue(SelectionWidthProperty);
            set => SetValue(SelectionWidthProperty, value);
        }

        public static readonly DependencyProperty SelectionWidthProperty =
            DependencyProperty.Register(nameof(SelectionWidth), typeof(double), typeof(ConnectedSelector), new PropertyMetadata(0.0));

        public double SelectionHeight
        {
            get => (double)GetValue(SelectionHeightProperty);
            set => SetValue(SelectionHeightProperty, value);
        }

        public static readonly DependencyProperty SelectionHeightProperty =
            DependencyProperty.Register(nameof(SelectionHeight), typeof(double), typeof(ConnectedSelector), new PropertyMetadata(0.0));

        public static readonly DependencyProperty SelectionBottomProperty =
            DependencyProperty.Register(nameof(SelectionBottom), typeof(double), typeof(ConnectedSelector), new PropertyMetadata(0.0));

        public bool SelectionExists
        {
            get => (bool)GetValue(SelectionExistsProperty);
            set => SetValue(SelectionExistsProperty, value);
        }

        public static readonly DependencyProperty SelectionExistsProperty =
            DependencyProperty.Register(nameof(SelectionExists), typeof(bool), typeof(ConnectedSelector), new PropertyMetadata(false));

        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(ConnectedSelector), new PropertyMetadata(TimeSpan.FromMilliseconds(0)));

        public IEasingFunction AnimationEase
        {
            get => (IEasingFunction)GetValue(AnimationEaseProperty);
            set => SetValue(AnimationEaseProperty, value);
        }

        public static readonly DependencyProperty AnimationEaseProperty =
            DependencyProperty.Register(nameof(AnimationEase), typeof(IEasingFunction), typeof(ConnectedSelector), new PropertyMetadata(null));

        static void OnPropertiesChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ConnectedSelector).UpdateSelectorBounds();
        }

        public ConnectedSelector()
        {
            Loaded += (sneder, args) =>  CompositionTarget.Rendering += (snedre, rags) => UpdateSelectorBounds();
        }

        void UpdateSelectorBounds()
        {
            if (TargetPanel != null)
            {
                bool doesSelectionExist = (SelectedObjectIndex >= 0) && (SelectedObjectIndex < TargetPanel.Children.Count);
                if (SelectionExists != doesSelectionExist)
                    SelectionExists = doesSelectionExist;

                if (SelectionExists && (PresentationSource.FromVisual(TargetPanel) != null) && (TargetPanel.Children[SelectedObjectIndex] is FrameworkElement panelChild))
                {
                    //FrameworkElement panelChild = (FrameworkElement);
                    var point = TargetPanel.PointFromScreen(panelChild.PointToScreen(new Point(0, 0)));
                    
                    BeginAnimation(SelectionLeftProperty, new DoubleAnimation()
                    {
                        To = point.X,
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEase
                    });

                    BeginAnimation(SelectionTopProperty, new DoubleAnimation()
                    {
                        To = point.Y,
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEase
                    });

                    BeginAnimation(SelectionRightProperty, new DoubleAnimation()
                    {
                        To = point.X + panelChild.ActualWidth,
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEase
                    });

                    BeginAnimation(SelectionBottomProperty, new DoubleAnimation()
                    {
                        To = point.Y + panelChild.ActualHeight,
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEase
                    });

                    BeginAnimation(SelectionWidthProperty, new DoubleAnimation()
                    {
                        To = panelChild.ActualWidth,
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEase
                    });

                    BeginAnimation(SelectionHeightProperty, new DoubleAnimation()
                    {
                        To = panelChild.ActualHeight,
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEase
                    });

                    //Debug.WriteLine("Pos: " + SelectionLeft + ", " + SelectionTop + ", " + SelectionRight + ", " + SelectionBottom);
                }
            }
        }
    }
}
