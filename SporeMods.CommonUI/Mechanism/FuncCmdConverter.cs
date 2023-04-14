using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using System.Threading.Tasks;

namespace SporeMods.CommonUI
{
    public static class Func
    {
        public static FuncCmdConverter Cmd { get; } = new FuncCmdConverter();
    }

    public class FuncCmdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MessageBox.Show(value.GetType().FullName);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    internal class FncCommand : ICommand
    {
        protected readonly Predicate<object> _canExecute;
        protected readonly Action<object> _execute;

        public FncCommand(Predicate<object> canExecute, Action<object> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
            => _execute(parameter);

#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 0067
    }

    internal static class CommandUtils
    {
        public static T EnsureParam<T>(object parameter)
            => (parameter is T tParam)
                ? tParam
                : default(T)
            ;
    }
    public class FuncCommand<T> : ICommand
    {
        protected readonly Predicate<T> _canExecute;
        protected readonly Action<T> _execute;
        public FuncCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            return _canExecute(CommandUtils.EnsureParam<T>(parameter));
        }

        public void Execute(object parameter)
            => _execute(CommandUtils.EnsureParam<T>(parameter));

#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 0067
    }

    public class ObjFuncCommand : FuncCommand<object>
    {
        public ObjFuncCommand(Action<object> execute, Predicate<object> canExecute = null)
            : base(execute, canExecute)
        { }
    }


    public class TaskCommand : ICommand
    {
        protected readonly Func<Task> _execute = null;
        protected readonly Func<object, Task> _executeWithParam = null;
        
        private TaskCommand()
        { }
        public TaskCommand(Func<Task> execute)
            : this()
        {
            _execute = execute;
        }
        public TaskCommand(Func<object, Task> execute)
            : this()
        {
            _executeWithParam = execute;
        }

        public virtual bool CanExecute(object parameter)
            => true;

        public async void Execute(object parameter)
        {
            if (_executeWithParam != null)
                await _executeWithParam(parameter);
            else
                await _execute();
        }

#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 0067
    }



    public class TaskCommand<T> : TaskCommand
    {
        protected readonly Predicate<T> _canExecute;
        public TaskCommand(Func<Task<T>> execute, Predicate<T> canExecute = null)
            : base(execute)
        {
            _canExecute = canExecute;
        }
        public TaskCommand(Func<object, Task<T>> execute, Predicate<T> canExecute = null)
            : base(execute)
        {
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return base.CanExecute(parameter);

            return _canExecute(CommandUtils.EnsureParam<T>(parameter));
        }
    }

    public class ObjTaskCommand : TaskCommand<object>
    {
        public ObjTaskCommand(Func<Task<object>> execute, Predicate<object> canExecute = null)
            : base(execute, canExecute)
        { }
        public ObjTaskCommand(Func<object, Task<object>> execute, Predicate<object> canExecute = null)
            : base(execute, canExecute)
        { }
    }
}