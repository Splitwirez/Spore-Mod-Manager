using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using SporeMods.Core;



namespace SporeMods.CommonUI
{
    public class DrunkFontManagerImpl : IFontManagerImpl
    {
        SKFontManager _skFontManager = SKFontManager.Default;

        IFontManagerImpl _prevImpl = null;
        bool _hasPrevImpl = false;
        internal DrunkFontManagerImpl(IFontManagerImpl prevImpl = null)
        {
            _prevImpl = prevImpl;
            _hasPrevImpl = _prevImpl != null;
        }

        bool TryCall<T>(Func<T> attempt, out T result)
        {
            if (_hasPrevImpl)
            {
                try
                {
                    result = attempt();
                    return true;
                }
                catch
                {

                }
            }

            result = default(T);
            return false;
        }

        
        string _defaultFontIsntRealItCantHurtYou = "Imagine if this returned Comic Sans";
        public string GetDefaultFontFamilyName()
        {
            if (TryCall<string>(() => _prevImpl.GetDefaultFontFamilyName(), out string defaultFont))
            {
                if (!defaultFont.IsNullOrEmptyOrWhiteSpace())
                    return defaultFont;
            }
            
            return _defaultFontIsntRealItCantHurtYou;
        }

        
        string[] _whatIsAnInstalledFontLmao = { };
        public IEnumerable<string> GetInstalledFontFamilyNames(bool checkForUpdates = false)
        {
            return _whatIsAnInstalledFontLmao;
        }

        [ThreadStatic] private static string[] t_languageTagBuffer;

        public bool TryMatchCharacter(int codepoint, FontStyle fontStyle,
            FontWeight fontWeight,
            FontFamily fontFamily, CultureInfo culture, out Typeface fontKey)
        {
            if (TryCall<Typeface?>((() =>
            {
                if (_prevImpl.TryMatchCharacter(codepoint, fontStyle, fontWeight, fontFamily, culture, out Typeface prevTypeface))
                    return prevTypeface;
                else
                    return null;
            }), out Typeface? result))
            {
                if ((result != null) && (result.HasValue))
                {
                    fontKey = result.Value;
                    return true;
                }
            }



            SKFontStyle skFontStyle;

            switch (fontWeight)
            {
                case FontWeight.Normal when fontStyle == FontStyle.Normal:
                    skFontStyle = SKFontStyle.Normal;
                    break;
                case FontWeight.Normal when fontStyle == FontStyle.Italic:
                    skFontStyle = SKFontStyle.Italic;
                    break;
                case FontWeight.Bold when fontStyle == FontStyle.Normal:
                    skFontStyle = SKFontStyle.Bold;
                    break;
                case FontWeight.Bold when fontStyle == FontStyle.Italic:
                    skFontStyle = SKFontStyle.BoldItalic;
                    break;
                default:
                    skFontStyle = new SKFontStyle((SKFontStyleWeight)fontWeight, SKFontStyleWidth.Normal, (SKFontStyleSlant)fontStyle);
                    break;
            }

            if (culture == null)
            {
                culture = CultureInfo.CurrentUICulture;
            }

            if (t_languageTagBuffer == null)
            {
                t_languageTagBuffer = new string[2];
            }

            t_languageTagBuffer[0] = culture.TwoLetterISOLanguageName;
            t_languageTagBuffer[1] = culture.ThreeLetterISOLanguageName;

            if (fontFamily != null && fontFamily.FamilyNames.HasFallbacks)
            {
                var familyNames = fontFamily.FamilyNames;

                for (var i = 1; i < familyNames.Count; i++)
                {
                    var skTypeface =
                        _skFontManager.MatchCharacter(familyNames[i], skFontStyle, t_languageTagBuffer, codepoint);

                    if (skTypeface == null)
                    {
                        continue;
                    }

                    fontKey = new Typeface(skTypeface.FamilyName, fontStyle, fontWeight);

                    return true;
                }
            }
            else
            {
                var skTypeface = _skFontManager.MatchCharacter(null, skFontStyle, t_languageTagBuffer, codepoint);

                if (skTypeface != null)
                {
                    fontKey = new Typeface(skTypeface.FamilyName, fontStyle, fontWeight);

                    return true;
                }
            }

            fontKey = default;

            return false;
        }

        public IGlyphTypefaceImpl CreateGlyphTypeface(Typeface typeface)
        {
            if (TryCall<IGlyphTypefaceImpl>(() => _prevImpl.CreateGlyphTypeface(typeface), out IGlyphTypefaceImpl result))
            {
                if (result != null)
                    return result;
            }

            Console.WriteLine("TYPEFACE: " + typeface.FontFamily.Name + " (" + typeface.Weight.ToString() + ")\n\n");

            SKTypeface skTypeface = null;

            var fontStyle = new SKFontStyle((SKFontStyleWeight)typeface.Weight, SKFontStyleWidth.Normal, (SKFontStyleSlant)typeface.Style);

            foreach (var familyName in typeface.FontFamily.FamilyNames)
            {
                skTypeface = _skFontManager.MatchFamily(familyName, fontStyle);

                if (skTypeface is null)
                    continue;

                break;
            }
            return new GlyphTypefaceImpl(skTypeface);
        }










        [DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern string GetWineVersion();

		static bool? IsRunningUnderWine(out Version version)
		{
			version = new Version(0, 0, 0, 0);
			try
			{
				string wineVer = GetWineVersion();
				if (!Version.TryParse(wineVer, out version))
					return null;

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
        
        public static void ServeDrinks()
        {
            /*bool wine = false;
            
            try
            {
                wine = IsRunningUnderWine(out Version _).HasValue;
            }
            catch (Exception e)
            {
                
            }



            if (true) //wine)
            {*/
                try
                {
                    var locator = AvaloniaLocator.Current;
                    var skiaFontManagerImpl = locator.GetService<IFontManagerImpl>();
                    /*Assembly avSkia = Assembly.Load("Avalonia.Skia");
                    Type type = avSkia.GetType("Avalonia.Skia.FontManagerImpl", true, false);

                    
                    var fontMgr = Activator.CreateInstance(type, true);
                    var getDefaultFontFamilyName = type.GetMethod("GetDefaultFontFamilyName");

                    if (getDefaultFontFamilyName != null)
                    {*/
                        FieldInfo field = locator.GetType().GetField("_registry", BindingFlags.NonPublic | BindingFlags.Instance);
                        var fieldValue = field.GetValue(locator);
                        Dictionary<Type, Func<object>> locatorRegistry = (Dictionary<Type, Func<object>>)fieldValue;

                        
                        Func<object> alchohol = (() => new DrunkFontManagerImpl(skiaFontManagerImpl));
                        if (locatorRegistry.ContainsKey(typeof(IFontManagerImpl)))
                            locatorRegistry[typeof(IFontManagerImpl)] = alchohol;
                        else
                            locatorRegistry.Add(typeof(IFontManagerImpl), alchohol);
                    //}
                    
                    string bar = "Avalonia FontManager should now be drunk out of its mind. Please don't let it try to drive home on its own.";
                    Console.WriteLine(bar);
                    Debug.WriteLine(bar);
                }
                catch (Exception ex)
                {
                    string barFight = $"ok hold up something is wrong\n\n\n{ex}";
                    Console.WriteLine(barFight);
                    Debug.WriteLine(barFight);
                }
            /*}
            else
            {
                string noDrincc = "Not running under wine. Just order water or something idk";
                Console.WriteLine(noDrincc);
                Debug.WriteLine(noDrincc);
            }*/
        }
    }
}