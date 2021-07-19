using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mechanism.Wpf.Core
{
    public class CommandBar : ItemsControl
    {
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            bool returnValue = item is ButtonBase;
            if (!returnValue)
            {
                Type type = item.GetType();
                while (type != typeof(object))
                {
                    type = type.BaseType;
                    if (type == typeof(ButtonBase))
                    {
                        returnValue = true;
                        break;
                    }
                }
            }
            return returnValue;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new Button();
        }

        public static readonly DependencyProperty LayersProperty = DependencyProperty.RegisterAttached("Layers", typeof(string), typeof(DependencyObject), new FrameworkPropertyMetadata(string.Empty));

        public static string GetLayers(DependencyObject element)
        {
            return (string)element.GetValue(LayersProperty);
        }

        public static void SetLayers(DependencyObject element, string value)
        {
            element.SetValue(LayersProperty, value);
        }

        public static readonly DependencyProperty AnyLayerVisibleProperty = DependencyProperty.RegisterAttached("AnyLayerVisible", typeof(Visibility), typeof(DependencyObject), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static Visibility GetAnyLayerVisible(DependencyObject element)
        {
            var returnValue = (Visibility)element.GetValue(AnyLayerVisibleProperty);
            //Debug.WriteLine("GetAnyLayerVisible: " + returnValue.ToString());
            return returnValue;
        }

        public static void SetAnyLayerVisible(DependencyObject element, Visibility value)
        {
            element.SetValue(AnyLayerVisibleProperty, value);
            //Debug.WriteLine("SetAnyLayerVisible: " + value.ToString());
        }

        public ObservableCollection<CommandBarLayer> CommandBarLayers
        {
            get => (ObservableCollection<CommandBarLayer>)GetValue(CommandBarLayersProperty);
            set
            {
                SetValue(CommandBarLayersProperty, value);
                EvaluateItemsVisibility();
            }
        }

        public static readonly DependencyProperty CommandBarLayersProperty =
            DependencyProperty.Register("CommandBarLayers", typeof(ObservableCollection<CommandBarLayer>), typeof(CommandBar), new PropertyMetadata(new ObservableCollection<CommandBarLayer>()));

        public CommandBar()
        {
            Loaded += (sneder, args) => EvaluateItemsVisibility();

            CommandBarLayers.CollectionChanged += (sneder, args) =>
            {
                if (args.OldItems != null)
                {
                    foreach (CommandBarLayer c in args.OldItems)
                    {
                        c.IdentifierChanged -= Layer_InfoChanged;
                        c.IsVisibleChanged -= Layer_InfoChanged;
                    }
                }

                if (args.NewItems != null)
                {
                    foreach (CommandBarLayer c in args.NewItems)
                    {
                        c.IdentifierChanged += Layer_InfoChanged;
                        c.IsVisibleChanged += Layer_InfoChanged;
                    }
                }
            };
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            EvaluateItemsVisibility();
        }

        private void Layer_InfoChanged(object sender, LayerChangedEventArgs e)
        {
            EvaluateItemsVisibility();
        }

        void EvaluateItemsVisibility()
        {
            foreach (DependencyObject item in Items)
            {
                string layerString = GetLayers(item);
                //Debug.WriteLine("layerString: " + layerString);
                if (string.IsNullOrWhiteSpace(layerString))
                    SetAnyLayerVisible(item, Visibility.Visible);
                else if (CommandBarLayers.Count == 0)
                {
                    //Debug.WriteLine("(CommandBarLayers.Count == 0)");
                    SetAnyLayerVisible(item, Visibility.Collapsed);
                }
                else
                {
                    //Debug.WriteLine("Evaluating...");
                    //string[] layers = layerString.Split(',');
                    bool isAnyLayerVisible = false;
                    foreach (CommandBarLayer l in CommandBarLayers)
                    {
                        if (l.IsVisible && layerString.Contains(l.ToString()))
                        {
                            isAnyLayerVisible = true;
                            break;
                        }
                    }

                    if (isAnyLayerVisible)
                        SetAnyLayerVisible(item, Visibility.Visible);
                    else
                        SetAnyLayerVisible(item, Visibility.Collapsed);
                }
            }
        }
    }

    public class CommandBarLayer : DependencyObject
    {
        public string Identifier
        {
            get => (string)GetValue(IdentifierProperty);
            set
            {
                if (value.Contains(','))
                    throw new Exception("CommandBarLayer identifiers cannot contain commas.");
                else
                {
                    string old = (string)GetValue(IdentifierProperty);
                    SetValue(IdentifierProperty, value);
                    IdentifierChanged?.Invoke(this, new LayerChangedEventArgs(old, value));
                }
            }
        }

        public static readonly DependencyProperty IdentifierProperty =
            DependencyProperty.Register("Identifier", typeof(string), typeof(CommandBarLayer), new PropertyMetadata(string.Empty));

        public override string ToString()
        {
            return Identifier;
        }

        public bool IsVisible
        {
            get => (bool)GetValue(IsVisibleProperty);
            set
            {
                bool old = (bool)GetValue(IsVisibleProperty);
                SetValue(IsVisibleProperty, value);
                IsVisibleChanged?.Invoke(this, new LayerChangedEventArgs(old, value));
            }
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(CommandBarLayer), new PropertyMetadata(true));

        public event EventHandler<LayerChangedEventArgs> IdentifierChanged;
        public event EventHandler<LayerChangedEventArgs> IsVisibleChanged;
    }

    public class LayerChangedEventArgs : EventArgs
    {
        public object OldValue { get; set; }
        public object NewValue { get; set; }

        public LayerChangedEventArgs(object oldV, object newV)
        {
            OldValue = oldV;
            NewValue = newV;
        }
    }
}
