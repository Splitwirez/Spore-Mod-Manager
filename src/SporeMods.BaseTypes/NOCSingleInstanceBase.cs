using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SporeMods.BaseTypes
{
    public abstract class NOCSingleInstanceBase<T> : NOCObjectBase where T : NOCSingleInstanceBase<T>, new()
    {
        static T _instance = null;
        public static T Instance
        {
            get => _instance;
            protected set
            {
                var oldInstance = _instance;
                _instance = value;
                
                if (oldInstance != null)
                    oldInstance.NotifyPropertyChanged(nameof(Instance));

                if (_instance != null)
                    _instance.NotifyPropertyChanged(nameof(Instance));
            }
        }


        const string INSTANCE_CREATION_FAILED = "The one and only \'{0}\' could not be created (NOT LOCALIZED).";
        public static T EnsureInstance()
        {
            string createInstanceFailed = string.Format(INSTANCE_CREATION_FAILED, typeof(T).FullName);

            try
            {
                var instance = new T();
                Instance = instance;
            }
            catch (Exception ex)
            {
                throw new NullReferenceException(createInstanceFailed, ex);
            }

            if (Instance == null)
            {
                throw new NullReferenceException(createInstanceFailed);
            }

            return Instance;
        }
    }
}
