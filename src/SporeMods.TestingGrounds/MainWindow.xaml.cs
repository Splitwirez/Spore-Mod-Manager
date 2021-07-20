using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SporeMods.CommonUI;
using SporeMods.Core;

namespace SporeMods.TestingGrounds
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            SettingsTest();
        }


        static void SettingsTest()
        {
            string name = "LastMgrVersion";
            MessageBox.Show(SmmInfo.Instance.MgrLastVersion.ToString());
            SmmInfo.Instance.MgrLastVersion = new Version(99, 99, 99, 99);
            MessageBox.Show(SmmInfo.Instance.MgrLastVersion.ToString());
        }
    }
}
