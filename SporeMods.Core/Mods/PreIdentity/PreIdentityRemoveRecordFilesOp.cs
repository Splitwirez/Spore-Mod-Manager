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
        private class RemoveRecordFilesOp : IAsyncOperation
        {
            PreIdentityMod _mod = null;
            ModTransaction _transaction = null;
            List<BackupFile> _backupFiles = new List<BackupFile>();
            string _recordDirPath = string.Empty;
            bool _createdDir = false;
            bool _deleteConfig = false;
            
            public RemoveRecordFilesOp(PreIdentityMod mod, ModTransaction transaction, bool deleteConfig)
            {
                _mod = mod;
                _transaction = transaction;
                _deleteConfig = deleteConfig;
                _recordDirPath = Path.Combine(Settings.ModConfigsPath, _mod.RecordDirName);
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
                        
                        double progressStep = JobBase.PROGRESS_OVERALL_MAX / (_mod.PackageNames.Count() + _mod.DllNames.Count() + 1);

                        string targetPath;
                        foreach (string name in _mod.PackageNames)
                        {
                            targetPath = Path.Combine(_recordDirPath, name);
                            _backupFiles.Add(BackupFiles.BackupFile(targetPath));
                            if (File.Exists(targetPath))
                                File.Delete(targetPath);
                            _transaction.Job.ActivityRangeProgress += progressStep;
                        }

                        foreach (string name in _mod.DllNames)
                        {
                            targetPath = Path.Combine(_recordDirPath, name);
                            _backupFiles.Add(BackupFiles.BackupFile(targetPath));
                            if (File.Exists(targetPath))
                                File.Delete(targetPath);
                            _transaction.Job.ActivityRangeProgress += progressStep;
                        }

                        targetPath = Path.Combine(_recordDirPath, ModConstants.ID_XML_FILE_NAME);
                        _backupFiles.Add(BackupFiles.BackupFile(targetPath));
                        if (File.Exists(targetPath))
                            File.Delete(targetPath);
                        else
                            Cmd.WriteLine($"'{targetPath}' doesnt't exist!");
                        _transaction.Job.ActivityRangeProgress += progressStep;

                        if (_deleteConfig)
                            Directory.Delete(_recordDirPath, true);

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

                if (_createdDir)
                    Directory.Delete(_recordDirPath);
            }

            public void Dispose()
            {
                foreach (var bkp in _backupFiles)
                {
                    bkp.Dispose();
                }
            }
        }
    }
}
