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
        private class ApplyOp : IAsyncOperation
        {
            PreIdentityMod _mod = null;
            ModTransaction _transaction = null;
            List<BackupFile> _backupFiles = new List<BackupFile>();

            public ApplyOp(PreIdentityMod mod, ModTransaction transaction)
            {
                _mod = mod;
                _transaction = transaction;
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
                        string modConfigsSubdir = Path.Combine(Settings.ModConfigsPath, _mod.RecordDirName);
                        
                        double progressStep = JobBase.PROGRESS_OVERALL_MAX / (_mod.PackageNames.Count() + _mod.DllNames.Count());
                        foreach (string name in _mod.PackageNames)
                        {
                            string fromPath = Path.Combine(modConfigsSubdir, name);
                            string toPath = Path.Combine(GameInfo.GalacticAdventuresData, name);
                            _backupFiles.Add(BackupFiles.BackupFile(toPath));
                            FileWrite.SafeCopyModFile(fromPath, toPath);
                            _transaction.Job.ActivityRangeProgress += progressStep;
                        }

                        foreach (string name in _mod.DllNames)
                        {
                            string fromPath = Path.Combine(modConfigsSubdir, name);
                            string toPath = Path.Combine(Settings.LegacyLibsPath, name);
                            _backupFiles.Add(BackupFiles.BackupFile(toPath));
                            FileWrite.SafeCopyModFile(fromPath, toPath);
                            _transaction.Job.ActivityRangeProgress += progressStep;
                        }

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
