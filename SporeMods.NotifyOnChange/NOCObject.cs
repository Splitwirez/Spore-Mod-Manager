using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SporeMods.NotifyOnChange
{
	public abstract class NOCObject : INotifyPropertyChanged
	{
		protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		internal void NotifyPropertyChanged(NOCPropertyBase property) =>
			NotifyPropertyChanged(property.Name);


		public event PropertyChangedEventHandler PropertyChanged;

		protected TProp AddProperty<TProp>(TProp property) where TProp : NOCPropertyBase
        {
			property.SetOwner(this);
			return property;
        }


		protected NOCProperty<TValue> AddProperty<TValue>(string name, TValue value = default(TValue)) =>
			AddProperty(new NOCProperty<TValue>(name, value));

		/*internal static class NotifyPropertyChangedBaseExtensions
		{
		}*/
	}
}
