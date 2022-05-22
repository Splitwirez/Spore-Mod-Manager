using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public enum RadioGroupDisplayMode
    {
        Standard,
        Grid
    }

    public class RadioGroup : FeatureBase
    {
        RadioGroupDisplayMode _displayMode = RadioGroupDisplayMode.Standard;
        public RadioGroupDisplayMode DisplayMode
        {
            get => _displayMode;
            set
            {
                _displayMode = value;
                NotifyPropertyChanged();
            }
        }


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

        protected RadioGroup(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
            : base(mod, element, fileNames)
        {
            foreach (var subEl in element.Elements())
            {
                RadioGroupOption child = RadioGroupOption.FromXml(this, mod, subEl, fileNames);
                child.PropertyChanged += Child_PropertyChanged;
                Children.Add(child);
            }

            foreach (var child in Children)
            {
                if (child.IsEnabled)
                {
                    if (Selected != null)
                        child.IsEnabled = false;
                    else
                        Selected = child;
                }
            }

            if (Children.All(x =>
            {
                if (x.Description.Count > 1)
                    return !x.Description.OfType<IModText>().All(y => y.ToString().Length > 88);
                else if (x.Description.Count < 1)
                    return false;
                else
                    return x.Description.Any(y => y is DescriptionImage);
            }))
                DisplayMode = RadioGroupDisplayMode.Grid;
        }

        private void Child_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IsEnabled))
                return;

            if (sender is RadioGroupOption newSelection)
            {
                if (!newSelection.IsEnabled)
                    return;

                
                foreach (var child in Children)
                {
                    if (child != newSelection)
                        child.IsEnabled = false;
                }
                //SelectChild(newSelection);

            }
        }

        private void SelectChild(RadioGroupOption newSelection)
        {
            if (newSelection == null)
                return;

            newSelection.IsEnabled = true;
        }

        public static RadioGroup FromXml(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
            => new RadioGroup(mod, element, fileNames);

        protected override void FinishSetup(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
        { }

        public override void Apply(ModTransaction transaction)
            => Selected?.Apply(transaction);

        public override void Purge(ModTransaction transaction)
        {
            foreach (var child in Children)
                child.Purge(transaction);
        }
    }
}
