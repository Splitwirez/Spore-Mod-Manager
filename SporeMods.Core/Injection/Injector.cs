using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SporeMods.Core.Injection
{
    class Injector
    {
        private const int WAIT_TIMEOUT = 0x102;
        private const uint MAXWAIT = 15000; //10000;


        public static void InjectDLL(PROCESS_INFORMATION pi, string dllPath)
        {
            Console.WriteLine("Injecting: " + dllPath);
            IntPtr retLib = NativeMethods.GetProcAddress(NativeMethods.GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            if (retLib == IntPtr.Zero)
            {
                throw new InjectException("LoadLibrary unreachable.");
            }
            /*IntPtr hProc;
            if (Program.processHandle == IntPtr.Zero)*/
            IntPtr hProc = NativeMethods.OpenProcess(NativeMethods.AccessRequired, false, pi.dwProcessId); //Open the process with all access
            if (hProc != IntPtr.Zero)
            {// Allocate memory to hold the path to the DLL file in the process' memory
                IntPtr objPtr = NativeMethods.VirtualAllocEx(hProc, IntPtr.Zero, (uint)dllPath.Length + 1, AllocationType.Commit, MemoryProtection.ReadWrite);
                if (objPtr == IntPtr.Zero)
                {
                    int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    MessageDisplay.ShowMessageBox("Error: Virtual alloc failure: \n" + lastError.ToString() + "\n" + "hProc: " + hProc.ToString() + "\nProgram.processHandle: " + (SporeLauncher.processHandle == IntPtr.Zero));
                    throw new System.ComponentModel.Win32Exception(lastError);
                    //throw new InjectException("Virtual alloc failure.");
                }

                //Write the path to the DLL file in the location just created
                var bytes = new byte[dllPath.Length + 1];
                for (int i = 0; i < dllPath.Length; i++)
                {
                    bytes[i] = (byte)dllPath[i];
                }
                bytes[dllPath.Length] = 0;

                UIntPtr numBytesWritten;
                MessageDisplay.DebugShowMessageBox("Beginning WriteProcessMemory");
                bool writeProcessMemoryOutput = NativeMethods.WriteProcessMemory(hProc, objPtr, bytes, (uint)bytes.Length, out numBytesWritten);
                if (!writeProcessMemoryOutput || numBytesWritten.ToUInt32() != bytes.Length)
                {
                    SporeLauncher.ThrowWin32Exception("Write process memory failed.");
                }
                MessageDisplay.DebugShowMessageBox("WriteProcessMemory output: " + writeProcessMemoryOutput.ToString());

                // Create a remote thread that begins at the LoadLibrary function and is passed as memory pointer
                IntPtr lpThreadId;
                IntPtr hRemoteThread = NativeMethods.CreateRemoteThread(hProc, IntPtr.Zero, 0, retLib, objPtr, 0, out lpThreadId);

                // Wait for the thread to finish
                if (hRemoteThread != IntPtr.Zero)
                {
                    if (NativeMethods.WaitForSingleObject(hRemoteThread, MAXWAIT) == WAIT_TIMEOUT)
                    {
                        //throw new InjectException("Wait for single object failed.");
                        SporeLauncher.ThrowWin32Exception("Wait for single object failed. This usually occurs if something has become stuck during injection, or if another error was left open for too long.");
                    }
                }
                else
                {
                    //throw new InjectException("Create remote thread failed.");
                    int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    MessageDisplay.ShowMessageBox("Error: Create remote thread failed: \n" + lastError.ToString());
                    throw new System.ComponentModel.Win32Exception(lastError);
                }

                NativeMethods.VirtualFreeEx(hProc, objPtr, (uint)bytes.Length, AllocationType.Release);

                NativeMethods.CloseHandle(hProc);
            }
            else
            {
                SporeLauncher.ThrowWin32Exception("OpenProcess failed!");
            }
        }
    }

    [Serializable()]
    public class InjectException : Exception
    {
        public InjectException() : base() { }
        public InjectException(string message) : base(message) { }
        public InjectException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected InjectException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
