using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Mechanism.Wpf.Core.Breadcrumbs
{
    //[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ListViewItem))]
    //[TemplatePart(Name = PartHeaderControl, Type = typeof(HeaderedContentControl))]
    [TemplatePart(Name = PartItemsPanel, Type = typeof(Panel))]
    [TemplatePart(Name = PartShowSubItemsToggleButton, Type = typeof(ToggleButton))]
    public class Breadcrumb : HeaderedItemsControl
    {
        public static event EventHandler<SelectedBreadcrumbChangedEventArgs> SelectionChanged;

        const String PartItemsPanel = "PART_ItemsPanel";
        const String PartShowSubItemsToggleButton = "PART_ShowSubItemsToggleButton";

        Panel _itemsPanel;
        ToggleButton _showSubItemsToggleButton;

        static Breadcrumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Breadcrumb), new FrameworkPropertyMetadata(typeof(Breadcrumb)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _itemsPanel = GetTemplateChild(PartItemsPanel) as Panel;
            _showSubItemsToggleButton = GetTemplateChild(PartShowSubItemsToggleButton) as ToggleButton;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ListViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var crumb = new ListViewItem();
            /*(_itemsPanel.Children[Items.IndexOf(obj)] as ListViewItem)*/
            crumb.MouseLeftButtonUp += SubItem_MouseLeftButtonUp; //.Selected += SubItem_Selected; //.Click += SubItem_Click;
            //crumb.PreviewMouseLeftButtonUp += Breadcrumb_PreviewMouseLeftButtonUp; //.SelectionChanged += Breadcrumb_SelectionChanged;
            return crumb;
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (false)
            {
                if (e.NewItems != null)
                {
                    foreach (object obj in e.NewItems)
                    {
                        /*for (int i = 0; i < _itemsPanel.Children.Count; i++)
                        {

                        }*/
                        (_itemsPanel.Children[Items.IndexOf(obj)] as ListViewItem).MouseLeftButtonUp += SubItem_MouseLeftButtonUp; //.Selected += SubItem_Selected; //.Click += SubItem_Click;
                    }
                }
            }
        }

        private void SubItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.WriteLine("aaaaa");
            _showSubItemsToggleButton.IsChecked = false;
            SelectionChanged?.Invoke(this, new SelectedBreadcrumbChangedEventArgs(sender));
            //var bar = (Parent as Panel).TemplatedParent as BreadcrumbsBar;
            //bar.SetItemAt((sender as ListViewItem).DataContext, bar.Items.IndexOf(this) + 1);
            //throw new NotImplementedException();
        }

        private void SubItem_Click(object sender, RoutedEventArgs e)
        {
            
        }

        /*const String PartHeaderControl = "PART_HeaderControl";

        HeaderedContentControl _headerControl;

        //[Bindable(true), Category("Content")]
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = HeaderedContentControl.HeaderProperty.AddOwner(typeof(Breadcrumb), new FrameworkPropertyMetadata((object)null));


        //[Bindable(true)]
        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty HeaderTemplateProperty =
                HeaderedContentControl.HeaderTemplateProperty.AddOwner(typeof(Breadcrumb), new FrameworkPropertyMetadata((DataTemplate)null));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _headerControl = GetTemplateChild(PartHeaderControl) as HeaderedContentControl;

            /*Binding headerBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath(HeaderProperty),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            _headerControl.SetBinding(HeaderedContentControl.HeaderProperty, headerBinding);

            Binding headerTemplateBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath(HeaderTemplateProperty),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            _headerControl.SetBinding(HeaderedContentControl.HeaderTemplateProperty, headerTemplateBinding);*
        }*/
    }

    public class SelectedBreadcrumbChangedEventArgs : EventArgs
    {
        public object NewCrumb { get; private set; } = null;

        public SelectedBreadcrumbChangedEventArgs(object newObj)
        {
            NewCrumb = newObj;
        }
    }
}
