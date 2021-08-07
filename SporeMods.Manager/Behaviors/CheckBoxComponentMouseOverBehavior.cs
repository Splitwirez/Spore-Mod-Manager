using Avalonia.Controls.Primitives;
using Avalonia.Xaml.Interactivity;

namespace SporeMods.Manager
{
	public class CheckBoxComponentMouseOverBehavior : Behavior<ToggleButton>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.PointerEnter += (sneder, args) =>
			{
				//(Window.GetWindow(AssociatedObject).Content as ManagerContent).CustomInstallerContentPaneScrollViewer.Content = (AssociatedObject.DataContext as ModComponent).Description;
#if RESTORE_LATER
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

						string imgPath = Path.Combine((configurator.DataContext as ManagedMod).StoragePath, component.Unique + ".png");
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
					configurator.SetBody(elements.ToArray());
				}
#endif
			};
		}
	}
}
