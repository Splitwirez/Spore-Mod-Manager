using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SporeMods.CommonUI
{
    public class ConnectedSelectDecorator : ContentControl
    {
        public Panel TargetPanel
        {
            get => (Panel)GetValue(TargetPanelProperty);
            set => SetValue(TargetPanelProperty, value);
        }

        public static readonly DependencyProperty TargetPanelProperty =
                    DependencyProperty.Register(nameof(TargetPanel), typeof(Panel), typeof(ConnectedSelectDecorator),
                        new PropertyMetadata(null, OnPropertiesChangedCallback));



        public int SelectedObjectIndex
        {
            get => (int)GetValue(SelectedObjectIndexProperty);
            set => SetValue(SelectedObjectIndexProperty, value);
        }

        public static readonly DependencyProperty SelectedObjectIndexProperty =
                    DependencyProperty.Register(nameof(SelectedObjectIndex), typeof(int), typeof(ConnectedSelectDecorator),
                        new PropertyMetadata(-1, OnPropertiesChangedCallback));



        public int CollectionSize
        {
            get => (int)GetValue(CollectionSizeProperty);
            set => SetValue(CollectionSizeProperty, value);
        }

        public static readonly DependencyProperty CollectionSizeProperty =
                    DependencyProperty.Register(nameof(CollectionSize), typeof(int), typeof(ConnectedSelectDecorator),
                        new PropertyMetadata(-1, OnPropertiesChangedCallback));



        public double SelectionLeft
        {
            get => (double)GetValue(SelectionLeftProperty);
            set => SetValue(SelectionLeftProperty, value);
        }

        public static readonly DependencyProperty SelectionLeftProperty =
            DependencyProperty.Register(nameof(SelectionLeft), typeof(double), typeof(ConnectedSelectDecorator), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));



        public double SelectionTop
        {
            get => (double)GetValue(SelectionTopProperty);
            set => SetValue(SelectionTopProperty, value);
        }

        public static readonly DependencyProperty SelectionTopProperty =
            DependencyProperty.Register(nameof(SelectionTop), typeof(double), typeof(ConnectedSelectDecorator), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));



        public double SelectionWidth
        {
            get => (double)GetValue(SelectionWidthProperty);
            set => SetValue(SelectionWidthProperty, value);
        }

        public static readonly DependencyProperty SelectionWidthProperty =
            DependencyProperty.Register(nameof(SelectionWidth), typeof(double), typeof(ConnectedSelectDecorator), new PropertyMetadata(0.0));



        public double SelectionHeight
        {
            get => (double)GetValue(SelectionHeightProperty);
            set => SetValue(SelectionHeightProperty, value);
        }

        public static readonly DependencyProperty SelectionHeightProperty =
            DependencyProperty.Register(nameof(SelectionHeight), typeof(double), typeof(ConnectedSelectDecorator), new PropertyMetadata(0.0));



        public bool SelectionExists
        {
            get => (bool)GetValue(SelectionExistsProperty);
            set => SetValue(SelectionExistsProperty, value);
        }

        public static readonly DependencyProperty SelectionExistsProperty =
            DependencyProperty.Register(nameof(SelectionExists), typeof(bool), typeof(ConnectedSelectDecorator), new PropertyMetadata(false));



        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(ConnectedSelectDecorator), new PropertyMetadata(TimeSpan.FromMilliseconds(0)));



        public IEasingFunction AnimationEase
        {
            get => (IEasingFunction)GetValue(AnimationEaseProperty);
            set => SetValue(AnimationEaseProperty, value);
        }

        public static readonly DependencyProperty AnimationEaseProperty =
            DependencyProperty.Register(nameof(AnimationEase), typeof(IEasingFunction), typeof(ConnectedSelectDecorator), new PropertyMetadata(null));



        static void OnPropertiesChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ConnectedSelectDecorator).UpdateSelectorBounds();
        }



        public ConnectedSelectDecorator()
        {
            //Loaded += ConnectedSelectDecorator_Loaded; //(sneder, args) => CompositionTarget.Rendering += (snedre, rags) => UpdateSelectorBounds();
            LayoutUpdated += (s, e) => UpdateSelectorBounds();
            SizeChanged += (s, e) => UpdateSelectorBounds();

            _leftAnim.Completed += (s, e) =>
            {
                _isAnimating = false;
                BeginAnimation(SelectionLeftProperty, null);
                SelectionLeft = _left;
            };
            _topAnim.Completed += (s, e) =>
            {
                BeginAnimation(SelectionTopProperty, null);
                SelectionTop = _top;
            };
            _widthAnim.Completed += (s, e) =>
            {
                BeginAnimation(SelectionWidthProperty, null);
                SelectionWidth = _width;
            };
            _heightAnim.Completed += (s, e) =>
            {
                BeginAnimation(SelectionHeightProperty, null);
                SelectionHeight = _height;
            };
        }

        
        DoubleAnimation _leftAnim = new DoubleAnimation();
        double _left = 0;
        
        DoubleAnimation _topAnim = new DoubleAnimation();
        double _top = 0;
        
        DoubleAnimation _widthAnim = new DoubleAnimation();
        double _width = 0;
        
        DoubleAnimation _heightAnim = new DoubleAnimation();
        double _height = 0;

        bool _isAnimating = false;
        FrameworkElement _prevSelected = null;
        
        void UpdateSelectorBounds()
        {
            //Console.WriteLine("0");
            if (TargetPanel != null)
            {
                bool doesSelectionExist = (SelectedObjectIndex >= 0) && (SelectedObjectIndex < TargetPanel.Children.Count);
                if (SelectionExists != doesSelectionExist)
                    SelectionExists = doesSelectionExist;

                if (SelectionExists && (PresentationSource.FromVisual(TargetPanel) != null) && (TargetPanel.Children[SelectedObjectIndex] is FrameworkElement panelChild))
                {
                    var point = panelChild.TranslatePoint(new Point(0, 0), this);

                    _left = point.X;
                    _top = point.Y;
                    _width = panelChild.ActualWidth;
                    _height = panelChild.ActualHeight;
                    
                    if ((_prevSelected == null) || (_prevSelected == panelChild))
                    {
                        if (!_isAnimating)
                        {
                            SelectionLeft = _left;
                            SelectionTop = _top;
                            SelectionWidth = _width;
                            SelectionHeight = _height;
                        }
                    }
                    else
                    {
                        Console.WriteLine("b");
                        _isAnimating = true;
                        
                        if (SelectionLeft != _left)
                        {
                            _leftAnim.To = _left;
                            _leftAnim.Duration = AnimationDuration;
                            _leftAnim.EasingFunction = AnimationEase;
                            BeginAnimation(SelectionLeftProperty, _leftAnim);
                        }

                        if (SelectionTop != _top)
                        {
                            _topAnim.To = _top;
                            _topAnim.Duration = AnimationDuration;
                            _topAnim.EasingFunction = AnimationEase;
                            BeginAnimation(SelectionTopProperty, _topAnim);
                        }

                        if (SelectionWidth != _width)
                        {
                            _widthAnim.To = _width;
                            _widthAnim.Duration = AnimationDuration;
                            _widthAnim.EasingFunction = AnimationEase;
                            BeginAnimation(SelectionWidthProperty, _widthAnim);
                        }

                        if (SelectionHeight != _height)
                        {
                            _heightAnim.To = _height;
                            _heightAnim.Duration = AnimationDuration;
                            _heightAnim.EasingFunction = AnimationEase;
                            BeginAnimation(SelectionHeightProperty, _heightAnim);
                        }
                    }
                    _prevSelected = panelChild;
                }
            }
        }
    }
}
