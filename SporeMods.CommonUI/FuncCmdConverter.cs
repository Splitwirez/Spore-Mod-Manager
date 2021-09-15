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
        private Predicate<object> canExecute;
        private Action<object> execute;

        public FncCommand(Predicate<object> canExecute, Action<object> execute)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            if (this.canExecute == null) return true;

            return this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class FuncCommand<T> : ICommand
    {
        private Predicate<T> _canExecute;
        private Action<T> _execute;

        public FuncCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;

            return _canExecute((T)parameter);
        }

        public void Execute(object parameter)
            => _execute((T)parameter);

        public event EventHandler CanExecuteChanged;
    }

    public class ObjFuncCommand : FuncCommand<object>
    {
        public ObjFuncCommand(Action<object> execute, Predicate<object> canExecute = null)
            : base(execute, canExecute)
        { }
    }
}