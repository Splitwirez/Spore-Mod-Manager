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
        private Predicate<object> _canExecute;
        private Action<object> _execute;

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

        public event EventHandler CanExecuteChanged;
    }

    public class FuncCommand<T> : ICommand
    {
        static T EnsureParam(object parameter)
            => (parameter is T tParam)
                ? tParam
                : default(T)
            ;


        private Predicate<T> _canExecute;
        private Action<T> _execute;
        public FuncCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            return _canExecute(EnsureParam(parameter));
        }

        public void Execute(object parameter)
            => _execute(EnsureParam(parameter));

        public event EventHandler CanExecuteChanged;
    }

    public class ObjFuncCommand : FuncCommand<object>
    {
        public ObjFuncCommand(Action<object> execute, Predicate<object> canExecute = null)
            : base(execute, canExecute)
        { }
    }
}