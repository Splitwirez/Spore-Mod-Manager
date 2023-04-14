using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SporeMods.CommonUI
{
    public class AsyncCommandBehavior : Behavior<ButtonBase>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(Task), typeof(ButtonBase), new FrameworkPropertyMetadata(CommandPropertyChanged));

        static void CommandPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is ButtonBase btn))
                return;
            var newValue = e.NewValue;
            

            if (newValue == null)
            {
                btn.Command = null;
                return;
            }

            Task newTask = (Task)newValue;
            if (btn.Command is AsyncCommand prev)
                prev.ExecTask = newTask;
            else
                btn.Command = new AsyncCommand(newTask);
        }


        public Task GetCommand(ButtonBase button)
            => (Task)button.GetValue(CommandProperty);
        public void SetCommand(ButtonBase button, Task task)
            => button.SetValue(CommandProperty, task);


        static AsyncCommandBehavior()
        {
            
        }


        internal class AsyncCommand : ICommand
        {
            Task _execTask = null;
            public Task ExecTask
            {
                get => _execTask;
                internal set => _execTask = value;
            }

            public AsyncCommand(Task task)
            {
                ExecTask = task;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
                => true;

            public async void Execute(object parameter)
            {
                await ExecTask;
            }
        }
    }
}
