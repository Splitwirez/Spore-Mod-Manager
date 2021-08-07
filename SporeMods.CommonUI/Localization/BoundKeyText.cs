using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
//using Avalonia.Xaml.Behaviors;
//using System.Windows.Media.Imaging;

namespace SporeMods.CommonUI.Localization
{
	public class BoundKeyText : AvaloniaObject //Behavior<TextBlock>
	{
        public static readonly AttachedProperty<string> KeyProperty =
            AvaloniaProperty.RegisterAttached<BoundKeyText, TextBlock, string>("Key");
        

        public static string GetKey(Control control) =>
            control.GetValue(KeyProperty);
        
        public static void SetKey(Control control, string value) =>
            control.SetValue(KeyProperty, value);
        
        static void OnKeyPropertyChanged(Control sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is string key)
                sender[!TextBlock.TextProperty] = new DynamicResourceExtension(key);
        }

        public string Key
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        static BoundKeyText()
        {
            KeyProperty.Changed.AddClassHandler<TextBlock>(OnKeyPropertyChanged);
            KeyProperty.Changed.AddClassHandler<TextBox>(OnKeyPropertyChanged);
        }

        /*void RefreshText(string key)
        {
            if ((AssociatedObject != null) && (key != null))
            {
                AssociatedObject[!TextBlock.TextProperty] = new DynamicResourceExtension(key);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            RefreshText(Key);
        }*/
    }
}
