using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Mechanism.Wpf.Core.Behaviors
{
    public class RevealSizeSynchronizerBehavior : Behavior<FrameworkElement>
    {
        /*public double SizeMultiplier
        {
            get => (double)GetValue(SizeMultiplierProperty);
            set => SetValue(SizeMultiplierProperty, value);
        }

        public static readonly DependencyProperty SizeMultiplierProperty =
            DependencyProperty.Register(nameof(SizeMultiplier), typeof(double), typeof(RevealSizeSynchronizerBehavior), new PropertyMetadata(1.0, OnSizeMultiplierPropertyChangedCallback));

        static void OnSizeMultiplierPropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is RevealSizeSynchronizerBehavior behavior)
                behavior.UpdateRevealSize();
        }*/

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject.TemplatedParent is FrameworkElement templatedParentElement)
                templatedParentElement.SizeChanged += TemplatedParentElement_SizeChanged;

            UpdateRevealSize();
        }

        private void TemplatedParentElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AssociatedObject.TemplatedParent is FrameworkElement templatedParentElement)
                UpdateRevealSize(e.NewSize.Width, e.NewSize.Height);
        }

        public void UpdateRevealSize()
        {
            if (AssociatedObject.TemplatedParent is FrameworkElement templatedParentElement)
                UpdateRevealSize(templatedParentElement.ActualWidth, templatedParentElement.ActualHeight);
        }

        private void UpdateRevealSize(double parentWidth, double parentHeight)
        {
            double newSize = 0.0;
            if (parentWidth > parentHeight)
                newSize = parentWidth;
            else
                newSize = parentHeight;

            AssociatedObject.Width = newSize;
            AssociatedObject.Height = newSize;
        }
    }
}
