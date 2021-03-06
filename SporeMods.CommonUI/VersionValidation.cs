﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using SporeMods.Core;

namespace SporeMods.CommonUI
{
    public static class VersionValidation
    {
        static bool _isConfigValidationCompleted = false;
        public static bool IsConfigValidationCompleted
        {
            get => _isConfigValidationCompleted;
        }

        public static bool IsConfigVersionCompatible(bool isWpfApp, out Version previousVersion)
        {
            previousVersion = null;
            if (Settings.LastModManagerVersion == null)
            {
                MessageBox.Show("The current config was generated by a pre-release version of the Spore Mod Manager which is too old to be compatible with the version you're using now. Please purge it to proceed.");
                
                if (isWpfApp)
                    Application.Current.Shutdown();
                else
                    Process.GetCurrentProcess().Close();

                return false;
            }

            previousVersion = Settings.LastModManagerVersion;
            bool compatible = Settings.ModManagerVersion >= Settings.LastModManagerVersion;
            
            if (compatible)
                Settings.LastModManagerVersion = Settings.ModManagerVersion;
            else
            {
                if (MessageBox.Show("The current config is for a newer version of the Spore Mod Manager than the version you're using. Check for updates now?", string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Updater.CheckForUpdates(true);
                

                if (isWpfApp)
                    Application.Current.Shutdown();
                else
                    Process.GetCurrentProcess().Close();
            }

            _isConfigValidationCompleted = true;
            return compatible;
        }
    }
}
