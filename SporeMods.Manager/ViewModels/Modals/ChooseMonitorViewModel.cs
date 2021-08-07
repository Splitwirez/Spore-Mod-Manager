using Avalonia;
using SporeMods.CommonUI;
using SporeMods.Core;
using SporeMods.NotifyOnChange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Platform;

namespace SporeMods.Manager.ViewModels
{
    public class ChooseMonitorViewModel : IModalViewModel<string>
    {
        TaskCompletionSource<string> _tcs = new TaskCompletionSource<string>();

        public TaskCompletionSource<string> GetCompletionSource() =>
            _tcs;

        public ChooseMonitorViewModel()
            : base()
        {
            //ChooseMonitorWindowsRequested?.Invoke(this, new ModalShownEventArgs(this, _tcs.Task));
        }

        public void ChooseMonitorCommand(object parameter)
        {
            if (parameter is Screen screen)
            {
                string output = screen.Bounds.X + "," + screen.Bounds.Y + "," + screen.Bounds.Width + "," + screen.Bounds.Bottom;
                
                _tcs.TrySetResult(output);
                ChooseMonitorWindowsEnded?.Invoke(this, null);
            }
        }

        public void CancelCommand(object parameter = null)
        {
            _tcs.TrySetResult(null);
            ChooseMonitorWindowsEnded?.Invoke(this, null);
        }


        //public static event EventHandler<ModalShownEventArgs> ChooseMonitorWindowsRequested;
        public static event EventHandler ChooseMonitorWindowsEnded;
    }
}