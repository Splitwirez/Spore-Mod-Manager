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
	internal class AppPackDirPath
	{
		readonly string _regValName;
		readonly IEnumerable<string> _regSubkeyNames;
		readonly IEnumerable<string> _folderNames;
		readonly bool _isAlreadySubfolder;
		internal AppPackDirPath(string regValName,
						IEnumerable<string> regSubkeyNames,
						IEnumerable<string> folderNames,
						bool isAlreadySubfolder)
		{
			_regValName = regValName;
			_regSubkeyNames = regSubkeyNames;
			_folderNames = folderNames;
			_isAlreadySubfolder = isAlreadySubfolder;
			
		}

		public bool _lol = false;
		internal List<string> ResolvePaths()
		{
			/*foreach (string fn in _folderNames)
			{
				Console.WriteLine($"{fn} AAAAAAA");
			}*/

			List<string> finalPaths = new List<string>();
			
			var applicableSubkeys = SporeAppPackConsts.MatchSporeSubkeyNames(_regSubkeyNames);
			/*foreach (string aps in applicableSubkeys)
			{
				Console.WriteLine($"{aps} B");
			}*/
			
			foreach (string subkeyName in applicableSubkeys)
			{
				using (RegistryKey key = SporeAppPackConsts.OpenSporeSubkey(subkeyName))
				{
					var keyValRaw = key.GetValue(_regValName, null, RegistryValueOptions.None);
					if ((keyValRaw != null) && (keyValRaw is string keyVal))
					{	
						keyVal = keyVal.Replace('/', '\\').Trim().Trim('"').TrimEnd('\\');
						List<string> subkeyFinalPaths = new List<string>();

						
						if (!Directory.Exists(keyVal))
						{
							//Console.WriteLine($"DIRECTORYN'T: {keyVal}");
							continue;
						}
						

						bool anyMatched = false;
						if (_isAlreadySubfolder)
						{
							string keyValName = Path.GetFileName(keyVal);
							foreach (string folderName in _folderNames)
							{
								if (keyValName.Equals(folderName, StringComparison.OrdinalIgnoreCase))
								{
									subkeyFinalPaths.Add(keyVal);
									anyMatched = true;
									break;
								}
								else if (_lol)
									Console.WriteLine($"non-match: '{folderName}'");
								//string firstMatch = _folderNames.FirstOrDefault(x => keyValName.Equals(x, StringComparison.OrdinalIgnoreCase));
								/*if (firstMatch != null)
								{
									subkeyFinalPaths.Add(firstMatch);
								}*/
							}
						}
						if ((!anyMatched) || (!_isAlreadySubfolder))
						{
							keyVal = keyVal.Substring(0, keyVal.LastIndexOf('\\')).TrimEnd('\\');
							if (_lol)
							Console.WriteLine($"Trimmed keyVal: '{keyVal}', _isAlreadySubfolder: '{_isAlreadySubfolder}'");
							foreach (string folderName in _folderNames)
							{
								string keyValCombined = Path.Combine(keyVal, folderName);
								if (_lol)
								Console.WriteLine($"folderName: '{folderName}', keyValCombined: '{keyValCombined}'");
								if (Directory.Exists(keyValCombined))
								{
									subkeyFinalPaths.Add(keyValCombined);
									break;
								}
									
								//.Any(x => keyValName.Equals(x, StringComparison.OrdinalIgnoreCase)))

							}
						}

						foreach (string subkeyFinalPath in subkeyFinalPaths)
						{
							if (!finalPaths.Any(x => x.Equals(subkeyFinalPath, StringComparison.OrdinalIgnoreCase)))
								finalPaths.Add(subkeyFinalPath);
						}
					}
				}
			}
			
			return finalPaths;
		}
	}
}
#endif