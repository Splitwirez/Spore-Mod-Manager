using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mechanism.Wpf.Core.Flair
{
    public class TravellingSelector : ContentControl
    {
        /*public IEnumerable<object> Collection
        {
            get => (IEnumerable<object>)GetValue(CollectionProperty);
            set => SetValue(CollectionProperty, value);
        }

        public static readonly DependencyProperty CollectionProperty =
                    DependencyProperty.RegisterAttached("Collection", typeof(IEnumerable<object>), typeof(TravellingSelector),
                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPropertiesChangedCallback));*/

        public Panel TargetPanel
        {
            get => (Panel)GetValue(TargetPanelProperty);
            set => SetValue(TargetPanelProperty, value);
        }

        public static readonly DependencyProperty TargetPanelProperty =
                    DependencyProperty.Register("TargetPanel", typeof(Panel), typeof(TravellingSelector),
                        new PropertyMetadata(null, OnTargetPanelPropertyChangedCallback));

        /*public object SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        public static readonly DependencyProperty SelectedObjectProperty =
                    DependencyProperty.RegisterAttached("SelectedObject", typeof(object), typeof(TravellingSelector),
                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPropertiesChangedCallback));*/

        public int SelectedObjectIndex
        {
            get => (int)GetValue(SelectedObjectIndexProperty);
            set => SetValue(SelectedObjectIndexProperty, value);
        }

        public static readonly DependencyProperty SelectedObjectIndexProperty =
                    DependencyProperty.Register("SelectedObjectIndex", typeof(int), typeof(TravellingSelector),
                        new PropertyMetadata(-1, OnPropertiesChangedCallback));

        public int CollectionSize
        {
            get => (int)GetValue(CollectionSizeProperty);
            set => SetValue(CollectionSizeProperty, value);
        }

        public static readonly DependencyProperty CollectionSizeProperty =
                    DependencyProperty.Register("CollectionSize", typeof(int), typeof(TravellingSelector),
                        new PropertyMetadata(-1, OnPropertiesChangedCallback));

        public double SelectionWidth
        {
            get => (double)GetValue(SelectionWidthProperty);
            set => SetValue(SelectionWidthProperty, value);
        }

        public static readonly DependencyProperty SelectionWidthProperty =
            DependencyProperty.Register("SelectionWidth", typeof(double), typeof(TravellingSelector), new PropertyMetadata(0.0));

        public double SelectionHeight
        {
            get => (double)GetValue(SelectionHeightProperty);
            set => SetValue(SelectionHeightProperty, value);
        }

        public static readonly DependencyProperty SelectionHeightProperty =
            DependencyProperty.Register("SelectionHeight", typeof(double), typeof(TravellingSelector), new PropertyMetadata(0.0));

        public bool SelectionExists
        {
            get => (bool)GetValue(SelectionExistsProperty);
            set => SetValue(SelectionExistsProperty, value);
        }

        public static readonly DependencyProperty SelectionExistsProperty =
            DependencyProperty.Register("SelectionExists", typeof(bool), typeof(TravellingSelector), new PropertyMetadata(false));

        /*public TranslateTransform SelectorTransform
        {
            get => (TranslateTransform)GetValue(SelectorTransformProperty);
            set => SetValue(SelectorTransformProperty, value);
        }

        public static readonly DependencyProperty SelectorTransformProperty =
            DependencyProperty.Register("SelectorTransform", typeof(TranslateTransform), typeof(TravellingSelector), new PropertyMetadata(new TranslateTransform(0,0)));*/

        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(TimeSpan), typeof(TravellingSelector), new PropertyMetadata(TimeSpan.FromMilliseconds(0)));

        public IEasingFunction AnimationEase
        {
            get => (IEasingFunction)GetValue(AnimationEaseProperty);
            set => SetValue(AnimationEaseProperty, value);
        }

        public static readonly DependencyProperty AnimationEaseProperty =
            DependencyProperty.Register("AnimationEase", typeof(IEasingFunction), typeof(TravellingSelector), new PropertyMetadata(null));

        TranslateTransform _selectorTransform = new TranslateTransform(0, 0);

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            (GetTemplateChild("PART_Selector") as UIElement).RenderTransform = _selectorTransform;
        }

        static void OnTargetPanelPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            var sned = (sender as TravellingSelector);
            if ((e.NewValue != null) && (e.NewValue is Panel panel))
            {
                panel.Loaded += sned.TravellingSelector_Loaded;
                panel.SizeChanged += sned.Panel_SizeChanged;
            }

            if ((e.OldValue != null) && (e.NewValue is Panel hPanel))
            {
                hPanel.Loaded -= sned.TravellingSelector_Loaded;
                hPanel.SizeChanged -= sned.Panel_SizeChanged;
            }

            sned.UpdateSelectorBounds();
        }

        private void Panel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSelectorBounds();
        }

        static void OnPropertiesChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as TravellingSelector).UpdateSelectorBounds();
        }

        static void zOnPropertiesChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Debug.WriteLine("TravellingSelector updated!");
            TravellingSelector selector = sender as TravellingSelector;
            if (selector.TargetPanel != null)
            {
                //Debug.WriteLine("selector.TargetPanel != null");

                bool doesSelectionExist = (selector.SelectedObjectIndex >= 0) && (selector.SelectedObjectIndex < selector.TargetPanel.Children.Count);
                if (selector.SelectionExists != doesSelectionExist)
                    selector.SelectionExists = doesSelectionExist;

                if (selector.SelectionExists)
                {
                    //Debug.WriteLine("selector.SelectionExists");
                    //int index = selector.Collection.ToList().IndexOf(selector.SelectedObject);
                    var panelPoint = selector.TargetPanel.PointToScreen(new Point(0, 0));
                    FrameworkElement panelChild = (FrameworkElement)(selector.TargetPanel.Children[selector.SelectedObjectIndex]);
                    var childPoint = panelChild.PointToScreen(new Point(0, 0));

                    double newX = childPoint.X - panelPoint.X;
                    DoubleAnimation xAnimation = new DoubleAnimation()
                    {
                        To = newX,
                        Duration = selector.AnimationDuration
                    };
                    /*xAnimation.Completed += (sneder, args) =>
                    {
                        selector._selectorTransform.BeginAnimation(TranslateTransform.XProperty, null);
                        selector._selectorTransform.X = newX;
                    };*/

                    double newY = childPoint.Y - panelPoint.Y;
                    DoubleAnimation yAnimation = new DoubleAnimation()
                    {
                        To = newY,
                        Duration = selector.AnimationDuration
                    };
                    /*yAnimation.Completed += (sneder, args) =>
                    {
                        selector._selectorTransform.BeginAnimation(TranslateTransform.YProperty, null);
                        selector._selectorTransform.Y = newY;
                    };*/

                    DoubleAnimation widthAnimation = new DoubleAnimation()
                    {
                        To = panelChild.ActualWidth,
                        Duration = selector.AnimationDuration
                    };
                    /*widthAnimation.Completed += (sneder, args) =>
                    {
                        selector.BeginAnimation(TravellingSelector.SelectionWidthProperty, null);
                        selector.SelectionWidth = panelChild.ActualWidth;
                    };*/

                    DoubleAnimation heightAnimation = new DoubleAnimation()
                    {
                        To = panelChild.ActualHeight,
                        Duration = selector.AnimationDuration
                    };
                    /*heightAnimation.Completed += (sneder, args) =>
                    {
                        selector.BeginAnimation(TravellingSelector.SelectionHeightProperty, null);
                        selector.SelectionWidth = panelChild.ActualHeight;
                    };*/

                    //E
                    //selector._selectorTransform.Y = newY; //childPoint.Y - panelPoint.Y;
                    selector.SelectionWidth = 133; //panelChild.ActualWidth;
                    selector.SelectionWidth = 30; //panelChild.ActualHeight;
                    
                    if (selector.AnimationEase != null)
                    {
                        xAnimation.EasingFunction = selector.AnimationEase;
                        yAnimation.EasingFunction = selector.AnimationEase;
                        widthAnimation.EasingFunction = selector.AnimationEase;
                        heightAnimation.EasingFunction = selector.AnimationEase;
                    }

                    //Debug.WriteLine((childPoint.X - panelPoint.X).ToString() + ", " + (childPoint.Y - panelPoint.Y).ToString() + ", " + panelChild.ActualWidth + ", " + panelChild.ActualHeight);
                    selector._selectorTransform.BeginAnimation(TranslateTransform.XProperty, xAnimation);
                    selector._selectorTransform.BeginAnimation(TranslateTransform.YProperty, yAnimation);
                    selector.BeginAnimation(TravellingSelector.SelectionWidthProperty, widthAnimation);
                    selector.BeginAnimation(TravellingSelector.SelectionHeightProperty, heightAnimation);
                }
            }
        }

        void UpdateSelectorBounds()
        {
            if (TargetPanel != null)
            {
                //Debug.WriteLine("TargetPanel != null");

                bool doesSelectionExist = (SelectedObjectIndex >= 0) && (SelectedObjectIndex < TargetPanel.Children.Count);
                if (SelectionExists != doesSelectionExist)
                    SelectionExists = doesSelectionExist;

                if (SelectionExists && (PresentationSource.FromVisual(TargetPanel) != null))
                {
                    //Debug.WriteLine("TravellingSelector: Selection exists");
                    //int index = Collection.ToList().IndexOf(SelectedObject);
                    var panelPoint = TargetPanel.PointToScreen(new Point(0, 0));
                    FrameworkElement panelChild = (FrameworkElement)(TargetPanel.Children[SelectedObjectIndex]);
                    var childPoint = panelChild.PointToScreen(new Point(0, 0));

                    double newX = childPoint.X - panelPoint.X;
                    DoubleAnimation xAnimation = new DoubleAnimation()
                    {
                        To = newX,
                        Duration = AnimationDuration
                    };
                    /*xAnimation.Completed += (sneder, args) =>
                    {
                        _selectorTransform.BeginAnimation(TranslateTransform.XProperty, null);
                        _selectorTransform.X = newX;
                    };*/

                    double newY = childPoint.Y - panelPoint.Y;
                    DoubleAnimation yAnimation = new DoubleAnimation()
                    {
                        To = newY,
                        Duration = AnimationDuration
                    };
                    /*yAnimation.Completed += (sneder, args) =>
                    {
                        _selectorTransform.BeginAnimation(TranslateTransform.YProperty, null);
                        _selectorTransform.Y = newY;
                    };*/

                    DoubleAnimation widthAnimation = new DoubleAnimation()
                    {
                        To = panelChild.ActualWidth,
                        Duration = AnimationDuration
                    };
                    /*widthAnimation.Completed += (sneder, args) =>
                    {
                        BeginAnimation(TravellingSelectionWidthProperty, null);
                        SelectionWidth = panelChild.ActualWidth;
                    };*/

                    DoubleAnimation heightAnimation = new DoubleAnimation()
                    {
                        To = panelChild.ActualHeight,
                        Duration = AnimationDuration
                    };
                    /*heightAnimation.Completed += (sneder, args) =>
                    {
                        BeginAnimation(TravellingSelectionHeightProperty, null);
                        SelectionWidth = panelChild.ActualHeight;
                    };*/

                    //E
                    //_selectorTransform.Y = newY; //childPoint.Y - panelPoint.Y;
                    SelectionWidth = 133; //panelChild.ActualWidth;
                    SelectionWidth = 30; //panelChild.ActualHeight;

                    if (AnimationEase != null)
                    {
                        xAnimation.EasingFunction = AnimationEase;
                        yAnimation.EasingFunction = AnimationEase;
                        widthAnimation.EasingFunction = AnimationEase;
                        heightAnimation.EasingFunction = AnimationEase;
                    }

                    //Debug.WriteLine((childPoint.X - panelPoint.X).ToString() + ", " + (childPoint.Y - panelPoint.Y).ToString() + ", " + panelChild.ActualWidth + ", " + panelChild.ActualHeight);
                    _selectorTransform.BeginAnimation(TranslateTransform.XProperty, xAnimation);
                    _selectorTransform.BeginAnimation(TranslateTransform.YProperty, yAnimation);
                    BeginAnimation(TravellingSelector.SelectionWidthProperty, widthAnimation);
                    BeginAnimation(TravellingSelector.SelectionHeightProperty, heightAnimation);
                }
                /*else
                    Debug.WriteLine("TravellingSelector: Selection does not exist");*/
            }
        }

        public TravellingSelector()
        {
            Loaded += TravellingSelector_Loaded;
            CompositionTarget.Rendering += (sneder, args) => UpdateSelectorBounds();
            UpdateSelectorBounds();
        }

        private void TravellingSelector_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSelectorBounds();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            UpdateSelectorBounds();
        }
    }
}
