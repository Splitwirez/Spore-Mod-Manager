using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;

namespace Mechanism.Wpf.Core.Windows
{
    public class ShadowWindow : CompositingWindow
    {
        /// <summary>
        /// Gets or sets the OwnerWindow of this <see cref="Mechanism.Wpf.Core.Windows.ShadowWindow"/>.
        /// </summary>
        public ShadowedWindow OwnerWindow
        {
            get => (ShadowedWindow)GetValue(OwnerWindowProperty);
            set => SetValue(OwnerWindowProperty, value);
        }

        public static readonly DependencyProperty OwnerWindowProperty =
            DependencyProperty.Register(nameof(OwnerWindow), typeof(ShadowedWindow), typeof(ShadowWindow), new PropertyMetadata(null, OnPropertiesChangedCallback));


        /// <summary>
        /// Gets or sets the distance between the bounds of this <see cref="Mechanism.Wpf.Core.Windows.ShadowWindow"/> and its OwnerWindow.
        /// </summary>
        public Thickness ShadowOffsetThickness
        {
            get => (Thickness)GetValue(ShadowOffsetThicknessProperty);
            set => SetValue(ShadowOffsetThicknessProperty, value);
        }

        public static readonly DependencyProperty ShadowOffsetThicknessProperty =
            DependencyProperty.Register(nameof(ShadowOffsetThickness), typeof(Thickness), typeof(ShadowWindow), new PropertyMetadata(new Thickness(0), OnPropertiesChangedCallback));

        public static void OnPropertiesChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ShadowWindow).SyncShadowToWindow();
        }

        WindowInteropHelper _helper = null;

        public ShadowWindow(ShadowedWindow owner) : base()
        {
            ShowInTaskbar = false;
            ShowActivated = false;

            OwnerWindow = owner;
            /*Binding ownerWindowBinding = new Binding()
            {
                Source = owner,
                //Path = new PropertyPath("."),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, OwnerWindowProperty, ownerWindowBinding);*/


            Binding shadowStyleBinding = new Binding()
            {
                Source = OwnerWindow,
                Path = new PropertyPath("ShadowStyle"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, Window.StyleProperty, shadowStyleBinding);

            Binding shadowTopmostBinding = new Binding()
            {
                Source = OwnerWindow,
                Path = new PropertyPath("Topmost"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, Window.TopmostProperty, shadowTopmostBinding);

            Binding shadowVisibilityBinding = new Binding()
            {
                Source = OwnerWindow,
                Path = new PropertyPath("IsWindowVisible"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, CompositingWindow.IsWindowVisibleProperty, shadowVisibilityBinding);

            OwnerWindow.Loaded += (sneder, args) =>
            {
                Show();

                System.Windows.Media.CompositionTarget.Rendering += (snedre, rags) =>
                {
                    if (IsVisible)
                        SyncShadowToWindow();
                };

                if (!OwnerWindow.IsVisible)
                    Hide();

                OwnerWindow.IsVisibleChanged += (snedre, rags) =>
                {
                    if (OwnerWindow.IsVisible)
                        Show();
                    else
                        Hide();
                };
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _helper = new WindowInteropHelper(this);
            NativeMethods.SetWindowLong(_helper.Handle, NativeMethods.GwlExstyle, (Int32)NativeMethods.GetWindowLong(_helper.Handle, NativeMethods.GwlExstyle) | NativeMethods.WsExToolwindow | NativeMethods.WsExTransparent);
        }

        public void SyncShadowToWindow()
        {
            if (IsVisible)
            {
                int X = (int)SystemScaling.WpfUnitsToRealPixels(OwnerWindow.Left - ShadowOffsetThickness.Left);
                int Y = (int)SystemScaling.WpfUnitsToRealPixels(OwnerWindow.Top - ShadowOffsetThickness.Top);
                int cx = (int)SystemScaling.WpfUnitsToRealPixels(OwnerWindow.ActualWidth + ShadowOffsetThickness.Left + ShadowOffsetThickness.Right);

                double vScreenLeft = SystemParameters.VirtualScreenLeft;
                double vScreenTop = SystemParameters.VirtualScreenTop;
                double vScreenWidth = SystemParameters.VirtualScreenWidth;
                double vScreenHeight = SystemParameters.VirtualScreenHeight;

                if (cx > vScreenWidth)
                    cx = (int)vScreenWidth;
                int cy = (int)SystemScaling.WpfUnitsToRealPixels(OwnerWindow.ActualHeight + ShadowOffsetThickness.Top + ShadowOffsetThickness.Bottom);
                if (cy > vScreenHeight)
                    cy = (int)vScreenHeight;

                NativeMethods.SetWindowPos(_helper.Handle, OwnerWindow.Handle, X, Y, cx, cy, NativeMethods.SwpNoActivate | 0x4000);
            }
        }
    }
}
