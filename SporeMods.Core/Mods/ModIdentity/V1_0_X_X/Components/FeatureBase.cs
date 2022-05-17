using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public abstract class FeatureBase : ComponentBase
    {
        bool _isEnabledByDefault = false;
        /// <summary>
		/// Whether this component is enabled by default.
		/// </summary>
		public bool IsEnabledByDefault
        {
            get => _isEnabledByDefault;
            protected set => SetIsEnabledByDefault(value);
        }

        protected virtual void SetIsEnabledByDefault(bool value)
        {
            _isEnabledByDefault = value;
            NotifyPropertyChanged(nameof(IsEnabledByDefault));
        }



        public FeatureBase(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
        {
            var uniqueAttr = element.Attribute("unique");
            if (uniqueAttr != null)
            {
                var displayNameAttr = element.Attribute("displayName");

                Unique = uniqueAttr.Value;
                DisplayName = new FixedModText((displayNameAttr != null) ? displayNameAttr.Value : Unique);
                ObtainFiles(mod, element, fileNames);

                var defaultCheckedAttr = element.Attribute("defaultChecked");
                if ((defaultCheckedAttr != null) && bool.TryParse(defaultCheckedAttr.Value, out bool defaultChecked))
                    IsEnabledByDefault = defaultChecked;

                Mod = mod;
            }
        }

        protected virtual void ObtainFiles(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
        {
            Files = ComponentBase.GetFiles(element);
            EnsureFiles(Files.Keys, fileNames, mod.Unique, mod.DisplayName.ToString());
        }

        public abstract bool ShouldApply { get; }

        public override void Apply(ModTransaction transaction)
        {
            if (ShouldApply)
                base.Apply(transaction);
        }
    }
}
