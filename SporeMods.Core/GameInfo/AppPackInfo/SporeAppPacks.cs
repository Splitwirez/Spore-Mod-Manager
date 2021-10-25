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
	public class SporeAppPacks
	{	
		static SporeAppPacks _instance = null;
		public static SporeAppPacks Instance
		{
			get => _instance;
		}

		public static void Ensure()
		{
			_instance = new SporeAppPacks();
			_instance.Resolve();
		}


		private AppPack _spore = null;
		internal AppPack Spore
		{
			get => _spore;
		}

#if CREEPY_AND_CUTE
		private AppPack _sporeCandC = null;
		internal AppPack SporeCandC
		{
			get => _sporeCandC;
		}
#endif


		private AppPack _sporeGA = null;
		internal AppPack SporeGA
		{
			get => _sporeGA;
		}
		

		private SporeAppPacks()
		{
			_spore = new AppPack(new List<string>() { "SPORE" },
													SporeAppPackConsts.DataRegValNames,
													new List<string>() { "Data" },
													true);
#if CREEPY_AND_CUTE
// http://forum.spore.kylenanakdewa.com/jforum/posts/listByUser/15/100481.html
	// "C:\Program Files (x86)\Origin Games\Spore\BP1Data\BoosterPack_01.package"
	// "/Applications/SPORE/Spore.app/Contents/Resources/transgaming/c_drive/Spore/BP1Data"
	// "/Applications/SPORE/Spore.app/Contents/Resources/transgaming/c_drive/SPORE_BP1/Data"
// https://zhidao.baidu.com/question/105828202.html
	// DataDir="C:\Program Files\Electronic Arts\SPORE_BP1\" ("DataDir"="C:\\Program Files\\Electronic Arts\\SPORE_BP1\\")
// https://www.gog.com/forum/spore/unable_to_find_data_folder_error_1004
	// DataDir="D:\GOGcom\SPORE\bp1content\" (take with a grain of salt - source says the path 'should be something like' what's shown)
// From troubeshooting on Discord
	// "C:\Program Files (x86)\Origin Games\Spore\BP1Data\BoosterPack_01.package"
			_sporeCandC = AppPack.CreateForOneFolder(new List<string>() { "spore Creepy and Cute Parts Pack", "SPORE(TM) Creepy & Cute Parts Pack", "SPORE_BP1" },
													new Dictionary<string, bool>()
													{
														{
															SporeAppPackConsts.REG_NAME_DATADIR,
															true
														},
														{
															SporeAppPackConsts.REG_NAME_DATADIR,
															false
														},
														{
															SporeAppPackConsts.REG_NAME_INSTALLLOC,
															false
														},
														{
															SporeAppPackConsts.REG_NAME_INSTALL_DIR,
															false
														},
													};,
													new List<string>() { "bp1content", "BP1Data", "Data" },
													true);
#endif
			
			_sporeGA = new AppPack(new List<string>() { "SPORE_EP1" },
													SporeAppPackConsts.DataRegValNames,
													new List<string>() { "DataEP1", "Data" },
													SporeAppPackConsts.DataRegValNames,
													new List<string>() { "SporebinEP1" });
		}

		void Resolve()
		{
			if (SporeAppPackConsts.SiezeSporeRegistryInfo())
			{
				if (SporeGA != null)
				{
					Console.WriteLine("\nGA:");
					SporeGA.ResolvePack(out List<string> sporeGA_data, out List<string> sporeGA_bin);

					/*Console.WriteLine("\nGA Data:");
					foreach (string path in sporeGA_data)
						Console.WriteLine($"\t{path}");
					
					Console.WriteLine("\nGA Bin:");
					foreach (string path in sporeGA_bin)
						Console.WriteLine($"\t{path}");*/
				}
				/*var binEP1 = new AppPackDir(new List<string>() { "SPORE_EP1" },
								SporeAppPackConsts.DataRegValNames,
								new List<string>() { "SporebinEP1" });

				Console.WriteLine("\nGA Bin take 2:");
				foreach (string path in binEP1.ResolvePaths())
					Console.WriteLine($"\t{path}");*/

				#if CREEPY_AND_CUTE
				if (SporeCandC != null)
				{
					SporeCandC.ResolvePack(out List<string> sporeCandC_data, out List<string> _c);

					Console.WriteLine("\nC&C Data:");
					foreach (string path in sporeCandC_data)
						Console.WriteLine($"\t{path}");
				}
#endif
				if (Spore != null)
				{
					Console.WriteLine("\nSpore:");
					Spore.ResolvePack(out List<string> spore_data, out List<string> _);
				
					/*Console.WriteLine("\nCore Data:");
					foreach (string path in spore_data)
						Console.WriteLine($"\t{path}");*/
					
					Console.WriteLine("\n");
				}
				
				SporeAppPackConsts.RelinquishSporeRegistryInfo();
			}
			//CoreSpore?.Resolve();
		}
	}


	internal static class SporeAppPackConsts
	{
		internal const string SPORE_REG_ROOT_AMD64 = @"SOFTWARE\Wow6432Node\Electronic Arts";
		internal const string SPORE_REG_ROOT_X86 = @"SOFTWARE\Electronic Arts";



		internal const string REG_NAME_INSTALLLOC = "InstallLoc";
		
		internal const string REG_NAME_INSTALL_DIR = "Install Dir";
		
		//Steam and GOG only have this one
		internal const string REG_NAME_DATADIR = "DataDir";


		internal static readonly Dictionary<string, bool> DataRegValNames = new Dictionary<string, bool>()
		{
			{
				REG_NAME_DATADIR,
				true
			},
			{
				REG_NAME_INSTALLLOC,
				false
			},
			{
				REG_NAME_INSTALL_DIR,
				false
			},
		};

		internal static readonly Dictionary<string, bool> BinRegValNames = new Dictionary<string, bool>()
		{
			{
				REG_NAME_DATADIR,
				true
			},
			{
				REG_NAME_INSTALLLOC,
				false
			},
			{
				REG_NAME_INSTALL_DIR,
				false
			},
		};/*

		static readonly Dictionary<string, bool> DataRegValNames = new Dictionary<string, bool>()
		{
			{
				REG_NAME_INSTALLLOC,
				true
			},
			{
				REG_NAME_INSTALL_DIR,
				true
			},
		};*/

		static RegistryKey _sporeRegistryRootKey = null;
		internal static bool SiezeSporeRegistryInfo(/*out RegistryKey sporeRootKey*/)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(SporeAppPackConsts.SPORE_REG_ROOT_AMD64, false);
			if (key == null)
				key = Registry.LocalMachine.OpenSubKey(SporeAppPackConsts.SPORE_REG_ROOT_X86, false);
			
			_sporeRegistryRootKey = key;
			//sporeRootKey = key;
			//TODO: Handle when we can't even see the 'Electronic Arts' root (which probably means it doesn't exist)
			return key != null;
		}

		internal static void RelinquishSporeRegistryInfo()
			=> _sporeRegistryRootKey.Dispose();

		internal static IEnumerable<string> MatchSporeSubkeyNames(IEnumerable<string> names)
			=> _sporeRegistryRootKey.GetSubKeyNames()
				.Where(keyThatExists =>
					names.Any(inSubkeyName =>
						keyThatExists.Equals(inSubkeyName, StringComparison.OrdinalIgnoreCase)));
		
		internal static RegistryKey OpenSporeSubkey(string name)
			=> _sporeRegistryRootKey.OpenSubKey(name);
		
		/*internal static void ProvideSporeRegistryRootKey(RegistryKey key)
		{
			if (_sporeRegistryRootKey == null)
				_sporeRegistryRootKey = key;
		}*/
	}
}
#endif