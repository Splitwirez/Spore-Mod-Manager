using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.NotifyOnChange
{
    public class NOCFilteringProperty<TVal> : NOCProperty<TVal>
    {
        public override TVal Value
        {
            get => base.Value;
            set
            {
                var oldVal = _value;
                var newVal = Coerce != null ? _coerce(Owner, oldVal) : oldVal;
                if (Validate != null ? _validate(Owner, oldVal, newVal) : true)
                {
                    base.Value = value;
                }
            }
        }

        Func<NOCObject, TVal, TVal> _coerce = null;
        public Func<NOCObject, TVal, TVal> Coerce
        {
            get => _coerce;
            set => _coerce = value;
        }

        Func<NOCObject, TVal, TVal, bool> _validate = null;

        public NOCFilteringProperty(string name, TVal defaultValue = default)
            : base(name, defaultValue)
        { }

        public Func<NOCObject, TVal, TVal, bool> Validate
        {
            get => _validate;
            set => _validate = value;
        }
    }
}
