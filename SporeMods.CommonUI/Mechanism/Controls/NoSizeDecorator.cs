using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SporeMods.CommonUI
{
    public class NoSizeDecorator : Decorator
    {
        protected override Size MeasureOverride(Size constraint)
        {
            //double width = SafeClamp(Width, MinWidth, MaxWidth);

            //double height = SafeClamp(Height, MinHeight, MaxHeight);
            
            Child.Measure(new Size(0, 0)); //width, height));
            return new Size(base.MeasureOverride(constraint).Width, base.MeasureOverride(constraint).Height);
        }

        static double SafeClamp(double value, double min, double max)
        {
            if (!IsSaneNumber(value))
                return 0;
            else
            {
                bool saneMin = IsSaneNumber(min);
                bool saneMax = IsSaneNumber(max);
                
                if (saneMin)
                {
                    if (saneMax)
                        return Math.Clamp(value, min, max);
                    else
                        return Math.Max(value, min);
                }
                else if (saneMax)
                    return Math.Min(value, max);
                else
                    return value;
            }
        }

        static bool IsSaneNumber(double value)
            => (!double.IsInfinity(value)) && (!double.IsNaN(value));
    }
}
