using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core
{
	public interface IModalViewModel
	{
	}

	public interface IModalViewModel<T> : IModalViewModel
	{
		TaskCompletionSource<T> GetCompletionSource();
	}
}