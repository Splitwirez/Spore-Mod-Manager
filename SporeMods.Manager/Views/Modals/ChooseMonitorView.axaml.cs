using SporeMods.CommonUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace SporeMods.Manager.Views
{
    public partial class ChooseMonitorView : UserControl
    {
		public ChooseMonitorView()
		{
			InitializeComponent();
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            ChooseMonitorPopupView.ShowAll(e.Root as Window, DataContext);
        }
	}
}
