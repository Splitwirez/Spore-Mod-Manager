using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SporeMods.Core.ModIdentity;

namespace SporeMods.Core.InstalledMods
{
    public class ManualInstalledFile : IInstalledMod, INotifyPropertyChanged
    {
        string _fileName = string.Empty;
        bool _legacy = false;
        public ManualInstalledFile(string fileName, ComponentGameDir location, bool legacy)
        {
            _fileName = fileName;
            Location = location;
            _legacy = legacy;
        }

        ComponentGameDir _location = ComponentGameDir.galacticadventures;
        public ComponentGameDir Location
        {
            get => _location;
            set
            {
                _location = value;
                NotifyPropertyChanged(nameof(Location));
            }
        }

        public string DisplayName => Path.GetFileNameWithoutExtension(_fileName);

        public string Unique => _fileName;

        public string RealName => _fileName;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasConfigsDirectory()
        {
            return false;
        }

        public async Task UninstallMod()
        {
            try
            {
                Task task = new Task(() =>
                {
                    FileWrite.SafeDeleteFile(FileWrite.GetFileOutputPath(_location.ToString(), _fileName, _legacy));
                    ManagedMods.SyncContext.Send(state => ManagedMods.Instance.ModConfigurations.Remove(this), null);
                });
                task.Start();
                await task;
            }
            catch (Exception ex)
            {
                MessageDisplay.RaiseError(new ErrorEventArgs(ex));
            }
        }

        bool _isProgressing = false;
        /// <summary>
        /// Whether or not this mod is currently being installed/reconfigured/removed.
        /// </summary>
        public bool IsProgressing
        {
            get => _isProgressing;
            set
            {
                _isProgressing = value;
                NotifyPropertyChanged(nameof(IsProgressing));
                IsProgressingChanged?.Invoke(this, null);
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler IsProgressingChanged;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
