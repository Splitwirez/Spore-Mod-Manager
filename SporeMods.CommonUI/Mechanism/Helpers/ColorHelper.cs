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
		/*public static Color ColorFromHsl(double inH, double inS, double inL)
        {
			float hslH = (float)inH;
			float hslS = (float)inS;
			float hslL = (float)inL;

            byte r = 0;
            byte g = 0;
            byte b = 0;

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

            Color retColor = Color.FromRgb(r, g, b);
			//Cmd.WriteLine($"retColor: {retColor.R}, {retColor.G}, {retColor.B},, {retColor.ScR}, {retColor.ScG}, {retColor.ScB}");
			return retColor;
        }

        static float HueToRGB(float v1, float v2, float vH)
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


		public static void ToHsl(this Color rgb, out double hslH, out double hslS, out double hslL)
		{
			float r = (rgb.R / 255.0f);
			float g = (rgb.G / 255.0f);
			float b = (rgb.B / 255.0f);
			Cmd.WriteLine($"rgb input: {r}, {g}, {b}");

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
			Cmd.WriteLine($"HSL result: {hslH}, {hslS}, {hslL}");
		}*/

		//https://www.programmingalgorithms.com/algorithm/hsv-to-rgb/
		public static Color ColorFromHsv(double inH, double inS, double inV, byte alpha = 0xFF)
        {
            double h = inH;
            double s = inS / 255;
            double v = inV/* / 255*/;
            double r = 0, g = 0, b = 0;

            if (s == 0)
            {
                r = v;
                g = v;
                b = v;
            }
            else
            {
                int i;
                double f, p, q, t;

                if (h == 360)
                    h = 0;
                else
                    h = h / 60;

                i = (int)Math.Truncate(h);
                f = h - i;

                p = v * (1.0 - s);
                q = v * (1.0 - (s * f));
                t = v * (1.0 - (s * (1.0 - f)));

                switch (i)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;

                    default:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }

            }

            Color finalColor = Color.FromArgb(alpha, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
			return finalColor;
        }

		public static void RgbToHsv(byte red, byte green, byte blue, out double hue, out double saturation, out double value)
		{
			byte R = red;
			byte G = green;
			byte B = blue;

			double delta, min;
			double h = 0, s, v;

			min = Math.Min(Math.Min(R, G), B);
			v = Math.Max(Math.Max(R, G), B);
			delta = v - min;

			if (v == 0.0)
				s = 0;
			else
				s = delta / v;

			if (s == 0)
				h = 0.0;

			else
			{
				if (R == v)
					h = (G - B) / delta;
				else if (G == v)
					h = 2 + (B - R) / delta;
				else if (B == v)
					h = 4 + (R - G) / delta;

				h *= 60;

				if (h < 0.0)
					h = h + 360;
			}

			hue = h;
			saturation = s;
			value = v / 255;
		}

		//https://www.programmingalgorithms.com/algorithm/rgb-to-hsv/
		public static void ToHsv(this Color rgb, out double hue, out double saturation, out double value)
			=> RgbToHsv(rgb.R, rgb.G, rgb.B, out hue, out saturation, out value);

		public static bool TryParseColor(string input, out Color color)
		{
			try
			{
				if (ColorConverter.ConvertFromString(input) is Color outColor)
				{
					color = outColor;
					return true;
				}
			}
			catch
			{

			}
			color = Colors.Black;
			return false;
		}

		/*static double Wrap(double initial, double min, double max)
		{
			/*if (max <= min)
				throw new Exception($"Nah you got your min ({min}) and max ({max}) mixed up or something - min has to be less than max.");* /
			bool aboveMax = initial > max;
			bool belowMin = initial < min;
			
			if (belowMin || aboveMax)
			{
				/*double startingValue = initial - min;
				
				double realMin = 0;
				double realMax = max - min;* /
				double diff = max - min;

				double nowValue = initial;
				/*if (aboveMax)
				{* /
				while (nowValue > max)
				{
					nowValue -= diff;
				}
				//}
				while (nowValue < min)
				{
					nowValue += diff;
				}
			}
		}*/
	}
}
