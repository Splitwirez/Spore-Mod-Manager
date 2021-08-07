using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace SporeMods.NotifyOnChange
{
    public abstract class NOCPropertyBase
    {
        NOCObject _owner = null;
        public NOCObject Owner
        {
            get => _owner;
        }

        string _name;
        public string Name
        {
            get => _name;
            protected set => _name = value;
        }


        public void Notify()
        {
            if (_owner != null)
                _owner.NotifyPropertyChanged(this);
            else
                throw new NullReferenceException($"Cannot call {nameof(Notify)} with no assigned {nameof(Owner)}. (NOT LOCALIZED)");
        }


        protected virtual void OnAdded(NOCObject owner)
        { }

        internal void SetOwner(NOCObject owner)
        {
            _owner = owner;
            OnAdded(_owner);
        }
    }
}
