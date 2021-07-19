using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Mechanism.Wpf.Core
{
    public class NoSizeDecorator : Decorator
    {
        protected override Size MeasureOverride(Size constraint)
        {
            // Ask for no space
            Child.Measure(new Size(0, 0));
            return new Size(0, base.MeasureOverride(constraint).Height);
        }
    }
}
