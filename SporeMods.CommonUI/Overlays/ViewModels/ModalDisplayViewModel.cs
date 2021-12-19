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
				Cmd.WriteLine($"===VM: CurrentModalVM is now \'{value}\'");
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
				Cmd.WriteLine($"===VM: CurrentModalView is now \'{value}\'");
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
				Cmd.WriteLine($"===VM: HasModal is now \'{value}\'");
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
            CurrentModalVM = args != null ? args.ViewModel : null;
            //await args.Task;
        }
	}
}
