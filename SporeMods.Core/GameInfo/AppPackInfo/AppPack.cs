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
	internal class AppPack
	{
		AppPackDir _binDir = null;
		AppPackDir _dataDir = null;

		internal static AppPack CreateForOneFolder(IEnumerable<string> regSubkeyNames,
						Dictionary<string, bool> regValNames,
						IEnumerable<string> folderNames,
						bool isData)
		{
			/*string[] subkeyNamesThatExist = SporeAppPackConsts.SporeRegistryRootKey.GetSubKeyNames();
			if (!subkeyNamesThatExist.Any(keyThatExists => regSubkeyNames.Any(inSubkeyName => keyThatExists.Equals(inSubkeyName, StringComparison.OrdinalIgnoreCase))))
			{
				//TODO: 
				throw new NotImplementedException("SEE TODO");
			}*/
			return new AppPack(regSubkeyNames, regValNames, folderNames, isData);
			/*for (int i = 0; i < subkeyNamesThatExist.Length; i++)
			{
				Console.WriteLine($"Subkey {i}: {subkeyNamesThatExist[i]}");
			}*/
			
		}


		internal static AppPack CreateForBothFolders(IEnumerable<string> regSubkeyNames,
						Dictionary<string, bool> dataRegValNames,
						IEnumerable<string> dataFolderNames,
						Dictionary<string, bool> binRegValNames,
						IEnumerable<string> binFolderNames)	
		{
			return new AppPack(regSubkeyNames, dataRegValNames, dataFolderNames, binRegValNames, binFolderNames);
		}
		internal AppPack(IEnumerable<string> regSubkeyNames,
						Dictionary<string, bool> regValNames,
						IEnumerable<string> folderNames,
						bool isData)
		{
			var packDir = new AppPackDir(regSubkeyNames, regValNames, folderNames, !isData);
			
			if (isData)
				_dataDir = packDir;
			else
				_binDir = packDir;
		}
		
		internal AppPack(IEnumerable<string> regSubkeyNames,
						Dictionary<string, bool> dataRegValNames,
						IEnumerable<string> dataFolderNames,
						Dictionary<string, bool> binRegValNames,
						IEnumerable<string> binFolderNames)
			: this(regSubkeyNames, dataRegValNames, dataFolderNames, true)
		{
			_binDir = new AppPackDir(regSubkeyNames, binRegValNames, binFolderNames, true);
			//_binDir._lol = true;
		}


		//count of -1 or less = no such folder, 0 = no paths found, 1 = all good, 2 or more = multiple possible paths were found
		internal void ResolvePack(out List<string> dataPaths, out List<string> binPaths)
		{
			if (_dataDir != null)
			{
				Console.WriteLine($"Data:");
				dataPaths = _dataDir.ResolvePaths();
				
				foreach (string path in dataPaths)
					Console.WriteLine($"\t{path}");
			}
			else
				dataPaths = null;

			if (_binDir != null)
			{
				Console.WriteLine($"Bin:");
				binPaths = _binDir.ResolvePaths();

				foreach (string path in binPaths)
					Console.WriteLine($"\t{path}");
			}
			else
				binPaths = null;
		}
	}
}
#endif