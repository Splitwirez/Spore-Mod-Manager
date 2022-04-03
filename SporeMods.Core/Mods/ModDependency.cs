using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.Mods
{
    public class ModDependency : NotifyPropertyChangedBase
    {
        string _unique = string.Empty;
        public string Unique
        {
            get => _unique;
            set
            {
                _unique = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsFulfilledBy(ISporeMod mod)
        {
            return Unique == mod.Unique;
        }
    }
}
