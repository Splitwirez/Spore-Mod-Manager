using SporeMods.Core;
using SporeMods.Core.Mods;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace SporeMods.Views
{
    /// <summary>
    /// Interaction logic for ModJobsBatchView.xaml
    /// </summary>
    public partial class ModJobsBatchView : UserControl
    {
		public ModJobsBatchView()
        {
            InitializeComponent();
        }

        /*private void ModSettings_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var modSettings = (SporeMods.CommonUI.Zone)sender;

            if (
                (e.NewValue != null)
                && (e.NewValue is IConfigurableMod mod)
                )
            {
                string viewTypeName = mod.GetSettingsViewTypeName();
                Type viewType = Type.GetType(viewTypeName);
                FrameworkElement view = (FrameworkElement)(Activator.CreateInstance(viewType));
                view.DataContext = mod.GetSettingsViewModel();
                modSettings.Content = view;
            }
            else
                modSettings.Content = null;
        }*/
    }

    public class ModSettingsDataContextChangedBehavior : Behavior<ContentPresenter>
    {
        public static readonly DependencyProperty ModProperty =
            DependencyProperty.Register(nameof(Mod), typeof(IConfigurableMod), typeof(ModSettingsDataContextChangedBehavior), new PropertyMetadata(null, OnModPropertyChanged));

        static void OnModPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ModSettingsDataContextChangedBehavior b)
            {
                if (
                    (e.NewValue != null)
                    && (e.NewValue is IConfigurableMod mod)
                    )
                {
                    string viewTypeName = mod.GetSettingsViewTypeName(false);
                    Type viewType = Type.GetType(viewTypeName);
                    FrameworkElement view = (FrameworkElement)(Activator.CreateInstance(viewType));
                    view.DataContext = mod.GetSettingsViewModel(false);
                    b.AssociatedObject.Content = view;
                }
                else
                    b.AssociatedObject.Content = null;
            }
        }

        public IConfigurableMod Mod
        {
            get => (IConfigurableMod)GetValue(ModProperty);
            set => SetValue(ModProperty, value);
        }
    }
}
