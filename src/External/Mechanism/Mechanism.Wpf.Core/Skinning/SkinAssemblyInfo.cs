using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Mechanism.Wpf.Core.Skinning
{
    public abstract class SkinAssemblyInfo
    {
        /// <summary>
        /// APPLICATION DEVELOPERS: Indicates whether or not this Skin's settings have been altered since the last time a skin was applied. Set to false for all loaded skins when a skin is applied.
        /// <br/>SKIN DEVELOPERS: Set to <see langword="true"/> when settings are changed in your Settings page. This property will be altered externally, so do not assume its value!
        /// </summary>
        public bool SettingsHaveChanged = false;

        /// <summary>
        /// APPLICATION DEVELOPERS: When set in a skin's implementation, provides a page which displays the skin's settings page, if any. May be <see langword="null"/> for skins with no settings.
        /// <br/>SKIN DEVELOPERS: Set to <see langword="true"/> If your skin has settings, provide a page which allows the user to alter them.
        /// </summary>
        public Page SettingsPage = null;

        /// <returns>When overridden in a skin's implementation, returns a ResourceDictionary which the <see cref="SkinManager"/> will merge into the App's resources when applied.</returns>
        public abstract ResourceDictionary GetSkinDictionary();

        /*
        /// APPLICATION DEVELOPERS: 
        /// <br/>SKIN DEVELOPERS: 
        /// */

        /// <summary>
        /// APPLICATION DEVELOPERS: If given a base skin settings directory path, <see cref="SkinManager"/> will use this to allow skins to automatically load their previously-saved settings at startup.
        /// <br/>SKIN DEVELOPERS: Load your skin's settings, if any, from within the directory provided. Do not assume that the path provided contains valid settings.
        /// </summary>
        /// <param name="inputDirectory">Path to a directory to load settings from. Do not assume that the directory contains valid settings.</param>
        public abstract void LoadSkinSettings(string inputDirectory);

        /// <summary>
        /// APPLICATION DEVELOPERS: If given a base skin settings directory path, <see cref="SkinManager"/> will use this to allow skins to save their current settings for later retrieval.
        /// <br/>SKIN DEVELOPERS: Save your skin's settings, if any, to the directory provided. Do not assume that the directory provided already contains valid settings, nor that it is empty.
        /// </summary>
        /// <param name="inputDirectory">Path to a directory to save settings to. Do not assume that the directory already contains valid settings, nor that it is empty.</param>
        public abstract void SaveSkinSettings(string outputDirectory);

        /// <summary>
        /// APPLICATION DEVELOPERS: When overridden in a skin's implementation, resets the settings for the current skin to their default values.
        /// <br/>SKIN DEVELOPERS: Reset your skin's settings to their default values.
        /// </summary>
        public abstract void ResetSkinSettings();

        /*string SkinName
        {
            get;
        }*/
    }
}
