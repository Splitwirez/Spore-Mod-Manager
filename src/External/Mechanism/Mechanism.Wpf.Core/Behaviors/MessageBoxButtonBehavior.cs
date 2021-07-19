using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;
using Mechanism.Wpf.Core.Windows;

namespace Mechanism.Wpf.Core.Behaviors
{
    public class MessageBoxButtonBehavior : Behavior<Button>
    {
        public MessageBoxContent Content
        {
            get => (MessageBoxContent)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(MessageBoxContent), typeof(MessageBoxButtonBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            /*if (Window.GetWindow(AssociatedObject).Content is MessageBoxContent content)
            {*/
            AssociatedObject.Click += (sneder, args) =>
            {
                Content.EndDialog(AssociatedObject, (MessageBoxAction)AssociatedObject.DataContext);
                //content.ButtonsListView.SelectedIndex = content.EnumStrings.IndexOf((string)(AssociatedObject.Content));
            };
            //}
        }
    }
}
