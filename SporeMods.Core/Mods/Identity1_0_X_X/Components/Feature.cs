using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public class Feature : FeatureBase
    {
        protected override void SetIsEnabledByDefault(bool value)
        {
            base.SetIsEnabledByDefault(value);
            NotifyPropertyChanged(nameof(IsEnabled));
        }


        public override bool ShouldApply => IsEnabled;

        public static Feature FromXml(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
            => new Feature(mod, element, fileNames);

        protected Feature(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
            : base(mod, element, fileNames)
        { }
    }
}
