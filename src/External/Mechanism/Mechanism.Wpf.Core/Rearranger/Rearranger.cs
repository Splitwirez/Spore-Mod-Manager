using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Mechanism.Wpf.Core.Rearranger
{
    [DefaultEvent("OnItemsChanged"), DefaultProperty("Items")]
    [ContentProperty("Items")]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(Rearranger))]
    [TemplatePart(Name = PartItemsDockPanel, Type = typeof(DockPanel))]
    [TemplatePart(Name = PartDragMovementCanvas, Type = typeof(Canvas))]
    [TemplatePart(Name = PartDragMovementGuide, Type = typeof(FrameworkElement))]
    public class Rearranger : ItemsControl
    {
        const String PartItemsDockPanel = "PART_ItemsDockPanel";
        const String PartDragMovementCanvas = "PART_DragMovementCanvas";
        const String PartDragMovementGuide = "PART_DragMovementGuide";

        DockPanel _itemsDockPanel;
        Canvas _dragMovementCanvas;
        FrameworkElement _dragMovementGuide;

        private object _currentItem = null;



        /// <summary>
        /// Gets (or sets) whether or not this Rearranger's panes can be reordered, resized, etc.
        /// </summary>
        public bool IsLocked
        {
            get => (bool)GetValue(IsLockedProperty);
            set => SetValue(IsLockedProperty, value);
        }

        public static DependencyProperty IsLockedProperty =
            DependencyProperty.Register(nameof(IsLocked), typeof(bool), typeof(Rearranger), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, OnIsLockedPropertyChangedCallback));

        static void OnIsLockedPropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Rearranger rerr)
                rerr.IsLockedChanged?.Invoke(sender, null);
        }
        public event EventHandler IsLockedChanged;



        /// <summary>
        /// Gets (or sets) whether or not this Rearranger's panes can be reordered, resized, etc.
        /// </summary>
        public bool IsDragging
        {
            get => (bool)GetValue(IsDraggingProperty);
            private set => SetValue(IsDraggingProperty, value);
        }

        public static DependencyProperty IsDraggingProperty =
            DependencyProperty.Register(nameof(IsDragging), typeof(bool), typeof(Rearranger), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));



        /// <summary>
        /// Gets (or sets) whether to collapse panes when the user "removes" them instead of actually removing them from the tree.
        /// </summary>
        public bool CollapseWhenRemoved
        {
            get => (bool)GetValue(CollapseWhenRemovedProperty);
            set => SetValue(CollapseWhenRemovedProperty, value);
        }

        public static DependencyProperty CollapseWhenRemovedProperty =
            DependencyProperty.Register(nameof(CollapseWhenRemoved), typeof(bool), typeof(Rearranger), new FrameworkPropertyMetadata(true));


        public static readonly DependencyProperty PaneTitleProperty = DependencyProperty.RegisterAttached("PaneTitle", typeof(string), typeof(Rearranger), new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// Gets the title to be shown on a pane containing this element
        /// </summary>
        /// <param name="element">the element from which to retrieve an associated title, if one exists</param>
        /// <returns>the title to be shown on a pane containing this element</returns>
        public static string GetPaneTitle(DependencyObject element)
        {
            return (string)element.GetValue(PaneTitleProperty);
        }

        /// <summary>
        /// Sets the title to be shown on a pane containing this element
        /// </summary>
        /// <param name="element">the element with which to associate a title</param>
        /// <param name="value">the title to associate with this element</param>
        public static void SetPaneTitle(DependencyObject element, string value)
        {
            element.SetValue(PaneTitleProperty, value);
        }



        public static DependencyProperty CanResizeProperty =
            DependencyProperty.RegisterAttached("CanResize", typeof(bool), typeof(Rearranger), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Gets whether or not this element can be resized
        /// </summary>
        /// <param name="element">the element whose resizeability is to be retrieved</param>
        /// <returns>whether or not a given element can be resized</returns>
        public static bool GetCanResize(DependencyObject element)
        {
            return (bool)element.GetValue(CanResizeProperty);
        }

        /// <summary>
        /// Sets whether or not this element can be resized
        /// </summary>
        /// <param name="element">the element whose resizeability is to be set</param>
        /// <param name="value">whether or not this element can be resized</param>
        public static void SetCanResize(DependencyObject element, bool value)
        {
            element.SetValue(CanResizeProperty, value);
        }



        public static DependencyProperty HideFrameWhenLockedProperty =
            DependencyProperty.RegisterAttached("HideFrameWhenLocked", typeof(bool), typeof(Rearranger), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Gets whether or not to hide the pane frame when this element's containing Rearranger is locked
        /// </summary>
        /// <param name="element">the element whose pane frame...um, holy something...how exactly does one say this stuff without sounding awkward beyond all comprehensibility?</param>
        /// <returns>whether to hide the pane frame when this element's containing Rearranger is locked</returns>
        public static bool GetHideFrameWhenLocked(DependencyObject element)
        {
            return (bool)element.GetValue(HideFrameWhenLockedProperty);
        }

        /// <summary>
        /// Sets whether or not to hide the pane frame when this element's containing Rearranger is locked
        /// </summary>
        /// <param name="element">the element whose pane frame resizeable-ness...uhhh...words, how do they work?</param>
        /// <param name="value">whether or not to hide the pane frame when this element's containing Rearranger is locked</param>
        public static void SetHideFrameWhenLocked(DependencyObject element, bool value)
        {
            element.SetValue(HideFrameWhenLockedProperty, value);
        }



        public static DependencyProperty HideTitlebarWhenLockedProperty =
            DependencyProperty.RegisterAttached("HideTitlebarWhenLocked", typeof(bool), typeof(Rearranger), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Sets whether or not to hide the pane titlebar when this element's containing Rearranger is locked
        /// </summary>
        /// <param name="element">titlebar? which element titlebar hide? Me element. You titlebar.</param>
        /// <param name="value">whether or not to hide the pane titlebar when this element's containing Rearranger is locked</param>
        public static bool GetHideTitlebarWhenLocked(DependencyObject element)
        {
            return (bool)element.GetValue(HideTitlebarWhenLockedProperty);
        }

        /// <summary>
        /// Sets whether or not to hide the pane titlebar when this element's containing Rearranger is locked
        /// </summary>
        /// <param name="element">sned hep</param>
        /// <param name="value">whether or not to hide the pane titlebar when this element's containing Rearranger is locked</param>
        public static void SetHideTitlebarWhenLocked(DependencyObject element, bool value)
        {
            element.SetValue(HideTitlebarWhenLockedProperty, value);
        }

        /*public static readonly DependencyProperty PaneDockSideProperty = DependencyProperty.RegisterAttached("PaneDockSide", typeof(Dock), typeof(Rearranger), new FrameworkPropertyMetadata(Dock.Left));

        public static Dock GetPaneDockSide(DependencyObject element)
        {
            return (Dock)element.GetValue(PaneDockSideProperty);
        }

        public static void SetPaneDockSide(DependencyObject element, Dock value)
        {
            element.SetValue(PaneDockSideProperty, value);
        }*/


        public Rearranger() : base()
        {
            RearrangeablePane.TitlebarMouseLeftButtonDown += Pane_TitlebarMouseLeftButtonDown;
            RearrangeablePane.RemoveButtonClicked += Pane_RemoveButtonClicked;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            bool val = item is RearrangeablePane;
            if (!val)
                _currentItem = item;

            return val;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            if (_currentItem is RearrangeablePane pane)
            {
                _currentItem = null;
                return pane;
            }
            else
            {
                _currentItem = null;

                return new RearrangeablePane()/*
            {
                Owner = this
            }*/;
            }
        }

        /*protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded != null)
            {
                if (visualRemoved is RearrangeablePane pane)
                    pane.TitlebarMouseLeftButtonDown += Pane_TitlebarMouseLeftButtonDown; //pane.Owner = this;
                else if (ContainerFromElement(this, visualAdded) is RearrangeablePane pane2)
                    pane2.TitlebarMouseLeftButtonDown -= Pane_TitlebarMouseLeftButtonDown; //pane2.Owner = this;
            }

            if (visualRemoved != null)
            {
                if (visualRemoved is RearrangeablePane pane)
                    pane.TitlebarMouseLeftButtonDown += Pane_TitlebarMouseLeftButtonDown; //pane.Owner = null;
                else if (ContainerFromElement(this, visualRemoved) is RearrangeablePane pane2)
                    pane2.TitlebarMouseLeftButtonDown -= Pane_TitlebarMouseLeftButtonDown; //pane2.Owner = null;
            }
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }*/

        private void Pane_TitlebarMouseLeftButtonDown(object sender, EventArgs e)
        {
            if ((sender != null) && (sender is RearrangeablePane pane))
                MovePane(pane);
        }

        private void Pane_RemoveButtonClicked(object sender, RoutedEventArgs e)
        {
            if ((sender != null) && (sender is RearrangeablePane pane))
            {
                if ((pane.Content != null) && (pane.Content is UIElement uiel))
                {
                    if (CollapseWhenRemoved)
                        uiel.Visibility = Visibility.Collapsed;
                    else
                        Items.Remove(uiel);
                }
            }
        }

        private static bool IsContainerForItemsControl(DependencyObject element, ItemsControl itemsControl)
        {
            return (element != null) && (element is RearrangeablePane);
        }

        public override void OnApplyTemplate()
        {
            /*foreach (object item in Items)
            {
                if ((item.GetType().FullName == "MS.Internal.NamedObject"))// || ((item as RearrangeablePane).Content.GetType().FullName == "MS.Internal.NamedObject"))
                    Items.Remove(item);
            }*/


            base.OnApplyTemplate();

            _itemsDockPanel = GetTemplateChild(PartItemsDockPanel) as DockPanel;

            _dragMovementCanvas = GetTemplateChild(PartDragMovementCanvas) as Canvas;

            //_dragMovementGhost = GetTemplateChild(PartDragMovementGhost) as Control;

            _dragMovementGuide = GetTemplateChild(PartDragMovementGuide) as FrameworkElement;
        }

        int EnsureValidIndex(int baseIndex)
        {
            int newIndex = baseIndex;
            if (newIndex >= _itemsDockPanel.Children.Count)
                newIndex = _itemsDockPanel.Children.Count - 1;
            if (newIndex < 0)
                newIndex = 0;
            return newIndex;
        }

        internal void MovePane(RearrangeablePane pane)
        {
            if (pane == null)
                throw new ArgumentNullException("The value of argument \"pane\" cannot be null.");
            else if (!_itemsDockPanel.Children.Contains(pane))
                throw new ArgumentException("The pane passed as argument \"pane\" does not belong to this Rearranger.");
            else
            {
                Point paneInitialPoint = pane.PointToScreen(new Point(0, 0));
                Vector paneCursorOffset = paneInitialPoint - SystemScaling.CursorPosition;

                pane.Visibility = Visibility.Hidden;
                //_dragMovementCanvas.Visibility = Visibility.Visible;
                IsDragging = true;
                Dock initialDock = DockPanel.GetDock(pane);
                int initialIndex = Items.IndexOf(pane);

                bool toNewLocation = false;
                Dock newDock = Dock.Left;
                int newIndex = 0;
                _itemsDockPanel.IsHitTestVisible = false;

                Timer timer = new Timer(10);

                timer.Elapsed += (sneder, args) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        bool done = Mouse.LeftButton == MouseButtonState.Released;
                        bool cancel = Keyboard.GetKeyStates(Key.Escape) == KeyStates.Down;
                        if (done || cancel)
                        {
                            timer.Stop();
                            //_dragMovementCanvas.Visibility = Visibility.Collapsed;
                            _itemsDockPanel.IsHitTestVisible = true;
                            IsDragging = false;

                            Debug.WriteLine("ITEMS BEFORE SHUFFLE: ");
                            for (int i = 0; i < Items.Count; i++)
                                Debug.WriteLine("Item at " + i + " is " + Items[i].GetType().FullName);

                            if ((!cancel) && (newIndex != initialIndex))
                            {
                                UIElement elem = pane;
                                if (/*(!IsItemItsOwnContainerOverride(pane)) && */(pane.Content != null) && (pane.Content is UIElement uiel))
                                    elem = uiel;

                                Debug.WriteLine("\nelem is RearrangeablePane: " + (elem is RearrangeablePane).ToString() + "\n");
                                Items.Remove(elem);
                                DockPanel.SetDock(elem, newDock);
                                if (toNewLocation)
                                    Items.Insert(EnsureValidIndex(newIndex), elem);
                                else
                                    Items.Insert(EnsureValidIndex(initialIndex), elem);
                            }
                            pane.Visibility = Visibility.Visible;

                            Debug.WriteLine("ITEMS AFTER SHUFFLE: ");
                            for (int i = 0; i < Items.Count; i++)
                                Debug.WriteLine("Item at " + i + " is " + Items[i].GetType().FullName);
                        }
                        else
                        {
                            paneCursorOffset = paneInitialPoint - SystemScaling.CursorPosition;
                            Canvas.SetLeft(_dragMovementGuide, Canvas.GetLeft(_dragMovementGuide) + paneCursorOffset.X);
                            Canvas.SetTop(_dragMovementGuide, Canvas.GetTop(_dragMovementGuide) + paneCursorOffset.Y);

                            Point rearrangerCursorPoint = PointFromScreen(SystemScaling.CursorPosition);
                            Point rearrangerPoint = PointToScreen(new Point(0, 0));
                            rearrangerPoint.X = SystemScaling.RealPixelsToWpfUnits(rearrangerPoint.X);
                            rearrangerPoint.Y = SystemScaling.RealPixelsToWpfUnits(rearrangerPoint.Y);
                            bool currentToNewLocation = false;

                            if (rearrangerCursorPoint.X < 50)
                            {
                                newIndex = 0;
                                newDock = Dock.Left;

                                Canvas.SetLeft(_dragMovementGuide, 0);
                                Canvas.SetTop(_dragMovementGuide, 0);
                                if ((pane.Content is FrameworkElement cont) && (cont.ActualWidth > 50))
                                    _dragMovementGuide.Width = cont.ActualWidth;
                                else
                                    _dragMovementGuide.Width = 50;
                                _dragMovementGuide.Height = ActualHeight;

                                currentToNewLocation = true;
                            }
                            else if (rearrangerCursorPoint.Y < 50)
                            {
                                newIndex = 0;
                                newDock = Dock.Top;

                                Canvas.SetLeft(_dragMovementGuide, 0);
                                Canvas.SetTop(_dragMovementGuide, 0);
                                _dragMovementGuide.Width = ActualWidth;
                                if ((pane.Content is FrameworkElement cont) && (cont.ActualHeight > 50))
                                    _dragMovementGuide.Height = cont.ActualHeight;
                                else
                                    _dragMovementGuide.Height = 50;

                                currentToNewLocation = true;
                            }
                            else if (rearrangerCursorPoint.X > (ActualWidth - 50))
                            {
                                newIndex = 0;
                                newDock = Dock.Right;

                                Canvas.SetTop(_dragMovementGuide, 0);
                                _dragMovementGuide.Width = 50; /*pane.ActualWidth;
                                if (_dragMovementGuide.ActualWidth < 50)
                                    _dragMovementGuide.Width = 50;*/
                                _dragMovementGuide.Height = ActualHeight;
                                Canvas.SetLeft(_dragMovementGuide, ActualWidth - _dragMovementGuide.ActualWidth);

                                currentToNewLocation = true;
                            }
                            else if (rearrangerCursorPoint.Y > (ActualHeight - 50))
                            {
                                newIndex = 0;
                                newDock = Dock.Bottom;

                                Canvas.SetLeft(_dragMovementGuide, 0);
                                _dragMovementGuide.Width = ActualWidth;
                                _dragMovementGuide.Height = 50; /*pane.ActualHeight;
                                if (_dragMovementGuide.ActualHeight < 50)
                                    _dragMovementGuide.Height = 50;*/
                                Canvas.SetTop(_dragMovementGuide, ActualHeight - _dragMovementGuide.ActualHeight);

                                currentToNewLocation = true;
                            }
                            else if (SystemScaling.IsMouseWithin((FrameworkElement)_itemsDockPanel.Children[_itemsDockPanel.Children.Count - 1]))
                            {
                                newIndex = _itemsDockPanel.Children.Count - 2;
                                newDock = Dock.Bottom;

                                RearrangeablePane lastPane = (RearrangeablePane)_itemsDockPanel.Children[_itemsDockPanel.Children.Count - 1];
                                Point lastPaneCurPos = lastPane.PointFromScreen(SystemScaling.CursorPosition);
                                double horizontal = lastPaneCurPos.X / lastPane.ActualWidth;
                                double vertical = lastPaneCurPos.Y / lastPane.ActualHeight;

                                if (horizontal > 0.5)
                                {
                                    if (vertical > 0.5)
                                    {
                                        if (horizontal > vertical)
                                            newDock = Dock.Right;
                                        else
                                            newDock = Dock.Bottom;
                                    }
                                    else
                                    {
                                        if (vertical > horizontal)
                                            newDock = Dock.Right;
                                        else
                                            newDock = Dock.Top;
                                    }
                                }
                                else
                                {
                                    if (vertical > 0.5)
                                    {
                                        if (horizontal > vertical)
                                            newDock = Dock.Left;
                                        else
                                            newDock = Dock.Bottom;
                                    }
                                    else
                                    {
                                        if (vertical > horizontal)
                                            newDock = Dock.Left;
                                        else
                                            newDock = Dock.Top;
                                    }
                                }

                                Point lastPaneRearrangerOffset = PointFromScreen(lastPane.PointToScreen(new Point(0, 0)));
                                if (newDock == Dock.Left)
                                {
                                    Canvas.SetLeft(_dragMovementGuide, lastPaneRearrangerOffset.X);
                                    Canvas.SetTop(_dragMovementGuide, lastPaneRearrangerOffset.Y);
                                    _dragMovementGuide.Width = 50;
                                    _dragMovementGuide.Height = lastPane.ActualHeight;
                                }
                                else if (newDock == Dock.Top)
                                {
                                    Canvas.SetLeft(_dragMovementGuide, lastPaneRearrangerOffset.X);
                                    Canvas.SetTop(_dragMovementGuide, lastPaneRearrangerOffset.Y);
                                    _dragMovementGuide.Width = lastPane.ActualWidth;
                                    _dragMovementGuide.Height = 50;
                                }
                                else if (newDock == Dock.Right)
                                {
                                    Canvas.SetLeft(_dragMovementGuide, (lastPaneRearrangerOffset.X + lastPane.ActualWidth) - 50);
                                    Canvas.SetTop(_dragMovementGuide, lastPaneRearrangerOffset.Y);
                                    _dragMovementGuide.Width = 50;
                                    _dragMovementGuide.Height = lastPane.ActualHeight;
                                }
                                else if (newDock == Dock.Bottom)
                                {
                                    Canvas.SetLeft(_dragMovementGuide, lastPaneRearrangerOffset.X);
                                    Canvas.SetTop(_dragMovementGuide, (lastPaneRearrangerOffset.Y + lastPane.ActualHeight) - 50);
                                    _dragMovementGuide.Width = lastPane.ActualWidth;
                                    _dragMovementGuide.Height = 50;
                                }

                                currentToNewLocation = true;
                            }
                            else
                            {
                                foreach (RearrangeablePane pn in _itemsDockPanel.Children)
                                {
                                    if (SystemScaling.IsMouseWithin(pn) && (_itemsDockPanel.Children.IndexOf(pn) != (_itemsDockPanel.Children.Count - 1)))
                                    {
                                        if (pn == pane)
                                        {
                                            newIndex = initialIndex;
                                            currentToNewLocation = false;

                                            Point pnPoint = _dragMovementCanvas.PointFromScreen(pn.PointToScreen(new Point(0, 0)));
                                            Canvas.SetLeft(_dragMovementGuide, pnPoint.X);
                                            Canvas.SetTop(_dragMovementGuide, pnPoint.Y);
                                            _dragMovementGuide.Width = pn.ActualWidth;
                                            _dragMovementGuide.Height = pn.ActualHeight;
                                        }
                                        else
                                        {
                                            currentToNewLocation = true;
                                            Point pnCurPoint = pn.PointFromScreen(SystemScaling.CursorPosition);
                                            Point pnPoint = _dragMovementCanvas.PointFromScreen(pn.PointToScreen(new Point(0, 0)));

                                            if ((DockPanel.GetDock(pn) == Dock.Top) || (DockPanel.GetDock(pn) == Dock.Bottom))
                                            {
                                                Canvas.SetLeft(_dragMovementGuide, pnPoint.X);

                                                if (pnCurPoint.Y > (pn.ActualHeight / 2))
                                                    Canvas.SetTop(_dragMovementGuide, pnPoint.Y + pn.ActualHeight / 2);
                                                else
                                                    Canvas.SetTop(_dragMovementGuide, pnPoint.Y);

                                                _dragMovementGuide.Width = pn.ActualWidth;
                                                _dragMovementGuide.Height = pn.ActualHeight / 2;
                                            }
                                            else
                                            {
                                                if (pnCurPoint.X > (pn.ActualWidth / 2))
                                                    Canvas.SetLeft(_dragMovementGuide, pnPoint.X + pn.ActualWidth / 2);
                                                else
                                                    Canvas.SetLeft(_dragMovementGuide, pnPoint.X);

                                                Canvas.SetTop(_dragMovementGuide, pnPoint.Y);

                                                _dragMovementGuide.Width = pn.ActualWidth / 2;
                                                _dragMovementGuide.Height = pn.ActualHeight;
                                            }


                                            if (DockPanel.GetDock(pn) == Dock.Top)
                                            {
                                                if (pnCurPoint.Y > (pn.ActualHeight / 2))
                                                    newIndex = _itemsDockPanel.Children.IndexOf(pn) + 1;
                                                else
                                                    newIndex = _itemsDockPanel.Children.IndexOf(pn);
                                            }
                                            else if (DockPanel.GetDock(pn) == Dock.Bottom)
                                            {
                                                if (pnCurPoint.Y <= (pn.ActualHeight / 2))
                                                    newIndex = _itemsDockPanel.Children.IndexOf(pn) - 1;
                                                else
                                                    newIndex = _itemsDockPanel.Children.IndexOf(pn);
                                            }
                                            else if (DockPanel.GetDock(pn) == Dock.Left)
                                            {
                                                if (pnCurPoint.X > (pn.ActualWidth / 2))
                                                    newIndex = _itemsDockPanel.Children.IndexOf(pn) + 1;
                                                else
                                                    newIndex = _itemsDockPanel.Children.IndexOf(pn);
                                            }
                                            else if (DockPanel.GetDock(pn) == Dock.Right)
                                            {
                                                if (pnCurPoint.X <= (pn.ActualWidth / 2))
                                                    newIndex = _itemsDockPanel.Children.IndexOf(pn) - 1;
                                                else
                                                    newIndex = _itemsDockPanel.Children.IndexOf(pn);
                                            }
                                            newDock = DockPanel.GetDock(pn);

                                            /*Canvas.SetLeft(_dragMovementGuide, pnPoint.X);
                                            Canvas.SetTop(_dragMovementGuide, pnPoint.Y);
                                            _dragMovementGuide.Width = pn.ActualWidth;
                                            _dragMovementGuide.Height = pn.ActualHeight;*/
                                        }
                                        break;
                                    }
                                }
                            }
                            toNewLocation = currentToNewLocation;
                        }
                    }));
                };
                timer.Start();
            }
        }
    }
}
