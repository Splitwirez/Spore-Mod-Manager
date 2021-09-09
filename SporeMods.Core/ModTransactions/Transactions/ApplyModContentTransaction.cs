using SporeMods.Core.Mods;
using SporeMods.Core.ModTransactions.Operations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.ModTransactions.Transactions
{
	public class ApplyModContentTransaction : ModTransaction
	{
		public readonly ManagedMod mod;

		public ApplyModContentTransaction(ManagedMod mod)
		{
			this.mod = mod;
		}

		public override async Task<bool> CommitAsync()
		{
			// It is possible that this is called RegisterSporemodModAsync, and some progress has already happened
			double totalProgress = 100.0;

			bool startsHere = !mod.IsProgressing;
			if (startsHere)
			{
				mod.Progress = 0;
				mod.IsProgressing = true;
			}
			else
			{
				totalProgress = 100.0 - mod.Progress;
			}

			// 1. Delete all mod-related files
			var progressRange = totalProgress * 0.1;
			var filesToDelete = mod.GetFilePathsToRemove();
			foreach (var file in filesToDelete)
			{
				Operation(new SafeDeleteFileOp(file));
				mod.Progress += progressRange / filesToDelete.Count;
			}

			// 2. Add the mod files
			progressRange = totalProgress * 0.9;
			if (mod.MustCopyAllFiles)
			{
				var files = Directory.EnumerateFiles(mod.StoragePath).Where(x => ModsManager.IsModFile(x));
				var progressInc = progressRange / files.Count();

				foreach (string s in files)
				{
					if (Path.GetExtension(s).ToLowerInvariant() == ".package")
						Operation(new SafeCopyFileOp(s, FileWrite.GetFileOutputPath(ComponentGameDir.GalacticAdventures, Path.GetFileName(s), mod.IsLegacy)));
					else if (Path.GetExtension(s).ToLowerInvariant() == ".dll")
						Operation(new SafeCopyFileOp(s, FileWrite.GetFileOutputPath(ComponentGameDir.ModAPI, Path.GetFileName(s), mod.IsLegacy)));

					mod.Progress += progressInc;
				}
			}
			else
			{
				EnableModAdvanced(progressRange);
			}

			// 3. Change the configuration and save it
			var newConfig = new ModConfiguration(mod.Configuration);
			newConfig.IsEnabled = true;
			Operation(new ChangeModConfigurationOp(mod, newConfig));

			mod.Progress = 0;
			mod.IsProgressing = false;

			return true;
		}

		//TODO: Implement this for XML Mod Identity v2.0.0.0, in a generalized way for components
		private void EnableModAdvanced(double progressRange)
		{
			int fileCount = GetEnabledComponentFileCount();
			double progressIncrease = progressRange / fileCount;

			foreach (var file in mod.Identity.FilesToRemove)
			{
				RemoveModFile(file);
				mod.Progress += progressIncrease;
			}

			foreach (var file in mod.Identity.Files)
			{
				AddModFile(file);
				mod.Progress += progressIncrease;
			}

			foreach (var component in mod.Identity.SubComponents)
			{
				if (component.IsGroup)
				{
					foreach (var subComponent in component.SubComponents)
					{
						if (subComponent.IsEnabled)
						{
							foreach (var file in subComponent.Files)
							{
								AddModFile(file);
								mod.Progress += progressIncrease;
							}
							break;
						}
					}
				}
				else if (component.IsEnabled)
				{
					foreach (var file in component.Files)
					{
						AddModFile(file);
						mod.Progress += progressIncrease;
					}
				}
			}

			EvaluateCompatibilityFixes(progressIncrease);
		}

		/// <summary>
		/// Applies all compatibility fixes in the mod that can be applied.
		/// </summary>
		/// <param name="progressIncrease"></param>
		private void EvaluateCompatibilityFixes(double progressIncrease)
		{
			foreach (var fix in mod.Identity.CompatibilityFixes)
			{
				bool proceed = true;
				foreach (var file in fix.RequiredFiles)
				{
					bool fileDoesntExist = !File.Exists(FileWrite.GetFileOutputPath(file.GameDir, file.Name, mod.IsLegacy));
					if ((!mod.IsLegacy) && (!fileDoesntExist) && (file.GameDir == ComponentGameDir.ModAPI))
						fileDoesntExist = !File.Exists(FileWrite.GetFileOutputPath(file.GameDir, file.Name, true));
					if (fileDoesntExist)
					{
						proceed = false;
						break;
					}
				}
				if (proceed)
				{
					foreach (var file in fix.FilesToRemove)
					{
						RemoveModFile(file);
						mod.Progress += progressIncrease;
					}
					foreach (var file in fix.FilesToAdd)
					{
						AddModFile(file);
						mod.Progress += progressIncrease;
					}
				}
			}
		}

		/// <summary>
		/// Removes all the files that match with the ModFile pattern.
		/// If the mod is legacy and the file goes in the ModAPI folder, they will be removed from the legacy folder.
		/// </summary>
		/// <param name="file"></param>
		private void RemoveModFile(ModFile file)
		{
			DirectoryInfo info = new DirectoryInfo(FileWrite.GetGameDirectory(file.GameDir, mod.IsLegacy));
			foreach (FileInfo f in info.EnumerateFiles(file.Name))
			{
				Operation(new SafeDeleteFileOp(f.FullName));
			}

			if (file.GameDir == ComponentGameDir.ModAPI && mod.IsLegacy)
			{
				foreach (FileInfo f in new DirectoryInfo(Settings.LegacyLibsPath).EnumerateFiles(file.Name))
				{
					Operation(new SafeDeleteFileOp(Path.Combine(Settings.LegacyLibsPath, f.Name)));
				}
			}
		}

		/// <summary>
		/// Adds a mod file into the game files.
		/// </summary>
		/// <param name="file"></param>
		private void AddModFile(ModFile file)
		{
			Operation(new SafeCopyFileOp(
							Path.Combine(mod.StoragePath, file.Name),
							FileWrite.GetFileOutputPath(file.GameDir, file.Name, mod.IsLegacy)
							));
		}

		private int GetEnabledComponentFileCount(BaseModComponent component)
		{
			int count = component.Files.Count;
			foreach (var child in component.SubComponents)
			{
				count += GetEnabledComponentFileCount(child);
			}
			return count;
		}

		/// <summary>
		/// Returns how many files have to be added or removed by the enabled components of this mod.
		/// </summary>
		/// <returns></returns>
		private int GetEnabledComponentFileCount()
		{
			int count = mod.Identity.FilesToRemove.Count;
			foreach (var fix in mod.Identity.CompatibilityFixes)
			{
				count += fix.FilesToAdd.Count;
				count += fix.FilesToRemove.Count;
			}
			return count + GetEnabledComponentFileCount(mod.Identity);
		}
	}
}