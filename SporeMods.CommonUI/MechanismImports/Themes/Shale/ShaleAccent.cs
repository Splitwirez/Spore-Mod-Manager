using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using DrawColor = System.Drawing.Color;
using System.Diagnostics;
using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;

namespace SporeMods.CommonUI.Themes.Shale
{
    public static class ShaleAccents
    {
        const string SHALE_ACCENT_PREFIX = "Settings!Appearance!Themes!Shale!Accents!";
        internal static readonly Dictionary<string, ShaleAccent> STANDARD_ACCENTS = new Dictionary<string, ShaleAccent>()
        {
            {
                nameof(Sky),
                new ShaleAccent()
                {
                    Hue = 183,
                    Saturation = 48,
                    DisplayNameKey = $"{SHALE_ACCENT_PREFIX}{nameof(Sky)}"
                }
            },
            {
                nameof(Ocean),
                new ShaleAccent()
                {
                    Hue = 231,
                    Saturation = 59,
                    DisplayNameKey = $"{SHALE_ACCENT_PREFIX}{nameof(Ocean)}"
                }
            },
            {
                nameof(Sand),
                new ShaleAccent()
                {
                    Hue = 33,
                    Saturation = 13,
                    DisplayNameKey = $"{SHALE_ACCENT_PREFIX}{nameof(Sand)}"
                }
            },
            {
                nameof(Coral),
                new ShaleAccent()
                {
                    Hue = 6,
                    Saturation = 100,
                    DisplayNameKey = $"{SHALE_ACCENT_PREFIX}{nameof(Coral)}"
                }
            },
            {
                nameof(Palm),
                new ShaleAccent()
                {
                    Hue = 109,
                    Saturation = 27,
                    DisplayNameKey = $"{SHALE_ACCENT_PREFIX}{nameof(Palm)}"
                }
            },
            {
                nameof(Coast),
                new ShaleAccent()
                {
                    Hue = 45,
                    Saturation = 98,
                    DisplayNameKey = $"{SHALE_ACCENT_PREFIX}{nameof(Coast)}"
                }
            },
        };

        

        
        static List<ShaleAccent> CreateShaleAccentsList()
            => STANDARD_ACCENTS.Values.ToList();
        /*{
            var accents = new List<ShaleAccent>();
            STANDARD_ACCENTS.Values.CopyTo(accents, 0);
            return accents;
        }*/
        
        static List<ShaleAccent> _standardAccents = CreateShaleAccentsList();
        public static List<ShaleAccent> StandardAccents
        {
            get => _standardAccents;
        }


        /// <summary>
        /// #FF8DD3D6
        /// </summary>
        public static ShaleAccent Sky { get; } = STANDARD_ACCENTS[nameof(Sky)];

        /// <summary>
        /// #1E2B73
        /// </summary>
        public static ShaleAccent Ocean { get; } = STANDARD_ACCENTS[nameof(Ocean)];

        /// <summary>
        /// #FFBCB3A7
        /// </summary>
        public static ShaleAccent Sand { get; } = STANDARD_ACCENTS[nameof(Sand)];

        /// <summary>
        /// #FF8577
        /// </summary>
        public static ShaleAccent Coral { get; } = STANDARD_ACCENTS[nameof(Coral)];

        /// <summary>
        /// #538049
        /// </summary>
        public static ShaleAccent Palm { get; } = STANDARD_ACCENTS[nameof(Palm)];

        /// <summary>
        /// #FDC82C
        /// </summary>
        public static ShaleAccent Coast { get; } = STANDARD_ACCENTS[nameof(Coast)]; //Sunlight
    }
    
    
    
    //[TypeConverter(typeof(ShaleAccentConverter))]
    public class ShaleAccent : ResourceDictionary//, ISupportInitialize
    {
        static readonly double MIN_HUE = byte.MinValue;
        static readonly double MAX_HUE = byte.MaxValue;
        
        double _hue = 0;
        public double Hue
        {
            get => _hue;
            set
            {
                _hue = value;
                RefreshColors();
            }
        }


        const double MIN_SATURATION = 64;
        const double MAX_SATURATION = byte.MaxValue;
        
        double _saturation = MIN_SATURATION;
        public double Saturation
        {
            get => _saturation;
            set
            {
                _saturation = Math.Clamp(value, MIN_SATURATION, MAX_SATURATION);
                RefreshColors();
            }
        }
        
        string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
            }
        }

        string _displayNameKey = string.Empty;
        public string DisplayNameKey
        {
            get => _displayNameKey;
            set
            {
                _displayNameKey = value;
                
                if (!_displayNameKey.IsNullOrEmptyOrWhiteSpace())
                    DisplayName = LanguageManager.Instance.GetLocalizedText(_displayNameKey);
            }
        }

        public Uri Source
        {
            get => base.Source;
            set { }
        }

        /*public Collection<ResourceDictionary> MergedDictionaries
        {
            get => base.MergedDictionaries;
            set { }
        }*/


        Brush _lightSwatchBrush = null;
        public Brush LightSwatchBrush
        {
            get => _lightSwatchBrush;
            private set => _lightSwatchBrush = value;
        }

        Brush _darkSwatchBrush = null;
        public Brush DarkSwatchBrush
        {
            get => _darkSwatchBrush;
            private set => _darkSwatchBrush = value;
        }

        /*public ShaleAccent(Color color, string displayName)
            : this(displayName)
        {
            /*DrawColor color = DrawColor.FromArgb(1, accentColor.R, accentColor.G, accentColor.B);
            Hue = color.GetHue();
            Saturation = color.GetHue();* /
            color.ToHsv(out double hue, out double saturation, out double _);
            Hue = hue;
            Saturation = saturation;
        }



        public ShaleAccent(Color color)
            : this(color, string.Empty)
        { }

        public ShaleAccent(double hue, double saturation = 128)
            : this(hue, saturation, string.Empty)
        { }

        public ShaleAccent(double hue, double saturation, string displayName)
            : this(hue, displayName)
        {
            Saturation = saturation;
        }

        public ShaleAccent(double hue, string displayName)
            : this(displayName)
        {
            Hue = hue;
        }

        private ShaleAccent(string displayName)
            : this()
        {
            DisplayName = displayName;
        }*/
        
        public ShaleAccent()
            : base()
        {
            //RefreshColors();
        }

        public static ShaleAccent FromColor(Color color)
        {
            color.ToHsv(out double hue, out double saturation, out double _);
            return new ShaleAccent()
            {
                Hue = hue,
                Saturation = saturation
            };
        }

        public static ShaleAccent FromRgb(byte red, byte green, byte blue)
        {
            ColorHelper.RgbToHsv(red, green, blue, out double hue, out double saturation, out double _);
            return new ShaleAccent()
            {
                Hue = hue,
                Saturation = saturation
            };
        }

        ResourceDictionary _prevColors = null;
        void RefreshColors()
        {
            if (true) //_initComplete && _refreshed)
            {
                if ((_prevColors != null) && (MergedDictionaries.Contains(_prevColors)))
                {
                    MergedDictionaries.Remove(_prevColors);
                }
                
                
                ResourceDictionary colors = CreateColors();
                MergedDictionaries.Add(colors);
                
                _prevColors = colors;
                //Console.WriteLine($"Colors added!\n\tHue: {Hue}\n\tSaturation: {Saturation}");
            }
        }

        /*void ISupportInitialize.BeginInit()
        {
            base.BeginInit();
        }
        
        void ISupportInitialize.EndInit()
        {
            base.EndInit();
            _initComplete = true;
            //RefreshColors();
        }*/
        bool _initComplete = true;

        bool _refreshed = false;
        protected override void OnGettingValue(object key, ref object value, out bool canCache)
        {
            if (_initComplete && (!_refreshed))
            {
                _refreshed = true;
                RefreshColors();
            }

            base.OnGettingValue(key, ref value, out canCache);
        }


        
        static readonly ResourceDictionary SHALE_ACCENT_BASE = new ResourceDictionary()
        {
            Source = new Uri("/SporeMods.CommonUI;component/MechanismImports/Themes/Shale/Colors/Accent.xaml", UriKind.RelativeOrAbsolute)
        };

        private ResourceDictionary CreateColors()
        {
            LightSwatchBrush = new SolidColorBrush(ColorHelper.ColorFromHsv(_hue, _saturation, 75));
            DarkSwatchBrush = new SolidColorBrush(ColorHelper.ColorFromHsv(_hue, _saturation, 25));
            MergedDictionaries.Clear();

            ResourceDictionary colors = new ResourceDictionary();
            foreach (string s in SHALE_ACCENT_BASE.Keys)
            {
                if (SHALE_ACCENT_BASE[s] is Color color)
                {
                    color.ToHsv(out double hue, out double sat, out double val);
                    sat = _saturation;
                    hue = _hue;
                    if (s.Contains("DarkColor"))
                        sat /= 3;

                    colors.Add(s, ColorHelper.ColorFromHsv(hue, sat, val));
                }
            }
            return colors;
        }


        public static ShaleAccent Parse(string input)
        {
            string inputStr = input;
            if (TryParse(inputStr, out ShaleAccent shaleAccent))
                return shaleAccent;
            else
                throw new Exception($"Cannot parse input '{input}' as a {nameof(ShaleAccent)}.");
        }

        public static bool TryParse(string input, out ShaleAccent shaleAccent)
        {
            string inputStr = input;
            if (ShaleAccents.STANDARD_ACCENTS.ContainsKey(inputStr))
                shaleAccent = ShaleAccents.STANDARD_ACCENTS[inputStr];
            else if (TryParseFromColorOrHue(inputStr, out shaleAccent))
                shaleAccent.DisplayName = inputStr;
            else if (TryGetSegments(inputStr, out string[] segments))
            {
                if (segments.Length <= 1)
                    shaleAccent = null;
                else
                {
                    bool parsedFromHAndS = false;
                    if (
                            double.TryParse(segments[1], out double saturation) &&
                            double.TryParse(segments[0], out double hue)
                        )
                    {
                        shaleAccent = new ShaleAccent()
                        {
                            Hue = hue,
                            Saturation = saturation
                        };
                        parsedFromHAndS = shaleAccent != null;
                    }


                    if (segments.Length == 2)
                    {
                        if (parsedFromHAndS)
                        {
                            shaleAccent.DisplayName = inputStr;
                        }
                        else if (TryParseFromColorOrHue(segments[0], out shaleAccent))
                        {
                            shaleAccent.DisplayName = segments[1];
                        }
                    }
                    else if ((segments.Length == 3) && parsedFromHAndS)
                        shaleAccent.DisplayName = segments[2];
                }
            }

            return shaleAccent != null;
        }

        static bool TryParseFromColorOrHue(string input, out ShaleAccent shaleAccent)
        {
            if (double.TryParse(input, out double hue))
                shaleAccent = new ShaleAccent()
                {
                    Hue = hue
                };
            else if (ColorHelper.TryParseColor(input, out Color parsedColor))
                shaleAccent = ShaleAccent.FromColor(parsedColor);
            else
                shaleAccent = null;
            
            return shaleAccent != null;
        }

        static bool TryGetSegments(string input, out string[] segments)
        {
            if (input.Contains(','))
                segments = input.Split(',');
            else if (input.Contains(' '))
                segments = input.Split(' ');
            else
                segments = null;
            
            return segments != null;
        }
    }


    [TypeConverter(typeof(GetShaleAccentConverter))]
    [MarkupExtensionReturnType(typeof(ShaleAccent))]
    public class GetShaleAccent : MarkupExtension
    {
        string _accentName = string.Empty;
        public string AccentName
        {
            get => _accentName;
            set => _accentName = value;
        }


        public override object ProvideValue(IServiceProvider serviceProvider)
            => ShaleAccents.STANDARD_ACCENTS[AccentName.ToString()];

        public GetShaleAccent()
        { }
    }

    public class GetShaleAccentConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
            => sourceType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
        {
            return new GetShaleAccent()
            {
                AccentName = source.ToString()
            };
        }
    }

    /*public class ShaleAccentConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
            => sourceType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
            => ShaleAccents.STANDARD_ACCENTS[source.ToString()];
    }

    internal class ShaleAccentParams
    {
        double _hue;
        double _saturation;
        string _displayNameKey;
        
        public ShaleAccentParams(double hue, double saturation, string displayNameKey)
        {
            _hue = hue;
            _saturation = saturation;
            _displayNameKey = displayNameKey;
        }

        public ShaleAccent ToShaleAccent()
        {
            return new ShaleAccent()
            {
                Hue = _hue,
                Saturation = _saturation,
                DisplayNameKey = _displayNameKey
            };
        }
    }*/


    /*public class ShaleAccentConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            return
                (sourceType == typeof(Color)) ||
                (sourceType == typeof(double)) ||
                (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
        {
            if (source is Color color)
            {
                ShaleAccent shaleAccent = ShaleAccent.FromColor(color);
                shaleAccent.DisplayName = color.ToString();
                return shaleAccent;
            }
            else if (source is double hue)
                return new ShaleAccent()
                {
                    Hue = hue,
                    DisplayName = hue.ToString()
                };
            else
                return ShaleAccent.Parse(source.ToString());
        }
    }*/
}
