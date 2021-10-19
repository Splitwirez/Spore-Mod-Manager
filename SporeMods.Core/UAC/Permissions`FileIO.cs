using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace SporeMods.Core
{
	public static partial class Permissions
	{
		//https://stackoverflow.com/questions/9108399/how-to-grant-full-permission-to-a-file-created-by-my-application-for-all-users
		/// <summary>
		/// If the current application is running as Administrator, attempts to grant full access to a specific directory and its files.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static bool GrantAccessDirectory(string fullPath)
		{
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
		}

		/// <summary>
		/// If the current application is running as Administrator, attempts to grant full access to a specific file.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static bool GrantAccessFile(string filePath)
		{
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
		}

		public static bool IsFileLocked(string filePath)
		{
			return IsFileLocked(filePath, FileAccess.Read);
		}

		//https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
		public static bool IsFileLocked(string filePath, FileAccess access)
		{
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
		}
	}
}