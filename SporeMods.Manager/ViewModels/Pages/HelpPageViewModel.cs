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

namespace SporeMods.Manager.ViewModels
{
    public class HelpPageViewModel : NOCObject
    {
        public HelpPageViewModel()
            : base()
        {

        }
        
        public async void OnWhatever()
        {
            /*bool returnValue = Task.Run<bool>(() =>
            {
                Task<bool> tsk = await MessageDisplay.ShowModal<bool>(new ModalTestViewModel());
                return tsk.Result;
            });*/

            bool retVal = await MessageDisplay.ShowModal<bool>(new ModalTestViewModel());
            Console.WriteLine($"Modal exited, returned {retVal}");
        }
    }
    public class ModalTestViewModel : IModalViewModel<bool>
    {
        TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();

        public TaskCompletionSource<bool> GetCompletionSource()
        {
            return _completionSource;
        }


        public void Yes() =>
            _completionSource.TrySetResult(true);

        public void No() =>
            _completionSource.TrySetResult(false);
    }
}