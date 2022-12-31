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
using System.Windows.Media.Imaging;
using System.Threading;
using System.ComponentModel;
using SporeMods.CommonUI.Themes.Shale;

namespace SporeMods.CommonUI
{
    public class SmmApp : Application
    {
		public SmmApp()
		{
			/*AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
			{
				if (e.Exception is FileNotFoundException fnfex)
                {
					CUIMsg.ShowException(fnfex, false);
                }
			};*/
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			DispatcherUnhandledException += App_DispatcherUnhandledException;
		}

		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			//CleanupForExit();
			CUIMsg.ShowException(e.Exception);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			//CleanupForExit();
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


		private void SetupUIStuff(bool firstTime = false)
		{
			if (firstTime)
			{
				Resources.MergedDictionaries.Add(new ResourceDictionary()
				{
					Source = new Uri("pack://application:,,,/SporeMods.CommonUI;component/SmmAppResources.xaml")
				});
			}
			ShaleHelper.EnsureResources();
			bool lightsOn = true;
			if (!firstTime)
				lightsOn = Settings.IsLoaded ? (!Settings.ShaleDarkTheme) : true;
			ShaleHelper.FlipLightSwitch(lightsOn);
			
			if (firstTime)
			{
				//Resources.MergedDictionaries.Add(ShaleAccents.Sky);

				FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
				{
					DefaultValue = Application.Current.FindResource(typeof(Window))
				});
			}
		}

		protected virtual bool ShouldRerunAsAdministrator()
			=> false;
		protected virtual bool ShouldEnsureUACPartner()
			=> false;

		bool _prepareForUAC = false;

		bool _isAdmin = Permissions.IsAdministrator();
		bool _rerunAsAdmin = false;
		bool _ensureUACPartner = false;
		protected override void OnStartup(StartupEventArgs e)
		{
			Externals.UIThread = SynchronizationContext.Current;
			CUIMsg.EnsureConsole();

			RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

			Externals.SpecifyFuncCommandType(typeof(FuncCommand<>));
			Externals.ProvideExtractOriginPrerequisitesFunc(VersionValidation.ExtractOriginPrerequisites);
			Externals.CreateBitmapImage = s =>
			{
				var mem = new MemoryStream();
				s.CopyTo(mem);
				s.Seek(0, SeekOrigin.Begin);
				mem.Seek(0, SeekOrigin.Begin);
				BitmapImage bitmap = null;
				using (var stream = mem)
				{
					bitmap = new BitmapImage();
					bitmap.BeginInit();
					bitmap.StreamSource = stream;
					bitmap.CacheOption = BitmapCacheOption.OnLoad;
					bitmap.EndInit();
					bitmap.Freeze();
				}
				return bitmap;
			};

			CoreMsg.ErrorOccurred += (sneder, args) =>
			{
				if (OnErrorOccurred(sneder, args))
					CleanupForExit();
			};

			CoreMsg.MessageBoxShown += (sneder, args) => Dispatcher.BeginInvoke(new Action(() => CUIMsg.ShowMessageBox(args.Content, args.Title)));
			CoreMsg.DebugMessageSent += (sneder, args) => Dispatcher.BeginInvoke(new Action(() => CUIMsg.ShowMessageBox(args.Content, args.Title)));


			LanguageManager.Ensure();
			SetupUIStuff(true);

			Settings.Ensure();
			if (!Settings.ForceSoftwareRendering)
				RenderOptions.ProcessRenderMode = RenderMode.Default;

			try
			{
				Externals.SpecifyNeedsPrerequisitesExtracted(VersionValidation.NeedsPrerequisitesExtracted);
			}
			catch (Exception ex)
			{
				Externals.SpecifyNeedsPrerequisitesExtracted(true);
			}

			SetupUIStuff();
			
			//GameInfo.Ensure();
			SteamInfo.Ensure();
			
			ModsManager.Ensure();
			ModSearch.Ensure();
			//SporeMods.Core.ModTransactions.ModTransactionManager.Ensure();

			

			Settings.EnsureDllsAreExtracted();
			Settings.ManagerInstallLocationPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
			
			Exit += (s, e) => CleanupForExit();

			if (!CrossProcess.AreAnyOtherSmmProcessesRunning)
				CommonUI.Updater.CheckForUpdates();


			_rerunAsAdmin = ShouldRerunAsAdministrator();
			_ensureUACPartner = ShouldEnsureUACPartner();


			if (CommonUI.VersionValidation.IsConfigVersionCompatible(true, out Version previousModMgrVersion))
			{
				if (UACPartnerCommands.RunLkImporter() == null)
				{
						if ((!_rerunAsAdmin) || (!_isAdmin))
							VersionValidation.WarnIfMissingOriginPrerequisites(); //Path.Combine(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName, "Launch Spore.dll"));

						bool finishStartupIsAdmin = _isAdmin;

					try
					{
						if (_rerunAsAdmin)
							finishStartupIsAdmin = UACPartnerCommands.PrepareAppForUAC(_ensureUACPartner, true);

					}
					catch (Win32Exception ex)
					{
						Cmd.WriteLine(ex);
						CleanupForExit();
					}
					DoFinishStartup(e, finishStartupIsAdmin);
				}
			}
		}

		void DoFinishStartup(StartupEventArgs e, bool isAdmin)
        {
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

		protected virtual bool OnErrorOccurred(object sender, Core.ErrorEventArgs e)
        {
			CUIMsg.ShowException(e.Exception, false);
			return true;
		}
	}
}
