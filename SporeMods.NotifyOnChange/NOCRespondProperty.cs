using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.NotifyOnChange
{
    public class NOCRespondProperty<TVal> : NOCProperty<TVal>
    {
        public override TVal Value
        {
            get => base.Value;
            set
            {
                var oldVal = _value;
                base.Value = value;
                if (_valueChangeResponse != null)
                    _valueChangeResponse(Owner, oldVal, _value);
            }
        }

        Action<NOCObject, TVal, TVal> _valueChangeResponse = null;
        public Action<NOCObject, TVal, TVal> ValueChangeResponse
        {
            get => _valueChangeResponse;
            set => _valueChangeResponse = value;
        }

        public NOCRespondProperty(string name, TVal defaultValue = default(TVal))
            : base(name, defaultValue)
        { }
    }
}
