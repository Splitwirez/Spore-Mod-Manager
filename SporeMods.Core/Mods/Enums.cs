using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SporeMods.Core.Mods
{
    public enum ImagePlacementType
    {
        /// <summary>
        /// Does not display an image at all
        /// </summary>
        None,
        /// <summary>
        /// Displays the image above the description text
        /// </summary>
        Before,
        /// <summary>
        /// Displays the image below the description text
        /// </summary>
        After,
        /// <summary>
        /// Displays only the image, with no text visible at all
        /// </summary>
        InsteadOf
    }

    public enum ComponentGameDir
    {
        ModAPI,
        GalacticAdventures,
        Spore,
        Tweak
    }

    /// <summary>
    /// Represents a file of a mod, which consists of the file name and the directory it belongs to.
    /// </summary>
    public class ModFile
    {
        public string Name;
        public ComponentGameDir GameDir;
    }

    /// <summary>
    /// This class is used to ensure compatibility between mods: adds or removes files depending on the existance of other files.
    /// </summary>
    public class ModCompatibilityFix
    {
        //TODO possibly add 'exclude' files too?

        /// <summary>
        /// The files that must exist for this compatibility fix to be applied.
        /// </summary>
        public List<ModFile> RequiredFiles = new List<ModFile>();

        /// <summary>
        /// The files that must be added when applying this compatibility fix.
        /// </summary>
        public List<ModFile> FilesToAdd = new List<ModFile>();

        /// <summary>
        /// The files that must be removed when applying this compatibility fix.
        /// </summary>
        public List<ModFile> FilesToRemove = new List<ModFile>();
    }
}
