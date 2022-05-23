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
        List<ModFile> _files = new List<ModFile>();
        public List<ModFile> Files
        {
            get => _files;
            protected set
            {
                _files = value;
                _fileNames = _files.ConvertAll<string>(x => x.FileName);
            }
        }
        IEnumerable<string> _fileNames = new List<string>();
        public IEnumerable<string> FileNames
        {
            get => _fileNames;
        }
        //public IEnumerable<string> FileNames => Files.ConvertAll<string>(x => x.FileName);
        
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

        public bool Is1_0_1_X => !(Mod is MI1_0_0_0Mod);
        public bool UsesLegacyDLLs => !Is1_0_1_X;


        public virtual void Apply(ModTransaction transaction)
        {
            string recordDirPath = Path.Combine(Settings.ModConfigsPath, Mod.RecordDirName);
            foreach (var file in Files)
            {
                string name = Path.GetFileName(file.FileName);
                Cmd.WriteLine($"PRINTING {name}");
                string sourcePath = Path.Combine(recordDirPath, name);
                string destPath = FileWrite.GetFileOutputPath(file.Dir, name, UsesLegacyDLLs);
                transaction.Operation(new CopyFileOp(sourcePath, destPath));
            }
        }

        public virtual void Purge(ModTransaction transaction)
        {
            string recordDirPath = Path.Combine(Settings.ModConfigsPath, Mod.RecordDirName);
            foreach (var file in Files)
            {
                string name = Path.GetFileName(file.FileName);
                string destPath = FileWrite.GetFileOutputPath(file.Dir, name, UsesLegacyDLLs);
                transaction.Operation(new DeleteFileOp(destPath));
            }
        }


        public static List<ModFile> GetFiles(XElement element)
            => GetFiles(
                (string.IsNullOrEmpty(element.Value) || string.IsNullOrWhiteSpace(element.Value))
                    ? new string[0]
                    : element.Value.Split('?')
                , "game", element);
            /*=> (string.IsNullOrEmpty(element.Value) || string.IsNullOrWhiteSpace(element.Value))
                ? new Dictionary<string, ComponentGameDir>()
                : GetFiles(element.Value.Split('?'), "game", element);*/
        public static List<ModFile> GetFiles(string[] fileNames, string fileGamesAttrName, XElement element)
        {
            var ret = new List<ModFile>();
            if (fileNames.Length == 0)
                return ret;

            
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
                ret.Add(new ModFile(fileNames[i],
                    fileGames != null
                        ? ParseGameDir(fileGames[i])
                        : ComponentGameDir.ModAPI)
                    );
            }

            return ret;
        }

        public static void EnsureFiles(MI1_0_X_XMod mod, IEnumerable<string> componentFileNames, IEnumerable<string> fileNamesInMod/*, string modUnique, string modDisplayName*/, string cUnique = null, string cDisplayName = null)
        {
            string modUnique = mod.Unique;
            string modDisplayName = (mod.DisplayName != null) ? mod.DisplayName.ToString() : mod.Unique;

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

                if (mod.IsIncoming)
                    throw new Exception(error);
                else
                    Cmd.WriteLine(error);
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
    public sealed class ModFile
    {
        public readonly string FileName = null;
        public readonly ComponentGameDir Dir = ComponentGameDir.ModAPI;

        public ModFile(string fileName, ComponentGameDir dir)
        {
            FileName = fileName;
            Dir = dir;
        }
    }
}
