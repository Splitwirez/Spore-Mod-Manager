using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public abstract class ComponentBase : NotifyPropertyChangedBase
    {
        public Dictionary<string, ComponentGameDir> Files { get; protected set; }  = new Dictionary<string, ComponentGameDir>();
        
        ThreadSafeObservableCollection<RadioGroupOption> _children = new ThreadSafeObservableCollection<RadioGroupOption>();
        public ThreadSafeObservableCollection<RadioGroupOption> Children
        {
            get => _children;
            protected set
            {
                _children = value;
                NotifyPropertyChanged();
            }
        }


        /// <summary>
		/// The mod in which this component is contained.
		/// </summary>
        MI1_0_X_XMod _mod = null;
        public MI1_0_X_XMod Mod
        {
            get => _mod;
            protected set => _mod = value;
        }

        /// <summary>
		/// Name of component used internally to track which features are enabled during reconfiguration and upgrades
		/// </summary>
        string _unique = string.Empty;
        public string Unique
        {
            get => _unique;
            protected set
            {
                _unique = value;
                _fallbackDisplayName = new FixedModText(_unique);
                NotifyPropertyChanged();
            }
        }


        /// <summary>
		/// Name of component shown to the user in the mod's installer
		/// </summary>
        IModText _fallbackDisplayName = new FixedModText(string.Empty);
        IModText _displayName = null;
        public IModText DisplayName
        {
            get => (_displayName != null) ? _displayName : _fallbackDisplayName;
            protected set
            {
                _displayName = value;
                NotifyPropertyChanged();
            }
        }


        /// <summary>
		/// Explanation of the component's purpose and effects to the user.
		/// </summary>
        IEnumerable<IModText> _description = new ThreadSafeObservableCollection<IModText>();
        public IEnumerable<IModText> Description
        {
            get => _description;
            protected set
            {
                _description = value;
                NotifyPropertyChanged();
            }
        }

        

        public virtual void Apply(ModTransaction transaction)
        {
            string recordDirPath = Path.Combine(Settings.ModConfigsPath, Mod.RecordDirName);
            foreach (string key in Files.Keys)
            {
                string name = Path.GetFileName(key);
                Cmd.WriteLine($"PRINTING {name}");
                string sourcePath = Path.Combine(recordDirPath, name);
                string destPath = FileWrite.GetFileOutputPath(Files[key], name, Mod is MI1_0_0_0Mod);
                transaction.Operation(new CopyFileOp(sourcePath, destPath));
            }
        }


        public static Dictionary<string, ComponentGameDir> GetFiles(XElement element)
            => GetFiles(element.Value.Split('?'), "game", element);
        public static Dictionary<string, ComponentGameDir> GetFiles(string[] fileNames, string fileGamesAttrName, XElement element)
        {
            var ret = new Dictionary<string, ComponentGameDir>();

            
            string[] fileGames = null;

            var gameAttr = element.Attribute(fileGamesAttrName);
            if (gameAttr != null)
            {
                fileGames = gameAttr.Value.Split('?');
                if (fileGames.Length < fileNames.Length)
                    throw new FormatException($"The '{fileGamesAttrName}' attribute must have at least as many elements as files in the component");
            }


            for (int i = 0; i < fileNames.Length; i++)
            {
                ret.Add(fileNames[i],
                    fileGames != null
                        ? ParseGameDir(fileGames[i])
                        : ComponentGameDir.ModAPI);
            }

            return ret;
        }

        public static void EnsureFiles(IEnumerable<string> componentFileNames, IEnumerable<string> fileNamesInMod, string modUnique, string modDisplayName, string cUnique = null, string cDisplayName = null)
        {
            string missing = null;

            foreach (string cName in componentFileNames)
            {
                if (!fileNamesInMod.Any(x => x.Equals(cName, StringComparison.OrdinalIgnoreCase)))
                    missing = cName;
            }

            if (missing == null)
                return;
            else
            {
                string error = $"File '{missing}' was missing for ";
                if (cUnique != null)
                {
                    string cDisp = cDisplayName != null ? cDisplayName : cUnique;
                    error += $"component '{cDisp}' ({cUnique}) of ";
                }
                error += $"mod '{modDisplayName}' ({modUnique})";

                throw new Exception(error);
            }
        }
        static ComponentGameDir ParseGameDir(string inVal)
        {
            if (Enum.TryParse<ComponentGameDir>(inVal, out ComponentGameDir result))
            {
                if (result == ComponentGameDir.Tweak)
                    return ComponentGameDir.ModAPI;
                else
                    return result;
            }

            return ComponentGameDir.ModAPI;
        }
    }
}
