using SporeMods.Core;
using SporeMods.Core.ModIdentity;
using SporeMods.Manager.Configurators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using System.Windows.Media.Imaging;

namespace SporeMods.Manager
{
    public class CheckBoxComponentMouseOverBehavior : Behavior<ToggleButton>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += (sneder, args) =>
            {
                //(Window.GetWindow(AssociatedObject).Content as ManagerContent).CustomInstallerContentPaneScrollViewer.Content = (AssociatedObject.DataContext as ModComponent).Description;
                var content = (Window.GetWindow(AssociatedObject).Content as ManagerContent);

                if (content.ConfiguratorBodyContentControl.Content is ModConfiguratorV1_0_0_0 configurator)
                {
                    var component = (AssociatedObject.DataContext as ModComponent);
                    List<UIElement> elements = new List<UIElement>()
                    {
                        new TextBlock()
                        {
                            Text = component.Description
                        }
                    };

                    if (!component.ImagePlacement.Equals("none", StringComparison.OrdinalIgnoreCase))
                    {

                        string imgPath = Path.Combine((configurator.DataContext as ModConfiguration).GetStoragePath(), component.Unique + ".png");
                        if (File.Exists(imgPath))
                        {
                            var image = new Image()
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Stretch = Stretch.Uniform,
                                Source = new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute)),
                                IsHitTestVisible = false
                            };
                            if (component.ImagePlacement.Equals("before", StringComparison.OrdinalIgnoreCase))
                            {
                                elements.Insert(0, image);
                            }
                            else if (component.ImagePlacement.Equals("after", StringComparison.OrdinalIgnoreCase))
                            {
                                elements.Add(image);
                            }
                            else if (component.ImagePlacement.Equals("insteadof", StringComparison.OrdinalIgnoreCase))
                            {
                                elements.Clear();
                                elements.Add(image);
                            }
                        }
                    }
                    configurator.SetBody(elements.ToArray());
                }
            };
        }
    }
}
