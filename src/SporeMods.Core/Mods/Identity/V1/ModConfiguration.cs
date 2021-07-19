using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Mods.Identity.V1
{
	/// <summary>
	/// Represents the current configuration of a managed mod: what components are installed, the tags,...
	/// </summary>
	public class ModConfiguration
	{
		public ModConfiguration(ManagedMod parent)
		{
			Parent = parent;
		}

		/// <summary>
		/// The ManagedMod that this configuration represents.
		/// </summary>
		public ManagedMod Parent { get; }

		/// <summary>
		/// The 'unique' identifiers of the components that are currently enabled
		/// </summary>
		public Dictionary<string, bool> UserSetComponents { get; } = new Dictionary<string, bool>();

		/// <summary>
		/// Tags used to search and classify this mod
		/// </summary>
		public List<string> Tags { get; } = new List<string>();

		/// <summary>
		/// Whether this mod is enabled (true) or disabled (false)
		/// </summary>
		public bool IsEnabled { get; set; }

		string _prevPath = string.Empty;
		public void Load(string path)
		{
			var document = XDocument.Load(path);
			
			var element = document.Root.Element("tags");
			if (element != null)
			{
				Tags.AddRange(element.Elements().Select(x => x.Value));
			}

			element = document.Root.Element("components");
			if (element != null)
			{
				foreach (XElement subEl in element.Elements())
				{
					var attr = subEl.Attribute("isEnabled");
					if ((attr != null) && bool.TryParse(attr.Value, out bool isEnabled))
						UserSetComponents[subEl.Value] = isEnabled;
					/*else
						UserSetComponents.Add(subEl.Value, false);*/
				}
				//EnabledComponents.AddRange(element.Elements().Select(x => x.Value));
			}

			element = document.Root.Element("isEnabled");
			if (element != null)
			{
				if (bool.TryParse(element.Value, out bool value))
				{
					IsEnabled = value;
				} 
				else
				{
					IsEnabled = true;
				}
			}

			_prevPath = path;
		}

		public void Reload()
		{
			if ((!string.IsNullOrEmpty(_prevPath)) && (!string.IsNullOrWhiteSpace(_prevPath)))
			{
				Load(_prevPath);
				ConfigurationReset?.Invoke(Parent, null);
			}
		}

		public void Save(string path)
		{
			var rootElement = new XElement("config");

			var element = new XElement("tags");
			element.Add(Tags.Select(x => new XElement("tag", x)));
			rootElement.Add(element);

			element = new XElement("components");
			foreach (string key in UserSetComponents.Keys)
			{
				XElement cmpEl = new XElement("component", key);
				cmpEl.SetAttributeValue("isEnabled", UserSetComponents[key]);
				element.Add(cmpEl);
			}
			//element.Add(EnabledComponents.Select(x => new XElement("component", x)));
			rootElement.Add(element);

			element = new XElement("isEnabled");
			element.SetValue(IsEnabled.ToString());
			rootElement.Add(element);

			var document = new XDocument(rootElement);
			document.Save(path);
		}

		/*public bool IsComponentEnabled(BaseModComponent component)
		{
			return EnabledComponents.Contains(component.Unique);
		}*/

		internal static event EventHandler ConfigurationReset;
	}
}
