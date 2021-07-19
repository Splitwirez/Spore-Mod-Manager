using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Mechanism.Wpf.Core
{
    public class SystemState : DependencyObject
    {
        internal static bool FakeCompositionOff
        {
            get => System.IO.File.Exists(Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\fakedwmoff.txt"));
        }

        public bool IsCompositionEnabled
        {
            get => (bool)GetValue(IsCompositionEnabledProperty);
            set => SetValue(IsCompositionEnabledProperty, value);
        }

        public static readonly DependencyProperty IsCompositionEnabledProperty =
            DependencyProperty.Register("IsCompositionEnabled", typeof(bool), typeof(SystemState), new UIPropertyMetadata(false));

        /*public static bool IsCompositionEnabled
        {
            get
            {
                if (SystemState.FakeCompositionOff)
                    return false;
                else
                    return NativeMethods.DwmIsCompositionEnabled();
            }
            set { }
        }*/

        public Color SystemGlassColor
        {
            get => (Color)GetValue(SystemGlassColorProperty);
            set => SetValue(SystemGlassColorProperty, value);
        }

        public static readonly DependencyProperty SystemGlassColorProperty =
            DependencyProperty.Register("SystemGlassColor", typeof(Color), typeof(SystemState), new UIPropertyMetadata(Colors.White));

        public static SystemState Instance = new SystemState();

        private SystemState()
        {
            if (SystemState.FakeCompositionOff)
                IsCompositionEnabled = false;
            else
                IsCompositionEnabled = NativeMethods.DwmIsCompositionEnabled();

            SystemGlassColor = GetGlassColor();
        }

        Color GetGlassColor()
        {
            if (IsCompositionEnabled)
            {
                ///https://stackoverflow.com/questions/13660976/get-the-active-color-of-windows-8-automatic-color-theme
                //WinApi.DwmGetColorizationColor(out coloures, out opaque);
                NativeMethods.DwmColorizationParams parameters = new NativeMethods.DwmColorizationParams();
                NativeMethods.DwmGetColorizationParameters(ref parameters);
                var coloures = parameters.ColorizationColor.ToString("X");
                while (coloures.Length < 8)
                {
                    coloures = "0" + coloures;
                }
                //Debug.WriteLine("coloures " + parameters.ColorizationColor.ToString("X"));
                var alphaBase = Int32.Parse(coloures.Substring(0, 2), NumberStyles.HexNumber);
                var alphaMultiplier = ((Double)(parameters.ColorizationColorBalance + parameters.ColorizationBlurBalance)) / 128;
                var alpha = (Byte)(alphaBase * alphaMultiplier);
                Debug.WriteLine("balance over 255: " + (((Double)(parameters.ColorizationColorBalance)) / 255) + "\nalpha: " + alpha);
                return Color.FromArgb(alpha, byte.Parse(coloures.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(coloures.Substring(4, 2), NumberStyles.HexNumber), byte.Parse(coloures.Substring(6, 2), NumberStyles.HexNumber));
            }
            else if (Environment.OSVersion.Version.Major <= 5)
            {
                return Color.FromArgb(0xFF, 0, 0x53, 0xE1);
            }
            else if (Environment.OSVersion.Version.Major == 6)
            {
                if (Environment.OSVersion.Version.Minor == 4)
                {
                    return Color.FromArgb(0xFF, 0x65, 0xC0, 0xF2);
                }
                if (Environment.OSVersion.Version.Minor == 3)
                {
                    return Color.FromArgb(0xFF, 0xF0, 0xC8, 0x69);
                }
                else if (Environment.OSVersion.Version.Minor == 2)
                {
                    return Color.FromArgb(0xFF, 0x6B, 0xAD, 0xF6);
                }
                else
                {
                    return Color.FromArgb(0xFF, 0xB9, 0xD1, 0xEA);
                }
            }
            else
            {
                return Color.FromArgb(0xFF, 0x18, 0x83, 0xD7);
            }
        }
    }
}
