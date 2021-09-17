using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using SporeMods.Core;
using SporeMods.CommonUI;

namespace SporeMods.ViewModels
{
	public class ModalDisplayViewModel : NotifyPropertyChangedBase
	{
		IModalViewModel _currentModalVM = null;
        public IModalViewModel CurrentModalVM
        {
            get => _currentModalVM;
            set
			{
				_currentModalVM = value;
				bool hasModal = _currentModalVM != null;
				CurrentModalView = hasModal ? Activator.CreateInstance(Type.GetType(value.GetViewTypeName())) : null;
				HasModal = hasModal;
				NotifyPropertyChanged();
			}
        }


		object _currentModalView = null;
        public object CurrentModalView
        {
            get => _currentModalView;
            set
			{
				_currentModalView = value;
				NotifyPropertyChanged();
			}
        }

        bool _hasModal;
        public bool HasModal
        {
            get => _hasModal;
            set
			{
				_hasModal = value;
				NotifyPropertyChanged();
			}
        }


		public ModalDisplayViewModel()
			: base()
		{
			Modal.Shown += Modal_Shown;
		}


		async void Modal_Shown(object sender, ModalShownEventArgs args)
        {
            CurrentModalVM = args.ViewModel;
            await args.Task;
            CurrentModalVM = null;
        }
	}
}
