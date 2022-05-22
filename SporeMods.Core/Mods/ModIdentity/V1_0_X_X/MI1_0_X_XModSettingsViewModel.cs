using SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents;
using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.Mods
{
    public class MI1_0_X_XModSettingsViewModel : NotifyPropertyChangedBase
    {
        IConfigurableMod _mod = null;
        public IConfigurableMod Mod
        {
            get => _mod;
            protected set
            {
                _mod = value;
                NotifyPropertyChanged();
            }
        }


        FeatureBase _highlightedFeature = null;
        public FeatureBase HighlightedFeature
        {
            get => _highlightedFeature;
            set
            {
                _highlightedFeature = value;
                NotifyPropertyChanged();
            }
        }

        object _highlightFeatureCommand = null;
        public object HighlightFeatureCommand
        {
            get => _highlightFeatureCommand;
            set
            {
                _highlightFeatureCommand = value;
                NotifyPropertyChanged();
            }
        }


        public MI1_0_X_XModSettingsViewModel(IConfigurableMod mod)
        {
            Mod = mod;
            HighlightFeatureCommand = Externals.CreateCommand<FeatureBase>(f => HighlightedFeature = f);
        }
    }
}
