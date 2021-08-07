using SporeMods.CommonUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace SporeMods.Manager.Views
{
    public partial class ChooseMonitorPopupView : Window
    {
        static List<ChooseMonitorPopupView> All = new List<ChooseMonitorPopupView>();

        public static void ShowAll(Window window, object vm)
        {
            Console.WriteLine("Handling ChooseMonitorWindowsRequested event...");
            var screens = window.Screens.All;
            foreach (var screen in screens)
            {
                ChooseMonitorPopupView view = new ChooseMonitorPopupView()
                {
                    Tag = screen,
                    DataContext = vm
                };
                All.Add(view);
                view.Show();
                view.Position = new PixelPoint(screen.WorkingArea.X + 32, screen.WorkingArea.Bottom - 160);
            }
        }

        public static void CloseAll()
        {
            var all = All.ToArray();
            foreach (var popup in all)
            {
                popup.Close();
                All.Remove(popup);
            }
        }

        static ChooseMonitorPopupView()
        {
            SporeMods.Manager.ViewModels.ChooseMonitorViewModel.ChooseMonitorWindowsEnded += (s, e) => CloseAll();
        }

		public ChooseMonitorPopupView()
		{
			InitializeComponent();
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
	}
}

/*using SporeMods.CommonUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace SporeMods.Manager.Views
{
    public partial class ChooseMonitorView : UserControl
    {
        static List<ChooseMonitorView> All = new List<ChooseMonitorView>();
        static ChooseMonitorView()
        {
            SporeMods.Manager.ViewModels.ChooseMonitorViewModel.ChooseMonitorWindowsRequested += (s, e) =>
            {
                var screens = App.MainWindow.Screens.All;
                foreach (var screen in screens)
                {
                    ChooseMonitorView view = new ChooseMonitorView()
                    {
                        Tag = screen,
                        DataContext = s,
                        Position = new PixelPoint(screen.WorkingArea.Left + 32, screen.WorkingArea.Top - 160)
                    };
                    All.Add(view);
                }
            };
        }

		public ChooseMonitorView()
		{
			InitializeComponent();
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
	}
}
*/