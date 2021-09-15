using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SporeMods.Core
{
	public static class Externals
	{	
		public static Func<Type, object, object, object> CREATE_FUNC_COMMAND;

		static Type _funcCommandType = null;
		public static void SpecifyFuncCommandType(Type cmdType)
			=> _funcCommandType = cmdType;

		public static object CreateCommand<T>(Action<T> execute, Predicate<T> canExecute = null)
		{
			var cmdType = (_funcCommandType).MakeGenericType(typeof(T));
			return Activator.CreateInstance(cmdType, execute, canExecute);
		}
    }
}