using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public class CompatFile : ComponentBase
    {
        public Dictionary<string, ComponentGameDir> CompatTargetFiles { get; protected set; } = new Dictionary<string, ComponentGameDir>();

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
                EnsureFiles(ret.Files.Keys, fileNames, mod.Unique, mod.DisplayName.ToString());
                var removeTargetsAttr = element.Attribute("removeTargetsAttr");
                if ((removeTargetsAttr != null) && bool.TryParse(removeTargetsAttr.Value, out bool removeTargets))
                    ret.RemoveTargets = removeTargets;

                return ret;
            }
            else
                return null;
        }
    }
}
