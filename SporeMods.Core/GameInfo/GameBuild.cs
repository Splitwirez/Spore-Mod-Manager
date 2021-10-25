#if SMMM_MORE
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SporeMods.Core
{
	public enum DistributionPlatform
	{
		Unknown,
		Disk,
		Origin,
		GogOrSteam,
	}

	public enum KnownPatch
	{
		Unknown,
		Patch1_5_1,
		March2017,
	}


	public class GameBuild
	{
		static readonly List<GameBuild> GAME_BUILDS = new List<GameBuild>()
		{
			/*Version version,
						long fileSize,
						DistributionPlatform platform,// = DistributionPlatform.Unknown,
						KnownPatch patch,// = KnownPatch.Unknown,
						string exeName,// = null,
						string coreDllName,
						string legacyModDllSuffix = null*/
			new GameBuild(
				version: new Version(1, 05, 0001, 0), //TODO: This is almost definitely wrong
				platform: DistributionPlatform.Disk,
				patch: KnownPatch.Patch1_5_1,
				fileSize: 24909584,
				coreDllName: CORE_DLL_DISK1_5_1,
				legacyModDllSuffix: LEGACY_DLL_SUFFIX_DISK1_5_1
			)/*,
			new GameBuild()
			{
				Platform = DistributionPlatform.Origin,
				KnownPatch.Patch1_5_1,
				Version = new Version(2, 0, 0, 5), //TODO: This is almost definitely wrong
				ExeName = SPOREAPP_MODAPIFIX,
				FileSize = 31347984
			},
			new GameBuild()
			{
				Platform = DistributionPlatform.Origin,
				KnownPatch.March2017,
				Version = new Version(3, 1, 0, 22),
				FileSize = 24898224,
				LatterExeName = SPOREAPP_MODAPIFIX,
				LatterFileSize = 24885248
			},
			new GameBuild()
			{
				Platform = DistributionPlatform.GogOrSteam,
				KnownPatch.Patch1_5_1,
				Version = new Version(2, 0, 0, 5), //TODO: This is almost definitely wrong
				FileSize = 24888320
			},
			new GameBuild()
			{
				Platform = DistributionPlatform.GogOrSteam,
				KnownPatch.March2017,
				Version = new Version(3, 1, 0, 22),
				FileSize = 24885248
			}*/
		};

		public static readonly GameBuild NO_GAME_BUILD
			= new GameBuild();




		public static bool Match(string filePath, out bool certainMatch, out GameBuild matched)
		{
			if (!File.Exists(filePath))
			{
				matched = NO_GAME_BUILD;
				certainMatch = false;
				return false;
			}
			
			certainMatch = false;
			var info = new FileInfo(filePath);
			long length = info.Length;
			Version version = new Version(0, 0, 0, 0);
			if (Version.TryParse(FileVersionInfo.GetVersionInfo(filePath).FileVersion, out Version exeVer))
			{
				version = exeVer;
			}
			
			var sizeMatch = GAME_BUILDS.FirstOrDefault(x => x.FileSize == length);
			
			if (sizeMatch == null)
			{
				matched = null;
				return false;
			}

			certainMatch = sizeMatch.Version == version;
			
			matched = sizeMatch;
			return true;
		}



		public readonly Version Version = null;
		public readonly long FileSize = -1;

		long _latterFileSize = -1;
		public long LatterFileSize
		{
			get => (_latterFileSize != -1) ? _latterFileSize : FileSize;
			private set
			{
				if (_latterFileSize == -1)
					_latterFileSize = value;
			}
		}

		public readonly DistributionPlatform Platform = DistributionPlatform.Unknown;
		public readonly KnownPatch Patch = KnownPatch.Unknown;
		public readonly string ExeName = null;
		
		string _latterExeName = null;
		public string LatterExeName
		{
			get => (_latterExeName != null) ? _latterExeName : ExeName;
			private set
			{
				if (_latterExeName == null)
					_latterExeName = value;
			}
		}
		public readonly string CoreDllName = null;
		public readonly string LegacyModDllSuffix = null;


		private GameBuild(Version version,
						long fileSize,
						DistributionPlatform platform,// = DistributionPlatform.Unknown,
						KnownPatch patch,// = KnownPatch.Unknown,
						string coreDllName,
						string legacyModDllSuffix = null,
						string exeName = null
						)
		{
			Version = version;
			FileSize = fileSize;
			Platform = platform;
			Patch = patch;
			ExeName = (exeName == null) ? SPOREAPP : exeName;
			CoreDllName = coreDllName;
			LegacyModDllSuffix = legacyModDllSuffix;
		}

		private GameBuild()
		{
			if (NO_GAME_BUILD != null)
				throw new Exception("Cannot create more than one empty '{nameof(typeof(GameBuild))}'.");

			Platform = DistributionPlatform.Unknown;
			Version = new Version(0, 0, 0, 0);
			FileSize = -1;
		}



		const string LEGACY_DLL_SUFFIX_DISK1_5_1 = "-disk";
		const string LEGACY_DLL_SUFFIX_MARCH2017 = "-steam_patched";

		const string CORE_DLL_DISK1_5_1 = "SporeModAPI.disk.dll";
		const string CORE_DLL_MARCH2017 = "SporeModAPI.march2017.dll";


		public const string SPOREAPP = "SporeApp.exe";
		public const string SPOREAPP_MODAPIFIX = "SporeApp_ModAPIFix.exe";
	}
}
#endif