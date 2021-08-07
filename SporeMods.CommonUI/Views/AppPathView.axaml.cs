using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;

namespace SporeMods.CommonUI.Views
{
    public class AppPathView : UserControl
    {
        public AppPathView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}