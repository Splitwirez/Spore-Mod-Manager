using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SporeMods.BaseTypes
{
	public abstract class NOCObject : INotifyPropertyChanged
	{
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		internal void NotifyPropertyChanged(NOCPropertyBase property) =>
			NotifyPropertyChanged(property.Name);


		public event PropertyChangedEventHandler PropertyChanged;

		protected TProp AddProperty<TProp>(TProp property) where TProp : NOCPropertyBase
        {
			property.SetOwner(this);
			return property;
			//_properties.Add(property);
        }


		protected NOCProperty<TValue> AddProperty<TValue>(string name, TValue value = default(TValue))
		{
			NOCProperty<TValue> property = new NOCProperty<TValue>(name)
			{
				Value = value
			};
			AddProperty(property);
			return property;
		}

		/*internal static class NotifyPropertyChangedBaseExtensions
		{
		}*/
	}
}
