using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using System.Timers;

using SporeMods.Core;
using static SporeMods.CommonUI.NativeMethods;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;

using RequestFilesViewModel = SporeMods.ViewModels.RequestFilesViewModel;
using WRect = System.Windows.Rect;
using System.Threading;

namespace SporeMods.Views
{
    /// <summary>
    /// Interaction logic for RequestFilesView.xaml
    /// </summary>
    public partial class RequestFilesView : UserControl
    {
		static bool _firstTimeShown = false;
        RequestFilesViewModel VM => DataContext as RequestFilesViewModel;


		Window _ownerWindow = null;
		IntPtr _ownerHwnd = IntPtr.Zero;
		IntPtr _servantHwnd = IntPtr.Zero;

		//Point _topLeft = new Point(0, 0);
		Point _bottomRight = new Point(0, 0);
		public RequestFilesView()
        {
            InitializeComponent();

			bool hasServant = ServantCommands.HasDragServant;
			bool sober = (!Settings.NonEssentialIsRunningUnderWine);
			if (hasServant || sober)
			{
				DropHereZone.Content = "Choose an installation method from below... (PLACEHOLDER) (NOT LOCALIZED)";
				if (hasServant)
					DropHereZone.Tag = "HasDragServant";
				else if (sober)
					DropHereZone.Tag = "NoDrop";
			}
			else
            {

            }


			Loaded += (s, e) =>
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					_ownerWindow = Window.GetWindow(this);
					_ownerHwnd = new WindowInteropHelper(_ownerWindow).Handle;

					if (ServantCommands.TryGetDragServantHwnd(out IntPtr servantHwnd))
						_servantHwnd = servantHwnd;
					if (IsWindow(_servantHwnd))
					{
						SetParent(_servantHwnd, _ownerHwnd);
						ShowWindow(_servantHwnd, 4);


						/*GetCoordsForSetWindowPos(out int X, out int Y, out int cx, out int cy);

						var flags = SwpNoActivate | SwpShowWindow | SwpNoZOrder;
						if (_firstTimeShown)
							_firstTimeShown = false;
						else
							flags |= SwpNoMove;*/

						RefreshDropHereZonePlacement(true);
						//(int)(_topLeft.X), (int)(_topLeft.Y)
						//SwpNoSize | 
					}

					ServantCommands.WatchForServantEvents = true;
					ServantCommands.FilesDropped += DragServant_FilesDropped;

                    //_ownerWindow.SizeChanged += OwnerWindow_SizeChanged;
                    //_ownerWindow.LayoutUpdated += OwnerWindow_LayoutUpdated;
                    //_ownerWindow.ContentRendered += OwnerWindow_ContentRendered;
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
					////////_ownerWindow.Deactivated += OwnerWindow_Deactivated;
				}), DispatcherPriority.Render, null);
			};
			Unloaded += (s, e) =>
			{
				if (IsWindow(_servantHwnd))
					ShowWindow(_servantHwnd, 0);
				
				ServantCommands.FilesDropped -= DragServant_FilesDropped;

				//_ownerWindow.SizeChanged -= OwnerWindow_SizeChanged;
				//_ownerWindow.SizeChanged -= OwnerWindow_LayoutUpdated;
				CompositionTarget.Rendering -= CompositionTarget_Rendering;
				////////_ownerWindow.Deactivated -= OwnerWindow_Deactivated;
				
				_ownerWindow = null;
			};
		}

		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			RefreshDropHereZonePlacement();


			if (IsWindow(_servantHwnd) && (GetForegroundWindow() == _servantHwnd))
				_ownerWindow?.Activate();
		}

		/*private void OwnerWindow_ContentRendered(object sender, EventArgs e)
		{
			RefreshDropHereZonePlacement();
		}

        private void OwnerWindow_LayoutUpdated(object sender, EventArgs e)
        {
			RefreshDropHereZonePlacement();
		}

        private void OwnerWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
			RefreshDropHereZonePlacement();
        }*/

		private void OwnerWindow_Deactivated(object sender, EventArgs e)
        {
			Dispatcher.BeginInvoke(new Action(() =>
			{
				if (IsWindow(_servantHwnd) && (GetForegroundWindow() == _servantHwnd))
				{
					_ownerWindow?.Activate();
				}
			}), DispatcherPriority.Render, null);
        }

        /*void GetCoordsForSetWindowPos(out int X, out int Y, out int cx, out int cy)
        {
			//_topLeft = DropHereZone.TranslatePoint(new Point(0, 0), _ownerWindow);

			//_topLeft = new Point;



			//WRect pos = transform.Transformpos(new WRect(0, 0, DropHereZone.DesiredSize.Width, DropHereZone.DesiredSize.Height));
			Console.WriteLine($"DesiredSize: {DropHereZone.DesiredSize}");


			//GeneralTransform transform = DropHereZone.TransformToVisual(_ownerWindow);
			//Point pos = transform.Transform(new Point((DropHereZone.RenderSize.Width / -2), (DropHereZone.DesiredSize.Height / -2)));
			Point pos = DropHereZone.TransformToAncestor(_ownerWindow).Transform(new Point(0, 0)); //DropHereZone.DesiredSize.Width / 2, DropHereZone.DesiredSize.Height / 2));

			X = (int)(SystemScaling.WpfUnitsToRealPixels(pos.X));
			Y = (int)(SystemScaling.WpfUnitsToRealPixels(pos.Y));


			//_bottomRight = DropHereZone.TranslatePoint(new Point(SystemScaling.WpfUnitsToRealPixels(DropHereZone.DesiredSize.Width), SystemScaling.WpfUnitsToRealPixels(DropHereZone.DesiredSize.Height)), _ownerWindow);
			//var basePoint = DropModsHereTextBlockGrid.PointToScreen(new Point(0, 0));


			cx = (int)SystemScaling.WpfUnitsToRealPixels(DropHereZone.DesiredSize.Width);
			cy = (int)SystemScaling.WpfUnitsToRealPixels(DropHereZone.DesiredSize.Height);
		}*/

		/*protected override void ParentLayoutInvalidated(UIElement child)
        {
            base.ParentLayoutInvalidated(child);
			RefreshDropHereZonePlacement(false);
		}*/

		void RefreshDropHereZonePlacement(bool activate = false)
        {
			if (
					(_ownerWindow != null) &&
					(_ownerWindow.IsAncestorOf(DropHereZone))
				)
			{
				//GetCoordsForSetWindowPos(out int X, out int Y, out int cx, out int cy);
				Point pos = DropHereZone.TransformToAncestor(_ownerWindow).Transform(new Point(0, 0));

				Thickness margin = DropHereZone.Margin;
				double marginL = margin.Left;
				double marginT = margin.Top;

				int X = (int)(SystemScaling.WpfUnitsToRealPixels(pos.X - marginL));
				int Y = (int)(SystemScaling.WpfUnitsToRealPixels(pos.Y - marginT));

				int cx = (int)SystemScaling.WpfUnitsToRealPixels(DropHereZone.RenderSize.Width + (margin.Right + marginL));
				int cy = (int)SystemScaling.WpfUnitsToRealPixels(DropHereZone.RenderSize.Height + (margin.Bottom + marginT));

				var flags = SwpShowWindow | SwpNoZOrder;
				if (!activate)
					flags |= SwpNoActivate;
				/*if (!move)
					flags |= SwpNoMove;
				if (!size)
					flags |= SwpNoSize;
				if (!show)
					flags |= SwpShowWindow;*/
				//RefreshDropHereZonePlacement()
				SetWindowPos(_servantHwnd, _ownerHwnd, X, Y, cx, cy, flags);
				//Console.WriteLine($"Moved to: {X}, {Y}, {cx}, {cy}"); // ...{move}, {size}, {show}");

				if (activate)
					_ownerWindow.Activate();
			}
		}

        private void DragServant_FilesDropped(object sender, FileDropEventArgs e)
        {
			Dispatcher.BeginInvoke(new Action(() => VM.GrantFiles(e.Files)), null);
            //throw new NotImplementedException();
        }

        private void DropHereZone_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var data = e.Data.GetData(DataFormats.FileDrop);

				if (data is IEnumerable<string> files)
					VM.GrantFiles(files);
				else
					Console.WriteLine("Wrong FileDrop data?? (PLACEHOLDER) (NOT LOCALIZED)");
			}
			else
			{
				Console.WriteLine("Wrong data! (PLACEHOLDER) (NOT LOCALIZED)");
			}
		}
	}
}
