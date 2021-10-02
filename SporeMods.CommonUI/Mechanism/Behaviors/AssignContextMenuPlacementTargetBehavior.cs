using SporeMods.Core;
using SporeMods.Core.Mods;
using SporeMods.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
	public class AssignContextMenuPlacementTargetBehavior : Behavior<FrameworkElement>
	{
        public static readonly DependencyProperty PlacementTargetProperty = DependencyProperty.Register(nameof(PlacementTarget), typeof(FrameworkElement), typeof(AssignContextMenuPlacementTargetBehavior), new PropertyMetadata(new PropertyChangedCallback((o, e) =>
        {
            if ((o is AssignContextMenuPlacementTargetBehavior bh) && (e.NewValue is FrameworkElement target))
                bh.Refresh(target);
        })));

        public FrameworkElement PlacementTarget
        {
            get => (FrameworkElement)GetValue(PlacementTargetProperty);
            set => SetValue(PlacementTargetProperty, value);
        }

        void Refresh(FrameworkElement target)
        {
            if (AssociatedObject == null)
                return;
            else if (AssociatedObject.ContextMenu == null)
                return;
            else
                AssociatedObject.ContextMenu.PlacementTarget = target;
        }


        protected override void OnAttached()
        {
            base.OnAttached();
            Refresh(PlacementTarget);
            DependencyPropertyDescriptor.FromName("ContextMenu", typeof(FrameworkElement), typeof(ContextMenu)).AddValueChanged(AssociatedObject, (s, e) => Refresh(AssociatedObject.ContextMenu));
        }
    }
}
