using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SporeMods.CommonUI
{
    public class FloorPanel : Panel
    {
        protected override Size MeasureOverride(Size constraint)
        {
            var children = Children;
            int count = children.Count;

            Size maxChildSize = Size.Empty;

            for (int i = 0; i < count; i++)
            {
                var child = children[i];

                child.Measure(constraint);
                var childDesiredSize = child.DesiredSize;

                maxChildSize = new Size(Math.Max(maxChildSize.Width, childDesiredSize.Width), Math.Max(maxChildSize.Height, childDesiredSize.Height));
            }

            return maxChildSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var children = InternalChildren;
            int count = children.Count;

            Rect rcChild = new Rect(finalSize);

            foreach (UIElement child in children)
            {
                child.Arrange(rcChild);
            }

            return finalSize;
        }
    }
}
