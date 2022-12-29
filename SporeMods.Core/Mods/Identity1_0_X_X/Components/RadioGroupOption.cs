﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents
{
    public class RadioGroupOption : FeatureBase
    {

        public RadioGroup Parent
        {
            get;
            protected set;
        }

        protected RadioGroupOption(RadioGroup parent, MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
            : base(mod, element, fileNames)
        {
            Parent = parent;
        }

        public static RadioGroupOption FromXml(RadioGroup parent, MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
            => new RadioGroupOption(parent, mod, element, fileNames);

        public override bool ShouldApply => IsEnabled;

        /*public static RadioGroupOption FromXml(MI1_0_X_XMod mod, RadioGroup parent, XElement element, IEnumerable<string> fileNames)
            => new RadioGroupOption(mod, element, fileNames)
            {
                Parent = parent
            };

        public bool IsSelected
        {
            get => Parent.Selected == this;
            set
            {
                Parent.Selected = this;
                foreach (RadioGroupOption g in Parent.Children)
                {
                    g.NotifyPropertyChanged();
                }
            }
        }

        public RadioGroupOption(MI1_0_X_XMod mod, XElement element, IEnumerable<string> fileNames)
            : base(mod, element, fileNames)
        { }*/
    }
}
