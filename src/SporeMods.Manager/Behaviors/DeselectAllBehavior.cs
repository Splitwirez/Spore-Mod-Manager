using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace SporeMods.Manager
{
	public class DeselectAllBehavior : Behavior<FrameworkElement>
	{
		public ItemsPresenter ItemsPresenterElement
		{
			get => (ItemsPresenter)GetValue(ItemsPresenterElementProperty);
			set => SetValue(ItemsPresenterElementProperty, value);
		}

		public static readonly DependencyProperty ItemsPresenterElementProperty =
			DependencyProperty.Register(nameof(ItemsPresenterElement), typeof(ItemsPresenter), typeof(DeselectAllBehavior), new PropertyMetadata(null));

		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
		}

		private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if ((AssociatedObject!= null) && (AssociatedObject.IsMouseOver))
			{
				bool areChildrenMousedOver = false;
				Panel panel = (Panel)VisualTreeHelper.GetChild(ItemsPresenterElement, 0);
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
			}
		}
	}
}
