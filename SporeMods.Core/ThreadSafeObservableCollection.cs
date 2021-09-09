using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Threading;

namespace SporeMods.Core
{
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        private SynchronizationContext _syncContext = null;
        private void RunOnMainSyncContext(SendOrPostCallback d)
        {
            _syncContext.Send(d, null);
        }


        public ThreadSafeObservableCollection()
            : base()
        {
            _syncContext = SynchronizationContext.Current;
        }

        protected override void ClearItems()
        {
            RunOnMainSyncContext(_ => base.ClearItems());
        }

        protected override void InsertItem(int index, T item)
        {
            RunOnMainSyncContext(_ => base.InsertItem(index, item));
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            RunOnMainSyncContext(_ => base.MoveItem(oldIndex, newIndex));
        }

        /*protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            RunOnMainSyncContext(_ => base.OnCollectionChanged(e));
        }*/

        protected override void RemoveItem(int index)
        {
            RunOnMainSyncContext(_ => base.RemoveItem(index));
        }

        protected override void SetItem(int index, T item)
        {
            RunOnMainSyncContext(_ => base.SetItem(index, item));
        }
    }
}
