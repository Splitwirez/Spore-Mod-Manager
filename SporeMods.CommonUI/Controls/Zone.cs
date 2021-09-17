using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SporeMods.CommonUI
{
    public class Zone : ContentControl
    {
        static Zone()
        {
            FocusableProperty.OverrideMetadata(typeof(Zone), new FrameworkPropertyMetadata(false));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Zone), new FrameworkPropertyMetadata(typeof(Zone)));
        }

        public Zone()
            : base()
        { }
    }
}
