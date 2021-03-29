using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SporeMods.KitUpgradeDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string SMM_URL = "http://davoonline.com/sporemodder/rob55rod/ModAPI/SMMNetCoreDownloadTest/SporeModManagerSetup.zip";
        static string EXECUTABLE_FOLDER = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).Directory.FullName;
        static string DOWNLOAD_FILENAME = Path.Combine(EXECUTABLE_FOLDER, "SporeModManagerSetup.zip");
        string SMM_SETUP_DEST = Path.Combine(EXECUTABLE_FOLDER, "SporeModManagerSetup.exe");
        bool _canClose = false;

        /*[DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
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
        }*/

        public MainWindow()
        {
            InitializeComponent();
            /*if (IsRunningUnderWine(out Version wineVersion))
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

            OutputTextBox.Text = WINDOW_TEXT;*/


        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_canClose)
                e.Cancel = true;
        }

        public async void MainWindow_ContentRendered(object sender, EventArgs w)
        {
            ContentRendered -= MainWindow_ContentRendered;

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
                args = args.Skip(1).ToArray();
            else
                args = new string[0];

            string combinedArgs = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                //MessageBox.Show(arg, "CommandLine option " + i);

                if (arg.Contains(" "))
                    arg = "\"" + arg + "\"";

                combinedArgs += arg + " ";
            }


            //MessageBox.Show("DOWNLOADING TO " + DOWNLOAD_FILENAME);
            if (File.Exists(DOWNLOAD_FILENAME))
                File.Delete(DOWNLOAD_FILENAME);
            if (File.Exists(SMM_SETUP_DEST))
                File.Delete(SMM_SETUP_DEST);

            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += (s, e) =>
                {
                    int percentage = e.ProgressPercentage;
                    //MessageBox.Show("Progress percentage: " + percentage);

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DownloadPercentageTextBlock.Text = "" + e.BytesReceived + " / " + e.TotalBytesToReceive + " bytes downloaded...";
                        DownloadProgressBar.Value = percentage;
                    }));
                };
                client.DownloadFileCompleted += (s, e) =>
                {
                    if (e.Error != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show(e.Error.ToString(), "Download failed");
                            
                            _canClose = true;
                            Close();
                        }));
                    }
                    else
                    {
                        /*Process.Start(new ProcessStartInfo(DOWNLOAD_FILENAME, combinedArgs)
                        {
                            UseShellExecute = true
                        });*/
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            DownloadPercentageTextBlock.Text = "Extracting...";
                            DownloadProgressBar.IsIndeterminate = true;
                        }));

                        Thread thread = new Thread(() =>
                        {
                            ZipFile.ExtractToDirectory(DOWNLOAD_FILENAME, EXECUTABLE_FOLDER);

                            Process.Start(new ProcessStartInfo(SMM_SETUP_DEST, combinedArgs)
                            {
                                UseShellExecute = true
                            });


                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _canClose = true;
                                Close();
                            }));
                        });
                        thread.Start();
                    }
                };


                await client.DownloadFileTaskAsync(SMM_URL, DOWNLOAD_FILENAME);
            }
        }
    }
}
