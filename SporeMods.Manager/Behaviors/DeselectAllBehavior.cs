/*using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace SporeMods.Manager
{
	public class DeselectAllBehavior : Behavior<Control>
	{
        public static readonly StyledProperty<ItemsPresenter> ItemsPresenterElementProperty =
            AvaloniaProperty.Register<DeselectAllBehavior, ItemsPresenter>(nameof(ItemsPresenterElement), null);

        public ItemsPresenter ItemsPresenterElement
		{
			get => GetValue(ItemsPresenterElementProperty);
			set => SetValue(ItemsPresenterElementProperty, value);
		}

		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject.PointerPressed += AssociatedObject_MouseLeftButtonDown;
		}

		private void AssociatedObject_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
		{
			if ((AssociatedObject!= null) && (AssociatedObject.IsPointerOver))
			{
				bool areChildrenMousedOver = false;
				Panel panel = Avalonia.VisualTree.VisualExtensions.FindDescendantOfType<Panel>(ItemsPresenterElement, false);
#if RESTORE_LATER
				ListViewItem[] items = new ListViewItem[panel.Children.Count];

				panel.Children.CopyTo(items, 0);
				foreach (ListViewItem item in items)
				{
					if (item.IsMouseOver)
					{
						areChildrenMousedOver = true;
						break;
					}
				}

				if (!areChildrenMousedOver)
				{
					((Window.GetWindow(AssociatedObject) as Window).Content as ManagerContent).InstalledModsListView.SelectedItem = null;
					e.Handled = true;
				}
#endif
			}
		}
	}
}
*/