using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Settings = SporeMods.Core.Settings;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Themes.Shale;

namespace SporeMods.Views
{
    /// <summary>
    /// Interaction logic for SmmAppearanceSettingsView.xaml
    /// </summary>Window
    public partial class SmmAppearanceSettingsView : UserControl
    {
        public SmmAppearanceSettingsView()
        {
            InitializeComponent();
        }

        private void LightsToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ShaleHelper.FlipLightSwitch(true);
        }

        private void LightsToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ShaleHelper.FlipLightSwitch(false);
        }



        private void UseCSDsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current?.Windows)
                WindowChromeHelper.SetUseCustomDecorations(window, true);
        }

        private void UseCSDsToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current?.Windows)
                WindowChromeHelper.SetUseCustomDecorations(window, false);
        }
    }
}
