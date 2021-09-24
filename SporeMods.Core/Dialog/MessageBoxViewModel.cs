using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core
{
    public class MessageBoxViewModel : ModalViewModel<object>
	{
		string _content = string.Empty;
		public string Content
		{
			get => _content;
			protected set
			{
				_content = value;
				NotifyPropertyChanged();
			}
		}


		public MessageBoxViewModel(string content, string title = null)
		{
			Content = DialogBox.EnsureContent(content);
			
			if (DialogBox.EnsureTitle(title, out string guaranteedTitle))
				Title = guaranteedTitle;

			DismissCommand = Externals.CreateCommand<object>(o =>
			{
				CompletionSource.TrySetResult(null);
			});
		}
		private MessageBoxViewModel()
		{ }


		public override string GetViewTypeName()
			=> "SporeMods.Views.MessageBoxView";
	}
}