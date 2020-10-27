using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SporeMods.KitImporter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            if (!Permissions.IsAdministrator())
                Permissions.RerunAsAdministrator();
            base.OnStartup(e);
        }
    }
}
