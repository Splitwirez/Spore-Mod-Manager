using System;

namespace SporeMods.Core
{
	public static partial class Permissions
	{
		public static bool IsAtleastWindowsVista()
		{
			return Environment.OSVersion.Version.Major >= 6;
		}
	}
}