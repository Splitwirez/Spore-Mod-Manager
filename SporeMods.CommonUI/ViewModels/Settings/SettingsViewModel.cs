using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SporeMods.ViewModels
{
    public class SettingsViewModel : NotifyPropertyChangedBase
    {
        ObservableCollection<NotifyPropertyChangedBase> _settingsVMs = new ObservableCollection<NotifyPropertyChangedBase>()
        {
            new LanguageSettingsViewModel(),
            new GamePathSettingsViewModel(),
            new GameWindowSettingsViewModel(),
            new GameEntrySettingsViewModel(),
            SmmAppearanceSettingsViewModel.Instance,
            new UpdateDeliverySettingsViewModel()
        };
        public ObservableCollection<NotifyPropertyChangedBase> SettingsVMs
        {
            get => _settingsVMs;
        }

        CreditsViewModel _creditsVM = new CreditsViewModel();
        public CreditsViewModel CreditsVM
        {
            get => _creditsVM;
        }
    }
}
