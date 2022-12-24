using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using SporeMods.Core;
using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    public partial class PreIdentityMod
    {
        string PACKAGE_DEST = "GA_DATA";
        string DLL_DEST = "LEGACY_DLL";


        public async Task<Exception> ExtractRecordFilesAsync(ModTransaction transaction, string inPath, ZipArchive archive = null)
        {
            bool isPackage = Path.GetExtension(inPath).Equals(ModUtils.MOD_FILE_EX_DBPF, StringComparison.OrdinalIgnoreCase);
            bool isSporemod = Path.GetExtension(inPath).Equals(ModUtils.MOD_FILE_EX_SPOREMOD, StringComparison.OrdinalIgnoreCase);
            //List<BackupFile> _backupFiles = new List<BackupFile>();
            string recordDirPath = string.Empty;


            return await Task<Exception>.Run(async () =>
            {
                try
                {
                    recordDirPath = Path.Combine(Settings.ModConfigsPath, RecordDirName);
                    transaction.Operation(new CreateDirectoryOp(recordDirPath));

                    string extractFilePath;
                    double progressQuantity = JobBase.PROGRESS_OVERALL_MAX;

                    XDocument identityDoc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<mod installerSystemVersion=\"1.1.0.0\"/>");

                    var identityRoot = identityDoc.Root;
                    identityRoot.SetAttributeValue(ModUtils.AT_DISP_NAME, DisplayName);
                    identityRoot.SetAttributeValue(ModUtils.AT_UNIQUE, Unique);
                    identityRoot.SetAttributeValue("copyAllFiles", true.ToString());
                    identityRoot.SetAttributeValue("canDisable", false.ToString());
                    identityRoot.SetAttributeValue("originalFileName", Path.GetFileName(inPath));


                    void increment(string fileName, string dest)
                    {
                        var el = new XElement("file", fileName);
                        el.SetAttributeValue("destination", dest);
                        identityRoot.Add(el);
                        transaction.Job.ActivityRangeProgress += progressQuantity;
                    }

                    
                    if (isPackage)
                    {
                        string fileName = Path.GetFileName(inPath);
                        extractFilePath = Path.Combine(recordDirPath, fileName);

                        transaction.Operation(new CopyFileOp(inPath, extractFilePath));
                        increment(fileName, PACKAGE_DEST);
                    }
                    else if (isSporemod)
                    {
                        var entries = archive.Entries.Where(x => !x.IsDirectory());

                        progressQuantity = JobBase.PROGRESS_OVERALL_MAX / (entries.Count() + 1);
                        foreach (var entry in entries)
                        {
                            string fileName = Path.GetFileName(entry.FullName);
                            extractFilePath = Path.Combine(recordDirPath, fileName);

                            await transaction.OperationAsync(new ExtractFileOp(entry, extractFilePath));
                            increment(fileName,
                                Path.GetExtension(fileName).Equals(".package", StringComparison.OrdinalIgnoreCase)
                                    ? PACKAGE_DEST
                                    : DLL_DEST);
                        }
                    }

                    extractFilePath = Path.Combine(recordDirPath, ModUtils.ID_XML_FILE_NAME);
                    await transaction.OperationAsync(new SaveXDocumentOp(identityDoc, extractFilePath));
                    transaction.Job.ActivityRangeProgress += progressQuantity;


                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            });
        }
    }
}
