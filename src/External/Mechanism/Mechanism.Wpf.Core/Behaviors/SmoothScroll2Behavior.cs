using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media.Animation;

namespace Mechanism.Wpf.Core.Behaviors
{
    public class SmoothScroll2Behavior : Behavior<ScrollViewer>
    {
        public static readonly DependencyProperty UseSmoothScrollingProperty =
            DependencyProperty.RegisterAttached("UseSmoothScrolling", typeof(bool), typeof(SmoothScroll2Behavior), new FrameworkPropertyMetadata(false, OnUseSmoothScrollingChanged));

        public static bool GetUseSmoothScrolling(DependencyObject element)
        {
            return (bool)element.GetValue(UseSmoothScrollingProperty);
        }

        public static void SetUseSmoothScrolling(DependencyObject element, bool value)
        {
            element.SetValue(UseSmoothScrollingProperty, value);
        }

        static void OnUseSmoothScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (!Interaction.GetBehaviors(d).Any(x => x is SmoothScroll2Behavior))
                    Interaction.GetBehaviors(d).Add(new SmoothScroll2Behavior());
            }
            else
            {
                if (!Interaction.GetBehaviors(d).Any(x => x is SmoothScroll2Behavior))
                {
                    foreach (var be in Interaction.GetBehaviors(d).OfType<SmoothScroll2Behavior>())
                        Interaction.GetBehaviors(d).Remove(be);
                }
            }
        }





        public static readonly DependencyProperty SmoothedHorizontalOffsetProperty =
            DependencyProperty.RegisterAttached("SmoothedHorizontalOffset", typeof(double), typeof(SmoothScroll2Behavior), new FrameworkPropertyMetadata(0.0, OnSmoothedHorizontalOffsetChanged));

        public static double GetSmoothedHorizontalOffset(DependencyObject element)
        {
            return (double)element.GetValue(SmoothedHorizontalOffsetProperty);
        }

        public static void SetSmoothedHorizontalOffset(DependencyObject element, double value)
        {
            element.SetValue(SmoothedHorizontalOffsetProperty, value);
        }

        static void OnSmoothedHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SmoothedHorizontalOffsetChanged?.Invoke(d, e);
        }

        public static event DependencyPropertyChangedEventHandler SmoothedHorizontalOffsetChanged;



        public static readonly DependencyProperty SmoothedVerticalOffsetProperty =
            DependencyProperty.RegisterAttached("SmoothedVerticalOffset", typeof(double), typeof(SmoothScroll2Behavior), new FrameworkPropertyMetadata(0.0, OnSmoothedVerticalOffsetChanged));

        public static double GetSmoothedVerticalOffset(DependencyObject element)
        {
            return (double)element.GetValue(SmoothedVerticalOffsetProperty);
        }

        public static void SetSmoothedVerticalOffset(DependencyObject element, double value)
        {
            element.SetValue(SmoothedVerticalOffsetProperty, value);
        }

        static void OnSmoothedVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SmoothedVerticalOffsetChanged?.Invoke(d, e);
        }

        public static event DependencyPropertyChangedEventHandler SmoothedVerticalOffsetChanged;




        static CubicEase _ease = new CubicEase()
        {
            EasingMode = EasingMode.EaseOut
        };
        static TimeSpan _duration = TimeSpan.FromMilliseconds(25);

        double _oldHorizontalOffset = 0;
        double _oldVerticalOffset = 0;
        protected override void OnAttached()
        {
            base.OnAttached();
            SetSmoothedHorizontalOffset(AssociatedObject, AssociatedObject.HorizontalOffset);
            SetSmoothedHorizontalOffset(AssociatedObject, AssociatedObject.VerticalOffset);
            _oldHorizontalOffset = AssociatedObject.HorizontalOffset;
            _oldVerticalOffset = AssociatedObject.VerticalOffset;
            
            AssociatedObject.ScrollChanged += (sneder, args) =>
            {
                DoubleAnimation horiz = new DoubleAnimation()
                {
                    From = _oldHorizontalOffset,
                    By = args.HorizontalChange,
                    EasingFunction = _ease,
                    Duration = _duration
                };
                horiz.Completed += (snader, ergs) => _oldHorizontalOffset = AssociatedObject.HorizontalOffset;

                AssociatedObject.BeginAnimation(SmoothedHorizontalOffsetProperty, horiz);

                DoubleAnimation vert = new DoubleAnimation()
                {
                    From = _oldVerticalOffset,
                    By = args.VerticalChange,
                    EasingFunction = _ease,
                    Duration = _duration
                };
                vert.Completed += (snader, ergs) => _oldHorizontalOffset = AssociatedObject.VerticalOffset;

                AssociatedObject.BeginAnimation(SmoothedVerticalOffsetProperty, vert);
            };
        }
    }

    public class SmoothScroll2PresenterBehavior : Behavior<ScrollContentPresenter>
    {
        public static readonly DependencyProperty UseSmoothScrollingProperty =
            DependencyProperty.RegisterAttached("UseSmoothScrolling", typeof(bool), typeof(SmoothScroll2PresenterBehavior), new FrameworkPropertyMetadata(false, OnUseSmoothScrollingChanged));

        public static bool GetUseSmoothScrolling(DependencyObject element)
        {
            return (bool)element.GetValue(UseSmoothScrollingProperty);
        }

        public static void SetUseSmoothScrolling(DependencyObject element, bool value)
        {
            element.SetValue(UseSmoothScrollingProperty, value);
        }

        static void OnUseSmoothScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (!Interaction.GetBehaviors(d).Any(x => x is SmoothScroll2PresenterBehavior))
                    Interaction.GetBehaviors(d).Add(new SmoothScroll2PresenterBehavior());
            }
            else
            {
                if (!Interaction.GetBehaviors(d).Any(x => x is SmoothScroll2PresenterBehavior))
                {
                    foreach (var be in Interaction.GetBehaviors(d).OfType<SmoothScroll2PresenterBehavior>())
                        Interaction.GetBehaviors(d).Remove(be);
                }
            }
        }



        protected override void OnAttached()
        {
            base.OnAttached();
            SmoothScroll2Behavior.SmoothedHorizontalOffsetChanged += SmoothScroll2Behavior_SmoothedHorizontalOffsetChanged;
            SmoothScroll2Behavior.SmoothedVerticalOffsetChanged += SmoothScroll2Behavior_SmoothedVerticalOffsetChanged;
        }

        private void SmoothScroll2Behavior_SmoothedHorizontalOffsetChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == AssociatedObject.TemplatedParent)
                AssociatedObject.SetHorizontalOffset((double)e.NewValue);
        }

        private void SmoothScroll2Behavior_SmoothedVerticalOffsetChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == AssociatedObject.TemplatedParent)
                AssociatedObject.SetVerticalOffset((double)e.NewValue);
        }
    }
}
