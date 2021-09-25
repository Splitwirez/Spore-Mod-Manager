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
using ColorMine;
using ColorMine.ColorSpaces;

namespace SporeMods.CommonUI.Themes.Shale
{
    public static class ShaleHelper
    {
        static ResourceDictionary _shaleLight = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/SporeMods.CommonUI;component/Themes/Shale/ShaleSmmLight.xaml", UriKind.RelativeOrAbsolute)
        };
        static ResourceDictionary _shaleDark = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/SporeMods.CommonUI;component/Themes/Shale/ShaleSmmDark.xaml", UriKind.RelativeOrAbsolute)
        };
        
        static ResourceDictionary _prevLightLevel = null;
        public static void FlipLightSwitch(bool lightsOn)
        {
            ResourceDictionary lightLevel = lightsOn ? _shaleLight : _shaleDark;
            var app = Application.Current;
            if (app != null)
            {
                if (app.Resources.MergedDictionaries[0] == _prevLightLevel)
                    app.Resources.MergedDictionaries[0] = lightLevel;
                else
                    app.Resources.MergedDictionaries.Insert(0, lightLevel);
                
                _prevLightLevel = lightLevel;
            }
        }
    }
}
