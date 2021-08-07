using Avalonia.Xaml.Interactivity;
using Avalonia.Controls;

namespace SporeMods.Manager
{
	public class CloseModalDialogButtonBehavior : Behavior<Button>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.Click += (sneder, args) =>
			{
				/*if (AssociatedObject.TemplatedParent is ContentControl control)
					control.IsManipulationEnabled = false;*/
			};
		}
	}
}
