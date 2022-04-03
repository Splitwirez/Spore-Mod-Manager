using System;
using System.Collections.Generic;
using System.Text;
using SporeMods.Core;

namespace SporeMods.Core.Mods
{
    public interface IModText
    {
        /*string Value
        {
            get;
        }*/
    }

    public class FixedModText : NotifyPropertyChangedBase, IModText
    {
        public FixedModText(string value)
            => Value = value;

        
        string _value = string.Empty;
        public string Value
        {
            get => _value;
            private set
            {
                _value = value;
                NotifyPropertyChanged();
            }
        }

        public override string ToString()
        => Value;
    }
}
