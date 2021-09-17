using SporeMods.CommonUI;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SporeMods.ViewModels
{
	public class CreditsItem : DependencyObject
	{
		public string Name
		{
			get => (string)GetValue(NameProperty);
			set => SetValue(NameProperty, value);
		}

		public static readonly DependencyProperty NameProperty =
		DependencyProperty.Register(nameof(Name), typeof(string), typeof(CreditsItem), new FrameworkPropertyMetadata(string.Empty));

		public string Contribution
		{
			get => (string)GetValue(ContributionProperty);
			set => SetValue(ContributionProperty, value);
		}

		public static readonly DependencyProperty ContributionProperty =
		DependencyProperty.Register(nameof(Contribution), typeof(string), typeof(CreditsItem), new FrameworkPropertyMetadata(string.Empty));

		public string Link
		{
			get => (string)GetValue(LinkProperty);
			set => SetValue(LinkProperty, value);
		}

		public static readonly DependencyProperty LinkProperty =
		DependencyProperty.Register(nameof(Link), typeof(string), typeof(CreditsItem), new FrameworkPropertyMetadata(string.Empty));

		public bool HasLink
		{
			get => (bool)GetValue(HasLinkProperty);
			set => SetValue(HasLinkProperty, value);
		}

		public static readonly DependencyProperty HasLinkProperty =
		DependencyProperty.Register(nameof(HasLink), typeof(bool), typeof(CreditsItem), new FrameworkPropertyMetadata(false));


		public void OpenLinkCommand(object parameter)
		{
			if (HasLink)
				WineHelper.OpenUrl(Link);

			//MessageBox.Show("OpenLink called!");
		}

		public CreditsItem(string name, string contribution)
		{
			Name = name;
			Contribution = contribution;
		}

		public CreditsItem(string name, string contribution, string link) : this(name, contribution)
		{
			Link = link;

			HasLink = !Link.IsNullOrEmptyOrWhiteSpace();
		}
	}


	public class OpenCreditsLinkBehavior : Behavior<Button>
	{
		protected override void OnAttached()
		{
			base.OnAttached();

			if (AssociatedObject.DataContext is CreditsItem crItem)
			{
				AssociatedObject.Click += (sneder, e) =>
				{
					if (crItem.HasLink)
						WineHelper.OpenUrl(crItem.Link);
				};
			}
		}
	}
}
