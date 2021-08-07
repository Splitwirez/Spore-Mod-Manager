using SporeMods.NotifyOnChange;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia;

namespace SporeMods.CommonUI.Localization
{
    public partial class LanguageManager : NOCSingleInstanceObject<LanguageManager>, IObservable<string>, IResourceHost
    {
        /// <summary>
        /// Gets a value indicating whether the object has resources.
        /// </summary>
        public bool HasResources
        {
            get => (_langDictionary != null) ? (_langDictionary.Keys.Count > 0) : false;
        }

        public bool TryGetResource(object key, out object? value)
        {
            /*if (_langDictionary != null)
            {
                if (_langDictionary.Keys.Contains(key))
                {
                    value = _langDictionary[key];
                    return true;
                }
            }
            value = null;
            return false;*/
            value = GetLocalizedText(key.ToString());
            return _langDictionary != null;
        }




        public event EventHandler<ResourcesChangedEventArgs> ResourcesChanged;

        public void NotifyHostedResourcesChanged(ResourcesChangedEventArgs e)
        {
            Console.WriteLine("e");
            ResourcesChanged?.Invoke(this, e);
        }


        List<IObserver<string>> _observers = new List<IObserver<string>>();

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                //LanguageChanged += (s, e) => observer.OnNext();
            }
            return new Unsubscriber(_observers, observer);
        }


        internal event EventHandler<LanguageEventArgs> LanguageChanged;
    }

    internal class Unsubscriber : IDisposable
    {
        private List<IObserver<string>> _observers;
        private IObserver<string> _observer;

        internal Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }



    public class LanguageEventArgs : EventArgs
    {
        public readonly Language PreviousLanguage = null;
        public readonly Language Language = null;

        internal LanguageEventArgs(Language oldLanguage, Language newLanguage)
        {
            PreviousLanguage = oldLanguage;
            Language = newLanguage;
        }
    }
}
