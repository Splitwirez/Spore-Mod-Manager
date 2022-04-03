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
        private class ExtractRecordFilesOp : IAsyncOperation
        {
            PreIdentityMod _mod = null;
            ModTransaction _transaction = null;
            string _inPath = string.Empty;
            ZipArchive _archive = null;
            bool _isPackage = false;
            bool _isSporemod = false;
            List<BackupFile> _backupFiles = new List<BackupFile>();
            string _recordDirPath = string.Empty;
            bool _createdDir = false;
            
            public ExtractRecordFilesOp(PreIdentityMod mod, ModTransaction transaction, string inPath, ZipArchive archive = null)
            {
                _mod = mod;
                _transaction = transaction;
                _inPath = inPath;
                _archive = archive;

                _isPackage = Path.GetExtension(_inPath).Equals(ModConstants.MOD_FILE_EX_DBPF, StringComparison.OrdinalIgnoreCase);
                _isSporemod = Path.GetExtension(_inPath).Equals(ModConstants.MOD_FILE_EX_SPOREMOD, StringComparison.OrdinalIgnoreCase);
            }

            Exception _exception = null;
            public Exception Exception
            {
                get => _exception;
            }

            public async Task<bool> DoAsync()
            {
                return await Task<bool>.Run(() =>
                {
                    try
                    {
                        _recordDirPath = Path.Combine(Settings.ModConfigsPath, _mod.RecordDirName);
                        if (!Directory.Exists(_recordDirPath))
                        {
                            Directory.CreateDirectory(_recordDirPath);
                            _createdDir = true;
                        }

                        string extractFilePath = null;
                        double progressQuantity = JobBase.PROGRESS_OVERALL_MAX;
                        if (_isPackage)
                        {
                            string fileName = Path.GetFileName(_inPath);
                            extractFilePath = Path.Combine(_recordDirPath, fileName);

                            _backupFiles.Insert(0, BackupFiles.BackupFile(extractFilePath));
                            
                            File.Copy(_inPath, extractFilePath, true);
                            _transaction.Job.ActivityRangeProgress += progressQuantity;
                        }
                        else if (_isSporemod)
                        {
                            var entries = _archive.Entries.Where(x => !x.IsDirectory());

                            progressQuantity = JobBase.PROGRESS_OVERALL_MAX / (entries.Count() + 1);
                            foreach (var entry in entries)
                            {
                                extractFilePath = Path.Combine(_recordDirPath, Path.GetFileName(entry.FullName));

                                //if (File.Exists(outputPath))
                                _backupFiles.Insert(0, BackupFiles.BackupFile(extractFilePath));

                                entry.ExtractToFile(extractFilePath, true);
                                _transaction.Job.ActivityRangeProgress += progressQuantity;
                            }
                        }

                        extractFilePath = Path.Combine(_recordDirPath, ModConstants.ID_XML_FILE_NAME);
                        _backupFiles.Insert(0, BackupFiles.BackupFile(extractFilePath));

                        XDocument identityDoc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<mod installerSystemVersion=\"1.1.0.0\"/>");
                        
                        var identityRoot = identityDoc.Root;
                        identityRoot.SetAttributeValue(ModConstants.AT_DISP_NAME, _mod.DisplayName);
                        identityRoot.SetAttributeValue(ModConstants.AT_UNIQUE, _mod.Unique);
                        identityRoot.SetAttributeValue("copyAllFiles", true.ToString());
                        identityRoot.SetAttributeValue("canDisable", false.ToString());

                        identityDoc.Save(extractFilePath);
                        _transaction.Job.ActivityRangeProgress += progressQuantity;


                        return true;
                    }
                    catch (Exception ex)
                    {
                        _exception = ex;
                        return false;
                    }
                });
            }

            public void Undo()
            {
                foreach (var bkp in _backupFiles)
                {
                    bkp.Restore();
                }

                if (_archive != null)
                    _archive.Dispose();

                if (_createdDir)
                    Directory.Delete(_recordDirPath);
            }

            public void Dispose()
            {
                foreach (var bkp in _backupFiles)
                {
                    bkp.Dispose();
                }


                if (_archive != null)
                    _archive.Dispose();
            }
        }
    }
}
