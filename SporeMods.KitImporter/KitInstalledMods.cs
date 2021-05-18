using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml.Linq;
using SporeMods.Core.Mods;
using Path = System.IO.Path;

namespace SporeMods.KitImporter
{
	public class KitMod
	{
		public string Name { get; set; } = null;
		public string Unique { get; set; } = null;
		public string DisplayName { get; set; } = null;
		public string ConfiguratorPath { get; set; } = null;
		public List<ModFile> Files { get; set; } = new List<ModFile>();
	}

	/// <summary>
	/// Used to read the list of mods installed in the old launcher kit.
	/// </summary>
	public class KitInstalledMods
	{
		public static event EventHandler<ImportFailureEventArgs> ModImportFailed;

		public List<KitMod> Mods = new List<KitMod>();

		public KitInstalledMods(string path)
		{
			var document = XDocument.Load(path);
			foreach (var modElem in document.Root.Elements())
			{
				if (modElem.Name.LocalName.ToLowerInvariant() != "mod") continue;

				var mod = new KitMod();

				try
				{

					var attr = modElem.Attribute("name");
					if (attr != null)
						mod.Name = attr.Value;

					string fallbackName = mod.Name;
					if (fallbackName.EndsWith(".package", StringComparison.OrdinalIgnoreCase) || fallbackName.EndsWith(".sporemod", StringComparison.OrdinalIgnoreCase))
						fallbackName = Path.GetFileNameWithoutExtension(fallbackName);

					attr = modElem.Attribute("displayName");
					if (attr != null)
						mod.DisplayName = attr.Value;
					else
						mod.DisplayName = fallbackName;

					attr = modElem.Attribute("configurator");
					if (attr != null)
						mod.ConfiguratorPath = attr.Value;

					attr = modElem.Attribute("unique");
					if (attr != null)
						mod.Unique = attr.Value;
					else
						mod.Unique = fallbackName;
					
					/*if (mod.Name != null)
					{
						if ((mod.Name.Length >= 8) && mod.Name.EndsWith(".package", StringComparison.OrdinalIgnoreCase))
						{
							mod.Unique = mod.Name.Substring(0, mod.Name.Length - 8);
						}
						else if ((mod.Name.Length >= 9) && mod.Name.EndsWith(".sporemod", StringComparison.OrdinalIgnoreCase))
						{
							mod.Unique = mod.Name.Substring(0, mod.Name.Length - 9);
						}
						else
							mod.Unique = mod.Name;
					}*/

					foreach (var fileElem in modElem.Elements())
					{
						if (fileElem.Name.LocalName.ToLowerInvariant() != "file") continue;

						string value = fileElem.Value;
						string[] values = new string[]
						{
						value
						};

						if (value.Contains('?'))
						{
							values = value.Split('?');
						}

						string gameValue = "modapi";
						if (fileElem.Attribute("game") != null)
						{
							gameValue = fileElem.Attribute("game").Value;
						}
						string[] gameValues = new string[]
						{
						gameValue
						};

						if (gameValue.Contains('?'))
						{
							gameValues = gameValue.Split('?');
						}



						for (int i = 0; i < values.Length; i++)
						{
							var val = values[i];
							var file = new ModFile();
							//string[] value = new 
							file.Name = val;
							file.GameDir = ComponentGameDir.ModAPI;

							/*attr = fileElem.Attribute("game");
							if (attr != null)
							{*/
							switch (gameValues[i].ToLowerInvariant())
							{
								case "galacticadventures":
									file.GameDir = ComponentGameDir.GalacticAdventures;
									break;
								case "spore":
									file.GameDir = ComponentGameDir.Spore;
									break;
								case "modapi":
								default:
									file.GameDir = ComponentGameDir.ModAPI;
									break;
							}
							//}

							mod.Files.Add(file);
						}
					}
					Mods.Add(mod);
				}
				catch (Exception ex)
				{
					ModImportFailed?.Invoke(mod, new ImportFailureEventArgs(ex, mod));
				}
			}
		}
	}
}
