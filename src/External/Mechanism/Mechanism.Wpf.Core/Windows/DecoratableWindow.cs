using Mechanism.Wpf.Core;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;

namespace Mechanism.Wpf.Core.Windows
{
    [TemplatePart(Name = PartTitlebar, Type = typeof(Thumb))]
    [TemplatePart(Name = PartFullscreenButton, Type = typeof(Button))]
    [TemplatePart(Name = PartMinimizeButton, Type = typeof(Button))]
    [TemplatePart(Name = PartMaximizeButton, Type = typeof(Button))]
    [TemplatePart(Name = PartRestoreButton, Type = typeof(Button))]
    [TemplatePart(Name = PartCloseButton, Type = typeof(Button))]
    [TemplatePart(Name = PartThumbBottom, Type = typeof(Thumb))]
    [TemplatePart(Name = PartThumbTop, Type = typeof(Thumb))]
    [TemplatePart(Name = PartThumbBottomRightCorner, Type = typeof(Thumb))]
    [TemplatePart(Name = PartThumbTopRightCorner, Type = typeof(Thumb))]
    [TemplatePart(Name = PartThumbTopLeftCorner, Type = typeof(Thumb))]
    [TemplatePart(Name = PartThumbBottomLeftCorner, Type = typeof(Thumb))]
    [TemplatePart(Name = PartThumbRight, Type = typeof(Thumb))]
    [TemplatePart(Name = PartThumbLeft, Type = typeof(Thumb))]

    [TemplatePart(Name = PartSystemMenuRestore, Type = typeof(MenuItem))]
    [TemplatePart(Name = PartSystemMenuMove, Type = typeof(MenuItem))]
    [TemplatePart(Name = PartSystemMenuSize, Type = typeof(MenuItem))]
    [TemplatePart(Name = PartSystemMenuMinimize, Type = typeof(MenuItem))]
    [TemplatePart(Name = PartSystemMenuMaximize, Type = typeof(MenuItem))]
    [TemplatePart(Name = PartSystemMenuClose, Type = typeof(MenuItem))]

    [ContentProperty("Content")]
    public partial class DecoratableWindow : ShadowedWindow
    {
        internal enum DragState
        {
            NotDragging,
            IsDragging,
            DragCancelled
        }

        const String PartTitlebar = "PART_Titlebar";
        const String PartFullscreenButton = "PART_FullscreenButton";
        const String PartMinimizeButton = "PART_MinimizeButton";
        const String PartMaximizeButton = "PART_MaximizeButton";
        const String PartRestoreButton = "PART_RestoreButton";
        const String PartCloseButton = "PART_CloseButton";
        const String PartThumbBottom = "PART_ThumbBottom";
        const String PartThumbTop = "PART_ThumbTop";
        const String PartThumbBottomRightCorner = "PART_ThumbBottomRightCorner";
        const String PartResizeGrip = "PART_ResizeGrip";
        const String PartThumbTopRightCorner = "PART_ThumbTopRightCorner";
        const String PartThumbTopLeftCorner = "PART_ThumbTopLeftCorner";
        const String PartThumbBottomLeftCorner = "PART_ThumbBottomLeftCorner";
        const String PartThumbRight = "PART_ThumbRight";
        const String PartThumbLeft = "PART_ThumbLeft";

        const String PartSystemMenuRestore = "PART_SystemMenuRestore";
        const String PartSystemMenuMove = "PART_SystemMenuMove";
        const String PartSystemMenuSize = "PART_SystemMenuSize";
        const String PartSystemMenuMinimize = "PART_SystemMenuMinimize";
        const String PartSystemMenuMaximize = "PART_SystemMenuMaximize";
        const String PartSystemMenuClose = "PART_SystemMenuClose";

        Button _fullscreenButton;
        Button _minButton;
        Button _maxButton;
        Button _restButton;
        Button _closeButton;

        Thumb _thumbBottom;
        Thumb _thumbBottomLeftCorner;
        Thumb _thumbBottomRightCorner;
        Thumb _resizeGrip;
        Thumb _thumbLeft;
        Thumb _thumbRight;
        Thumb _thumbTop;
        Thumb _thumbTopLeftCorner;
        Thumb _thumbTopRightCorner;

        Thumb/*FrameworkElement*/ _titlebar;

        MenuItem _systemMenuRestore;
        MenuItem _systemMenuMove;
        MenuItem _systemMenuSize;
        MenuItem _systemMenuMinimize;
        MenuItem _systemMenuMaximize;
        MenuItem _systemMenuClose;

        new public WindowStyle WindowStyle
        {
            get => (WindowStyle)GetValue(WindowStyleProperty);
            set => SetValue(WindowStyleProperty, value);
        }

        new public static readonly DependencyProperty WindowStyleProperty =
            DependencyProperty.Register("WindowStyle", typeof(WindowStyle), typeof(DecoratableWindow), new PropertyMetadata(WindowStyle.SingleBorderWindow));

        /*public double ShadowOpacity
        {
            get => (double)GetValue(ShadowOpacityProperty);
            set => SetValue(ShadowOpacityProperty, value);
        }

        public static readonly DependencyProperty ShadowOpacityProperty =
            DependencyProperty.Register("ShadowOpacity", typeof(double), typeof(DecoratableWindow), new PropertyMetadata((double)1));

        public Visibility ShadowVisibility
        {
            get => (Visibility)GetValue(ShadowVisibilityProperty);
            set => SetValue(ShadowVisibilityProperty, value);
        }

        public static readonly DependencyProperty ShadowVisibilityProperty =
            DependencyProperty.Register("ShadowVisibility", typeof(Visibility), typeof(DecoratableWindow), new PropertyMetadata(Visibility.Visible));*/

        public bool ShowFullscreenButton
        {
            get => (bool)GetValue(ShowFullscreenButtonProperty);
            set => SetValue(ShowFullscreenButtonProperty, value);
        }

        public static readonly DependencyProperty ShowFullscreenButtonProperty =
            DependencyProperty.Register("ShowFullscreenButton", typeof(bool), typeof(DecoratableWindow), new PropertyMetadata(false));

        public bool IsFullscreen
        {
            get => (bool)GetValue(IsFullscreenProperty);
            set => SetValue(IsFullscreenProperty, value);
        }

        public static readonly DependencyProperty IsFullscreenProperty =
            DependencyProperty.Register("IsFullscreen", typeof(bool), typeof(DecoratableWindow), new PropertyMetadata(false, OnIsFullscreenPropertyChangedCallback));

        public bool IsFullscreenTitlebarHidden
        {
            get => (bool)GetValue(IsFullscreenTitlebarHiddenProperty);
            set => SetValue(IsFullscreenTitlebarHiddenProperty, value);
        }

        public static readonly DependencyProperty IsFullscreenTitlebarHiddenProperty =
            DependencyProperty.Register("IsFullscreenTitlebarHidden", typeof(bool), typeof(DecoratableWindow), new PropertyMetadata(true));

        static void OnIsFullscreenPropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
                (sender as DecoratableWindow).WindowState = WindowState.Normal;
        }

        public bool AutohideTitlebarWhenFullscreen
        {
            get => (bool)GetValue(AutohideTitlebarWhenFullscreenProperty);
            set => SetValue(AutohideTitlebarWhenFullscreenProperty, value);
        }

        public static readonly DependencyProperty AutohideTitlebarWhenFullscreenProperty =
            DependencyProperty.Register("AutohideTitlebarWhenFullscreen", typeof(bool), typeof(DecoratableWindow), new PropertyMetadata(true));

        public bool ShowTitlebarText
        {
            get => (bool)GetValue(ShowTitlebarTextProperty);
            set => SetValue(ShowTitlebarTextProperty, value);
        }

        public static readonly DependencyProperty ShowTitlebarTextProperty =
            DependencyProperty.Register("ShowTitlebarText", typeof(bool), typeof(DecoratableWindow), new PropertyMetadata(true));

        public bool ShowTitlebarIcon
        {
            get => (bool)GetValue(ShowTitlebarIconProperty);
            set => SetValue(ShowTitlebarIconProperty, value);
        }

        public static readonly DependencyProperty ShowTitlebarIconProperty =
            DependencyProperty.Register("ShowTitlebarIcon", typeof(bool), typeof(DecoratableWindow), new PropertyMetadata(true));

        /*public object TitleBarContent
        {
            get => GetValue(TitleBarContentProperty);
            set => SetValue(TitleBarContentProperty, value);
        }

        public static readonly DependencyProperty TitleBarContentProperty =
                    DependencyProperty.RegisterAttached("TitleBarContent", typeof(object), typeof(DecoratableWindow),
                        new PropertyMetadata(null));

        public object FullWindowContent
        {
            get => GetValue(FullWindowContentProperty);
            set => SetValue(FullWindowContentProperty, value);
        }

        public static readonly DependencyProperty FullWindowContentProperty =
                    DependencyProperty.RegisterAttached("FullWindowContent", typeof(object), typeof(DecoratableWindow),
                        new PropertyMetadata(null));*/

        public double TitlebarHeight
        {
            get => (double)GetValue(TitlebarHeightProperty);
            set => SetValue(TitlebarHeightProperty, value);
        }

        public static readonly DependencyProperty TitlebarHeightProperty =
            DependencyProperty.Register("TitlebarHeight", typeof(double), typeof(DecoratableWindow), new PropertyMetadata(30.0));

        public Style DragOverlayStyle
        {
            get => (Style)GetValue(DragOverlayStyleProperty);
            set => SetValue(DragOverlayStyleProperty, value);
        }

        public static readonly DependencyProperty DragOverlayStyleProperty =
            DependencyProperty.Register(nameof(DragOverlayStyle), typeof(Style), typeof(DecoratableWindow), new PropertyMetadata());

        /*static DecoratableWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DecoratableWindow), new FrameworkPropertyMetadata("{x:Type apictrl:DecoratableWindow}"));
        }*/

        /*static DecoratableWindow()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(DecoratableWindow), new FrameworkPropertyMetadata(typeof(DecoratableWindow)));
        }*/

        /*static DecoratableWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DecoratableWindow), new FrameworkPropertyMetadata(typeof(DecoratableWindow)));
        }*/

        ShadowedWindow _dragOverlayWindow = null;

        public DecoratableWindow() : base()
        {
            _dragOverlayWindow = new ShadowedWindow()
            {
                Visibility = Visibility.Hidden,
                Topmost = true
            };
            _dragOverlayWindow.SourceInitialized += (sneder, args) =>
            {
                NativeMethods.SetWindowLong(_dragOverlayWindow.Handle, NativeMethods.GwlExstyle, (Int32)NativeMethods.GetWindowLong(_dragOverlayWindow.Handle, NativeMethods.GwlExstyle) | NativeMethods.WsExToolwindow | NativeMethods.WsExTransparent);
            };

            Binding dragOverlayStyleBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("DragOverlayStyle"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_dragOverlayWindow, Window.StyleProperty, dragOverlayStyleBinding);

            Binding dragOverlayMinWidthBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("MinWidth"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_dragOverlayWindow, Window.MinWidthProperty, dragOverlayMinWidthBinding);

            Binding dragOverlayMinHeightBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("MinHeight"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_dragOverlayWindow, Window.MinHeightProperty, dragOverlayMinHeightBinding);

            Binding dragOverlayMaxWidthBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("MaxWidth"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_dragOverlayWindow, Window.MaxWidthProperty, dragOverlayMaxWidthBinding);

            Binding dragOverlayMaxHeightBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("MaxHeight"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_dragOverlayWindow, Window.MaxHeightProperty, dragOverlayMaxHeightBinding);

            /*base.WindowStyle = WindowStyle.None;
            base.AllowsTransparency = true;*/

            //StateChanged += DecoratableWindow_StateChanged;

            ////DefaultStyleKey = typeof(DecoratableWindow);
            //Style = (Style)Resources[typeof(DecoratableWindow)];

            ////SetResourceReference(StyleProperty, typeof(DecoratableWindow));

            /*bool windowResourceIsNull = TryFindResource(GetType()) == null;
            bool appResourceIsNull = TryFindResource(GetType()) == null;

            bool defaultStyleFallback = false;

            if (Application.Current == null)
                defaultStyleFallback = windowResourceIsNull;
            else
                defaultStyleFallback = windowResourceIsNull && appResourceIsNull;

            if ((Style == null) && defaultStyleFallback)
            {
                Debug.WriteLine("DecoratableWindow type: " + GetType().FullName);
                SetResourceReference(StyleProperty, typeof(DecoratableWindow));
            }*/

            /*Loaded += (sneder, args) =>
            {
                SyncShadowToWindowSize();
            };*/
        }

        protected override void OverrideStyleMetadata()
        {
            if (!OverriddenTypes.Contains(GetType()))
            {
                //System.Windows.MessageBox.Show("OverrideStyleMetadata() yes" + GetType().FullName);
                DefaultStyleKeyProperty.OverrideMetadata(GetType(), new FrameworkPropertyMetadata(typeof(DecoratableWindow)));
                OverriddenTypes.Add(GetType());
            }
            /*else
                System.Windows.MessageBox.Show("OverrideStyleMetadata() no" + GetType().FullName);*/
            base.OverrideStyleMetadata();
        }

        /*double _maxWidth = double.PositiveInfinity;
        double _maxHeight = double.PositiveInfinity;*/
        /*private void DecoratableWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                _maxWidth = MaxWidth;
                _maxHeight = MaxHeight;
                System.Windows.Forms.Screen s = System.Windows.Forms.Screen.FromHandle(_handle); //System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)SystemScaling.WpfUnitsToRealPixels(Left), (int)SystemScaling.WpfUnitsToRealPixels(Top)));
                MaxWidth = s.WorkingArea.Width;
                MaxHeight = s.WorkingArea.Height;
            }
            else
            {
                MaxWidth = _maxWidth;
                MaxHeight = _maxHeight;
            }
            
        }*/

        double _maxWidth = double.PositiveInfinity;
        double _maxHeight = double.PositiveInfinity;

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Maximized)
            {
                _maxWidth = MaxWidth;
                _maxHeight = MaxHeight;
                //System.Windows.Forms.Screen s = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)SystemScaling.WpfUnitsToRealPixels(Left + (Width / 2)), (int)SystemScaling.WpfUnitsToRealPixels(Top + (Height / 2)))); //System.Windows.Forms.Screen.FromHandle(Handle); //System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)SystemScaling.WpfUnitsToRealPixels(Left), (int)SystemScaling.WpfUnitsToRealPixels(Top)));
                NativeMethods.MonitorInfoEx s = NativeMethods.MonitorFromHandle(Handle);
                NativeMethods.Rect monBounds = s.rcMonitor;
                NativeMethods.Rect work = NativeMethods.MonitorFromHandle(Handle).rcWork;
                NativeMethods.GetWindowRect(Handle, out NativeMethods.RECT winRect);


                double verticalWidth = SystemScaling.RealPixelsToWpfUnits(monBounds.Left - winRect.Left);
                double horizontalHeight = SystemScaling.RealPixelsToWpfUnits(monBounds.Top - winRect.Top);

                if (!IsFullscreen)
                {
                    MaxWidth = SystemScaling.RealPixelsToWpfUnits(work.Right - work.Left);
                    MaxHeight = SystemScaling.RealPixelsToWpfUnits(work.Bottom - work.Top);
                    verticalWidth = SystemScaling.RealPixelsToWpfUnits(work.Left - winRect.Left);
                    horizontalHeight = SystemScaling.RealPixelsToWpfUnits(work.Top - winRect.Top);
                }
                else
                {
                    MaxWidth = SystemScaling.RealPixelsToWpfUnits(monBounds.Right - monBounds.Left);
                    MaxHeight = SystemScaling.RealPixelsToWpfUnits(monBounds.Bottom - monBounds.Top);
                }

                Margin = new Thickness(verticalWidth, horizontalHeight, (verticalWidth) * -1, (horizontalHeight) * -1);
            }
            else
            {
                IsFullscreen = false;
                MaxWidth = _maxWidth;
                MaxHeight = _maxHeight;
                Margin = new Thickness(0);
            }

            /*if (_fullscreenButton != null)
            {
                if (IsFullscreen)
                    _fullscreenButton.Visibility = Visibility.Collapsed;
                else
                    _fullscreenButton.Visibility = Visibility.Visible;
            }*/

            ValidateSystemMenuItemStates();
        }

        /*protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
            if (this.SizeToContent != SizeToContent.Manual)
            {
                SyncShadowToWindowSize();
            }
        }*/

        Cursor _prevCursor = null;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _titlebar = GetTemplateChild(PartTitlebar) as Thumb;
            if (_titlebar != null)
            {
                _titlebar.PreviewMouseLeftButtonDown += Titlebar_MouseLeftButtonDown;
                _titlebar.MouseLeave += (sneder, args) => ResetTitlebarCursor();
            }

            _fullscreenButton = GetTemplateChild(PartFullscreenButton) as Button;
            if (_fullscreenButton != null)
                _fullscreenButton.Click += (sneder, args) =>
                {
                    if (!IsFullscreen)
                    {
                        IsFullscreen = true;
                        WindowState = WindowState.Maximized;
                    }
                };

            _minButton = GetTemplateChild(PartMinimizeButton) as Button;
            if (_minButton != null)
                _minButton.Click += (sneder, args) => WindowState = WindowState.Minimized;


            _maxButton = GetTemplateChild(PartMaximizeButton) as Button;
            if (_maxButton != null)
                _maxButton.Click += (sneder, args) => WindowState = WindowState.Maximized;


            _restButton = GetTemplateChild(PartRestoreButton) as Button;
            if (_restButton != null)
                _restButton.Click += (sneder, args) => WindowState = WindowState.Normal;

            _closeButton = GetTemplateChild(PartCloseButton) as Button;
            if (_closeButton != null)
                _closeButton.Click += (sneder, args) => Close();



            _thumbLeft = GetTemplateChild(PartThumbLeft) as Thumb;
            if (_thumbLeft != null)
            {
                _thumbLeft.DragStarted += ThumbAny_DragStarted;
                _thumbLeft.DragCompleted += ThumbAny_DragCompleted;
                _thumbLeft.DragDelta += ThumbLeft_DragDelta;
            }

            _thumbTop = GetTemplateChild(PartThumbTop) as Thumb;
            if (_thumbTop != null)
            {
                _thumbTop.DragStarted += ThumbAny_DragStarted;
                _thumbTop.DragCompleted += ThumbAny_DragCompleted;
                _thumbTop.DragDelta += ThumbTop_DragDelta;
            }

            _thumbRight = GetTemplateChild(PartThumbRight) as Thumb;
            if (_thumbRight != null)
            {
                _thumbRight.DragStarted += ThumbAny_DragStarted;
                _thumbRight.DragCompleted += ThumbAny_DragCompleted;
                _thumbRight.DragDelta += ThumbRight_DragDelta;
            }

            _thumbBottom = GetTemplateChild(PartThumbBottom) as Thumb;
            if (_thumbBottom != null)
            {
                _thumbBottom.DragStarted += ThumbAny_DragStarted;
                _thumbBottom.DragCompleted += ThumbAny_DragCompleted;
                _thumbBottom.DragDelta += ThumbBottom_DragDelta;
            }

            _thumbTopLeftCorner = GetTemplateChild(PartThumbTopLeftCorner) as Thumb;
            if (_thumbTopLeftCorner != null)
            {
                _thumbTopLeftCorner.DragStarted += ThumbAny_DragStarted;
                _thumbTopLeftCorner.DragCompleted += ThumbAny_DragCompleted;
                _thumbTopLeftCorner.DragDelta += ThumbTopLeftCorner_DragDelta;
            }

            _thumbTopRightCorner = GetTemplateChild(PartThumbTopRightCorner) as Thumb;
            if (_thumbTopRightCorner != null)
            {
                _thumbTopRightCorner.DragStarted += ThumbAny_DragStarted;
                _thumbTopRightCorner.DragCompleted += ThumbAny_DragCompleted;
                _thumbTopRightCorner.DragDelta += ThumbTopRightCorner_DragDelta;
            }

            _thumbBottomRightCorner = GetTemplateChild(PartThumbBottomRightCorner) as Thumb;
            if (_thumbBottomRightCorner != null)
            {
                _thumbBottomRightCorner.DragStarted += ThumbAny_DragStarted;
                _thumbBottomRightCorner.DragCompleted += ThumbAny_DragCompleted;
                _thumbBottomRightCorner.DragDelta += ThumbBottomRightCorner_DragDelta;
            }

            _thumbBottomLeftCorner = GetTemplateChild(PartThumbBottomLeftCorner) as Thumb;
            if (_thumbBottomLeftCorner != null)
            {
                _thumbBottomLeftCorner.DragStarted += ThumbAny_DragStarted;
                _thumbBottomLeftCorner.DragCompleted += ThumbAny_DragCompleted;
                _thumbBottomLeftCorner.DragDelta += ThumbBottomLeftCorner_DragDelta;
            }


            _resizeGrip = GetTemplateChild(PartResizeGrip) as Thumb;
            if (_resizeGrip != null)
            {
                _resizeGrip.DragStarted += ThumbAny_DragStarted;
                _resizeGrip.DragCompleted += ThumbAny_DragCompleted;
                _resizeGrip.DragDelta += ThumbBottomRightCorner_DragDelta;
            }


            _systemMenuRestore = GetTemplateChild(PartSystemMenuRestore) as MenuItem;
            if (_systemMenuRestore != null)
                _systemMenuRestore.Click += (sneder, args) => WindowState = WindowState.Normal;

            _systemMenuMove = GetTemplateChild(PartSystemMenuMove) as MenuItem;
            if (_systemMenuMove != null)
                _systemMenuMove.Click += (sneder, args) =>
                {
                    if (_titlebar != null)
                    {
                        var titlebarPoint = _titlebar.PointToScreen(new Point(0, 0));
                        SystemScaling.CursorPosition = new Point(titlebarPoint.X + (_titlebar.ActualWidth / 2), titlebarPoint.Y + 10);

                        if (_titlebar.Cursor != Cursors.SizeAll)
                        {
                            _prevCursor = _titlebar.Cursor;
                            _titlebar.Cursor = Cursors.SizeAll;
                        }
                    }
                };

            _systemMenuSize = GetTemplateChild(PartSystemMenuSize) as MenuItem;

            _systemMenuMinimize = GetTemplateChild(PartSystemMenuMinimize) as MenuItem;
            if (_systemMenuMinimize != null)
                _systemMenuMinimize.Click += (sneder, args) => WindowState = WindowState.Minimized;

            _systemMenuMaximize = GetTemplateChild(PartSystemMenuMaximize) as MenuItem;
            if (_systemMenuMaximize != null)
                _systemMenuMaximize.Click += (sneder, args) => WindowState = WindowState.Maximized;

            _systemMenuClose = GetTemplateChild(PartSystemMenuClose) as MenuItem;
            if (_systemMenuClose != null)
                _systemMenuClose.Click += (sneder, args) => Close();

            ValidateSystemMenuItemStates();
        }

        void ValidateSystemMenuItemStates()
        {
            if (WindowState == WindowState.Maximized)
            {
                if (_systemMenuRestore != null)
                    _systemMenuRestore.IsEnabled = true;

                if (_systemMenuMaximize != null)
                    _systemMenuMaximize.IsEnabled = false;

                if (_systemMenuMove != null)
                    _systemMenuMove.IsEnabled = false;
            }
            else
            {
                if (_systemMenuRestore != null)
                    _systemMenuRestore.IsEnabled = false;

                if (_systemMenuMaximize != null)
                    _systemMenuMaximize.IsEnabled = true;

                if (_systemMenuMove != null)
                {
                    if (_titlebar != null)
                        _systemMenuMove.IsEnabled = true;
                    else
                        _systemMenuMove.IsEnabled = false;
                }
            }

            if (_systemMenuSize != null)
                _systemMenuSize.IsEnabled = false;
        }

        void ResetTitlebarCursor()
        {
            if ((_titlebar.Cursor == Cursors.SizeAll) && (_prevCursor != Cursors.SizeAll))
                _titlebar.Cursor = _prevCursor;
        }

        void Titlebar_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            ResetTitlebarCursor();
            if (e.ClickCount == 2)
            {
                //Debug.WriteLine("(e.ClickCount == 2)");
                if ((WindowState != WindowState.Maximized) && (ResizeMode != ResizeMode.CanMinimize) && (ResizeMode != ResizeMode.NoResize))
                {
                    /*Debug.WriteLine("(WindowState != WindowState.Maximized)");
                    Debug.WriteLine("(ResizeMode != ResizeMode.CanMinimize): " + (ResizeMode != ResizeMode.CanMinimize).ToString());
                    Debug.WriteLine("(ResizeMode != ResizeMode.NoResize): " + (ResizeMode != ResizeMode.NoResize).ToString());
                    if ()*/
                    WindowState = WindowState.Maximized;
                }
                else
                    WindowState = WindowState.Normal;
            }
            else
            {
                if (WindowState == WindowState.Maximized)
                {
                    var newPos = SystemScaling.CursorPosition; //e.GetPosition(_titlebar);
                    System.Timers.Timer timer = new System.Timers.Timer(1);
                    timer.Elapsed += (sneder, args) =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (Mouse.LeftButton == MouseButtonState.Released)
                                timer.Stop();
                            else if (
                                (SystemScaling.CursorPosition.X > newPos.X + 10)
                                || (SystemScaling.CursorPosition.X < newPos.X - 10)
                                || (SystemScaling.CursorPosition.Y > newPos.Y + 10)
                                || (SystemScaling.CursorPosition.Y < newPos.Y - 10)
                                )
                            {
                                //var offset = e.GetPosition(_titlebar);
                                WindowState = WindowState.Normal;
                                //Left = SystemScaling.CursorPosition.X - offset.X;
                                //Top = SystemScaling.CursorPosition.Y - offset.Y;
                                DragMove();
                                //////SyncShadowToWindow();
                                timer.Stop();
                            }
                        }));
                    };
                    timer.Start();
                }
                else if (Mouse.LeftButton == MouseButtonState.Pressed)
                    DragMove();
            }
        }


        static uint _uFlags = 0x0004 | 0x0010;

        int _oldX = 0;
        int _oldY = 0;
        int _oldCx = 100;
        int _oldCy = 100;

        int _newX = 0;
        int _newY = 0;
        int _newCx = 100;
        int _newCy = 100;

        DragState _dragResizingState = DragState.NotDragging;

        void ThumbAny_DragStarted(object sender, DragStartedEventArgs e)
        {
            _dragResizingState = DragState.IsDragging;
            PreviewKeyDown += DecoratableWindow_PreviewKeyDown;
            _dragOverlayWindow.PreviewKeyDown += DecoratableWindow_PreviewKeyDown;

            _oldX = (int)Left;
            _oldY = (int)Top;
            _oldCx = (int)Width;
            _oldCy = (int)Height;
            
            _newX = (int)Left;
            _newY = (int)Top;
            _newCx = (int)Width;
            _newCy = (int)Height;

            if (!SystemParameters.DragFullWindows)
            {
                //NativeMethods.ShowWindow(_dragOverlayWindow.Handle, 8);
                NativeMethods.SetWindowPos(_dragOverlayWindow.Handle, IntPtr.Zero, 0, 0, 0, 0, /*0x0004 | */0x0002 | 0x0001 | 0x0010);
                _dragOverlayWindow.Visibility = Visibility.Visible;
                //_dragOverlayWindow.Show();
            }
        }

        private void DecoratableWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _dragResizingState = DragState.DragCancelled;

                NativeMethods.SetWindowPos(Handle, IntPtr.Zero, SystemScaling.WpfUnitsToRealPixels(_oldX), SystemScaling.WpfUnitsToRealPixels(_oldY), SystemScaling.WpfUnitsToRealPixels(_oldCx), SystemScaling.WpfUnitsToRealPixels(_oldCy), _uFlags);
            }
        }

        void ScaledSetWindowPos(int X, int Y, int cx, int cy)
        {
            if (_dragResizingState != DragState.IsDragging)
                _dragOverlayWindow.Visibility = Visibility.Hidden;
            else
            {
                if (SystemParameters.DragFullWindows)
                    NativeMethods.SetWindowPos(Handle, IntPtr.Zero, SystemScaling.WpfUnitsToRealPixels(X), SystemScaling.WpfUnitsToRealPixels(Y), SystemScaling.WpfUnitsToRealPixels(cx), SystemScaling.WpfUnitsToRealPixels(cy), _uFlags);
                else
                {
                    _newX = X;
                    _newY = Y;
                    _newCx = cx;
                    _newCy = cy;
                    if (_dragResizingState == DragState.IsDragging)
                        NativeMethods.SetWindowPos(_dragOverlayWindow.Handle, IntPtr.Zero, SystemScaling.WpfUnitsToRealPixels(X), SystemScaling.WpfUnitsToRealPixels(Y), SystemScaling.WpfUnitsToRealPixels(cx), SystemScaling.WpfUnitsToRealPixels(cy), _uFlags); //0x0010
                    //X, Y, cx, cy
                }
            }
        }

        void ThumbAny_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if ((!SystemParameters.DragFullWindows) && (_dragResizingState == DragState.IsDragging))
            {
                NativeMethods.SetWindowPos(Handle, IntPtr.Zero, SystemScaling.WpfUnitsToRealPixels(_newX), SystemScaling.WpfUnitsToRealPixels(_newY), SystemScaling.WpfUnitsToRealPixels(_newCx), SystemScaling.WpfUnitsToRealPixels(_newCy), _uFlags);
                //SystemScaling.WpfUnitsToRealPixels((int)_dragOverlayWindow.Left), SystemScaling.WpfUnitsToRealPixels((int)_dragOverlayWindow.Top), SystemScaling.WpfUnitsToRealPixels((int)_dragOverlayWindow.Width), SystemScaling.WpfUnitsToRealPixels((int)_dragOverlayWindow.Height)
                //
            }
            _dragOverlayWindow.Visibility = Visibility.Hidden;

            _dragResizingState = DragState.NotDragging;
            PreviewKeyDown -= DecoratableWindow_PreviewKeyDown;
            _dragOverlayWindow.PreviewKeyDown -= DecoratableWindow_PreviewKeyDown;
        }


        void ThumbTopLeftCorner_DragDelta(Object sender, DragDeltaEventArgs e)
        {
            ScaledSetWindowPos((int)(Left + e.HorizontalChange), (int)(Top + e.VerticalChange), (int)(Width - e.HorizontalChange), (int)(Height - e.VerticalChange));
        }

        void ThumbTopRightCorner_DragDelta(Object sender, DragDeltaEventArgs e)
        {
            ScaledSetWindowPos((int)Left, (int)(Top + e.VerticalChange), (int)(Width + e.HorizontalChange), (int)(Height - e.VerticalChange));
        }

        void ThumbBottomRightCorner_DragDelta(Object sender, DragDeltaEventArgs e)
        {
            ScaledSetWindowPos((int)Left, (int)Top, (int)(Width + e.HorizontalChange), (int)(Height + e.VerticalChange));
        }

        void ThumbBottomLeftCorner_DragDelta(Object sender, DragDeltaEventArgs e)
        {
            ScaledSetWindowPos((int)(Left + e.HorizontalChange), (int)Top, (int)(Width - e.HorizontalChange), (int)(Height + e.VerticalChange));
        }


        void ThumbLeft_DragDelta(Object sender, DragDeltaEventArgs e)
        {
            ScaledSetWindowPos((int)(Left + e.HorizontalChange), (int)Top, (int)(Width - e.HorizontalChange), (int)Height);
        }

        void ThumbTop_DragDelta(Object sender, DragDeltaEventArgs e)
        {
            ScaledSetWindowPos((int)Left, (int)(Top + e.VerticalChange), (int)Width, (int)(Height - e.VerticalChange));
        }

        void ThumbRight_DragDelta(Object sender, DragDeltaEventArgs e)
        {
            ScaledSetWindowPos((int)Left, (int)Top, (int)(Width + e.HorizontalChange), (int)Height);
        }

        void ThumbBottom_DragDelta(Object sender, DragDeltaEventArgs e)
        {
            ScaledSetWindowPos((int)Left, (int)Top, (int)Width, (int)(Height + e.VerticalChange));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _dragOverlayWindow.Close();
        }
    }


        public class WindowStateIsMaximizedToBoolConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            if ((WindowState)value == WindowState.Maximized)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}