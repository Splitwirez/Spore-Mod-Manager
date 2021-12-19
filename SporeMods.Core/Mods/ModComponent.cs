using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{ 
	public class ModComponent : BaseModComponent
	{
		public ModComponent(ModIdentity identity, string uniqueTag)
			: base(identity, uniqueTag)
		{
			/*Console.WriteLine($"Unique: '{Unique}'");
			Console.WriteLine($"Has identity: {Identity != null}");
			if (Identity != null)
			{
				Console.WriteLine($"\tHas ParentMod: '{Identity.ParentMod != null}'");
			}*/


			string imageName = $"{Unique}.png";
			//Console.WriteLine($"image name: {imageName}");

			if ((Identity != null) && Identity.TryGetImage(imageName, out System.Drawing.Image image))
				Image = image;
			/*else
				Console.WriteLine($"{Unique} image = nope");*/

			
			ModConfiguration.ConfigurationReset += (sender, e) =>
			{
				if ((sender is ManagedMod mod) && (mod == Identity.ParentMod))
				{
					IsEnabled = GetIsEnabled();
				}
			};
		}

		protected override bool GetIsEnabled()
		{
			if (Identity.ParentMod.Configuration.UserSetComponents.TryGetValue(Unique, out bool isEnabled))
				return isEnabled;
			else
				return EnabledByDefault;
		}


		protected override void SetIsEnabled(bool value)
		{
			if (Identity.ParentMod.Configuration.UserSetComponents.ContainsKey(Unique))
				Identity.ParentMod.Configuration.UserSetComponents.Remove(Unique);

			Identity.ParentMod.Configuration.UserSetComponents.Add(Unique, value);

			RaiseIsEnabledChanged();
		}

		/// <summary>
		/// Controls the position of an image associated with the component's description. 
		/// Specify an image by including it in your .sporemod with a filename matching the component's unique. 
		/// If the image does not fit into the window, autoscrolling will be used to allow the user to see it in its entirety.
		/// </summary>
		public ImagePlacementType ImagePlacement { get; set; } = ImagePlacementType.None;

		public System.Drawing.Image Image { get; set; } = null;
	}
}
