using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SporeMods.CommonUI
{
    public class UniformStackPanel : StackPanelEx
    {
        public static readonly DependencyProperty ForceScronchProperty =
            DependencyProperty.Register("ForceScronch", typeof(bool), typeof(UniformStackPanel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public bool ForceScronch
        {
            get => (bool)GetValue(ForceScronchProperty);
            set => SetValue(ForceScronchProperty, value);
        }


        static UniformStackPanel()
        {
            UseLayoutRoundingProperty.OverrideMetadata(typeof(UniformStackPanel), new FrameworkPropertyMetadata(true));
        }

        protected override Size MeasureOverride(Size constraint)
        {
            bool fHorizontal = (Orientation == Orientation.Horizontal);
            if (ForceScronch)
                return fHorizontal ? new Size(0, constraint.Height) : new Size(constraint.Width, 0);

            var children = InternalChildren;
            int count = children.Count;

            double spacing = Spacing;
            double totalSpaceBetween = spacing * Math.Max(0, count - 1);

            double baseChildExtent = ((fHorizontal ? constraint.Width : constraint.Height) / count) - totalSpaceBetween; //fHorizontal ? (constraint.Width - totalSpaceBetween) / count : (constraint.Height - totalSpaceBetween) / count;
            double maxChildExtent = baseChildExtent;
            
            double maxChildBreadth = 0;

            for (int i = 0; i < count; i++)
            {
                UIElement child = InternalChildren[i];

                if (child == null || (child.Visibility == Visibility.Collapsed))
                { continue; }

                child.Measure(constraint);
                Size childSize = child.DesiredSize;
                
                maxChildExtent = Math.Max(maxChildExtent, fHorizontal ? childSize.Width : childSize.Height);

                maxChildBreadth = Math.Max(maxChildBreadth, fHorizontal ? childSize.Height : childSize.Width);
            }

            double finalExtent = (maxChildExtent * count) + totalSpaceBetween;
            return new Size(fHorizontal ? finalExtent : maxChildBreadth, fHorizontal ? maxChildBreadth : finalExtent);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var children = InternalChildren;
            int count = children.Count;
            bool fHorizontal = (Orientation == Orientation.Horizontal);

            double spacing = Spacing;
            double totalSpaceBetween = spacing * Math.Max(0, count - 1);

            double childWidth;
            double childHeight;

            if (fHorizontal)
            {
                childWidth = (finalSize.Width - totalSpaceBetween) / count;
                childHeight = finalSize.Height;
            }
            else
            {
                childWidth = (finalSize.Height - totalSpaceBetween) / count;
                childHeight = finalSize.Width;
            }

            Rect rcChild = new Rect(0, 0, childWidth, childHeight);

            for (int i = 0; i < count; i++)
            {
                UIElement child = InternalChildren[i];

                if (child == null || (child.Visibility == Visibility.Collapsed))
                { continue; }

                child.Arrange(rcChild);
                if (fHorizontal)
                    rcChild.X += (childWidth + spacing);
                else
                    rcChild.Y += (childWidth + spacing);
            }

            return finalSize;
        }
    }
}
