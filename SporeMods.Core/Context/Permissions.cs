using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
//using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace SporeMods.Core
{
	public static class Permissions
	{
		/*public static void ForwardDotnetEnvironmentVariables(ref ProcessStartInfo info)
		{
			info.EnvironmentVariables["DOTNET_ROOT"] = Environment.ExpandEnvironmentVariables("%DOTNET_ROOT%");
			info.EnvironmentVariables["DOTNET_ROOT(x86)"] = Environment.ExpandEnvironmentVariables("%DOTNET_ROOT(x86)%");
			info.EnvironmentVariables["PATH"] = Environment.ExpandEnvironmentVariables("%PATH%");
			info.EnvironmentVariables["DOTNET_MULTILEVEL_LOOKUP"] = Environment.ExpandEnvironmentVariables("0");
		}*/

		/*public static bool AreAnyOtherModManagersRunning()
		{
			//string dir = Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).ToString();
			string exe = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
			return Process.GetProcessesByName(exe).Length > 1;

			/*foreach (string s in mgrExecutables) //Directory.EnumerateFiles(dir).Where(x => x.ToLowerInvariant().EndsWith(".exe"))
			{
				Process[] processes = Process.GetProcessesByName(s);
				if ((s.ToLowerInvariant() == Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName).ToLowerInvariant()) && (processes.Length > 1))
				{
					Debug.WriteLine("SAME EXE");
					return true;
				}
				else if (processes.Length > 0)
				{
					Debug.WriteLine("DIFFERENT EXE");
					return true;
				}
			}*
			return false;
		}*/

		public static bool IsAtleastWindowsVista()
		{
			#if LINUX_BUILD
				return false;
			#else
				return Environment.OSVersion.Version.Major >= 6;
			#endif
		}

		public static bool IsAdministrator()
		{
			#if LINUX_BUILD
				return true; //TODO: Proper non-Windows implementation
			#else

			if (Environment.OSVersion.Version.Major <= 5)
				return true;
			else
			{
				WindowsIdentity identity = WindowsIdentity.GetCurrent();
				WindowsPrincipal principal = new WindowsPrincipal(identity);
				return principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
			#endif
		}

		public static Process RerunAsAdministrator()
		{
			return RerunAsAdministrator(GetProcessCommandLineArgs());
		}

		public static string GetProcessCommandLineArgs()
		{
			var args = Environment.GetCommandLineArgs().ToList();
			if (args.Count > 0)
				args.RemoveAt(0);

			string returnVal = string.Empty;
			foreach (string s in args)
			{
				//returnVal = returnVal + "\"" + s + "\" ";
				if (s.Contains(' '))
					returnVal = returnVal + "\"" + s + "\" ";
				else
					returnVal = returnVal + s + " ";
			}

			return returnVal;
		}

		public static Process RerunAsAdministrator(string args)
		{
			return RerunAsAdministrator(args, true);
		}

		public static Process RerunAsAdministrator(bool closeCurrent)
		{
			return RerunAsAdministrator(GetProcessCommandLineArgs(), closeCurrent);
		}

		public static Process RerunAsAdministrator(string args, bool closeCurrent)
		{
			//https://stackoverflow.com/questions/133379/elevating-process-privilege-programmatically/10905713
			var exeName = Process.GetCurrentProcess().MainModule.FileName;
			Process process = null;
			ProcessStartInfo startInfo = new ProcessStartInfo(exeName, args)
			{
				UseShellExecute = true,
				Verb = "runas"
			};
			//ForwardDotnetEnvironmentVariables(ref startInfo);
			process = Process.Start(startInfo);

			if (closeCurrent && (process != null))
				Process.GetCurrentProcess().Kill();

			return process;
		}

		//https://stackoverflow.com/questions/9108399/how-to-grant-full-permission-to-a-file-created-by-my-application-for-all-users
		/// <summary>
		/// If the current application is running as Administrator, attempts to grant full access to a specific directory and its files.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static bool GrantAccessDirectory(string fullPath)
		{
			#if LINUX_BUILD
				return true; //TODO: Proper non-Windows implementation
			#else

			if (Permissions.IsAdministrator() && Directory.Exists(fullPath))
			{ 
				DirectoryInfo dInfo = new DirectoryInfo(fullPath);
				DirectorySecurity dSecurity = dInfo.GetAccessControl();
				dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl,
																 InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
																 PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
				dInfo.SetAccessControl(dSecurity);
				return true;
			}
			return false;
			#endif
		}

		/// <summary>
		/// If the current application is running as Administrator, attempts to grant full access to a specific file.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static bool GrantAccessFile(string filePath)
		{
			#if LINUX_BUILD
			//if (!SmmInfo.IsWindowsLike)
				return true; //TODO: Proper non-Windows implementation
			#else
			
			if (Permissions.IsAdministrator() && File.Exists(filePath))
			{
				//var security = File.GetAccessControl(filePath);
				var sec = new FileSecurity(filePath, AccessControlSections.All);
				sec.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), 
															 FileSystemRights.FullControl, InheritanceFlags.None,
															 PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
				
				return true;

				//string parentPath = Path.GetDirectoryName(filePath);
				//if (Directory.Exists(parentPath))
				//{
				//	return GrantAccess(parentPath);
				//}
			}
			return false;
			#endif
		}

		public static bool IsFileLocked(string filePath)
		{
			return IsFileLocked(filePath, FileAccess.Read);
		}

		//https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
		public static bool IsFileLocked(string filePath, FileAccess access)
		{
			#if LINUX_BUILD
			//if (!SmmInfo.IsWindowsLike)
				return false; //TODO: Proper non-Windows implementation
			
			#else
			
			FileStream stream = null;

			try
			{
				stream = new FileInfo(filePath).Open(FileMode.Open, access, FileShare.None);
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			//file is not locked
			return false;
			#endif
		}
	}
}