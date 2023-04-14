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
    public static class ShaleHelper
    {
        static ResourceDictionary _shaleLight = GetDictionary("pack://application:,,,/SporeMods.CommonUI;component/Themes/Shale/ShaleSmmLight.xaml");
        static ResourceDictionary _shaleDark = GetDictionary("pack://application:,,,/SporeMods.CommonUI;component/Themes/Shale/ShaleSmmDark.xaml");
        static ResourceDictionary GetDictionary(string uriPath)
        {
            var dictionary = new ResourceDictionary()
            {
                Source = new Uri(uriPath, UriKind.RelativeOrAbsolute)
            };
            
            //ResourceDictionaryHelper.Flatten(ref dictionary);
            
            /*var shaleControls = dictionary.MergedDictionaries[1].MergedDictionaries.Last();
            int index = dictionary.MergedDictionaries[1].MergedDictionaries.IndexOf(shaleControls);
            dictionary.MergedDictionaries[1].MergedDictionaries.Remove(shaleControls);
            ResourceDictionaryHelper.Flatten(ref shaleControls);
            dictionary.MergedDictionaries[1].MergedDictionaries.Insert(index, shaleControls);*/
            
            return dictionary;
        }
        
        
        public static void EnsureResources()
        {
            var app = Application.Current;
            if (app == null)
                return;


            int accentIndex = 0; //app.Resources.MergedDictionaries.Count;
            var prevAccent = app.Resources.MergedDictionaries.FirstOrDefault(x => x is ShaleAccent accent);
            if (prevAccent != null)
            {
                accentIndex = app.Resources.MergedDictionaries.IndexOf(prevAccent);
                app.Resources.MergedDictionaries.Remove(prevAccent);
            }

            app.Resources.MergedDictionaries.Insert(accentIndex, ShaleAccents.Sky);
            if (prevAccent == null)
                FlipLightSwitch(true, accentIndex + 1);
        }

        static ResourceDictionary _prevLightLevel = null;
        public static void FlipLightSwitch(bool lightsOn)
            => FlipLightSwitch(lightsOn, 0); //Application.Current != null ? Application.Current.Resources.MergedDictionaries.Count : -1);
        static void FlipLightSwitch(bool lightsOn, int index)
        {
            var app = Application.Current;
            if (app == null)
                return;
            ResourceDictionary lightLevel = lightsOn ? _shaleLight : _shaleDark;

            int lightsIndex = index;
            
            if ((_prevLightLevel != null) && app.Resources.MergedDictionaries.Contains(_prevLightLevel))
            {
                lightsIndex = app.Resources.MergedDictionaries.IndexOf(_prevLightLevel);
                //app.Resources.MergedDictionaries.Remove(_prevLightLevel);
                //_prevLightLevel.Source = lightLevel.Source;
                app.Resources.MergedDictionaries[lightsIndex] = lightLevel;
            }
            else
            {
                app.Resources.MergedDictionaries.Insert(0, lightLevel);
                /*_prevLightLevel = new ResourceDictionary()
                {
                    Source = lightLevel.Source
                };
                app.Resources.MergedDictionaries.Insert(0, _prevLightLevel);*/
            }


            //app.Resources.MergedDictionaries.Insert(lightsIndex, lightLevel);


            _prevLightLevel = lightLevel;
        }
    }
}
