using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SporeMods.CommonUI;

using Cmd = SporeMods.Core.Cmd;

namespace SporeMods.CommonUI
{
    [TemplatePart(Name = PartGrip, Type = typeof(Button))]
    [TemplatePart(Name = PartOffsetter, Type = typeof(Canvas))]
    [TemplatePart(Name = PartStateText, Type = typeof(TextBlock))]
    [TemplatePart(Name = PartGripContainer, Type = typeof(Panel))]

    public partial class ToggleSwitch : CheckBox
    {
        const string PartGrip = "PART_Grip";
        const string PartOffsetter = "PART_Offsetter";
        const string PartStateText = "PART_StateText";
        const string PartGripContainer = "PART_GripContainer";

        public string TrueText
        {
            get => (string)GetValue(TrueTextProperty);
            set => SetValue(TrueTextProperty, value);
        }

        public static readonly DependencyProperty TrueTextProperty =
            DependencyProperty.RegisterAttached("TrueText", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("True"));

        public string FalseText
        {
            get => (string)GetValue(FalseTextProperty);
            set => SetValue(FalseTextProperty, value);
        }

        public static readonly DependencyProperty FalseTextProperty =
            DependencyProperty.RegisterAttached("FalseText", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("False"));

        public string NullText
        {
            get => (string)GetValue(NullTextProperty);
            set => SetValue(NullTextProperty, value);
        }

        public static readonly DependencyProperty NullTextProperty =
            DependencyProperty.RegisterAttached("NullText", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("Null"));

        public double OffsetterWidth
        {
            get => (double)GetValue(OffsetterWidthProperty);
            set => SetValue(OffsetterWidthProperty, value);
        }

        public static readonly DependencyProperty OffsetterWidthProperty =
            DependencyProperty.RegisterAttached("OffsetterWidth", typeof(double), typeof(ToggleSwitch), new PropertyMetadata((double)0));

        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
            IsCheckedProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));
        }

        public ToggleSwitch()
        {
            Loaded += ToggleSwitch_Loaded;
            SizeChanged += ToggleSwitch_SizeChanged;
        }

        private void ToggleSwitch_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OffsetterWidth = GetOffsetterWidth();
        }

        /*protected override Size MeasureOverride(Size constraint)
        {
            constraint.Width = GetOffsetterWidth();
            //base.MeasureOverride(constraint);
            return constraint;
        }*/

        private double GetOffsetterWidth()
        {
            double widthValue = 0;

            double gripContainerWidth = _gripContainer.ActualWidth;
            if (_gripContainer.Width > gripContainerWidth)
                gripContainerWidth = _gripContainer.Width;

            double gripWidth = _grip.ActualWidth;
            if (_grip.Width > gripWidth)
                gripWidth = _grip.Width;

            if ((IsChecked == null) & IsThreeState)
                widthValue = (gripContainerWidth / 2) - (gripWidth / 2);
            else if (IsChecked == false)
                widthValue = 0;
            else
                widthValue = gripContainerWidth - gripWidth;

            Cmd.WriteLine("ToggleSwitch OffsetterWidth: " + widthValue);
            return widthValue;
        }

        private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            OnIsCheckedChanged(this, new DependencyPropertyChangedEventArgs());
        }

        /*protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
            HalfWidth = Width / 2;
        }*/

        static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggle = (d as ToggleSwitch);

            toggle.ChangeChecked();
        }

        protected void ChangeChecked()
        {
            try
            {
                AnimateGripPosition();

                Binding textBinding = new Binding()
                {
                    Source = this,
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                if (IsChecked == true)
                {
                    //_stateText.Text = TrueText;
                    textBinding.Path = new PropertyPath("TrueText");
                }
                else if (IsChecked == false)
                {
                    //_stateText.Text = FalseText;
                    textBinding.Path = new PropertyPath("FalseText");
                }
                else
                {
                    //_stateText.Text = NullText;
                    textBinding.Path = new PropertyPath("NullText");
                }

                BindingOperations.SetBinding(_stateText, TextBlock.TextProperty, textBinding);
            }
            catch (Exception ex)
            {
                Cmd.WriteLine("TOGGLESWITCH ONISCHECKEDCHANGED FAILED: " + ex);
            }
        }

        public void AnimateGripPosition()
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromMilliseconds(125),
                EasingFunction = new QuinticEase()
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            double targetWidth = GetOffsetterWidth();

            animation.To = targetWidth;

            animation.Completed += delegate
            {
                OffsetterWidth = targetWidth;
                BeginAnimation(ToggleSwitch.OffsetterWidthProperty, null);
            };

            BeginAnimation(ToggleSwitch.OffsetterWidthProperty, animation);
        }

        Button _grip = new Button();
        Canvas _offsetter = new Canvas();
        TextBlock _stateText = new TextBlock();
        Panel _gripContainer = new StackPanel();

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _grip = GetTemplateChild(PartGrip) as Button;
            _grip.PreviewMouseLeftButtonDown += (sendurr, args) => ToggleSwitch_PreviewMouseLeftButtonDown(this, args);
            _offsetter = GetTemplateChild(PartOffsetter) as Canvas;
            _stateText = GetTemplateChild(PartStateText) as TextBlock;
            _gripContainer = GetTemplateChild(PartGripContainer) as Panel;
            OnIsCheckedChanged(this, new DependencyPropertyChangedEventArgs());
        }

        private void ToggleSwitch_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool? originalValue = (sender as ToggleSwitch).IsChecked;
            //var toggleSwitch = (sender as ToggleSwitch);

            bool isDragging = false;
            double offsetter = OffsetterWidth;
            //var grip = toggleSwitch._grip;

            double toggleX = SystemScaling.RealPixelsToWpfUnits((sender as ToggleSwitch).PointToScreen(new System.Windows.Point(0, 0)).X);
            double gripInitialX = SystemScaling.RealPixelsToWpfUnits((sender as ToggleSwitch)._grip.PointToScreen(new System.Windows.Point(0, 0)).X);
            double gripX = SystemScaling.RealPixelsToWpfUnits((sender as ToggleSwitch)._grip.PointToScreen(new System.Windows.Point(0, 0)).X);

            double cursorStartX = SystemScaling.RealPixelsToWpfUnits(System.Windows.Forms.Cursor.Position.X);
            double cursorCurrentX = SystemScaling.RealPixelsToWpfUnits(System.Windows.Forms.Cursor.Position.X);
            double cursorChange = (cursorCurrentX - cursorStartX);
            double offset = (gripX - toggleX) + (cursorCurrentX - cursorStartX);
            //System.Windows.Point cursorStartOffsetPoint = new System.Windows.Point(toggleSwitch.Margin.Left, grip.Margin.Top);

            var timer = new System.Timers.Timer(1);

            timer.Elapsed += delegate
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        //toggleX = DpiManager.ConvertPixelsToWpfUnits((sender as ToggleSwitch).PointToScreen(new System.Windows.Point(0, 0)).X);
                        cursorCurrentX = SystemScaling.RealPixelsToWpfUnits(System.Windows.Forms.Cursor.Position.X);

                        cursorChange = (cursorCurrentX - cursorStartX);

                        offset = cursorChange + (gripX - toggleX);
                        ////Cmd.WriteLine(cursorChange.ToString() + "," + offset.ToString());

                        if ((cursorChange > 2) | (cursorChange < -2))
                        {
                            isDragging = true;
                        }

                        var newWidth = offsetter + cursorChange;
                        if (_gripContainer.ActualWidth - _grip.ActualWidth >= newWidth)
                            OffsetterWidth = newWidth;
                    }
                    else
                    {
                        timer.Stop();
                        if (isDragging)
                        {
                            double regionWidth = _gripContainer.ActualWidth - _grip.ActualWidth;

                            double isCheckedOffset = 0;

                            if (IsThreeState)
                            {
                                if (IsChecked == true)
                                    isCheckedOffset = regionWidth;
                                else if (IsChecked == null)
                                    isCheckedOffset = (regionWidth / 2);

                                double toggleChange = cursorChange + isCheckedOffset;

                                if (toggleChange < (regionWidth / 3))
                                    IsChecked = false;
                                else if (toggleChange > (regionWidth * (2.0 / 3.0)))
                                    IsChecked = true;
                                else
                                    IsChecked = null;
                            }
                            else
                            {
                                if (IsChecked == true)
                                    isCheckedOffset = regionWidth;

                                double toggleChange = cursorChange + isCheckedOffset;

                                if (toggleChange < (regionWidth / 2))
                                    IsChecked = false;
                                else
                                    IsChecked = true;
                            }
                        }
                        else
                        {
                            base.OnClick();
                        }
                        if (originalValue == IsChecked)
                        {
                            AnimateGripPosition();
                        }
                    }
                }));
            };
            timer.Start();
        }
    }
}
