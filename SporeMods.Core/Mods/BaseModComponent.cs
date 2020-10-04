using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SporeMods.Core.Mods
{
    /// <summary>
    /// An abstract representation of any of the components or parts that make up a mod, including the mod itself.
    /// A mod component is uniquely identified by its 'Unique' property. Apart from that, it also has a display name 
    /// and a description. Components are defined hierarchically, through its 'Parent'  and 'SubComponents' properties.
    /// The class also has a list of files that are used by the component.
    /// </summary>
    public abstract class BaseModComponent
    {
        public BaseModComponent(string uniqueTag)
        {
            Unique = uniqueTag;
        }

        /// <summary>
        /// Name of component used internally to track which features are enabled during reconfiguration and upgrades
        /// </summary>
        public string Unique { get; set; }

        /// <summary>
        /// Name of component shown to the user in the mod's installer
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Explanation of the component's purpose and effects to the user. If the description does not fit into the window, 
        /// autoscrolling will be used to allow the user to see it in its entirety.
        /// </summary>
        public string Description { get; set; }

        public BaseModComponent Parent { get; set; }

        public List<BaseModComponent> SubComponents { get; } = new List<BaseModComponent>();

        public bool IsGroup { get { return SubComponents.Any(); } }

        public List<ModFile> Files { get; } = new List<ModFile>();
    }
}
