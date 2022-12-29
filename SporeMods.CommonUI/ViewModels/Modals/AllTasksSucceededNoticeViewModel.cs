using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;

namespace SporeMods.ViewModels
{
	public class AllTasksSucceededNoticeViewModel : ModalViewModel<object>
	{
		public AllTasksSucceededNoticeViewModel()
			: base()
		{
			Title="All done! (PLACEHOLDER) (NOT LOCALIZED)";
			DismissCommand = Externals.CreateCommand<object>(o => CompletionSource.TrySetResult(null));
		}
	}
}
