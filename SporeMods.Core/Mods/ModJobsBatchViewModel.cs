using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
    public class ModJobsBatchViewModel : ModalViewModel<List<ModJobBatchModEntry>>
    {
        IEnumerable<ModJobBatchEntryBase> _entries = null;
        public IEnumerable<ModJobBatchEntryBase> Entries
        {
            get => _entries;
            protected set
            {
                _entries = value;
                NotifyPropertyChanged();
            }
        }


        bool _isAnalysisComplete = false;
        public bool IsAnalysisComplete
        {
            get => _isAnalysisComplete;
            protected set
            {
                _isAnalysisComplete = value;
                NotifyPropertyChanged();
            }
        }


        object _confirmCommand = null;
        public object ConfirmCommand
        {
            get => _confirmCommand;
            protected set
            {
                _confirmCommand = value;
                NotifyPropertyChanged();
            }
        }


        /*object _cancelCommand = null;
        public object CancelCommand
        {
            get => _cancelCommand;
            protected set
            {
                _cancelCommand = value;
                NotifyPropertyChanged();
            }
        }*/


        public ModJobsBatchViewModel()
            : base()
        {
            Title = "Install mods (NOT LOCALIZED)";
            DismissCommand = Externals.CreateCommand<List<ModJobBatchModEntry>>(o => CompletionSource.TrySetResult(new List<ModJobBatchModEntry>()));
            CanDismiss = false;
        }

        public void OnAnalysisFinished(IEnumerable<ModJobBatchEntryBase> entries)
        {
            Entries = entries;
            IsAnalysisComplete = true;
            CanDismiss = true;

            ConfirmCommand = Externals.CreateCommand<List<ModJobBatchModEntry>>(async o =>
            {
                var finalEntries = 
                    await Task<List<ModJobBatchModEntry>>.Run(() =>
                    {
                        return Entries.OfType<ModJobBatchModEntry>()
                        .Where(x => x.CanProceed && x.ShouldProceed)
                        /*{
                            if (x is InstallBatchModEntry modEntry)
                            {
                                return modEntry.CanInstall && modEntry.ShouldInstall;
                            }
                            else
                                return false;
                        })*/
                        .ToList();
                    });
                
                CompletionSource.TrySetResult(finalEntries);
            });
        }

        public override string GetViewTypeName()
        {
            return "SporeMods.Views.ModJobsBatchView";
        }
    }



    public abstract class ModJobBatchEntryBase : NotifyPropertyChangedBase
    {
        bool _canProceed = false;
        public bool CanProceed
        {
            get => _canProceed;
            protected set
            {
                _canProceed = value;
                NotifyPropertyChanged();
            }
        }


        string _cantProceedReason = string.Empty;
        public string CantProceedReason
        {
            get => _cantProceedReason;
            protected set
            {
                _cantProceedReason = value;
                NotifyPropertyChanged();
            }
        }
    }

    public class ModJobBatchErrorEntry : ModJobBatchEntryBase
    {
        string _errorTextKey = string.Empty;
        string _modFileName = string.Empty;
        string _modFilePath = string.Empty;
        Exception _exception = null;
        string _exceptionDetails = "'None (PLACEHOLDER) (NOT LOCALIZED)'";
        public ModJobBatchErrorEntry(string modFilePath, string errorTextKey, Exception exception)
        {
            _modFilePath = modFilePath;
            _modFileName = Path.GetFileNameWithoutExtension(_modFilePath);
            _errorTextKey = errorTextKey;
            _exception = exception;
            
            if (_exception != null)
                _exceptionDetails = $"{_exception.GetType()}: '{_exception.Message}'";
            
            CanProceed = false;
            Refresh();
        }

        void Refresh()
        {
            CantProceedReason = string.Format("Cannot install the mod '{1}' because '{3}'. Exception details: {2}. (PLACEHOLDER) (NOT LOCALIZED)", _modFilePath, _modFileName, _exceptionDetails, _errorTextKey);
        }

        public override string ToString()
            => CantProceedReason;
    }


    public class ModJobBatchModEntry : ModJobBatchEntryBase
    {
        bool _shouldProceed = true;
        public bool ShouldProceed
        {
            get => _shouldProceed;
            set
            {
                _shouldProceed = value;
                NotifyPropertyChanged();
            }
        }



        ISporeMod _mod = null;
        public ISporeMod Mod
        {
            get => _mod;
            protected set
            {
                _mod = value;
                NotifyPropertyChanged();
            }
        }

        public readonly List<ISporeMod> UpgradingFrom = new List<ISporeMod>();

        public readonly string ModPath = string.Empty;
        public ModJobBatchModEntry(string modPath, ISporeMod mod)
        {
            ModPath = modPath;
            Mod = mod;
            CanProceed = true; //TODO: Future user settings
            ShouldProceed = CanProceed;
        }

        public override string ToString()
        {
            var mod = Mod;
            return $"{mod.DisplayName} ({mod.Unique}):\n{{\n\tIsExperimental: {mod.IsExperimental}\n\tCausesSaveDataDependency: {mod.CausesSaveDataDependency}\n\tRequiresGalaxyReset: {mod.RequiresGalaxyReset}\n}}";
        }
    }
}
