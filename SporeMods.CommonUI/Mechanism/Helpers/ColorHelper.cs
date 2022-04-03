using SporeMods.Core;
using SporeMods.CommonUI.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SporeMods.CommonUI
{
    public static class ColorHelper
    {
        //https://www.programmingalgorithms.com/algorithm/rgb-to-hsl/
        //https://www.programmingalgorithms.com/algorithm/hsl-to-rgb/
        public static Color ToRGB(double hslHue, double hslSaturation, double hslLightness, byte alpha = 0xFF)
        {
            //Console.WriteLine($"In: {hslHue}, {hslSaturation}, {hslLightness}");
            byte r = 0;
            byte g = 0;
            byte b = 0;

            float hslH = (float)hslHue;
            float hslS = (float)hslSaturation;
            float hslL = (float)hslLightness;

            if (hslS == 0)
            {
                r = g = b = (byte)(hslL * 255);
            }
            else
            {
                float v1, v2;
                float hue = (float)hslH / 360;

                v2 = (hslL < 0.5) ? (hslL * (1 + hslS)) : ((hslL + hslS) - (hslL * hslS));
                v1 = 2 * hslL - v2;

                r = (byte)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                g = (byte)(255 * HueToRGB(v1, v2, hue));
                b = (byte)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
            }

            return Color.FromArgb(alpha, r, g, b);
        }

        private static float HueToRGB(float v1, float v2, float vH)
        {
            if (vH < 0)
                vH += 1;

            if (vH > 1)
                vH -= 1;

            if ((6 * vH) < 1)
                return (v1 + (v2 - v1) * 6 * vH);

            if ((2 * vH) < 1)
                return v2;

            if ((3 * vH) < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

            return v1;
        }



        public static void FromRGB(Color rgb, out float hslH, out float hslS, out float hslL)
        {
            hslH = 0;
            hslS = 0;
            hslL = 0;

            float r = (rgb.R / 255.0f);
            float g = (rgb.G / 255.0f);
            float b = (rgb.B / 255.0f);

            float min = Math.Min(Math.Min(r, g), b);
            float max = Math.Max(Math.Max(r, g), b);
            float delta = max - min;

            hslL = (max + min) / 2;

            if (delta == 0)
            {
                hslH = 0;
                hslS = 0.0f;
            }
            else
            {
                hslS = (hslL <= 0.5) ? (delta / (max + min)) : (delta / (2 - max - min));

                float hue;

                if (r == max)
                {
                    hue = ((g - b) / 6) / delta;
                }
                else if (g == max)
                {
                    hue = (1.0f / 3) + ((b - r) / 6) / delta;
                }
                else
                {
                    hue = (2.0f / 3) + ((r - g) / 6) / delta;
                }

                if (hue < 0)
                    hue += 1;
                if (hue > 1)
                    hue -= 1;

                hslH = (int)(hue * 360);
            }
        }
    }
}
