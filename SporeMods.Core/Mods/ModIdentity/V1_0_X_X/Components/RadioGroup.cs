using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public class RadioGroup : FeatureBase
    {
        RadioGroupOption _selected = null;
        public RadioGroupOption Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                NotifyPropertyChanged();
            }
        }

        public override bool ShouldApply => Selected != null;

        public RadioGroup(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
            : base(mod, element, fileNames)
        {
            foreach (var subEl in element.Elements())
            {
                Children.Add(new RadioGroupOption(mod, subEl, fileNames));
            }
            
            Selected = Children.FirstOrDefault(x => x.IsEnabledByDefault);
        }

        protected override void ObtainFiles(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
        { }
    }
}
