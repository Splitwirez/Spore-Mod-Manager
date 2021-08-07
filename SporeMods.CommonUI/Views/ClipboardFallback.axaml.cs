using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.Windows.Input;
using Avalonia.Markup.Xaml;

namespace SporeMods.CommonUI
{
	/// <summary>
	/// Interaction logic for ClipboardFallback.xaml
	/// </summary>
	public partial class ClipboardFallback : UserControl
	{
		public ClipboardFallback()
		{
			InitializeComponent();
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

		public ClipboardFallback Setup(string instruction, string content)
		{
			this.Find<TextBlock>("InstructionTextBlock").Text = instruction;
			this.Find<TextBox>("ContentTextBox").Text = content;
			
			return this;
		}
	}
}
