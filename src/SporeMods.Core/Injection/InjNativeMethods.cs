using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using System.Runtime.InteropServices;

namespace SporeMods.Injection
{
	[Flags]
	public enum AllocationType
	{
		Commit = 0x1000,
		Reserve = 0x2000,
		Decommit = 0x4000,
		Release = 0x8000,
		Reset = 0x80000,
		Physical = 0x400000,
		TopDown = 0x100000,
		WriteWatch = 0x200000,
		LargePages = 0x20000000
	}

	[Flags]
	public enum MemoryProtection
	{
		Execute = 0x10,
		ExecuteRead = 0x20,
		ExecuteReadWrite = 0x40,
		ExecuteWriteCopy = 0x80,
		NoAccess = 0x01,
		ReadOnly = 0x02,
		ReadWrite = 0x04,
		WriteCopy = 0x08,
		GuardModifierflag = 0x100,
		NoCacheModifierflag = 0x200,
		WriteCombineModifierflag = 0x400
	}

	[Flags]
	public enum ProcessCreationFlags : uint
	{
		ZERO_FLAG = 0x00000000,
		CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
		CREATE_DEFAULT_ERROR_MODE = 0x04000000,
		CREATE_NEW_CONSOLE = 0x00000010,
		CREATE_NEW_PROCESS_GROUP = 0x00000200,
		CREATE_NO_WINDOW = 0x08000000,
		CREATE_PROTECTED_PROCESS = 0x00040000,
		CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
		CREATE_SEPARATE_WOW_VDM = 0x00001000,
		CREATE_SHARED_WOW_VDM = 0x00001000,
		CREATE_SUSPENDED = 0x00000004,
		CREATE_UNICODE_ENVIRONMENT = 0x00000400,
		DEBUG_ONLY_THIS_PROCESS = 0x00000002,
		DEBUG_PROCESS = 0x00000001,
		DETACHED_PROCESS = 0x00000008,
		EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
		INHERIT_PARENT_AFFINITY = 0x00010000
	}

	public struct STARTUPINFO
	{
		public uint cb;
		public string lpReserved;
		public string lpDesktop;
		public string lpTitle;
		public uint dwX;
		public uint dwY;
		public uint dwXSize;
		public uint dwYSize;
		public uint dwXCountChars;
		public uint dwYCountChars;
		public uint dwFillAttribute;
		public uint dwFlags;
		public short wShowWindow;
		public short cbReserved2;
		public IntPtr lpReserved2;
		public IntPtr hStdInput;
		public IntPtr hStdOutput;
		public IntPtr hStdError;
	}

	public struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;
		public IntPtr hThread;
		public uint dwProcessId;
		public uint dwThreadId;
	}

	[Flags]
	public enum ThreadAccess : int
	{
		TERMINATE = (0x0001),
		SUSPEND_RESUME = (0x0002),
		GET_CONTEXT = (0x0008),
		SET_CONTEXT = (0x0010),
		SET_INFORMATION = (0x0020),
		QUERY_INFORMATION = (0x0040),
		SET_THREAD_TOKEN = (0x0080),
		IMPERSONATE = (0x0100),
		DIRECT_IMPERSONATION = (0x0200)
	}

	public class InjNativeMethods
	{
		//[DllImport("kernel32.dll", SetLastError = true)]
		//public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, 
		//    IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

		//[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		//public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CreateProcess(string lpApplicationName,
			   string lpCommandLine, IntPtr lpProcessAttributes,
			   IntPtr lpThreadAttributes,
			   bool bInheritHandles, ProcessCreationFlags dwCreationFlags,
			   IntPtr lpEnvironment, string lpCurrentDirectory,
			   ref STARTUPINFO lpStartupInfo,
			   out PROCESS_INFORMATION lpProcessInformation);

		public static uint AccessRequired = 0x0002 | 0x0020 | 0x0008 | 0x0400 | 0x0010; //0x0002 | 0x0020 | 0x0008; //0xF0000 | 0x00100000 | 0xFFFF;

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int ResumeThread(IntPtr hThread);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int SuspendThread(IntPtr hThread);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool CloseHandle(IntPtr handle);

		//[DllImport("kernel32.dll", SetLastError = true)]
		//static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

		//[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		//static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
		//    uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
			IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

		[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
			uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
			uint dwSize, AllocationType dwFreeType);

		//MonitorInfo
		[StructLayout(LayoutKind.Sequential)]
		public struct Rect
		{
			public Rect(Int32 left, Int32 top, Int32 right, Int32 bottom)
			{
				Left = left;
				Top = top;
				Right = right;
				Bottom = bottom;
			}

			public Int32 Left;
			public Int32 Top;
			public Int32 Right;
			public Int32 Bottom;
		}

		public delegate Boolean MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern Boolean EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

		private const Int32 CchDeviceName = 32;

		[Flags]
		public enum MonitorInfoF
		{
			Primary = 0x1
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct MonitorInfoEx
		{
			public Int32 cbSize;
			public Rect rcMonitor;
			public Rect rcWork;
			public MonitorInfoF dwFlags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchDeviceName)]
			public String szDevice;
		}

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern Boolean GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

		public static ObservableCollection<MonitorInfoEx> AllMonitors
		{
			get
			{
				var monitors = new ObservableCollection<MonitorInfoEx>();
				MonitorEnumDelegate callback = delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
				{
					MonitorInfoEx info = new MonitorInfoEx
					{
						cbSize = Marshal.SizeOf(typeof(MonitorInfoEx))
					};
					if (!GetMonitorInfo(hMonitor, ref info))
					{
						throw new System.ComponentModel.Win32Exception();
					}

					monitors.Add(info);
					return true;
				};

				EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);

				return monitors;
			}
		}

		/*[DllImport("ntdll.dll", SetLastError = false)]
		public static extern IntPtr NtSuspendProcess(IntPtr ProcessHandle);*/
		[DllImport("kernel32.dll")]
		public static extern bool DebugActiveProcess(int dwProcessId);


		[DllImport("kernel32.dll")]
		public static extern bool DebugActiveProcessStop(int dwProcessId);







		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


		public static IntPtr SetWindowLong(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong)/* => IntPtr.Size == 8
		? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
		: SetWindowLong32(hWnd, nIndex, dwNewLong);*/
		{
			if (IntPtr.Size == 4)
				return SetWindowLong32(hWnd, nIndex, dwNewLong);
			else
				return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
		}

		[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
		static extern IntPtr SetWindowLong32(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
		static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);


		public const Int32
			GwlStyle = -16,
			GwlExstyle = -20;

		public const Int32
			WsCaption = 0x00C00000,
			WsBorder = 0x00800000,
			WsSizeBox = 0x00040000,
			WsMaximize = 0x01000000;

		public const Int32
			WsExToolwindow = 0x00000080,
			WsExTransparent = 0x00000020,
			WsExNoActivate = 0x08000000;

		public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);


		public static IntPtr GetWindowLong(IntPtr hWnd, Int32 nIndex)/* => IntPtr.Size == 8
		? GetWindowLongPtr64(hWnd, nIndex)
		: GetWindowLongPtr32(hWnd, nIndex);*/
		{
			if (IntPtr.Size == 4)
				return GetWindowLong32(hWnd, nIndex);
			else
				return GetWindowLongPtr64(hWnd, nIndex);
		}

		[DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(IntPtr hWnd);
	}
}