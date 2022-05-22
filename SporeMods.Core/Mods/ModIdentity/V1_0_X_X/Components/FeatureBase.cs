using System;
using System.Collections.Generic;
using System.Linq;
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



        /// <summary>
		/// Explanation of the component's purpose and effects to the user.
		/// </summary>
        ThreadSafeObservableCollection<object> _description = new ThreadSafeObservableCollection<object>();
        public ThreadSafeObservableCollection<object> Description
        {
            get => _description;
            protected set
            {
                _description = value;
                NotifyPropertyChanged();
            }
        }


        ImagePlacementType _imagePlacement = ImagePlacementType.None;
        public ImagePlacementType ImagePlacement
        {
            get => _imagePlacement;
            protected set
            {
                _imagePlacement = value;
                NotifyPropertyChanged();
            }
        }



        protected FeatureBase(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
        {
            var uniqueAttr = element.Attribute("unique");
            if (uniqueAttr != null)
            {
                var displayNameAttr = element.Attribute("displayName");

                Unique = uniqueAttr.Value;
                DisplayName = new FixedModText((displayNameAttr != null) ? displayNameAttr.Value : Unique);
                FinishSetup(mod, element, fileNames);

                Mod = mod;
            }
        }

        protected virtual void FinishSetup(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
        {
            Files = ComponentBase.GetFiles(element);
            EnsureFiles(mod, FileNames, fileNames); //, mod.Unique, mod.DisplayName.ToString());

            var defaultCheckedAttr = element.Attribute("defaultChecked");
            if ((defaultCheckedAttr != null) && bool.TryParse(defaultCheckedAttr.Value, out bool defaultChecked))
                IsEnabledByDefault = defaultChecked;

            IModText descriptionText = null;
            var descriptionAttr = element.Attribute("description");
            if (descriptionAttr != null)
                descriptionText = new FixedModText(descriptionAttr.Value);
            bool hasDescText = descriptionText != null;

            ImagePlacementType placement = ImagePlacementType.None;
            var imagePlacementAttr = element.Attribute("imagePlacement");
            if (imagePlacementAttr != null)
                Enum.TryParse<ImagePlacementType>(imagePlacementAttr.Value, out placement);
            ImagePlacement = placement;


            string descImageName = Unique + ".png";
            bool hasDescImage = (placement != ImagePlacementType.None) && fileNames.Any(x => x.Equals(descImageName, StringComparison.OrdinalIgnoreCase));


            DescriptionImage descImage = null;
            if (hasDescImage)
            {
                descImage = new DescriptionImage(mod, descImageName);

                hasDescText = hasDescText && (placement != ImagePlacementType.InsteadOf);

                if ((!hasDescText) || (placement == ImagePlacementType.Before))
                    Description.Add(descImage);
            }

            if (hasDescText)
                Description.Add(descriptionText);

            if (hasDescImage && (placement == ImagePlacementType.After))
                Description.Add(descImage);
        }

        public abstract bool ShouldApply { get; }

        public override void Apply(ModTransaction transaction)
        {
            if (ShouldApply)
                base.Apply(transaction);
        }
    }

    public class DescriptionImage : NotifyPropertyChangedBase
    {
        public string FileName { get; protected set; } = string.Empty;
        public object Image { get; protected set; } = null;
        public DescriptionImage(MI1_0_X_XMod mod, string fileName)
        {
            FileName = fileName;
            Image = Externals.CreateBitmapImage(mod.GetImageStream(fileName));
        }

        //FileName
    }
}
