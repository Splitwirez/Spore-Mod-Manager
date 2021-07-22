using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SporeMods.TestingGrounds
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            OnStartUp.EnsureAllGood(true, () => base.OnStartup(e));
        }
    }
}
