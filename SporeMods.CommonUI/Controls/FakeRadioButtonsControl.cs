using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Data;
using SporeMods.Core;
using Avalonia.LogicalTree;
using System.Collections.Specialized;

namespace SporeMods.CommonUI
{
    public class FakeRadioButtonsControl : ListBox
    {
        public static readonly StyledProperty<object> SelectedValueProperty =
            AvaloniaProperty.Register<FakeRadioButtonsControl, object>(nameof(SelectedValue), defaultBindingMode: BindingMode.TwoWay);

        public object SelectedValue
        {
            get => GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        static FakeRadioButtonsControl()
        {
            SelectedValueProperty.Changed.AddClassHandler<FakeRadioButtonsControl>((sender, e) => sender.UpdateSelectedValue(e.NewValue));
        }


        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);
            UpdateSelectedValue(SelectedValue);
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsCollectionChanged(sender, e);
            UpdateSelectedValue(SelectedValue);
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            UpdateSelectedValue(SelectedValue);
        }


        internal bool _selectedValueUpdatedByItem = false;
        protected virtual void UpdateSelectedValue(object newValue)
        {
            if (!_selectedValueUpdatedByItem)
            {
                var items = Items.OfType<FakeRadioButton>().ToList();
                if (items.Count > 0)
                {
                    
                    FakeRadioButton newSelected = null;
                    if (newValue == null)
                    {
                        if (items.Any(x => x.Value == null))
                            newSelected = items.First(x => x.Value == null);
                    }
                    else
                    {
                        string newValStr = newValue is string newStr ? newStr : newValue.ToString();

                        if (items.Any(x => newValStr.Equals(x.ToString(), StringComparison.OrdinalIgnoreCase)))
                        {
                            /*object itemValue = x.Value;
                            string itemValStr = itemValue != null ? itemValue.ToString() : string.Empty;
                            return newValStr.Equals(itemValStr, StringComparison.OrdinalIgnoreCase);*/
                            newSelected = items.FirstOrDefault(x => newValStr.Equals(x.ToString(), StringComparison.OrdinalIgnoreCase));
                        }
                        //else if (items.Any(x => (x.Value != null ? x.Value.ToString() : string.Empty).Equals()
                    }
                    //if (newSelected == null)
                    if (SelectedItem != newSelected)
                        SelectedItem = newSelected;
                }
                else
                    Console.WriteLine("NO ITEMS???");
            }
        }


        /// <inheritdoc/>
        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new ItemContainerGenerator<FakeRadioButton>(
                this, 
                FakeRadioButton.ContentProperty,
                FakeRadioButton.ContentTemplateProperty);
        }
    }

    public class FakeRadioButton : ListBoxItem, IStyleable
    {
        public static readonly StyledProperty<object> ValueProperty =
            AvaloniaProperty.Register<FakeRadioButtonsControl, object>(nameof(Value));

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        

        public static readonly DirectProperty<FakeRadioButton, bool?> IsCheckedProperty =
            ToggleButton.IsCheckedProperty.AddOwner<FakeRadioButton>(o => o.IsChecked, (o, v) => o.IsChecked = v);

        bool? _isChecked = false;
        public bool? IsChecked
        {

            get => _isChecked;
            set 
            { 
                SetAndRaise(IsCheckedProperty, ref _isChecked, value);
                UpdatePseudoClasses(IsChecked);
            }
        }
        private void UpdatePseudoClasses(bool? isChecked)
        {
            PseudoClasses.Set(":checked", isChecked == true);
            PseudoClasses.Set(":unchecked", isChecked == false);
            PseudoClasses.Set(":indeterminate", isChecked == null);
        }
        
        static FakeRadioButton()
        {
            IsSelectedProperty.Changed.AddClassHandler<FakeRadioButton>((o, e) =>
            {
                o.IsChecked = ((bool)e.NewValue);
                o.UpdateParentValue();
            });
            ValueProperty.Changed.AddClassHandler<FakeRadioButton>((o, e) => o.UpdateParentValue());
        }


        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            UpdateParentValue();
        }

        protected virtual void UpdateParentValue()
        {
            if (IsSelected)
            {
                string output = string.Empty;
                if (IsSet(ValueProperty))
                {
                    if (Parent is FakeRadioButtonsControl parentButtons)
                    {
                        parentButtons._selectedValueUpdatedByItem = true;
                        parentButtons.SelectedValue = Value;
                        parentButtons._selectedValueUpdatedByItem = false;
                    }
                    else
                        output += "NO APPLICABLE PARENT   ";
                }
                else
                    output += "VALUE NOT SET   ";
                
                if (!output.IsNullOrEmptyOrWhiteSpace())
                    Console.WriteLine(output.Trim());
            }
        }

        Type IStyleable.StyleKey => typeof(RadioButton);


        public override string ToString()
        {
            object value = Value;
            return value != null ? (value is string str ? str : value.ToString()) : string.Empty;
        }
    }
}