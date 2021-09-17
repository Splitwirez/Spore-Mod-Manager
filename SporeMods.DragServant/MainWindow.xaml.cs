using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static SporeMods.CommonUI.NativeMethods;
//using CompositingWindow = SporeMods.CommonUI.Windows.CompositingWindow;
using SporeMods.Core;
using SporeMods.CommonUI.Localization;
using System.Diagnostics;

namespace SporeMods.DragServant
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		IntPtr _winHandle = IntPtr.Zero;
		public MainWindow()
		{
			InitializeComponent();
			//Hide();
			/*ShowInTaskbar = false;
			ShowActivated = false;*/
			//this.SizeChanged  += (sneder, args) => RefreshText();

			/*RootGrid.IsVisibleChanged += (sneder, args) =>
			{
				if (args.NewValue is bool val)
					RefreshText(val);
			};*/

			Activated += (sneder, args) => SetStyles();
		}

		/*int count = 0;
		public static void RefreshText()
        {
			/*
			//Settings.ReparseSettingsDoc();
			Language lang = LanguageManager.Instance.CurrentLanguage;
			lang = LanguageManager.Instance.CurrentLanguage;
			//DropModsHereTextBlock.Text = LanguageManager.Instance.GetLocalizedText("DontRunAsAdmin") + " " + count + "\n" + (lang != null ? lang.DisplayName : "NO IT REALLY IS NULL") + "\n" + LanguageManager.Instance.Languages.Count + " LANGUAGES" + "\n" + LanguageManager.Instance.Languages.First().DisplayName + "\n" + Settings.GetElementValue("CurrentLanguageCode");
			count++;
			//MessageBox.Show(lang != null ? lang.DisplayName : "NO IT REALLY IS NULL");
			*
			//LanguageManager.Instance.TryRefreshWpfResources();
			//DropModsHereTextBlock.Text = LanguageManager.Instance.GetLocalizedText("DontRunAsAdmin");
		}*/

		public void RefreshText(string text)
		{
			DropModsHereTextBlock.Text = text;
		}

		/*protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
			RefreshText();
        }*/

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			//var _ = LanguageManager.Instance.CurrentLanguage;
			_winHandle = new WindowInteropHelper(this).EnsureHandle();
			SetStyles();

			//NativeMethods.SetWindowLong(Handle, NativeMethods.GwlStyle, NativeMethods.GetWindowLong(Handle, NativeMethods.GwlStyle).ToInt32() & ~(0x00000000 | 0x00C00000 | 0x00800000 | 0x00080000 | 0x00040000));
			////////HwndSource.FromHwnd(_winHandle).AddHook(new HwndSourceHook(WndProc));
			//SetWindowLong(_winHandle, GwlExstyle, (Int32)(GetWindowLong(_winHandle, GwlExstyle)) | WsExToolwindow | WsExNoActivate);
			//Hide();
			//Path.Combine(Settings.TempFolderPath, "LaunchGame")
		}

		/*IntPtr WndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled)
		{
			if (msg == 0x0018)
            {
				RefreshText();
            }
			handled = false;
			return IntPtr.Zero;
		}*/
			
		void SetStyles()
		{
			//SetWindowLong(_winHandle, GwlExstyle, (Int32)(GetWindowLong(_winHandle, GwlExstyle)) | ~WsExToolwindow);
			SetWindowLong(_winHandle, GwlExstyle, ((Int32)(GetWindowLong(_winHandle, GwlExstyle)) | WsExToolwindow) & ~0x00040000);
			//SetWindowLong(helper.Handle, GwlExstyle, (Int32)GetWindowLong(helper.Handle, GwlExstyle) | ~0x00040000); //WS_EX_APPWINDOW
		}

		/*protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			ShowInTaskbar = false;
			ShowActivated = false;
		}*/

		private void Window_PreviewDrop(object sender, DragEventArgs e)
		{
			/*foreach (ResourceDictionary dict in App.Current.Resources.MergedDictionaries)
			{
				string outp = string.Empty;
				foreach (object h in dict.Keys)
				{
					outp += h.ToString() + ",          " + dict[h] + "\n";
				}
				MessageBox.Show(outp, dict.ToString());
			}*/


			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if (Settings.DebugMode)
				{
					foreach (string f in files)
					{
						MessageBox.Show(f);
					}
				}

				string draggedFilesPath = Path.Combine(Settings.TempFolderPath, "draggedFiles");
				File.WriteAllLines(draggedFilesPath, files);
				Permissions.GrantAccessFile(draggedFilesPath);
			}
		}
	}
}
