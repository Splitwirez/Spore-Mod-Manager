using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml.Linq;
using SporeMods.Core.Mods;
using System.Threading;

namespace SporeMods.Core.ModInstallationaa
{
	/// <summary>
	/// Extracts the ModInfo.xml file from the mod zip archive, generating one if the zip doesn't have one.
	/// If the zip is null, it also generates the file.
	/// Undoing this action removes the extracted file.
	/// You can optionally specify a CountdownEvent; if you do, it will send one signal when the file is extracted.
	/// </summary>
	public class ExtractXmlIdentityOp : IModSyncOperation
    {
        private readonly ZipArchive zip;
		// Output folder
        private readonly string outputDirPath;
		private readonly string unique;
		private readonly string displayName;
		private readonly CountdownEvent countdownLatch;

		public ExtractXmlIdentityOp(ZipArchive zip, string outputDirPath, string unique, string displayName, CountdownEvent countdownLatch = null)
        {
            this.zip = zip;
            this.outputDirPath = outputDirPath;
			this.unique = unique;
			this.displayName = displayName;
			this.countdownLatch = countdownLatch;
		}

		private static void CreateModInfoXml(string unique, string displayName, string dir, out XDocument document)
		{
			document = XDocument.Parse(@"<mod>
</mod>");
			document.Root.SetAttributeValue("unique", unique);
			document.Root.SetAttributeValue("displayName", displayName);
			document.Root.SetAttributeValue("installerSystemVersion", ModIdentity.XmlModIdentityVersion1_1_0_0.ToString());
			document.Root.SetAttributeValue("copyAllFiles", true.ToString());
			document.Root.SetAttributeValue("canDisable", false.ToString());
		}

		public bool Do()
        {
			if (zip != null && zip.TryGetEntry(ManagedMod.MOD_INFO, out ZipArchiveEntry entry))
			{
				string xmlOutPath = Path.Combine(outputDirPath, ManagedMod.MOD_INFO);
				entry.ExtractToFile(xmlOutPath, true);
				Permissions.GrantAccessFile(xmlOutPath);
			}
			else
			{
				string legacyPath = Path.Combine(outputDirPath, "UseLegacyDLLs");
				string modInfoPath = Path.Combine(outputDirPath, ManagedMod.MOD_INFO);
				File.WriteAllText(legacyPath, string.Empty);
				CreateModInfoXml(unique, displayName, outputDirPath, out XDocument document);
				document.Save(modInfoPath);
				Permissions.GrantAccessFile(legacyPath);
				Permissions.GrantAccessFile(modInfoPath);
			}
			if (countdownLatch != null) countdownLatch.Signal();
			return true;
		}

		public void Undo()
        {
			string xmlOutPath = Path.Combine(outputDirPath, ManagedMod.MOD_INFO);
			if (File.Exists(xmlOutPath))
            {
				File.Delete(xmlOutPath);
            }
		}
    }
}
