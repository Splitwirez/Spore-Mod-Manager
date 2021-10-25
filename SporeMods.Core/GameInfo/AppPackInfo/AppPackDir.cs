#if SMMM_MORE
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SporeMods.Core
{
	/*internal enum PackDirType
	{
		Data,
		Bin
	}*/


	internal class AppPackDir
	{
		readonly IEnumerable<string> _regSubkeyNames;
		readonly Dictionary<string, bool> _regValNames;
		readonly IEnumerable<string> _folderNames;

		List<AppPackDirPath> _dirPaths = new List<AppPackDirPath>();
		bool _lol = false;

		internal AppPackDir(IEnumerable<string> regSubkeyNames,
						Dictionary<string, bool> regValNames,
						IEnumerable<string> folderNames,
						bool lol = false)
		{
			_lol = lol;
			_regSubkeyNames = regSubkeyNames;
			_regValNames = regValNames;
			_folderNames = folderNames;
			/*foreach (string fn in _folderNames)
			{
				Console.WriteLine($"folder name: '{fn}'");
			}*/

			foreach (string regValName in _regValNames.Keys)
			{
				//Console.WriteLine($"key: '{regValName}',    value: {_regValNames[regValName]}");
				var dirPath = new AppPackDirPath(regValName, _regSubkeyNames, _folderNames, _regValNames[regValName]);
				dirPath._lol = _lol;
				_dirPaths.Add(dirPath);
			}
		}
		
		internal List<string> ResolvePaths()
		{
			bool resolved = true;
			List<string> paths = new List<string>();
			foreach (AppPackDirPath dirPath in _dirPaths)
			{
				paths.AddRange(dirPath.ResolvePaths());
			}
			return paths;
		}
	}
}
#endif