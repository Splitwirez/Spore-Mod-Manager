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
        public ModComponent(string uniqueTag)
            : base(uniqueTag)
        {
        }

        /// <summary>
        /// Controls the position of an image associated with the component's description. 
        /// Specify an image by including it in your .sporemod with a filename matching the component's unique. 
        /// If the image does not fit into the window, autoscrolling will be used to allow the user to see it in its entirety.
        /// </summary>
        public ImagePlacementType ImagePlacement { get; set; } = ImagePlacementType.None;

        /// <summary>
        /// Whether this component is enabled by default.
        /// </summary>
        public bool EnabledByDefault { get; set; }
    }
}
