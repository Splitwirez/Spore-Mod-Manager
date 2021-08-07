using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace SporeMods.NotifyOnChange
{
    public class NOCProperty<TVal> : NOCPropertyBase
    {
        public NOCProperty(string name)
        {
            Name = name;
        }

        public NOCProperty(string name, TVal defaultValue)
            : this(name)
        {
            _value = defaultValue;
        }

        protected TVal _value = default(TVal);
        public virtual TVal Value
        {
            get => _value;
            set
            {
                _value = value;
                Notify();
            }
        }

        protected override void OnAdded(NOCObject owner)
        {
            base.OnAdded(owner);
        }
    }
}
