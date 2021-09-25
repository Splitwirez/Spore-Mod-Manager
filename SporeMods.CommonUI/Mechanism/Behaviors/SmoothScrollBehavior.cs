using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using System.Windows.Interactivity;

namespace SporeMods.CommonUI
{
    public class SmoothScrollBehavior : Behavior<ScrollContentPresenter>
    {
        /*public static readonly DependencyProperty VerticalIntervalProperty =
            DependencyProperty.Register("VerticalInterval", typeof(double), typeof(SmoothScrollBehavior), new FrameworkPropertyMetadata(0.0));
        
        public double VerticalInterval
        {
            get => (double)GetValue(VerticalIntervalProperty);
            set => SetValue(VerticalIntervalProperty, value);
        }*/
        
        public static readonly DependencyProperty VerticalSmoothingProperty =
            DependencyProperty.Register("VerticalSmoothing", typeof(double), typeof(SmoothScrollBehavior), new FrameworkPropertyMetadata(0.0, OnVerticalSmoothingPropertyChangedCallback));
        
        public static void OnVerticalSmoothingPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Console.WriteLine($"Smoothing to {e.NewValue}");
            ((SmoothScrollBehavior)d)._currentVerticalSmoothing = (double)(e.NewValue);
        }

        static readonly bool VERTICAL_SCROLL_PAGES = System.Windows.Forms.SystemInformation.MouseWheelScrollLines == -1;
        static readonly double VERTICAL_INTERVAL = SystemScaling.RealPixelsToWpfUnits(System.Windows.Forms.SystemInformation.MouseWheelScrollLines * 18); //new System.Windows.Forms.VScrollBar().SmallChange);

        ScrollViewer _viewer = null;
        
        FrameworkElement _content = null;
        TranslateTransform _transform = new TranslateTransform();

        DispatcherTimer _smoothScrollTimer = null;

        EasingFunctionBase _currentEasing = new QuinticEase()
        {
            EasingMode = EasingMode.EaseOut
        };

        protected override void OnAttached()
        {
            base.OnAttached();

            _viewer = AssociatedObject.TemplatedParent as ScrollViewer;
            
            

            BindingOperations.SetBinding(_transform, TranslateTransform.YProperty,
            new Binding()
            {
                Source = this,
                Path = new PropertyPath("VerticalSmoothing"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });


            if (AssociatedObject.Content is FrameworkElement el)
            /*{
                el.RenderTransform = transform;*/
                _content = el;
            //}
            
            _viewer.PreviewMouseWheel += WhenMouseWheel;
            /*_viewer.ScrollChanged += (s, e) =>
            {
                if (_smoothScrollTimer.IsEnabled)
                {
                    _smoothScrollTimer.Stop();
                    _content.RenderTransform = null;
                }
            };*/


            _animStartVerticalOffset = _viewer.VerticalOffset;

            _smoothScrollTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(1), DispatcherPriority.Render, SmoothScrollTimer_Elapsed, this.Dispatcher);
            _smoothScrollTimer.Stop();
        }

        
        //bool _isAnimatingVertically = false;

        /*DoubleAnimation CreateAnimation()
        {
            /*if (_verticalAnimation != null)
                _verticalAnimation.Completed -= WhenAnimationCompleted;* /

            DoubleAnimation anim = new DoubleAnimation()
            {
                //From = from,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new QuinticEase()
                {
                    EasingMode = EasingMode.EaseOut
                }
            };
            anim.Completed += WhenAnimationCompleted;
            return anim;
        }
        DoubleAnimation CreateAnimation(double from)
        {
            DoubleAnimation anim = CreateAnimation();
            anim.From = from;
            return anim;
        }

        void WhenAnimationCompleted(object sender, EventArgs e)
        {
            _verticalAnimations.RemoveAt(0);
            Console.WriteLine($"Completed: {_verticalAnimations.Count}");
            
            if (_verticalAnimations.Count <= 0)
            {
                BeginAnimation(VerticalSmoothingProperty, null);
                _isAnimatingVertically = false;
            }
        }*/

        TimeSpan _animDuration = TimeSpan.FromMilliseconds(125);
        private double _animDurationMs = 0;
        private double _animTimeRemainingMs = 0;

        private void SmoothScrollTimer_Elapsed(object sender, EventArgs e)
        {
            double totalDistanceY = _animEndVerticalOffset - _animStartVerticalOffset;
            
            double percentage = 1 - Math.Min(Math.Max(0, _animTimeRemainingMs / _animDurationMs), 1);
            double easedPercentage = _currentEasing.Ease(percentage);
            //Console.WriteLine("percentage: " + percentage + "; \teasedPercentage: " + easedPercentage);

            _currentVerticalOffset = _animStartVerticalOffset + (totalDistanceY * easedPercentage);

            _viewer.ScrollToVerticalOffset(_currentVerticalOffset);
            //_animTimeRemainingMs--;
            //Console.WriteLine("_animTimeRemaining: " + _animTimeRemaining);
            
            //_currentVerticalSmoothing = _currentVerticalOffset;
            
            
            _animTimeRemainingMs--;
            if (_animTimeRemainingMs <= 0)
            {
                _smoothScrollTimer.Stop();
                
                _content.RenderTransform = null;
                _viewer.ScrollToVerticalOffset(_animEndVerticalOffset);
                //_animStartVerticalOffset = _animEndVerticalOffset;
            }
        }
        
        //List<DoubleAnimation> _verticalAnimations = new List<DoubleAnimation>();
        
        double _currentVerticalOffset = 0.0;
        double _animStartVerticalOffset = 0.0;
        double _animEndVerticalOffset = 0.0;
        //double _animStartVerticalSmoothing = 0.0;
        double _currentVerticalSmoothing = 0.0;
        
        private void WhenMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scrollChange = 0;
            double jump = VERTICAL_SCROLL_PAGES ? _viewer.ActualHeight : VERTICAL_INTERVAL;
            if (e.Delta > 0)
                scrollChange = -jump; //_viewer.VerticalOffset;
            else if (e.Delta < 0)
                scrollChange = jump;
            else
                return;
            
            e.Handled = true;
            DoAnimation(scrollChange);

            //double y = _viewer.VerticalOffset + scrollChange;

            /*double height = _isLogical ? scrollable!.ScrollSize.Height : 50;
            y += -e.Delta.Y * height;
            y = Math.Max(y, 0);
            y = Math.Min(y, Extent.Height - Viewport.Height);*/
            
            

            //}
            //_verticalAnimations.Add(newAnimation);
            //BeginAnimation(VerticalSmoothingProperty, newAnimation); //, HandoffBehavior.SnapshotAndReplace);
        }

        void DoAnimation(double vDelta)
        {
            bool enabled = _smoothScrollTimer.IsEnabled;
            _smoothScrollTimer.Stop();

            if (enabled)
            {
                _animStartVerticalOffset = _currentVerticalOffset;
                _animEndVerticalOffset += vDelta;
            }
            else
            {
                _animStartVerticalOffset = _viewer.VerticalOffset;
                _animEndVerticalOffset = _viewer.VerticalOffset + vDelta;
                
                _animDurationMs = _animDuration.TotalMilliseconds;
            }
            _animStartVerticalOffset = Math.Min(Math.Max(_animStartVerticalOffset, 0), _viewer.ExtentHeight);
            _animEndVerticalOffset = Math.Min(Math.Max(_animEndVerticalOffset, 0), _viewer.ExtentHeight);
            

            _animTimeRemainingMs = _animDurationMs;
            _smoothScrollTimer.Start();
        }
    }
}
