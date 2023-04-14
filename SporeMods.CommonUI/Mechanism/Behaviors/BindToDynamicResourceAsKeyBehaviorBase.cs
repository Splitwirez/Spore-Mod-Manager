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

/*namespace SporeMods.CommonUI
{
	public abstract class BindToDynamicResourceAsKeyBehaviorBase<T> : Behavior<T>
	{
        public static readonly DependencyProperty ResourceKeyProperty = DependencyProperty.Register(nameof(ResourceKey), typeof(string), typeof(BindToDynamicResourceAsKeyBehaviorBase<T>), new PropertyMetadata(new PropertyChangedCallback((o, e) =>
        {
            if ((o is BindToDynamicResourceAsKeyBehaviorBase<T> bl) && (e.NewValue is string key))
                bl.Refresh(key);
        })));

        public string ResourceKey
        {
            get => (string)GetValue(ResourceKeyProperty);
            set => SetValue(ResourceKeyProperty, value);
        }

        void Refresh(string key)
        {
            if ((AssociatedObject != null) && (key != null))
                AssociatedObject.SetResourceReference(TargetProperty, key);
        }

        protected abstract DependencyProperty TargetProperty
        {
            get;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            Refresh(ResourceKey);
        }
    }
}*/