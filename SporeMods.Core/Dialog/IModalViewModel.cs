using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core
{
	public interface IModalViewModel
	{
		public virtual string GetViewTypeName()
			=> this.GetType().FullName.Replace("ViewModel", "View");

		bool CanDismiss { get; }
		object DismissCommand { get; }

		string Title { get; }
		bool HasTitle { get; }
	}

	public interface IModalViewModel<T> : IModalViewModel
	{
		TaskCompletionSource<T> CompletionSource { get; }
	}

	public abstract class ModalViewModel<T> : NotifyPropertyChangedBase, IModalViewModel, IModalViewModel<T>
	{
		TaskCompletionSource<T> _completionSource = new TaskCompletionSource<T>();
		
		public virtual TaskCompletionSource<T> CompletionSource
		{
			get => _completionSource;
		}

		object _dismissCommand = null;
		public object DismissCommand
		{
			get => _dismissCommand;
			set
			{
				_dismissCommand = value;
				NotifyPropertyChanged();
				CanDismiss = value != null;
			}
		}

		bool _canDismiss = false;
		public bool CanDismiss
		{
			get => _canDismiss;
			protected set
			{
				_canDismiss = value;
				NotifyPropertyChanged();
			}
		}



		string _title = string.Empty;
		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				NotifyPropertyChanged();
				HasTitle = !value.IsNullOrEmptyOrWhiteSpace();
			}
		}

		bool _hasTitle = false;
		public bool HasTitle
		{
			get => _hasTitle;
			protected set
			{
				_hasTitle = value;
				NotifyPropertyChanged();
			}
		}
	}
}