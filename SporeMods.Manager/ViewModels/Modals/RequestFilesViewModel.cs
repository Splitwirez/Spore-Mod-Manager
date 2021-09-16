using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;


using FClipboard = System.Windows.Forms.Clipboard;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SporeMods.ViewModels
{
	public enum FileRequestPurpose
	{
		InstallMods
	}
	
	public class RequestFilesViewModel : ModalViewModel<IEnumerable<string>>
	{
		string _descriptionDrag = string.Empty;
		public string DescriptionDrag
		{
			get => _descriptionDrag;
			set
			{
				_descriptionDrag = value;
				NotifyPropertyChanged();
			}
		}
		
		string _descriptionPasteBrowse = string.Empty;
		public string DescriptionPasteBrowse
		{
			get => _descriptionPasteBrowse;
			set
			{
				_descriptionPasteBrowse = value;
				NotifyPropertyChanged();
			}
		}

		FuncCommand<object> _browseCommand = null;
		public FuncCommand<object> BrowseCommand
		{
			get => _browseCommand;
			set
			{
				_browseCommand = value;
				NotifyPropertyChanged();
			}
		}

		FuncCommand<object> _pasteCommand = null;
		public FuncCommand<object> PasteCommand
		{
			get => _pasteCommand;
			set
			{
				_pasteCommand = value;
				NotifyPropertyChanged();
			}
		}


		FileRequestPurpose _purpose;
		
		const string PURPOSE_PLACEHOLDER = "%PURPOSE%";
		static readonly string TITLE_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!Header";
		static readonly string DESCRIPTION_DRAG_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!DescriptionDrag";
		static readonly string DESCRIPTION_PASTE_BROWSE_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!DescriptionPasteBrowse";
		
		static readonly string WRONG_FILES_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!WrongFiles";
		
		static readonly string BROWSE_HEADER_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!Browse!Header";
		static readonly string BROWSE_FILTER_KEY_BASE = $"FilesRequest!{PURPOSE_PLACEHOLDER}!Browse!Filter";

		const string MOD_FILE_EXTENSIONS = "*.sporemod, *.package";

		static readonly TextDataFormat[] TEXT_FORMATS =
		{
			TextDataFormat.CommaSeparatedValue,
			TextDataFormat.Html,
			TextDataFormat.Rtf,
			TextDataFormat.Text,
			TextDataFormat.UnicodeText,
			TextDataFormat.Xaml
		};

		public RequestFilesViewModel(FileRequestPurpose purpose, bool acceptMultiple)
			: base()
		{
			_purpose = purpose;

			Title = GetText(TITLE_KEY_BASE);
			DescriptionDrag = GetText(DESCRIPTION_DRAG_KEY_BASE);
			DescriptionPasteBrowse = GetText(DESCRIPTION_PASTE_BROWSE_KEY_BASE);
			

			_browseCommand = new FuncCommand<object>(o =>
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

			_pasteCommand = new FuncCommand<object>(o =>
			{
				if (FClipboard.ContainsFileDropList())
				{
					var files = FClipboard.GetFileDropList();
					GrantFiles(files.Cast<string>());
				}
				/*else
				{
					bool anyText = false;
					foreach (TextDataFormat format in TEXT_FORMATS)
					{
						if (Clipboard.ContainsText(format))
						{
							MessageBox.Show($"FORMAT: {format}\nDATA: {Clipboard.GetText(format)}");
							anyText = true;
							break;
						}
					}

					if (!anyText)
						MessageBox.Show("No text!");
				}*/
			});

			DismissCommand = new FuncCommand<object>(o => CompletionSource.TrySetResult(null));
		}

		public bool GrantFiles(IEnumerable<string> fileNames)
		{
			if (fileNames != null)
			{
				if (fileNames.Count() > 0)
				{
					/*string msgbox = string.Empty;
					foreach (string h in fileNames)
					{
						msgbox += $"'h'\n";
					}
					MessageBox.Show(msgbox);*/

					CompletionSource.TrySetResult(fileNames);
					return true;
				}
				else
					MessageBox.Show("Zero files in collection!");
			}
			else
				MessageBox.Show("Null collection!");
			
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
