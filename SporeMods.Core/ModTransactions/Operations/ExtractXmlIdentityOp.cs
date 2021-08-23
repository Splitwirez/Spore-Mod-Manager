using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml.Linq;
using SporeMods.Core.Mods;
using System.Threading;

namespace SporeMods.Core.ModTransactions.Operations
{
	/// <summary>
	/// Extracts the ModInfo.xml file from the mod zip archive, generating one if the zip doesn't have one.
	/// If the zip is null, it also generates the file.
	/// Undoing this action removes the extracted file.
	/// You can optionally specify a CountdownEvent; if you do, it will send one signal when the file is extracted.
	/// </summary>
	public class ExtractXmlIdentityOp : IModSyncOperation
    {
        public readonly ZipArchive zip;
		// Output folder
		public readonly string outputDirPath;
		public readonly string unique;
		public readonly string displayName;
		public readonly CountdownEvent countdownLatch;
		private ModBackupFile backup;

		public ExtractXmlIdentityOp(ZipArchive zip, string outputDirPath, string unique, string displayName, CountdownEvent countdownLatch = null)
        {
            this.zip = zip;
            this.outputDirPath = outputDirPath;
			this.unique = unique;
			this.displayName = displayName;
			this.countdownLatch = countdownLatch;
		}

		public bool Do()
        {
			string xmlOutPath = Path.Combine(outputDirPath, ManagedMod.MOD_INFO);
			backup = ModBackupFiles.CreateBackup(xmlOutPath);
			if (zip != null && zip.TryGetEntry(ManagedMod.MOD_INFO, out ZipArchiveEntry entry))
			{
				entry.ExtractToFile(xmlOutPath, true);
				Permissions.GrantAccessFile(xmlOutPath);
			}
			else
			{
				string legacyPath = Path.Combine(outputDirPath, ManagedMod.PATH_USELEGACYDLLS);
				File.WriteAllText(legacyPath, string.Empty);
				XmlModIdentity.CreateModInfoXml(unique, displayName, outputDirPath, out XDocument document);
				document.Save(xmlOutPath);
				Permissions.GrantAccessFile(legacyPath);
				Permissions.GrantAccessFile(xmlOutPath);
			}
			if (countdownLatch != null) countdownLatch.Signal();
			return true;
		}

		public void Undo()
        {
			backup.Restore();
			ModBackupFiles.DisposeBackup(backup);
		}
    }
}
