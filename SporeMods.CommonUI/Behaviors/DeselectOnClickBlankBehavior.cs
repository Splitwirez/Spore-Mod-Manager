using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace SporeMods.CommonUI
{
    public class DeselectOnClickBlankBehavior : Behavior<FrameworkElement>
	{
		public ItemsPresenter ItemsPresenterElement
		{
			get => (ItemsPresenter)GetValue(ItemsPresenterElementProperty);
			set => SetValue(ItemsPresenterElementProperty, value);
		}

		public static readonly DependencyProperty ItemsPresenterElementProperty =
			DependencyProperty.Register(nameof(ItemsPresenterElement), typeof(ItemsPresenter), typeof(DeselectOnClickBlankBehavior), new PropertyMetadata(null));

		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
		}

		private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if ((AssociatedObject != null) && (AssociatedObject.IsMouseOver))
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
					(AssociatedObject.TemplatedParent as ListBox).SelectedItem = null;
					e.Handled = true;
				}
			}
		}
	}
}
