using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.Mods
{
    public class ChangeSettingsForModViewModel : ModalViewModel<bool>
    {
        IConfigurableMod _mod = null;
        public IConfigurableMod Mod
        {
            get => _mod;
            protected set
            {
                _mod = value;
                NotifyPropertyChanged();
            }
        }

        IViewLocatable _modSettingsVM = null;
        public IViewLocatable ModSettingsVM
        {
            get => _modSettingsVM;
            protected set
            {
                _modSettingsVM = value;
                NotifyPropertyChanged();
            }
        }


        public ChangeSettingsForModViewModel(IConfigurableMod mod)
        {
            Mod = mod;
            ModSettingsVM = mod.GetSettingsViewModel(true);
            DismissCommand = Externals.CreateCommand<bool>(o => CompletionSource.TrySetResult(false));
            ConfirmCommand = Externals.CreateCommand<bool>(o => CompletionSource.TrySetResult(true));
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
    }
}
