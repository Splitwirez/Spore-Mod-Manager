using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mechanism.Wpf.Core.Flair
{
    public partial class CutCornerBorder : Border
    {
        public CornerRadius CornerDistance
        {
            get => (CornerRadius)GetValue(CornerDistanceProperty);
            set => SetValue(CornerDistanceProperty, value);
        }

        public static readonly DependencyProperty CornerDistanceProperty =
            DependencyProperty.Register("CornerDistance", typeof(CornerRadius), typeof(CutCornerBorder), new PropertyMetadata(new CornerRadius(5)));

        new CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        new static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CutCornerBorder), new PropertyMetadata(new CornerRadius(0)));

        new public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        new public static DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(CutCornerBorder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnBackgroundPropertyChangedCallback));

        static void OnBackgroundPropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CutCornerBorder).path.Fill = (Brush)(e.NewValue);
        }

        new public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        new public static DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(CutCornerBorder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnBorderBrushPropertyChangedCallback));

        static void OnBorderBrushPropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CutCornerBorder).path.Stroke = (Brush)(e.NewValue);
        }

        new public Double BorderThickness
        {
            get => (Double)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        new public static DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Double), typeof(CutCornerBorder), new FrameworkPropertyMetadata((Double)0, FrameworkPropertyMetadataOptions.AffectsRender, OnBorderThicknessPropertyChangedCallback));

        static void OnBorderThicknessPropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CutCornerBorder).path.StrokeThickness = (Double)(e.NewValue);
        }

        Path path = new Path()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Visibility = Visibility.Visible
        };

        public CutCornerBorder()
        {
            base.Background = new VisualBrush()
            {
                Visual = path
            };
            //Child = path;
            SizeChanged += CutCornerBorder_SizeChanged;
        }

        private void CutCornerBorder_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            var constraint = e.NewSize;
            path.Width = constraint.Width;
            path.Height = constraint.Height;
            SetBorder();
        }

        void SetBorder()
        {
            var geom = Geometry.Parse("M 0 " + CornerDistance.TopLeft.ToString() + " L " + CornerDistance.TopLeft.ToString() + " 0 " +
                "L " + (path.Width - CornerDistance.TopRight).ToString() + " 0 L " + path.Width.ToString() + " " + CornerDistance.TopRight.ToString() + " " +
                "L " + path.Width.ToString() + " " + (path.Height - CornerDistance.BottomRight).ToString() + " L " + (path.Width - CornerDistance.BottomRight).ToString() + " " + path.Height.ToString() + " " +
                "L " + CornerDistance.BottomLeft.ToString() + " " + path.Height.ToString() + " L 0 " + (path.Height - CornerDistance.BottomLeft).ToString()
                + " Z");
            path.Data = geom;
            Clip = geom;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            path.Width = constraint.Width;
            path.Height = constraint.Height;
            SetBorder();
            return constraint;
        }
    }

    public class CutCornerDecorator : Decorator
    {
        static FrameworkPropertyMetadataOptions _options = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange;

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(CutCornerDecorator), new FrameworkPropertyMetadata(new CornerRadius(6), _options));

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(CutCornerDecorator), new FrameworkPropertyMetadata(null, _options));

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(CutCornerDecorator), new FrameworkPropertyMetadata(null, _options));

        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(CutCornerDecorator), new FrameworkPropertyMetadata(new Thickness(1), _options));

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            DrawCutCorners(drawingContext);
        }

        private void DrawCutCorners(DrawingContext dc)
        {
            dc.DrawGeometry(this.Background, new Pen(BorderBrush, BorderThickness.Left), GetCutCornerGeometry(ActualWidth, ActualHeight));
        }

        private Geometry GetCutCornerGeometry(double width, double height)
        {
            return PathGeometry.Parse("M 0 " + CornerRadius.TopLeft + " L " + CornerRadius.TopLeft + " 0 L " + (width - CornerRadius.TopRight) + " 0 L " + width + " " + CornerRadius.TopRight + " L " + width + " " + (height - CornerRadius.BottomRight) + " L " + (width - CornerRadius.BottomRight) + " " + height + " L " + CornerRadius.BottomLeft + " " + height + " L 0 " + (height - CornerRadius.BottomLeft) + " Z");
        }
    }
}
