using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SporeMods.Core
{
	public static class Externals
	{	
		public static Func<Type, object, object, object> CREATE_FUNC_COMMAND;

		static Type _funcCommandType = null;
		public static void SpecifyFuncCommandType(Type cmdType)
		{
			if (_funcCommandType == null)
				_funcCommandType = cmdType;
		}

		public static object CreateCommand<T>(Action<T> execute, Predicate<T> canExecute = null)
		{
			var cmdType = (_funcCommandType).MakeGenericType(typeof(T));
			return Activator.CreateInstance(cmdType, execute, canExecute);
		}

		static Action _extractOriginPrerequisites = null;
		public static void ExtractOriginPrerequisites()
			=> _extractOriginPrerequisites();

		public static void ProvideExtractOriginPrerequisitesFunc(Action h)
        {
			if (_extractOriginPrerequisites == null)
				_extractOriginPrerequisites = h;
        }

		static bool _needsPrerequisitesExtracted = false;
		public static bool NeedsPrerequisitesExtracted
        {
			get => _needsPrerequisitesExtracted;

		}

		public static bool SpecifyNeedsPrerequisitesExtracted(bool val)
			=> _needsPrerequisitesExtracted = val;

		
		public static Func<string, string> GetLocalizedText = (key) => key;

		public static Func<Stream, object> CreateBitmapImage = null;
		public static SynchronizationContext UIThread = null;
	}
}