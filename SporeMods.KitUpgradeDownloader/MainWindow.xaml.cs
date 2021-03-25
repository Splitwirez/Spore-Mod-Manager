using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace SporeMods.KitUpgradeDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern string GetWineVersion();

        static string WINDOW_TEXT = string.Empty;

        public static bool IsRunningUnderWine(out Version version)
        {
            version = new Version(0, 0, 0, 0);
            try
            {
                string wineVer = GetWineVersion();
                WINDOW_TEXT += "raw WINE version string: " + wineVer + "\n\n";
                if (!Version.TryParse(wineVer, out version))
                    WINDOW_TEXT += "Running under WINE. Unable to identify what version.\n\n";

                return true;
            }
            catch (Exception ex)
            {
                WINDOW_TEXT += "Failed to call GetWineVersion():\n" + ex.ToString() + "\n\n";
                return false;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            if (IsRunningUnderWine(out Version wineVersion))
            {
                WINDOW_TEXT += "Running under WINE version " + wineVersion + "\n\n";
                var minVer = new Version(6, 0);
                if (wineVersion >= minVer)
                {
                    WINDOW_TEXT += "This version of WINE is probably new enough. (>= " + minVer + ")\n\n";
                }
                else
                    WINDOW_TEXT += "This version of WINE is probably too old! (<= " + minVer + ")\n\n";
            }
            else
                WINDOW_TEXT += "Not running under WINE\n\n";

            OutputTextBox.Text = WINDOW_TEXT;
        }
    }
}
