using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SporeMods.NotifyOnChange
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
            get => EnsureInstance();
            protected set
            {
                //var oldInstance = _instance;
                _instance = value;
                
                /*if (oldInstance != null)
                    oldInstance.NotifyPropertyChanged(nameof(Instance));

                if (_instance != null)
                    _instance.NotifyPropertyChanged(nameof(Instance));*/
            }
        }


        const string INSTANCE_CREATION_FAILED = "The one and only \'{0}\' could not be created (NOT LOCALIZED).";
        static string CreateInstanceFailed = string.Format(INSTANCE_CREATION_FAILED, typeof(T).FullName);
        
        static bool _ensured = false;
        internal static T EnsureInstance()
        {
            if (!_ensured)
            {
                var instance = new T();
                Instance = instance;

                if (_instance == null)
                {
                    throw new NullReferenceException(CreateInstanceFailed);
                }
                else
                {
                    _ensured = true;
                }
            }
            return _instance;
        }

        public override bool Ensure() =>
            EnsureInstance() != null;
    }
}
