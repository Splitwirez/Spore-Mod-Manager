using SporeMods.Core;
using SporeMods.Core.ModIdentity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace SporeMods.Manager
{
    public class CheckBoxComponentMouseOverBehavior : Behavior<ToggleButton>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += (sneder, args) =>
            {
                //(Window.GetWindow(AssociatedObject).Content as ManagerContent).CustomInstallerContentPaneScrollViewer.Content = (AssociatedObject.DataContext as ModComponent).Description;
                var content = (Window.GetWindow(AssociatedObject).Content as ManagerContent);

                content.CustomInstallerContentStackPanel.Children.Clear();
                content.CustomInstallerContentStackPanel.Children.Add(new TextBlock()
                {
                    Text = (AssociatedObject.DataContext as ModComponent).Description
                });
            };
        }
    }
}
