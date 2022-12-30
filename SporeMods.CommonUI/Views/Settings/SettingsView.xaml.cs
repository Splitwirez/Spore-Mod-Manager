using SporeMods.Core;
using System.Windows;
using System.Windows.Controls;

namespace SporeMods.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
		public SettingsView()
        {
            InitializeComponent();
            Loaded += (s, e) => Cmd.WriteLine("Loaded");
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Cmd.WriteLine("OnApplyTemplate");
            InvalidateMeasure();
            InvalidateArrange();
        }
    }
}
