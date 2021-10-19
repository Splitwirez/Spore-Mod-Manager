using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
using CUIMsg = SporeMods.CommonUI.MessageDisplay;
using CoreMsg = SporeMods.Core.MessageDisplay;
using SporeMods.Views;
using SporeMods.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SporeMods.CommonUI
{
    public class SmmApp : Application
    {
		public SmmApp()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			DispatcherUnhandledException += App_DispatcherUnhandledException;
		}

		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			CleanupForExit();
			CUIMsg.ShowException(e.Exception);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			CleanupForExit();
			if (e.ExceptionObject is Exception exc)
				CUIMsg.ShowException(exc);
		}

		public static SmmApp Current
			=> ((Application.Current != null) && (Application.Current is SmmApp app)) ? app : null;


		UACLimitedPartnerDragWindow _dragWindow = null;
		public UACLimitedPartnerDragWindow DragWindow
        {
			get => _dragWindow;
			set => _dragWindow = value;
		}


		private void SetupThemeStuff()
		{
			SporeMods.CommonUI.Themes.Shale.ShaleHelper.FlipLightSwitch(!Settings.ShaleDarkTheme);

			FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
			{
				DefaultValue = Application.Current.FindResource(typeof(Window))
			});
		}

		protected virtual bool ShouldRerunAsAdministrator()
			=> false;
		protected virtual bool ShouldEnsureUACPartner()
			=> false;

		protected override void OnStartup(StartupEventArgs e)
		{
			CUIMsg.EnsureConsole();

			if (Settings.ForceSoftwareRendering)
				RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

			Externals.SpecifyFuncCommandType(typeof(FuncCommand<>));

			CoreMsg.ErrorOccurred += (sender, args) =>
			{
				CleanupForExit();
				CUIMsg.ShowException(args.Exception);
			};
			CoreMsg.MessageBoxShown += (sneder, args) => Dispatcher.BeginInvoke(new Action(() => CUIMsg.ShowMessageBox(args.Content, args.Title)));
			CoreMsg.DebugMessageSent += (sneder, args) => Dispatcher.BeginInvoke(new Action(() => CUIMsg.ShowMessageBox(args.Content, args.Title)));


			var _ = LanguageManager.Instance;
			SetupThemeStuff();

			Settings.EnsureDllsAreExtracted();
			Settings.ManagerInstallLocationPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
			CommonUI.Updater.CheckForUpdates();

			Exit += (s, e) => CleanupForExit();


			if (CommonUI.VersionValidation.IsConfigVersionCompatible(true, out Version previousModMgrVersion))
			{
				if (UACPartnerCommands.RunLkImporter() == null)
				{
					try
					{
						bool isAdmin = Permissions.IsAdministrator();
						bool rerunAsAdmin = ShouldRerunAsAdministrator();
						bool ensureUACPartner = ShouldEnsureUACPartner();


						if (rerunAsAdmin) // && EnsureUACPartner())
							DoFinishStartup(e, UACPartnerCommands.PrepareAppForUAC(ensureUACPartner, true));
						else
							DoFinishStartup(e, false);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
						CleanupForExit();
					}
				}
			}
		}

		void DoFinishStartup(StartupEventArgs e, bool isAdmin)
        {
			VersionValidation.WarnIfMissingOriginPrerequisites(Path.Combine(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName, "Launch Spore.dll"));

			ShutdownMode = ShutdownMode.OnLastWindowClose;
			base.OnStartup(e);

			FinishStartup(isAdmin);
		}

		protected virtual void FinishStartup(bool isAdmin)
        { }

		public virtual void CleanupForExit()
		{
			UACPartnerCommands.CloseOtherPartnerProcess();
		}
	}
}
