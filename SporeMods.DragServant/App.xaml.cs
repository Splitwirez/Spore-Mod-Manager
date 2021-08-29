using SporeMods.CommonUI;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace SporeMods.DragServant
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		FileSystemWatcher _launcherWatcher = new FileSystemWatcher(Settings.TempFolderPath)
		{
			EnableRaisingEvents = true,
			IncludeSubdirectories = false
		};


		public App()
		{
			_launcherWatcher.Created += (sneder, args) =>
			{
				string fileName = Path.GetFileName(args.FullPath);
				while (Permissions.IsFileLocked(args.FullPath))
				{ }

				if (File.Exists(args.FullPath))
				{
					bool processed = true;
					switch (fileName)
					{
						case "LaunchGame":
							if (!Environment.GetCommandLineArgs().Contains(UpdaterService.IgnoreUpdatesArg))
								CrossProcess.StartLauncher();
							break;
						case "OpenUrl":
							string path = File.ReadAllText(args.FullPath);
							if (path.StartsWith("http"))
								Process.Start(new ProcessStartInfo(path)
								{
									UseShellExecute = true
								});
							break;
						case "ChangeText":
							string newText = File.ReadAllText(args.FullPath);
							Dispatcher.BeginInvoke(new Action(() =>
							{
								if (MainWindow is MainWindow mainWin)
									mainWin.RefreshText(newText);
							}));
							break;
						default:
							processed = false;
							break;
					}

					if (processed)
						File.Delete(args.FullPath);
				}
			};
		}

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
			var _ = CommonUI.Localization.LanguageManager.Instance;
			var __ = CommonUI.Localization.LanguageManager.Instance.CurrentLanguage;

			Console.WriteLine(CommonUI.Localization.LanguageManager.Instance.GetLocalizedText("OK"));
		}
    }
}
