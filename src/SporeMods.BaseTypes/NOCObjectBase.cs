using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SporeMods.BaseTypes
{
	public abstract class NOCObjectBase : INotifyPropertyChanged
	{
		internal virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public event PropertyChangedEventHandler PropertyChanged;

		List<INOCProperty> _properties = new List<INOCProperty>();

		public IReadOnlyList<INOCProperty> GetProperties() =>
			new ReadOnlyCollection<INOCProperty>(_properties);

		public NOCProperty<TValue> AddProperty<TValue>(string name, TValue value = default(TValue))
		{
			NOCProperty<TValue> property = new NOCProperty<TValue>()
			{
				Owner = this,
				Name = name,
				Value = value,
			};
			_properties.Add(property);
			return property;
		}

		/*internal static class NotifyPropertyChangedBaseExtensions
		{
		}*/
	}
}
