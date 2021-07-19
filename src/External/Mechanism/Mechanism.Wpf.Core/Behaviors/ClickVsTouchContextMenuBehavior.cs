using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;
using System.Windows.Media;

namespace Mechanism.Wpf.Core.Behaviors
{
    public class ClickVsTouchContextMenuBehavior : Behavior<ContextMenu>
    {
        UIElement _target = null;
        FrameworkElement _initialOwner = null;
        Timer _expire = new Timer(2000);
        bool _expireStarted = false;

    protected override void OnAttached()
        {
            base.OnAttached();

            _expire.Elapsed += (sneder, args) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Debug.WriteLine("EXPIRE ELAPSED");
                    SetExpiration(false);
                    if (_notYetOpened)
                        AttachedProperties.SetLastClickWasTouch(AssociatedObject, false);
                }));
            };

            AssociatedObject.Initialized += (sneder, args) =>
            {
                Touch.FrameReported += Touch_FrameReported;
            };

            DependencyPropertyDescriptor.FromProperty(ContextMenu.PlacementTargetProperty, typeof(ContextMenu)).AddValueChanged(AssociatedObject, AssociatedObject_PlacementTargetChanged);

            if (AssociatedObject.IsOpen)
                _notYetOpened = false;

            AssociatedObject.Opened += AssociatedObject_Opened;

            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            Debug.WriteLine("Touch_FrameReported");
            if (_notYetOpened)
            {
                AttachedProperties.SetLastClickWasTouch(AssociatedObject, true);
                SetExpiration(true);
            }
        }

        void SetExpiration(bool start)
        {
            if (_expireStarted != start)
            {
                if (start)
                    _expire.Start();
                else
                    _expire.Stop();
            }

            _expireStarted = start;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Original source type: " + e.OriginalSource.GetType().FullName + "\n" + e.RoutedEvent.OwnerType.FullName);
            UpdatePlacementTargetEventHandlers();

            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        bool _notYetOpened = true;

        private void AssociatedObject_Opened(object sender, RoutedEventArgs e)
        {
            if (!_notYetOpened)
                UpdateLastClickWasTouch();

            _notYetOpened = false;
        }

        void UpdateLastClickWasTouch()
        {
            if (_target != null)
                AttachedProperties.SetLastClickWasTouch(AssociatedObject, AttachedProperties.GetLastClickWasTouch(_target));
        }

        void AssociatedObject_PlacementTargetChanged(object sender, EventArgs e)
        {
            UpdatePlacementTargetEventHandlers();
        }

        void UpdatePlacementTargetEventHandlers()
        {
            UpdatePlacementTargetEventHandlers(ContextMenuService.GetPlacementTarget(AssociatedObject));
        }

        void UpdatePlacementTargetEventHandlers(UIElement newPlacementTarget)
        {
            if ((newPlacementTarget != null) && (_target != newPlacementTarget))
            {
                Debug.WriteLine("UpdatePlacementTargetEventHandlers 1");
                _target = newPlacementTarget;

                var behaviors = Interaction.GetBehaviors(_target);
                bool hasBehavior = false;
                for (int i = 0; i < behaviors.Count; i++)
                {
                    if (behaviors[i] is ClickVsTouchBehavior)
                    {
                        hasBehavior = true;
                        break;
                    }
                }

                if (!hasBehavior)
                {
                    Debug.WriteLine("UpdatePlacementTargetEventHandlers 2");
                    Interaction.GetBehaviors(_target).Add(new ClickVsTouchBehavior());
                }
            }
        }
    }
}
