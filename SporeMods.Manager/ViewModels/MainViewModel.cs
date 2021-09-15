using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;

namespace SporeMods.ViewModels
{
	public class MainViewModel : NotifyPropertyChangedBase
	{
		int _selectedTabIndex = 0;

		public int SelectedTabIndex
		{
			get => _selectedTabIndex;
			set
			{
				_selectedTabIndex = value;
				NotifyPropertyChanged();
			}
		}

		string _title = string.Empty;

		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				NotifyPropertyChanged();
			}
		}



		public MainViewModel()
			: base()
		{
			LanguageManager.LanguageChanged += (s, e) => RefreshTitle();
			RefreshTitle();

			_installMods = new FuncCommand<object>(o =>
			{
				InstallMods();
			});
		}

		void RefreshTitle()
		{
			Title = LanguageManager.Instance.GetLocalizedText("Header!" +
#if SET_BUILD_CHANNEL
				"WithBuildChannel"
#else
				"NoBuildChannel"
#endif
				)
				.Replace("%VERSION%", Settings.ModManagerVersion.ToString())
				.Replace("%DLLSBUILD%", Settings.CurrentDllsBuildString)
				.Replace("%DOTNETRUNTIME%", Settings.TargetFramework)
#if SET_BUILD_CHANNEL
				.Replace("%BUILDCHANNEL%", Settings.BuildChannel)
#endif
				;
		}



		public FuncCommand<object> ShowTestDialogCommand { get; }
			= new FuncCommand<object>(o =>
		{
			DialogBox.ShowAsync("Hello world!", "Yeehaw");
		});


		FuncCommand<object> _installMods = null;
		public FuncCommand<object> InstallModsCommand
		{
			get => _installMods;
		}

		async Task InstallMods()
		{
			var files = await Modal.Show(new RequestFilesViewModel(FileRequestPurpose.InstallMods, true));
			if (files != null)
				ModInstallation.InstallModsAsync(files.ToArray());
		}

		public void ConfigureSelectedModCommand(object parameter)
		{
			//TODO: Implement
		}

		public void LaunchGameCommand(object parameter)
		{
			//TODO: Implement
		}
	}
}
