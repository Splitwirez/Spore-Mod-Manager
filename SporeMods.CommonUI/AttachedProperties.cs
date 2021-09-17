using SporeMods.CommonUI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

/*#if NETCOREAPP3_0
using Microsoft.Xaml.Behaviors;
#else*/
using System.Windows.Interactivity;
//#endif

namespace SporeMods.CommonUI
{
    public class CornerCurvesConverter : System.Windows.CornerRadiusConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            //return (sourceType == typeof(string)) || (sourceType == typeof(bool)) || (sourceType == typeof(Boolean));
            TypeCode code = Type.GetTypeCode(sourceType);
            switch (code)
            {
                case TypeCode.String:
                    return true;
                case TypeCode.Boolean:
                    return true;

                default:
                    return false;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
        {
            return (destinationType == typeof(CornerCurves)) || (destinationType == typeof(string)) || (destinationType == typeof(String));
        }

        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
        {
            if (source != null)
            {
                if (source is bool boolVal)
                    return new CornerCurves(boolVal);
                else if (source is string strVal)
                {
                    string[] values = strVal.Replace(" ", ",").Replace(",,", ",").Split(",".ToCharArray());
                    if ((values.Length == 1) && bool.TryParse(values[0], out bool uniformBool))
                        return new CornerCurves(uniformBool);
                    else if ((values.Length == 4) && bool.TryParse(values[0], out bool topLeftBool) && bool.TryParse(values[1], out bool topRightBool) && bool.TryParse(values[2], out bool bottomRightBool) && bool.TryParse(values[3], out bool bottomLeftBool))
                        return new CornerCurves(topLeftBool, topRightBool, bottomRightBool, bottomLeftBool);
                }
            }

            throw new NotSupportedException("Cannot convert this type to 'CornerCurves'!");
        }

        public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
        {
            if (value is CornerCurves curve)
            {
                if (destinationType == typeof(bool))
                    return curve.TopLeft;
                else
                    return curve.ToString();
            }
            throw new Exception("Cannot convert 'CornerCurves' to this type!");
        }
    }


    [TypeConverter(typeof(CornerCurvesConverter))]
    public class CornerCurves : Freezable
    {
        public bool TopLeft
        {
            get => (bool)GetValue(TopLeftProperty);
            set => SetValue(TopLeftProperty, value);
        }

        public static readonly DependencyProperty TopLeftProperty =
            DependencyProperty.Register(nameof(TopLeft), typeof(bool), typeof(CornerCurves), new PropertyMetadata(true));

        public bool TopRight
        {
            get => (bool)GetValue(TopRightProperty);
            set => SetValue(TopRightProperty, value);
        }

        public static readonly DependencyProperty TopRightProperty =
            DependencyProperty.Register(nameof(TopRight), typeof(bool), typeof(CornerCurves), new PropertyMetadata(true));

        public bool BottomLeft
        {
            get => (bool)GetValue(BottomLeftProperty);
            set => SetValue(BottomLeftProperty, value);
        }

        public static readonly DependencyProperty BottomLeftProperty =
            DependencyProperty.Register(nameof(BottomLeft), typeof(bool), typeof(CornerCurves), new PropertyMetadata(true));

        public bool BottomRight
        {
            get => (bool)GetValue(BottomRightProperty);
            set => SetValue(BottomRightProperty, value);
        }

        public static readonly DependencyProperty BottomRightProperty =
            DependencyProperty.Register(nameof(BottomRight), typeof(bool), typeof(CornerCurves), new PropertyMetadata(true));

        /*public bool TopLeft { get; set; } = true;
        public bool TopRight { get; set; } = true;
        public bool BottomRight { get; set; } = true;
        public bool BottomLeft { get; set; } = true;*/

        public CornerCurves()
        {
        }

        public CornerCurves(bool uniformValue)
        {
            TopLeft = uniformValue;
            TopRight = uniformValue;
            BottomRight = uniformValue;
            BottomLeft = uniformValue;
        }

        public CornerCurves(bool topLeft, bool topRight, bool bottomRight, bool bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CornerCurves(true);
        }

#if NETCOREAPP3_0
        public override string? ToString()
#else
        public override string ToString()
#endif
        {
            return TopLeft + "," + TopRight + "," + BottomRight + "," + BottomLeft;
        }
    }

    public class BorderPresenceConverter : System.Windows.CornerRadiusConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            //return (sourceType == typeof(string)) || (sourceType == typeof(bool)) || (sourceType == typeof(Boolean));
            TypeCode code = Type.GetTypeCode(sourceType);
            switch (code)
            {
                case TypeCode.String:
                    return true;
                case TypeCode.Boolean:
                    return true;

                default:
                    return false;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
        {
            return (destinationType == typeof(BorderPresence)) || (destinationType == typeof(string)) || (destinationType == typeof(String));
        }

        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
        {
            if (source != null)
            {
                if (source is bool boolVal)
                    return new BorderPresence(boolVal);
                else if (source is string strVal)
                {
                    string[] values = strVal.Replace(" ", ",").Replace(",,", ",").Split(",".ToCharArray());
                    if ((values.Length == 1) && bool.TryParse(values[0], out bool uniformBool))
                        return new BorderPresence(uniformBool);
                    else if ((values.Length == 4) && bool.TryParse(values[0], out bool topLeftBool) && bool.TryParse(values[1], out bool topRightBool) && bool.TryParse(values[2], out bool bottomRightBool) && bool.TryParse(values[3], out bool bottomLeftBool))
                        return new BorderPresence(topLeftBool, topRightBool, bottomRightBool, bottomLeftBool);
                }
            }

            throw new NotSupportedException("Cannot convert this type to 'BorderPresence'!");
        }

        public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
        {
            if (value is BorderPresence brd)
            {
                if (destinationType == typeof(bool))
                    return brd.Left;
                else
                    return brd.ToString();
            }
            throw new Exception("Cannot convert 'BorderPresence' to this type!");
        }
    }

    [TypeConverter(typeof(BorderPresenceConverter))]
    public class BorderPresence : Freezable
    {
        public bool Left
        {
            get => (bool)GetValue(LeftProperty);
            set => SetValue(LeftProperty, value);
        }

        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register(nameof(Left), typeof(bool), typeof(BorderPresence), new PropertyMetadata(true));

        public bool Top
        {
            get => (bool)GetValue(TopProperty);
            set => SetValue(TopProperty, value);
        }

        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register(nameof(Top), typeof(bool), typeof(BorderPresence), new PropertyMetadata(true));

        public bool Right
        {
            get => (bool)GetValue(RightProperty);
            set => SetValue(RightProperty, value);
        }

        public static readonly DependencyProperty RightProperty =
            DependencyProperty.Register(nameof(Right), typeof(bool), typeof(BorderPresence), new PropertyMetadata(true));

        public bool Bottom
        {
            get => (bool)GetValue(BottomProperty);
            set => SetValue(BottomProperty, value);
        }

        public static readonly DependencyProperty BottomProperty =
            DependencyProperty.Register(nameof(Bottom), typeof(bool), typeof(BorderPresence), new PropertyMetadata(true));

        public BorderPresence()
        {
        }

        public BorderPresence(bool uniformValue)
        {
            Left = uniformValue;
            Top = uniformValue;
            Right = uniformValue;
            Bottom = uniformValue;
        }

        public BorderPresence(bool left, bool top, bool right, bool bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BorderPresence(true);
        }

#if NETCOREAPP3_0
        public override string? ToString()
#else
        public override string ToString()
#endif
        {
            return Left + "," + Top + "," + Right + "," + Bottom;
        }
    }

    public class AttachedProperties : DependencyObject
    {
        /*static AttachedProperties()
        {
            AttachedProperties.CornerCurvesProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata(new CornerCurves(true)));
        }*/

        public static readonly DependencyProperty CornerCurvesProperty =
            DependencyProperty.RegisterAttached("CornerCurves", typeof(CornerCurves), typeof(AttachedProperties), new FrameworkPropertyMetadata(new CornerCurves(true)));

        public static CornerCurves GetCornerCurves(DependencyObject element)
        {
            return (CornerCurves)element.GetValue(CornerCurvesProperty);
        }

        public static void SetCornerCurves(DependencyObject element, CornerCurves value)
        {
            element.SetValue(CornerCurvesProperty, value);
        }

        public static readonly DependencyProperty BorderPresenceProperty =
            DependencyProperty.RegisterAttached("BorderPresence", typeof(BorderPresence), typeof(AttachedProperties), new FrameworkPropertyMetadata(new BorderPresence(true)));

        public static BorderPresence GetBorderPresence(DependencyObject element)
        {
            return (BorderPresence)element.GetValue(BorderPresenceProperty);
        }

        public static void SetBorderPresence(DependencyObject element, BorderPresence value)
        {
            element.SetValue(BorderPresenceProperty, value);
        }

        public static readonly DependencyProperty IsContextMenuTouchableProperty =
            DependencyProperty.RegisterAttached("IsContextMenuTouchable", typeof(bool), typeof(AttachedProperties), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnIsContextMenuTouchablePropertyChangedCallback));

        public static bool GetIsContextMenuTouchable(DependencyObject element)
        {
            return (bool)element.GetValue(IsContextMenuTouchableProperty);
        }

        public static void SetIsContextMenuTouchable(DependencyObject element, bool value)
        {
            element.SetValue(IsContextMenuTouchableProperty, value);
        }

        internal static void OnIsContextMenuTouchablePropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ContextMenu menu)
            {
                if (((bool)e.NewValue) && (!(bool)e.OldValue))
                {
                    Interaction.GetBehaviors(menu).Add(new ClickVsTouchContextMenuBehavior());

                    /*if (!menu.IsOpen)
                    {
                        menu.IsOpen = true;
                        menu.IsOpen = false;
                    }*/
                }
                else if (((bool)e.OldValue) && (!(bool)e.NewValue))
                {
                    var behaviors = Interaction.GetBehaviors(menu);
                    for (int i = 0; i < behaviors.Count; i++)
                    {
                        var behavior = behaviors[i];
                        if (behavior is ClickVsTouchContextMenuBehavior)
                        {
                            Interaction.GetBehaviors(menu).Remove(behavior);
                            break;
                        }
                    }
                }
            }
        }
        
        public bool LastClickWasTouch
        {
            get => (bool)GetValue(LastClickWasTouchProperty);
            set => SetValue(LastClickWasTouchProperty, value);
        }

        public static readonly DependencyProperty LastClickWasTouchProperty = DependencyProperty.Register("LastClickWasTouch",
            typeof(bool), typeof(AttachedProperties),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, OnLastClickWasTouchPropertyChangedCallback));

        public static bool GetLastClickWasTouch(DependencyObject element)
        {
            return (bool)element.GetValue(LastClickWasTouchProperty);
        }

        public static void SetLastClickWasTouch(DependencyObject element, bool value)
        {
            element?.SetValue(LastClickWasTouchProperty, value);
        }

        internal static void OnLastClickWasTouchPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ContextMenu menu)
                Debug.WriteLine("ContextMenu LastClickWasTouch: " + GetLastClickWasTouch(menu).ToString());
            else if (sender is MenuItem item)
                Debug.WriteLine("MenuItem LastClickWasTouch: " + GetLastClickWasTouch(item).ToString());
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(AttachedProperties), new PropertyMetadata(null));

        public static object GetIcon(DependencyObject element)
        {
            return element.GetValue(IconProperty);
        }

        public static void SetIcon(DependencyObject element, object value)
        {
            element.SetValue(IconProperty, value);
        }



        public static readonly DependencyProperty CastDirectionProperty =
            DependencyProperty.RegisterAttached("CastDirection", typeof(ExpandDirection), typeof(AttachedProperties), new FrameworkPropertyMetadata(ExpandDirection.Down));

        public static ExpandDirection GetCastDirection(Separator element)
        {
            return (ExpandDirection)element.GetValue(CastDirectionProperty);
        }

        public static void SetCastDirection(Separator element, ExpandDirection value)
        {
            element.SetValue(CastDirectionProperty, value);
        }
    }
}
