using SporeMods.Core;
using SporeMods.Core.Mods;
using SporeMods.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using System.Windows.Media.Imaging;

namespace SporeMods.Manager
{
	public class BindTextToDynamicResourceAsKeyBehavior : Behavior<TextBlock>
	{
        public static readonly DependencyProperty ResourceKeyProperty = DependencyProperty.Register(nameof(ResourceKey), typeof(string), typeof(BindTextToDynamicResourceAsKeyBehavior), new PropertyMetadata(new PropertyChangedCallback((o, e) =>
        {
            if ((o is BindTextToDynamicResourceAsKeyBehavior bl) && (e.NewValue is string key))
                bl.RefreshText(key);
        })));

        public string ResourceKey
        {
            get => (string)GetValue(ResourceKeyProperty);
            set => SetValue(ResourceKeyProperty, value);
        }

        void RefreshText(string key)
        {
            if ((AssociatedObject != null) && (key != null))
                AssociatedObject.SetResourceReference(TextBlock.TextProperty, key);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            RefreshText(ResourceKey);
        }
    }
}
