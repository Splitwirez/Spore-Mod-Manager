using SporeMods.Core;
using SporeMods.Core.ModIdentity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace SporeMods.Manager
{
    public class ComponentCheckBoxBehavior : Behavior<CheckBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsChecked = (AssociatedObject.DataContext as ModComponent).IsEnabled;
            AssociatedObject.Checked += AssociatedObject_Checked;
            AssociatedObject.Unchecked += AssociatedObject_Checked;
        }

        private void AssociatedObject_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AssociatedObject.IsChecked.Value)
                (AssociatedObject.DataContext as ModComponent).IsEnabled = true;
            else
                (AssociatedObject.DataContext as ModComponent).IsEnabled = false;

            MessageDisplay.DebugShowMessageBox("IsChecked: " + AssociatedObject.IsChecked.Value + "\nIsEnabled: " + (AssociatedObject.DataContext as ModComponent).IsEnabled);
        }
    }
}
