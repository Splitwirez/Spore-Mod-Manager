#if REMOVED
using SporeMods.Core;
using SporeMods.Core.Mods;
using SporeMods.CommonUI.Configurators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using System.Windows.Media.Imaging;

namespace SporeMods.CommonUI
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

					if (component.ImagePlacement != ImagePlacementType.None)
					{
						if ((configurator.DataContext != null) && (configurator.DataContext is ManagedMod mod) && (!(string.IsNullOrEmpty(mod.StoragePath) || string.IsNullOrWhiteSpace(mod.StoragePath))))
						{
							string imgPath = Path.Combine(mod.StoragePath, component.Unique + ".png");
							if (File.Exists(imgPath))
							{
								Image image = new Image()
								{
									HorizontalAlignment = HorizontalAlignment.Stretch,
									Stretch = Stretch.Uniform,
									IsHitTestVisible = false
								};

								MemoryStream mStream = new MemoryStream();
								using (FileStream fStream = new FileStream(imgPath, FileMode.Open, FileAccess.Read))
								{
									fStream.Seek(0, SeekOrigin.Begin);
									fStream.CopyTo(mStream);
								}
								mStream.Seek(0, SeekOrigin.Begin);

								image.Source = BitmapFrame.Create(mStream); //new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute));

								if (component.ImagePlacement == ImagePlacementType.Before)
								{
									elements.Insert(0, image);
								}
								else if (component.ImagePlacement == ImagePlacementType.After)
								{
									elements.Add(image);
								}
								else if (component.ImagePlacement == ImagePlacementType.InsteadOf)
								{
									elements.Clear();
									elements.Add(image);
								}
							}
						}
						else if (configurator.DataContext == null)
							Debug.WriteLine("configurator.DataContext == null");
						else
							Debug.WriteLine($"configurator DataContext is a '{configurator.DataContext.GetType().FullName}'.");
					}
					configurator.SetBody(elements.ToArray());
				}
			};
		}
	}
}
#endif
