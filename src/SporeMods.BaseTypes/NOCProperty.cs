using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace SporeMods.BaseTypes
{
    public class NOCProperty<TVal> : NOCPropertyBase
    {
        //internal NOCObject Owner = null;

        internal string _name;
        public override string Name
        {
            get => _name;
        }


        public NOCProperty(string name, TVal defaultValue = default(TVal))
        {
            _name = name;
            Value = defaultValue;
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
    }
}