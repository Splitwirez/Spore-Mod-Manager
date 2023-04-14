using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SporeMods.CommonUI
{
    partial class FuncBinding
    {
        class GetFuncConverter : IValueConverter
        {
            static readonly Type _TASK = typeof(Task);
            static readonly string _CMD_PREFIX = $"{nameof(GetFuncConverter)}: ";
            
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null)
                {
                    Cmd.WriteLine($"{_CMD_PREFIX}{nameof(value)} was null");
                    return null;
                }

                string methodName = (string)parameter;
                var method = value.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                if (method == null)
                {
                    Cmd.WriteLine($"{_CMD_PREFIX}Method '{methodName}' not found!");
                    return null;
                }

                var funcParamCount = method.GetParameters().Length;
                if (funcParamCount > 1)
                {
                    Cmd.WriteLine($"{_CMD_PREFIX}Method '{methodName}' has {funcParamCount} parameters! (cannot have more than 1)");
                    return null;
                }


                if
                (
                    _TASK.IsAssignableFrom(method.ReturnType)
                )
                {
                    return (funcParamCount > 0)
                    ? new TaskCommand(p => (Task<object>)method.Invoke(value, new object[] { p }))
                    : new TaskCommand(() => (Task)(method.Invoke(value, new object[0])))
                ;
                }
                return (funcParamCount > 0)
                    ? new FuncCommand<object>(p => method.Invoke(value, new object[] { p }))
                    : new FuncCommand<object>(_ => method.Invoke(value, new object[0]))
                ;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotImplementedException();
        }
    }
}
