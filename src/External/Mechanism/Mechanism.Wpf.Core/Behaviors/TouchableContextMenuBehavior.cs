using Mechanism.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Mechanism.Wpf.Core.Behaviors
{
    public class TouchableContextMenuBehavior : Behavior<ContextMenu>
    {
        ContextMenu _targetMenu;

        bool _wasOpenedWithTouch = false;

        /*static void OnOpenedWithTouchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetMenuOpenedWithTouch((d as TouchableContextMenuBehavior)._targetMenu, (bool)(e.NewValue));
            Debug.WriteLine("OnOpenedWithTouchChanged " + e.NewValue.ToString());
        }*/

        /*public static readonly DependencyProperty MenuOpenedWithTouchProperty = DependencyProperty.RegisterAttached("MenuOpenedWithTouch", typeof(bool), typeof(ContextMenu), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static bool GetMenuOpenedWithTouch(ContextMenu element)
        {
            var retrievedValue = (bool)(element.GetValue(MenuOpenedWithTouchProperty));
            Debug.WriteLine("GetMenuOpenedWithTouch " + retrievedValue.ToString());
            return retrievedValue;
        }

        public static void SetMenuOpenedWithTouch(ContextMenu element, bool value)
        {
            element.SetValue(MenuOpenedWithTouchProperty, value);
            Debug.WriteLine("SetMenuOpenedWithTouch " + value.ToString());
        }*/

        /*, OnAttachedTouchableBehaviorChanged*/

        /*static void OnAttachedTouchableBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //SetAttachedTouchableBehavior((d as TouchableContextMenuBehavior)._targetMenu, (bool)e.NewValue);
            Interaction.GetBehaviors((d as TouchableContextMenuBehavior)._targetMenu).Add(new TouchableContextMenuBehavior());
        }*/

        /*public static readonly DependencyProperty AttachedTouchableBehaviorProperty =
            DependencyProperty.RegisterAttached("AttachedTouchableBehavior", typeof(bool), typeof(ContextMenu), new PropertyMetadata(false));
        //new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure, OnAttachedTouchableBehaviorChanged)

        public static void SetAttachedTouchableBehavior(DependencyObject element, bool value)
        {
            element.SetValue(AttachedTouchableBehaviorProperty, value);
        }

        public static bool GetAttachedTouchableBehavior(DependencyObject element)
        {
            return (bool)element.GetValue(AttachedTouchableBehaviorProperty);
        }

        private static void OnAttachedTouchableBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //////Interaction.GetBehaviors((d as TouchableContextMenuBehavior)._targetMenu).Add(new TouchableContextMenuBehavior());
        }*/

        public TouchableContextMenuBehavior()
        {

        }

        protected override void OnAttached()
        {
            _targetMenu = AssociatedObject;

            base.OnAttached();

            _targetMenu.Opened += (sneder, args) =>
            {
                //Debug.WriteLine("OpenedWithTouch: " + AttachedProperties.GetOpenedWithTouch(_targetMenu).ToString());
                //Debug.WriteLine(args.Source.GetType().FullName);
                AttachedProperties.SetLastClickWasTouch(_targetMenu, _wasOpenedWithTouch);
            };
            if (_targetMenu.IsLoaded)
                Load();
            else
            {
                _targetMenu.Loaded += TargetMenu_Loaded;
                //_targetMenu.Initialized += TargetMenu_Initialized;
                _targetMenu.ApplyTemplate();
                //_targetMenu.UpdateLayout();
            }
        }

        private void TargetMenu_Initialized(object sender, EventArgs e)
        {
            bool open = _targetMenu.IsOpen;
            if (!open)
            {
                _targetMenu.IsOpen = true;
                _targetMenu.IsOpen = false;
            }

            _targetMenu.Initialized -= TargetMenu_Initialized;
        }

        private void TargetMenu_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
            _targetMenu.Loaded -= TargetMenu_Loaded;
        }

        void Load()
        {
            Debug.WriteLine("Load()");
            var source = ContextMenuService.GetPlacementTarget(_targetMenu);
            var placeTarget = _targetMenu.PlacementTarget as UIElement;
            if (source != null)
            {
                Debug.WriteLine("source is valid");
                (source as UIElement).TouchDown += Source_TouchDown;
                (source as UIElement).MouseDown += Source_MouseDown;
            }
            else if (placeTarget != null)
            {
                Debug.WriteLine("source is null, placeTarget is valid");
                placeTarget.TouchDown += Source_TouchDown;
                placeTarget.MouseDown += Source_MouseDown;
            }
            else
                Debug.WriteLine("source is null, placeTarget is null");
        }

        private void Source_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Source_MouseDown(object sender, MouseButtonEventArgs e)");
            if (
                (!((e.OriginalSource as UIElement).AreAnyTouchesOver))
                && (!_targetMenu.IsOpen)
                )
                /*_was*/
                _wasOpenedWithTouch = false;
        }

        private void Source_TouchDown(Object sender, TouchEventArgs e)
        {
            Debug.WriteLine("Source_TouchDown(object sender, TouchEventArgs e)");
            //TouchStarted = DateTime.Now;
            Timer touchTimer = new Timer(1);
            touchTimer.Elapsed += delegate
            {
                _targetMenu.Dispatcher.Invoke(new Action(() =>
                {
                    if (_targetMenu.IsOpen)
                    {
                        _wasOpenedWithTouch = true;
                        AttachedProperties.SetLastClickWasTouch(_targetMenu, _wasOpenedWithTouch);
                        //else if (!((e.OriginalSource as UIElement).AreAnyTouchesOver))
                        touchTimer.Stop();
                    }

                    //if (_wasOpenedWithTouch)
                }));
            };
            touchTimer.Start();
        }
    }
}