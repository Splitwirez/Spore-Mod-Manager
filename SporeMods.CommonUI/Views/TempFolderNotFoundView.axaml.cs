using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using System.Xml.Linq;
using SporeMods.Core;
using MessageDisplay = SporeMods.Core.MessageDisplay;
//using System.Windows.Interop;
using System.Runtime.InteropServices;
using SporeMods.Core.Mods;
using Avalonia.Controls.Primitives;
using SporeMods.Core.Injection;
using static SporeMods.Core.Injection.SporeLauncher;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace SporeMods.CommonUI.Views
{
	public partial class TempFolderNotFoundView : UserControl
	{
		public TempFolderNotFoundView()
		{
			InitializeComponent();
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

		async void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFolderDialog dialog = new OpenFolderDialog()
			{
				Title = "CHOOSE GAME FOLDER (PLACEHOLDER) (NOT LOCALIZED)"
			};

            while (true)
			{
				string path = await dialog.ShowAsync(this.VisualRoot as Window);

				if ((path != null) && (Directory.Exists(path)))
				{
					(DataContext as BadPathEventArgs).GetCompletionSource().TrySetResult(path);
					/*if (badPath.DlcLevel == GameInfo.GameDlc.CoreSpore)
					{
						Settings.Instance.ForcedCoreSporeDataPath = path;
					}
					else if (badPath.DlcLevel == GameInfo.GameDlc.GalacticAdventures)
					{
						if (badPath.IsSporebin)
							Settings.Instance.ForcedGalacticAdventuresSporebinEP1Path = path;
						else
							Settings.Instance.ForcedGalacticAdventuresDataPath = path;
						
						
					}*/
				}
			}
		}
    }
}
