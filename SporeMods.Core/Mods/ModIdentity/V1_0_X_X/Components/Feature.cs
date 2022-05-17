using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public class Feature : FeatureBase
    {
        /// <summary>
		/// Whether this component is enabled.
		/// </summary>
        bool? _isUserEnabled = null;
        public bool? IsUserEnabled
        {
            get => _isUserEnabled;
            set
            {
                _isUserEnabled = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }

        public bool IsEnabled
        {
            get => IsUserEnabled.HasValue ? IsUserEnabled.Value : IsEnabledByDefault;
            set => IsUserEnabled = value;
        }

        protected override void SetIsEnabledByDefault(bool value)
        {
            base.SetIsEnabledByDefault(value);
            NotifyPropertyChanged(nameof(IsEnabled));
        }


        public override bool ShouldApply => IsEnabled;

        public static Feature FromXml(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
            => new Feature(mod, element, fileNames);

        public Feature(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
            : base(mod, element, fileNames)
        {
        }
    }
}
