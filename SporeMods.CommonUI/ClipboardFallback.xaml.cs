using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SporeMods.CommonUI
{
    /// <summary>
    /// Interaction logic for ClipboardFallback.xaml
    /// </summary>
    public partial class ClipboardFallback : UserControl
    {
        public ClipboardFallback(string instruction, string content)
        {
            InitializeComponent();
            InstructionTextBlock.Text = instruction;
            ContentTextBox.Text = content;
        }
    }
}
