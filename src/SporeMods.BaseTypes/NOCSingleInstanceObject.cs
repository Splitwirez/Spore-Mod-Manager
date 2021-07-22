using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SporeMods.BaseTypes
{
    public static class NOCSingleInstances
    {
        public static List<NOCSingleInstanceObject> All = new List<NOCSingleInstanceObject>();
    }
    public abstract class NOCSingleInstanceObject : NOCObject
    {
        public static List<NOCSingleInstanceObject> ALL_INSTANCES = new List<NOCSingleInstanceObject>();


        public abstract bool Ensure();
    }

    public abstract class NOCSingleInstanceObject<T> : NOCSingleInstanceObject where T : NOCSingleInstanceObject<T>, new()
    {
        static NOCSingleInstanceObject()
        {
            NOCSingleInstances.All.Add(EnsureInstance());
        }


        static T _instance = null;
        public static T Instance
        {
            get => _instance;
            protected set
            {
                var oldInstance = _instance;
                _instance = value;
                
                /*if (oldInstance != null)
                    oldInstance.NotifyPropertyChanged(nameof(Instance));

                if (_instance != null)
                    _instance.NotifyPropertyChanged(nameof(Instance));*/
            }
        }


        const string INSTANCE_CREATION_FAILED = "The one and only \'{0}\' could not be created (NOT LOCALIZED).";
        static string CreateInstanceFailed = string.Format(INSTANCE_CREATION_FAILED, typeof(T).FullName);
        public static T EnsureInstance()
        {
            if (Instance == null)
            {
                try
                {
                    var instance = new T();
                    Instance = instance;
                }
                catch (Exception ex)
                {
                    throw new NullReferenceException(CreateInstanceFailed, ex);
                }

                if (Instance == null)
                {
                    throw new NullReferenceException(CreateInstanceFailed);
                }
            }
            return Instance;
        }

        public override bool Ensure() =>
            EnsureInstance() != null;
    }
}
