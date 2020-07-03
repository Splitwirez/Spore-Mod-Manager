using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SporeMods.Manager
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

        public CreditsItem(string name, string contribution)
        {
            Name = name;
            Contribution = contribution;
        }

        public CreditsItem(string name, string contribution, string link)
        {
            Name = name;
            Contribution = contribution;
            Link = link;
        }
    }
}
