
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
                MessageBox.Show("PLACEHOLDER: No config version");
                
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
                MessageBox.Show("PLACEHOLDER: Incompatible config version");

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
