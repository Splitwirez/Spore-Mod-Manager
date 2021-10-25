#if !OLD_GAME_PATH_STUFF
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SporeMods.Core
{
	public static class GameInfo
	{
		public enum GameExecutableType
		{
			Disk__1_5_1,
			Origin__1_5_1,
			Origin__March2017,
			GogOrSteam__1_5_1,
			GogOrSteam__March2017,
			None
		}
	}
}
#endif