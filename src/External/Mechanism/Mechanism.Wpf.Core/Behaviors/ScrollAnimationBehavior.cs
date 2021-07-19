using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Input;

using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Media;
using Mechanism.Wpf.Core;

namespace Mechanism.Wpf.Core.Behaviors
{
    //Somewhat based on https://stackoverflow.com/questions/20731402/animated-smooth-scrolling-on-scrollviewer/20846310
    public static class ScrollAnimationBehavior
    {
        private static ScrollViewer _listBoxScroller = new ScrollViewer();

        public static DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset",
                                                typeof(double),
                                                typeof(ScrollAnimationBehavior),
                                                new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }

        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }

        public static DependencyProperty AnimatedVerticalOffsetProperty =
            DependencyProperty.RegisterAttached("AnimatedVerticalOffset",
                                                typeof(double),
                                                typeof(ScrollAnimationBehavior),
                                                new UIPropertyMetadata(0.0, OnAnimatedVerticalOffsetChanged));

        public static void SetAnimatedVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(AnimatedVerticalOffsetProperty, value);
        }

        public static double GetAnimatedVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(AnimatedVerticalOffsetProperty);
        }

        static void OnAnimatedVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = target as ScrollViewer;

            double difference = ((double)e.NewValue - (double)e.OldValue);
            //Debug.WriteLine("Scroll animation difference: " + difference);

            /*double toVal = Math.Max(scrollViewer.VerticalOffset + difference, GetPointsToScroll(scrollViewer));

            if (difference < 0)
                toVal = Math.Min(scrollViewer.VerticalOffset + difference, -GetPointsToScroll(scrollViewer));

            if (toVal < 0)
                toVal = 0;
            else if (toVal > scrollViewer.ScrollableHeight)
                toVal = scrollViewer.ScrollableHeight;*/

            DoubleAnimation verticalAnimation = new DoubleAnimation()
            {
                From = scrollViewer.VerticalOffset,
                To = (double)e.NewValue,//scrollViewer.VerticalOffset + difference, //toVal,
                EasingFunction = GetEasingFunction(scrollViewer),
                Duration = new Duration(GetTimeDuration(scrollViewer))
            };

            scrollViewer.BeginAnimation(VerticalOffsetProperty, verticalAnimation);
        }

        public static DependencyProperty TimeDurationProperty =
            DependencyProperty.RegisterAttached("TimeDuration",
                                                typeof(TimeSpan),
                                                typeof(ScrollAnimationBehavior),
                                                new PropertyMetadata(new TimeSpan(0, 0, 0, 0, 0)));

        public static void SetTimeDuration(FrameworkElement target, TimeSpan value)
        {
            target.SetValue(TimeDurationProperty, value);
        }

        public static TimeSpan GetTimeDuration(FrameworkElement target)
        {
            return (TimeSpan)target.GetValue(TimeDurationProperty);
        }

        public static DependencyProperty EasingFunctionProperty =
            DependencyProperty.RegisterAttached("EasingFunction",
                                                typeof(IEasingFunction),
                                                typeof(ScrollAnimationBehavior),
                                                new UIPropertyMetadata(null));

        public static void SetEasingFunction(FrameworkElement target, IEasingFunction value)
        {
            target.SetValue(EasingFunctionProperty, value);
        }

        public static IEasingFunction GetEasingFunction(FrameworkElement target)
        {
            return (IEasingFunction)target.GetValue(EasingFunctionProperty);
        }

        public static DependencyProperty PointsToScrollProperty =
            DependencyProperty.RegisterAttached("PointsToScroll",
                                                typeof(double),
                                                typeof(ScrollAnimationBehavior),
                                                new PropertyMetadata(-1.0));

        public static void SetPointsToScroll(FrameworkElement target, double value)
        {
            double realValue = value;
            if (value == -1)
                realValue = SystemParameters.WheelScrollLines;
            target.SetValue(PointsToScrollProperty, realValue);
        }

        public static double GetPointsToScroll(FrameworkElement target)
        {
            double outVal = (double)target.GetValue(PointsToScrollProperty);
            if (outVal == -1)
                outVal = SystemParameters.WheelScrollLines;
            return outVal;
        }

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = target as ScrollViewer;

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        public static DependencyProperty IsEnabledProperty =
                                                DependencyProperty.RegisterAttached("IsEnabled",
                                                typeof(bool),
                                                typeof(ScrollAnimationBehavior),
                                                new UIPropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(FrameworkElement target, bool value)
        {
            target.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(FrameworkElement target)
        {
            return (bool)target.GetValue(IsEnabledProperty);
        }

        private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if ((sender != null) && sender is ScrollViewer target)
                {
                    if (target.IsLoaded)
                        scrollerLoaded(target, null);
                    else
                        target.Loaded += new RoutedEventHandler(scrollerLoaded);
                }

                /*if ((sender != null) && sender is ListBox targetBox)
                {
                    targetBox.Loaded += new RoutedEventHandler(listboxLoaded);
                }*/
            }
            else
            {
                //Debug.WriteLine("Disabling...");
                if ((sender != null) && sender is ScrollViewer target)
                {
                    //Debug.WriteLine("...for ScrollViewer");
                    UnsetEventHandlersForScrollViewer(target);
                    target.Loaded -= new RoutedEventHandler(scrollerLoaded);
                }

                /*if ((sender != null) && sender is ListBox targetBox)
                {
                    Debug.WriteLine("...for ListBox");
                    UnsetListBoxBindingsAndHandlers(targetBox);
                    targetBox.Loaded -= new RoutedEventHandler(listboxLoaded);
                }*/
            }
        }

        private static void AnimateScroll(ScrollViewer scrollViewer, double ToValue)
        {
            if (GetIsEnabled(scrollViewer))
                SetAnimatedVerticalOffset(scrollViewer, ToValue);

            /*Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(verticalAnimation);
            Storyboard.SetTarget(verticalAnimation, scrollViewer);
            Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(ScrollAnimationBehavior.VerticalOffsetProperty));
            storyboard.Begin();*/
        }

        private static double NormalizeScrollPos(ScrollViewer scroll, double scrollChange, Orientation o)
        {
            double returnValue = scrollChange;

            if (scrollChange < 0)
            {
                returnValue = 0;
            }

            if (o == Orientation.Vertical && scrollChange > scroll.ScrollableHeight)
            {
                returnValue = scroll.ScrollableHeight;
            }
            else if (o == Orientation.Horizontal && scrollChange > scroll.ScrollableWidth)
            {
                returnValue = scroll.ScrollableWidth;
            }

            return returnValue;
        }

        private static void UpdateScrollPosition(object sender)
        {
            ListBox listbox = sender as ListBox;

            if (listbox != null)
            {
                double scrollTo = 0;

                for (int i = 0; i < (listbox.SelectedIndex); i++)
                {
                    ListBoxItem tempItem = listbox.ItemContainerGenerator.ContainerFromItem(listbox.Items[i]) as ListBoxItem;

                    if (tempItem != null)
                    {
                        scrollTo += tempItem.ActualHeight;
                    }
                }

                AnimateScroll(_listBoxScroller, scrollTo);
            }
        }

        private static void SetEventHandlersForScrollViewer(ScrollViewer scroller)
        {
            scroller.ScrollChanged += new ScrollChangedEventHandler(ScrollViewerScrollChanged);
            scroller.PreviewMouseWheel += new MouseWheelEventHandler(ScrollViewerPreviewMouseWheel);
            scroller.PreviewKeyDown += new KeyEventHandler(ScrollViewerPreviewKeyDown);
        }

        private static void UnsetEventHandlersForScrollViewer(ScrollViewer scroller)
        {
            scroller.ScrollChanged -= new ScrollChangedEventHandler(ScrollViewerScrollChanged);
            scroller.PreviewMouseWheel -= new MouseWheelEventHandler(ScrollViewerPreviewMouseWheel);
            scroller.PreviewKeyDown -= new KeyEventHandler(ScrollViewerPreviewKeyDown);
            //Debug.WriteLine("Unsetting EventHandlers for ScrollViewer");
        }

        private static void scrollerLoaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scroller = sender as ScrollViewer;

            SetEventHandlersForScrollViewer(scroller);
        }

        private static void listboxLoaded(object sender, RoutedEventArgs e)
        {
            ListBox listbox = sender as ListBox;

            _listBoxScroller = FindVisualChildHelper.GetFirstChildOfType<ScrollViewer>(listbox);
            SetListBoxBindingsAndHandlers(listbox);
        }

        private static void SetListBoxBindingsAndHandlers(ListBox listbox)
        {
            SetEventHandlersForScrollViewer(_listBoxScroller);

            _listBoxScroller.SetBinding(TimeDurationProperty, new Binding()
            {
                Source = listbox,
                Path = new PropertyPath(TimeDurationProperty),
                Mode = BindingMode.OneWay
            });

            _listBoxScroller.SetBinding(PointsToScrollProperty, new Binding()
            {
                Source = listbox,
                Path = new PropertyPath(PointsToScrollProperty),
                Mode = BindingMode.OneWay
            });

            _listBoxScroller.SetBinding(EasingFunctionProperty, new Binding()
            {
                Source = listbox,
                Path = new PropertyPath(EasingFunctionProperty),
                Mode = BindingMode.OneWay
            });

            /*SetTimeDuration(_listBoxScroller, new TimeSpan(0, 0, 0, 0, 200));
            SetPointsToScroll(_listBoxScroller, 16.0);*/

            listbox.SelectionChanged += new SelectionChangedEventHandler(ListBoxSelectionChanged);
            listbox.Loaded += new RoutedEventHandler(ListBoxLoaded);
            listbox.LayoutUpdated += new EventHandler(ListBoxLayoutUpdated);
        }

        private static void UnsetListBoxBindingsAndHandlers(ListBox listbox)
        {
            UnsetEventHandlersForScrollViewer(_listBoxScroller);

            BindingOperations.ClearBinding(_listBoxScroller, TimeDurationProperty);

            BindingOperations.ClearBinding(_listBoxScroller, PointsToScrollProperty);

            BindingOperations.ClearBinding(_listBoxScroller, EasingFunctionProperty);

            /*SetTimeDuration(_listBoxScroller, new TimeSpan(0, 0, 0, 0, 200));
            SetPointsToScroll(_listBoxScroller, 16.0);*/

            listbox.SelectionChanged -= new SelectionChangedEventHandler(ListBoxSelectionChanged);
            listbox.Loaded -= new RoutedEventHandler(ListBoxLoaded);
            listbox.LayoutUpdated -= new EventHandler(ListBoxLayoutUpdated);
        }

        private static void ScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scroller = (ScrollViewer)sender;
            //Debug.WriteLine("Scroll info: " + GetPointsToScroll(scroller) + ", " + SystemParameters.WheelScrollLines + ", " + e.Delta);
            double mouseWheelChange = (double)e.Delta / 3;
            double newVOffset = GetAnimatedVerticalOffset(scroller) - (/*((e.Delta / -120.0) * TextBlock.GetLineHeight(scroller))*/mouseWheelChange * GetPointsToScroll(scroller)); //(mouseWheelChange / 3);

            if (newVOffset < 0)
            {
                AnimateScroll(scroller, 0);
                //Debug.WriteLine("ScrollViewerPreviewMouseWheel " + 0);
            }
            else if (newVOffset > scroller.ScrollableHeight)
            {
                AnimateScroll(scroller, scroller.ScrollableHeight);
                //Debug.WriteLine("ScrollViewerPreviewMouseWheel " + 1);
            }
            else
            {
                AnimateScroll(scroller, newVOffset);
                //Debug.WriteLine("ScrollViewerPreviewMouseWheel " + 2);
            }

            e.Handled = true;
        }

        private static void ScrollViewerPreviewKeyDown(object sender, KeyEventArgs e)
        {
            ScrollViewer scroller = (ScrollViewer)sender;

            Key keyPressed = e.Key;
            double newVerticalPos = GetVerticalOffset(scroller);
            bool isKeyHandled = false;

            //bool isListBox = (scroller.TemplatedParent != null) && ;
            ListBox listBox = null;
            if ((scroller.TemplatedParent != null) && (scroller.TemplatedParent is ListBox box))
                listBox = scroller.TemplatedParent as ListBox;

            if (false) //listBox == null)
            {
                if (keyPressed == Key.Down)
                {
                    /*if ((listBox != null) && (listBox.SelectedIndex < (listBox.Items.Count - 1)))
                        listBox.SelectedIndex++;
                    else*/
                    newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos + GetPointsToScroll(scroller)), Orientation.Vertical);
                    isKeyHandled = true;
                }
                else if (keyPressed == Key.PageDown)
                {
                    newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos + scroller.ViewportHeight), Orientation.Vertical);
                    isKeyHandled = true;
                }
                else if ((keyPressed == Key.Up) && (listBox == null))
                {
                    /*if ((listBox != null) && (listBox.SelectedIndex > 0))
                        listBox.SelectedIndex--;
                    else*/
                    newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos - GetPointsToScroll(scroller)), Orientation.Vertical);
                    isKeyHandled = true;
                }
                else if (keyPressed == Key.PageUp)
                {
                    newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos - scroller.ViewportHeight), Orientation.Vertical);
                    isKeyHandled = true;
                }

                if (newVerticalPos != GetVerticalOffset(scroller))
                {
                    AnimateScroll(scroller, newVerticalPos);
                }

                e.Handled = isKeyHandled;
            }
        }

        private static void ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scroller = (ScrollViewer)sender;

            if ((scroller.VerticalOffset != GetAnimatedVerticalOffset(scroller)) && (scroller.VerticalOffset != GetVerticalOffset(scroller)))
                SetAnimatedVerticalOffset(scroller, scroller.VerticalOffset);
        }

        private static void ListBoxLayoutUpdated(object sender, EventArgs e)
        {
            //Debug.WriteLine("ListBoxLayoutUpdated");
            UpdateScrollPosition(sender);
        }

        private static void ListBoxLoaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("ListBoxLoaded");
            UpdateScrollPosition(sender);
        }

        private static void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Debug.WriteLine("ListBoxSelectionChanged");
            ListBox listBox = sender as ListBox;

            if (listBox != null)
            {
                double scrollTo = 0;
                bool confirmScroll = true;

                if (e.AddedItems.Count > 0)
                {
                    Point listPoint = _listBoxScroller.PointToScreen(new Point(0, 0));
                    listPoint.X = SystemScaling.RealPixelsToWpfUnits(listPoint.X);
                    listPoint.Y = SystemScaling.RealPixelsToWpfUnits(listPoint.Y);
                    ListBoxItem tempItem = listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[listBox.Items.IndexOf(e.AddedItems[0])]) as ListBoxItem;
                    Point itemPoint = tempItem.PointToScreen(new Point(0, 0));
                    itemPoint.X = SystemScaling.RealPixelsToWpfUnits(itemPoint.X);
                    itemPoint.Y = SystemScaling.RealPixelsToWpfUnits(itemPoint.Y);

                    //Debug.WriteLine("MEASURED VALUES: " + itemPoint.Y + ", " + listPoint.Y + ", " + tempItem.ActualHeight + ", " + _listBoxScroller.ActualHeight);
                    if (itemPoint.Y < listPoint.Y)
                    {
                        //Debug.WriteLine("SCROLLING UP: " + itemPoint.Y + ", " + listPoint.Y);
                        scrollTo = _listBoxScroller.VerticalOffset + (itemPoint.Y - listPoint.Y);
                        //Debug.WriteLine("scrollTo: " + scrollTo);
                    }
                    else if ((itemPoint.Y + tempItem.ActualHeight) > (listPoint.Y + _listBoxScroller.ActualHeight + _listBoxScroller.Padding.Top + _listBoxScroller.Padding.Bottom))
                    {
                        //Debug.WriteLine("SCROLLING DOWN: " + (itemPoint.Y + tempItem.ActualHeight) + ", " + (listPoint.Y + _listBoxScroller.ActualHeight + _listBoxScroller.Padding.Top + _listBoxScroller.Padding.Bottom));
                        scrollTo = (itemPoint.Y + tempItem.ActualHeight) - ((listPoint.Y + _listBoxScroller.ActualHeight) - _listBoxScroller.ContentVerticalOffset);
                    }
                    else
                        confirmScroll = false;


                    /*for (int i = 0; i < (listbox.Items.IndexOf(e.AddedItems[0])); i++)
                    {
                        ListBoxItem tempItem = listbox.ItemContainerGenerator.ContainerFromItem(listbox.Items[i]) as ListBoxItem;

                        if (tempItem != null)
                        {
                            scrollTo += tempItem.ActualHeight;
                        }
                    }*/
                }
                else if (e.RemovedItems.Count > 0)
                {
                    for (int i = 0; i < (listBox.Items.IndexOf(e.RemovedItems[0])); i++)
                    {
                        ListBoxItem tempItem = listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[i]) as ListBoxItem;

                        if (tempItem != null)
                        {
                            scrollTo += tempItem.ActualHeight;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < (listBox.SelectedIndex); i++)
                    {
                        ListBoxItem tempItem = listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[i]) as ListBoxItem;

                        if (tempItem != null)
                        {
                            scrollTo += tempItem.ActualHeight;
                        }
                    }
                }

                if (confirmScroll)
                    AnimateScroll(_listBoxScroller, scrollTo);
            }
        }
    }

    public static class FindVisualChildHelper
    {
        public static T GetFirstChildOfType<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return null;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);

                var result = (child as T) ?? GetFirstChildOfType<T>(child);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
