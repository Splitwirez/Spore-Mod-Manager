using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace SporeMods.BaseTypes
{
    public class NOCProperty<TVal> : INOCProperty
    {
        internal NOCObjectBase Owner = null;

        TVal _value = default(TVal);

        string _name;
        public string Name
        {
            get => _name;
            internal set => _name = value;
        }


        internal NOCProperty()
        { }

        public TVal Value
        {
            get => GetValue();
            set
            {
                if (SetValue(value))
                    Refresh();
            }
        }

        protected virtual TVal GetValue() =>
            _value;

        protected virtual bool SetValue(TVal value)
        {
            _value = value;
            return true;
        }

        public void Refresh()
        {
            Owner.NotifyPropertyChanged(_name);
        }

        /*CreateInstance<TValue, TOwner>(params object[] parameters) where TOwner : NotifyPropertyChangedBase
        {
            return new NCProperty<TValue, TOwner>(parameters[0].ToString(), (TValue)parameters[1]);
        }*/
    }
}