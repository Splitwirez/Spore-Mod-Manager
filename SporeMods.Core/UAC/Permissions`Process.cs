using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using Microsoft.Win32;

namespace SporeMods.Core
{
	public static partial class Permissions
	{
		static WindowsIdentity _identityCached = null;
		static WindowsIdentity _identity
        {
			get
			{
				if (_identityCached == null)
					_identityCached = WindowsIdentity.GetCurrent();
				return _identityCached;
			}
		}

		static WindowsPrincipal _principalCached = null;
		static WindowsPrincipal _principal
		{
			get
			{
				if (_principalCached == null)
					_principalCached = new WindowsPrincipal(_identity);
				return _principalCached;
			}
		}

		public static bool IsAdministrator()
			=> IsAtleastWindowsVista() ? _principal.IsInRole(WindowsBuiltInRole.Administrator) : true;

		//https://stackoverflow.com/questions/1220213/detect-if-running-as-administrator-with-or-without-elevated-privileges
		//https://github.com/falahati/UACHelper
		/*public static bool IsExplicitlyElevated()
			=> IsAdministrator() && _identity.Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);*/

		private const string REGISTRY_ADDRESS = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";

		public static bool IsUACEnabled
		{
			get
			{
				if (!IsAtleastWindowsVista())
					return false;

				using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
				using (var key = baseKey.OpenSubKey(REGISTRY_ADDRESS, false))
				{
					return (key?.GetValue("EnableLUA", 0) as int? ?? 0) > 0;
				}
			}
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
				if (s.Contains(' ') && (!s.StartsWith('"')) && (!s.EndsWith('"')))
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
	}
}