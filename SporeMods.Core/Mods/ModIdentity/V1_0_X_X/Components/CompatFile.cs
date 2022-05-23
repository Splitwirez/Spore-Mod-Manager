using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public class CompatFile : ComponentBase
    {
        //public List<ModFile> CompatTargetFiles { get; protected set; } = new List<ModFile>();
        List<ModFile> _compatTargetFiles = new List<ModFile>();
        public List<ModFile> CompatTargetFiles
        {
            get => _compatTargetFiles;
            protected set
            {
                _compatTargetFiles = value;
                _compatTargetFileNames = _compatTargetFiles.ConvertAll<string>(x => x.FileName);
            }
        }
        IEnumerable<string> _compatTargetFileNames = new List<string>();
        public IEnumerable<string> CompatTargetFileNames
        {
            get => _compatTargetFileNames;
        }


        bool _removeTargets = false;
        public bool RemoveTargets
        {
            get => _removeTargets;
            set
            {
                _removeTargets = value;
                NotifyPropertyChanged();
            }
        }

        public static CompatFile FromXml(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
        {
            
            var targetFileNameAttr = element.Attribute("compatTargetFileName");
            if (targetFileNameAttr != null)
            {
                var ret = new CompatFile()
                {
                    Files = ComponentBase.GetFiles(element),
                    CompatTargetFiles = ComponentBase.GetFiles(targetFileNameAttr.Value.Split('?'), "compatTargetGame", element),
                    Mod = mod
                };
                EnsureFiles(mod, ret.FileNames, fileNames); //, mod.Unique, mod.DisplayName.ToString());
                var removeTargetsAttr = element.Attribute("removeTargetsAttr");
                if ((removeTargetsAttr != null) && bool.TryParse(removeTargetsAttr.Value, out bool removeTargets))
                    ret.RemoveTargets = removeTargets;

                return ret;
            }
            else
                return null;
        }

        public override void Apply(ModTransaction transaction)
        {
            List<string> targetPaths = new List<string>();
            foreach (var file in CompatTargetFiles)
            {
                string targetPath = FileWrite.GetFileOutputPath(file.Dir, file.FileName, UsesLegacyDLLs);
                if (File.Exists(targetPath))
                    targetPaths.Add(targetPath);
                else
                    return;
            }
            
            if (RemoveTargets)
            {
                foreach (string targetPath in targetPaths)
                {
                    transaction.Operation(new DeleteFileOp(targetPath));
                }
            }
            
            base.Apply(transaction);
        }
    }
}
