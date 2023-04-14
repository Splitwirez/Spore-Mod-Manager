using SporeMods.Core;
using SporeMods.Core.Mods;
using SporeMods.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using System.Windows.Media.Imaging;

namespace SporeMods.CommonUI
{
	public class BindToDynamicResourceAsKeyBehavior : Behavior<FrameworkElement>
	{
		public static readonly DependencyProperty TargetPropertyProperty = DependencyProperty.Register
		(
			nameof(TargetProperty)
			, typeof(DependencyProperty)
			, typeof(BindTextToDynamicResourceAsKeyBehavior)
			, new PropertyMetadata(new PropertyChangedCallback(
				(o, e) =>
				{
					if (o is BindToDynamicResourceAsKeyBehavior bl)
						bl.Refresh();
				}
			))
		);

        public DependencyProperty TargetProperty
        {
            get => (DependencyProperty)GetValue(TargetPropertyProperty);
            set => SetValue(TargetPropertyProperty, value);
        }
		

		public static readonly DependencyProperty ResourceKeyProperty = DependencyProperty.Register(
			nameof(ResourceKey)
			, typeof(string)
			, typeof(BindToDynamicResourceAsKeyBehavior)
			, new PropertyMetadata(new PropertyChangedCallback(
				(o, e) =>
				{
					if ((o is BindToDynamicResourceAsKeyBehavior bl) && (e.NewValue is string key))
						bl.Refresh(key);
				}
			))
		);

        public string ResourceKey
        {
            get => (string)GetValue(ResourceKeyProperty);
            set => SetValue(ResourceKeyProperty, value);
        }


        protected virtual void Refresh()
			=> Refresh(ResourceKey);
		protected virtual void Refresh(string key)
        {
			var assoc = AssociatedObject;
			if (assoc == null)
				return;
			
			var prop = TargetProperty;
			if (prop == null)
				throw new InvalidOperationException("nani the h*ck");
            
			//if ((AssociatedObject != null) && (key != null))
			if (key != null)
				assoc.SetResourceReference(prop, key);
			else
			{
				BindingOperations.ClearBinding(assoc, prop);
				assoc.SetValue(prop, key);
			}
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            Refresh();
        }
    }
}