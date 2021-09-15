using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SporeMods.ViewModels
{
	public enum FileRequestPurpose
	{
		InstallMods
	}
	
	public class RequestFilesViewModel : ModalViewModel<IEnumerable<string>>
	{
		string _description = string.Empty;
		public string Description
		{
			get => _description;
			set
			{
				_description = value;
				NotifyPropertyChanged();
			}
		}



		FuncCommand<object> _browseInsteadCommand = null;
		public FuncCommand<object> BrowseInsteadCommand
		{
			get => _browseInsteadCommand;
			set
			{
				_browseInsteadCommand = value;
				NotifyPropertyChanged();
			}
		}


		FileRequestPurpose _purpose;
		
		const string PURPOSE_PLACEHOLDER = "%PURPOSE%";
		static readonly string TITLE_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!Header";
		static readonly string DESCRIPTION_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!Description";
		
		static readonly string WRONG_FILES_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!WrongFiles";
		
		static readonly string BROWSE_HEADER_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!Browse!Header";
		static readonly string BROWSE_FILTER_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!Browse!Filter";

		const string MOD_FILE_EXTENSIONS = "*.sporemod, *.package";

		public RequestFilesViewModel(FileRequestPurpose purpose, bool acceptMultiple)
			: base()
		{
			_purpose = purpose;

			Title = GetText(TITLE_KEY_BASE);
			Description = GetText(DESCRIPTION_KEY_BASE);
			

			_browseInsteadCommand = new FuncCommand<object>(o =>
			{
				OpenFileDialog dialog = new OpenFileDialog()
				{
					Multiselect = acceptMultiple,
					Title = GetText(BROWSE_HEADER_KEY_BASE),
					Filter = GetText(BROWSE_FILTER_KEY_BASE) + "|*.sporemod;*.package"
				};

				/*try
				{
					string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
					if (Directory.Exists(downloadsPath))
						dialog.InitialDirectory = downloadsPath;
				}
				catch (Exception ex)
				{

				}*/
				if (dialog.ShowDialog() == true)
				{
					GrantFiles(dialog.FileNames);
				}
			});

			DismissCommand = new FuncCommand<object>(o => CompletionSource.TrySetResult(null));
		}

		public bool GrantFiles(IEnumerable<string> fileNames)
		{
			if ((fileNames != null) && (fileNames.Count() > 0))
			{
				CompletionSource.TrySetResult(fileNames);
				return true;
			}
			else
				return false;
		}

		string GetText(string key)
		{
			string outText = LanguageManager.Instance.GetLocalizedText(key.Replace(PURPOSE_PLACEHOLDER, _purpose.ToString()));
			
			if ((key == WRONG_FILES_KEY_BASE) || (key == BROWSE_FILTER_KEY_BASE))
				outText = outText.Replace("%EXTENSIONS%", MOD_FILE_EXTENSIONS);
			
			return outText;
		}
	}
}
