using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mechanism.Wpf.Core.Windows
{
    public static class MessageBoxButtons
    {
        public enum OkButton
        {
            OK
        }

        public enum OkCancelButtons
        {
            OK,
            Cancel
        }

        public enum IgnoreRetryAbortButtons
        {
            Ignore,
            Retry,
            Abort
        }

        public enum YesNoButtons
        {
            Yes,
            No
        }
    }

    public class OkActionSet : IMessageBoxActionSet
    {
        public string GetDisplayName(object value)
        {
            if (value is string valueString)
                return valueString;
            else
                return value.ToString();
        }

        public IEnumerable<object> Actions
        {
            get
            {
                object[] objects = new object[Enum.GetNames(typeof(MessageBoxButtons.OkButton)).Count()];
                Enum.GetValues(typeof(MessageBoxButtons.OkButton)).CopyTo(objects, 0);
                return objects.ToList();
            }
        }
    }

    public class OkCancelActionSet : IMessageBoxActionSet
    {
        public string GetDisplayName(object value)
        {
            if (value is string valueString)
                return valueString;
            else
                return value.ToString();
        }

        public IEnumerable<object> Actions
        {
            get
            {
                object[] objects = new object[Enum.GetNames(typeof(MessageBoxButtons.OkCancelButtons)).Count()];
                Enum.GetValues(typeof(MessageBoxButtons.OkCancelButtons)).CopyTo(objects, 0);
                return objects.ToList();
            }
        }
    }

    public class IgnoreRetryAbortActionSet : IMessageBoxActionSet
    {
        public string GetDisplayName(object value)
        {
            if (value is string valueString)
                return valueString;
            else
                return value.ToString();
        }

        public IEnumerable<object> Actions
        {
            get
            {
                object[] objects = new object[Enum.GetNames(typeof(MessageBoxButtons.IgnoreRetryAbortButtons)).Count()];
                Enum.GetValues(typeof(MessageBoxButtons.IgnoreRetryAbortButtons)).CopyTo(objects, 0);
                return objects.ToList();
            }
        }
    }

    public class YesNoActionSet : IMessageBoxActionSet
    {
        public string GetDisplayName(object value)
        {
            if (value is string valueString)
                return valueString;
            else
                return value.ToString();
        }

        public IEnumerable<object> Actions
        {
            get
            {
                object[] objects = new object[Enum.GetNames(typeof(MessageBoxButtons.YesNoButtons)).Count()];
                Enum.GetValues(typeof(MessageBoxButtons.YesNoButtons)).CopyTo(objects, 0);
                return objects.ToList();
            }
        }
    }

    public static class MessageBox
    {
        public static MessageBoxButtons.OkButton Show(string text, string caption)
        {
            return (MessageBoxButtons.OkButton)(MessageBox<OkActionSet>.Show(text, caption));
        }
    }

    public interface IMessageBoxActionSet
    {
        IEnumerable<object> Actions { get; }

        string GetDisplayName(object value);
    }

    public class MessageBoxAction : DependencyObject
    {
        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(MessageBoxAction), new PropertyMetadata(string.Empty));

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(MessageBoxAction), new PropertyMetadata(null));

        public MessageBoxAction(object value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public static class MessageBox<TActionSet> where TActionSet : IMessageBoxActionSet, new()
    {
        public static object Show(string text, string caption)
        {
            return Show(text, caption, null);
        }

        public static object Show(string text, string caption, FrameworkElement icon)
        {
            MessageBoxContent content = new MessageBoxContent(new TActionSet(), text, icon);
            DecoratableWindow window = new DecoratableWindow()
            {
                Title = caption,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight
            };

            window.Closing += (sender, args) =>
            {
                if (!content.CanClose)
                    args.Cancel = true;
            };

            window.SetResourceReference(DecoratableWindow.StyleProperty, "MessageBoxWindowStyle");

            object value = null;

            content.ResultButtonClicked += (sneder, args) =>
            {
                var arg = args as MessageBoxEventArgs;
                value = arg.Result;
            };

            window.Content = content;

            window.ShowDialog();

            return value;
        }
    }
}