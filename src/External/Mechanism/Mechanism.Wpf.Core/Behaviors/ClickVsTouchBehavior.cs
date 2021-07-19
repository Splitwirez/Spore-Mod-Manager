using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

namespace Mechanism.Wpf.Core.Behaviors
{
    public class ClickVsTouchBehavior : Behavior<FrameworkElement>
    {

        public UIElement PlacementTarget
        {
            get
            {
                if (AssociatedObject is ContextMenu menu)
                    return ContextMenuService.GetPlacementTarget(menu);
                else if (AssociatedObject is Popup popup)
                    return popup.PlacementTarget;
                else
                    return null;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            DependencyPropertyDescriptor.FromProperty(FrameworkElement.ContextMenuProperty, typeof(FrameworkElement)).AddValueChanged(AssociatedObject, UpdateContextMenu);
            
            UpdateEventHandlers();
            if (AssociatedObject.ContextMenu != null)
                AttachedProperties.SetLastClickWasTouch(AssociatedObject, AttachedProperties.GetLastClickWasTouch(AssociatedObject.ContextMenu));
        }

        void UpdateContextMenu(object sender, EventArgs e)
        {
            UpdateEventHandlers();
        }

        void UpdateEventHandlers()
        {
            if (AssociatedObject.ContextMenu != null)
            {
                AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
                //AssociatedObject.MouseMove += (sneder, args) => Debug.WriteLine("Mouse moved on Owner");
            }
            else
                AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
        }

        private void AssociatedObject_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is UIElement srCommonel)
            {
                AttachedProperties.SetLastClickWasTouch(AssociatedObject, srCommonel.AreAnyTouchesOver);

                Debug.WriteLine("AssociatedObject_PreviewMouseDown, " + AttachedProperties.GetLastClickWasTouch(AssociatedObject).ToString());
            }
            else
                Debug.WriteLine("AssociatedObject_PreviewMouseDown, null");
        }
    }
}
