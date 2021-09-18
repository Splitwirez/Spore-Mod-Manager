/*using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace SporeMods.CommonUI
{
	public class CopyErrorBehavior : Behavior<Button>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.Click += (sneder, args) =>
			{
				var context = AssociatedObject.DataContext as InstallError;
				Clipboard.SetText(context.DisplayName + "\n\n" + context.InstallException.ToString());
			};
		}
	}
}
*/