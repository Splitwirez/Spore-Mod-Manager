using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SporeMods.CommonUI
{
    public class StackPanelEx : StackPanel
    {
        /// <summary>
        /// Defines the <see cref="Spacing"/> property.
        /// </summary>
        public static readonly DependencyProperty SpacingProperty =
                DependencyProperty.Register("Spacing", typeof(double), typeof(StackPanelEx), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        /// <summary>
        /// Gets or sets a uniform distance (in pixels) between stacked items. It is applied in the
        /// direction of the StackLayout's Orientation.
        /// </summary>
        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }


        /// <summary>
        /// General StackPanel layout behavior is to grow unbounded in the "stacking" direction (Size To Content).
        /// Children in this dimension are encouraged to be as large as they like.  In the other dimension,
        /// StackPanel will assume the maximum size of its children.
        /// </summary>
        /// <param name="availableSize">Constraint</param>
        /// <returns>Desired size</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size stackDesiredSize = new Size();
            var children = Children;
            Size layoutSlotSize = availableSize;
            bool fHorizontal = (Orientation == Orientation.Horizontal);
            double spacing = Spacing;
            bool hasVisibleChild = false;

            //
            // Initialize child sizing and iterator data
            // Allow children as much size as they want along the stack.
            //
            if (fHorizontal)
            {
                layoutSlotSize.Width = Double.PositiveInfinity;
            }
            else
            {
                layoutSlotSize.Height = Double.PositiveInfinity;
            }

            //
            //  Iterate through children.
            //  While we still supported virtualization, this was hidden in a child iterator (see source history).
            //
            for (int i = 0, count = children.Count; i < count; ++i)
            {
                // Get next child.
                var child = children[i];

                if (child == null)
                { continue; }

                bool isVisible = child.IsVisible;

                if (isVisible && !hasVisibleChild)
                {
                    hasVisibleChild = true;
                }

                // Measure the child.
                child.Measure(layoutSlotSize);
                Size childDesiredSize = child.DesiredSize;

                // Accumulate child size.
                if (fHorizontal)
                {
                    stackDesiredSize.Width += (isVisible ? spacing : 0) + childDesiredSize.Width;
                    stackDesiredSize.Height = Math.Max(stackDesiredSize.Height, childDesiredSize.Height);
                }
                else
                {
                    stackDesiredSize.Width = Math.Max(stackDesiredSize.Width, childDesiredSize.Width);
                    stackDesiredSize.Height += (isVisible ? spacing : 0) + childDesiredSize.Height;
                }
            }

            if (fHorizontal)
            {
                stackDesiredSize.Width -= hasVisibleChild ? spacing : 0;
            }
            else
            {
                stackDesiredSize.Height -= hasVisibleChild ? spacing : 0;
            }

            return stackDesiredSize;
        }

        /// <summary>
        /// Content arrangement.
        /// </summary>
        /// <param name="finalSize">Arrange size</param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var children = Children;
            bool fHorizontal = (Orientation == Orientation.Horizontal);
            Rect rcChild = new Rect(finalSize);
            double previousChildSize = 0.0;
            var spacing = Spacing;

            //
            // Arrange and Position Children.
            //
            for (int i = 0, count = children.Count; i < count; ++i)
            {
                var child = children[i];

                if (child == null || !child.IsVisible)
                { continue; }

                if (fHorizontal)
                {
                    rcChild.X += previousChildSize;
                    previousChildSize = child.DesiredSize.Width;
                    rcChild.Width = previousChildSize;
                    rcChild.Height = Math.Max(finalSize.Height, child.DesiredSize.Height);
                    previousChildSize += spacing;
                }
                else
                {
                    rcChild.Y += previousChildSize;
                    previousChildSize = child.DesiredSize.Height;
                    rcChild.Height = previousChildSize;
                    rcChild.Width = Math.Max(finalSize.Width, child.DesiredSize.Width);
                    previousChildSize += spacing;
                }

                ArrangeChild(child, rcChild, finalSize, Orientation);
            }

            return finalSize;
        }

        internal virtual void ArrangeChild(
            UIElement child,
            Rect rect,
            Size panelSize,
            Orientation orientation)
        {
            child.Arrange(rect);
        }
    }
}
