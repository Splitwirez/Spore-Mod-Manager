using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows.Controls;

namespace SporeMods.Manager
{
	public class CloseModalDialogButtonBehavior : Behavior<Button>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.Click += (sneder, args) =>
			{
				if (AssociatedObject.TemplatedParent is ContentControl control)
					control.IsManipulationEnabled = false;
			};
		}
	}
}
