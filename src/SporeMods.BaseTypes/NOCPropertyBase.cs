using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace SporeMods.BaseTypes
{
    public abstract class NOCPropertyBase
    {
        NOCObject _owner;
        public NOCObject Owner
        {
            get => _owner;
        }

        public abstract string Name
        {
            get;
        }


        public void Notify() =>
            _owner?.NotifyPropertyChanged(this);


        protected virtual void OnAdded(NOCObject owner)
        { }

        internal void SetOwner(NOCObject owner)
        {
            _owner = owner;
            OnAdded(_owner);
        }
    }
}