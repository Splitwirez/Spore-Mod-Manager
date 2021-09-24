using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SporeMods.CommonUI
{
    public class SmooshPanel : Panel
    {
        /// <summary>
        /// Defines the <see cref="Spacing"/> property.
        /// </summary>
        public static readonly DependencyProperty MaxChildExtentProperty =
            DependencyProperty.Register("MaxChildExtent", typeof(double), typeof(SmooshPanel), new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        /// <summary>
        /// Gets or sets a uniform distance (in pixels) between stacked items. It is applied in the
        /// direction of the StackLayout's Orientation.
        /// </summary>
        public double MaxChildExtent
        {
            get => (double)GetValue(MaxChildExtentProperty);
            set => SetValue(MaxChildExtentProperty, value);
        }


        public static readonly DependencyProperty UnsmooshToFillProperty =
            DependencyProperty.Register("UnsmooshToFill", typeof(bool), typeof(SmooshPanel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public bool UnsmooshToFill
        {
            get => (bool)GetValue(UnsmooshToFillProperty);
            set => SetValue(UnsmooshToFillProperty, value);
        }


        protected override Size MeasureOverride(Size availableSize)
        {
            
            var children = Children;
            int count = children.Count;
            
            _childBoundsCache.Clear();
            
            double childExtent = MaxChildExtent;
            
            double targetWidth = 0;
            int numPerRow = 0;

            double inWidth = availableSize.Width;
            while (targetWidth < inWidth)
            {
                targetWidth += childExtent;
                numPerRow++;
            }
            
            if (UnsmooshToFill)
                numPerRow = Math.Min(numPerRow, count);
            
            numPerRow = Math.Max(1, numPerRow);

            childExtent = inWidth / numPerRow; //(numPerRow > 1) ? (targetWidth / numPerRow) : 1;

            Size layoutSlotSize = new Size(childExtent, double.PositiveInfinity);

            
            double[] heights = new double[numPerRow];
            int column = 0;

            Rect rcChild/*;
            if (!measure)
                rcChild*/ = new Rect(availableSize);
            
            for (int index = 0; index < count; index++)
            {
                var child = children[index];

                if ((child == null) || (!child.IsVisible))
                    continue;

                if (column >= numPerRow)
                    column = 0;

                // Measure the child.
                /*if (measure)
                {*/
                    child.Measure(layoutSlotSize);
                
                    double childDesiredHeight = child.DesiredSize.Height;
                    
                    int targetColumn = column; //0;
                    double targetColumnHeight = heights[targetColumn]; /*double.MaxValue;
                    
                    for (int t = 0; t < numPerRow; t++)
                    {
                        if (heights[t] < targetColumnHeight)
                        {
                            targetColumnHeight = heights[t];
                            targetColumn = t;
                        }
                    }*/

                    
                    rcChild.X = childExtent * targetColumn;
                    rcChild.Y = targetColumnHeight;

                    rcChild.Width = childExtent;
                    rcChild.Height = childDesiredHeight;
                    
                    _childBoundsCache.Add(rcChild);
                    //child.Arrange(rcChild);
                //}
                
                heights[targetColumn] += childDesiredHeight;
                column++;
            }

            return new Size(inWidth, heights.Max());
        }

        List<Rect> _childBoundsCache = new List<Rect>();

        protected override Size ArrangeOverride(Size finalSize)
        {
            var children = Children;
            int count = children.Count;

            for (int i = 0; i < count; i++)
            {
                children[i].Arrange(_childBoundsCache[i]);
            }
            //return DoSizeStuff(finalSize, false);
            return finalSize;
        }
    }
}
