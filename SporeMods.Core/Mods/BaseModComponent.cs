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
        public BaseModComponent(ModIdentity identity, string uniqueTag)
        {
            Unique = uniqueTag;
            Identity = identity;
        }

        /// <summary>
        /// The mod identity in which this component is contained.
        /// </summary>
        public ModIdentity Identity { get; }

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

        /// <summary>
        /// Whether this component is enabled by default.
        /// </summary>
        public bool EnabledByDefault { get; set; } = false;

        /// <summary>
        /// Whether the sub components of this component are exclusive.
        /// </summary>
        public bool IsGroup { get; set; }

        public List<ModFile> Files { get; } = new List<ModFile>();

        public bool IsInGroup { get
            {
                return Parent != null && Parent.IsGroup;
            } }

        public bool IsEnabled
        {
            get
            {
                if (Identity.ParentMod.Configuration.UserSetComponents.TryGetValue(Unique, out bool isEnabled))
                    return isEnabled;
                else
                    return EnabledByDefault;
                //return Identity.ParentMod.Configuration.IsComponentEnabled(this);
            }
            set
            {
                if (Identity.ParentMod.Configuration.UserSetComponents.ContainsKey(Unique))
                    Identity.ParentMod.Configuration.UserSetComponents.Remove(Unique);

                Identity.ParentMod.Configuration.UserSetComponents.Add(Unique, value);
                /*if (value)
                    Identity.ParentMod.Configuration.EnabledComponents.Add(Unique);
                else
                    Identity.ParentMod.Configuration.EnabledComponents.Remove(Unique);*/
            }
        }
    }
}
