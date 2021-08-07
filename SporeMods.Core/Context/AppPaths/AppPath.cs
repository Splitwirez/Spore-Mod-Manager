using Microsoft.Win32;
using SporeMods.NotifyOnChange;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core
{
    public enum ExpansionPack
    {
        None,
        GalacticAdventures
    }

    public partial class AppPath : NOCObject
    {
        string _explicitPathXmlTag = null;
        string _dirNameBase = null;
        bool _allowNoSuffix = false;
        ExpansionPack _dlcLevel = (ExpansionPack)(-1);
        public AppPath(bool isData, ExpansionPack dlc, string displayNameKey, string explicitPathXmlTag)
            : base()
        {
            //throw new Exception();
            _dirNameBase = isData ? "Data" : "Sporebin";
            _dlcLevel = dlc;
            _explicitPathXmlTag = explicitPathXmlTag;
            _allowNoSuffix = isData;

            var allInstallPaths = GetAllGameInstallPathsFromRegistry();

            _displayNameKey = AddProperty(nameof(DisplayNameKey), displayNameKey);
            _displayNameKey.Notify();

            _autoPaths = AddProperty(nameof(AutoPaths), allInstallPaths);
            _autoPath = AddProperty(nameof(AutoPath), allInstallPaths.Count == 1 ? allInstallPaths[0] : string.Empty); /* new NOCRespondProperty<string>(nameof(AutoPath), allInstallPaths.Count == 1 ? allInstallPaths[0] : string.Empty)
            { 
                ValueChangeResponse = (x, o, n) =>
                {
                    if (((AppPath)x).UseAutoPath)
                        ((AppPath)x).NotifyPropertyChanged(nameof(Path));
                }
            });*/

            _explicitPath = AddProperty(nameof(ExplicitPath), string.Empty); //allInstallPaths.Count > 0 ? allInstallPaths[0] : string.Empty); //TODO: Get properly

            _needsExplicitPath = AddProperty(nameof(NeedsExplicitPath), allInstallPaths.Count != 1);
            _useAutoPath = AddProperty(nameof(UseAutoPath), allInstallPaths.Count == 1);

            //Task.run
        }

        public async Task<bool> EnsureAutoDetectOK()
        {
            if (NeedsExplicitPath)
            {
                string guess = await MessageDisplay.ShowModal<string>(new AppPathAutoDetectFail().Guess(this));
                return ((guess != null) && (!guess.IsNullOrEmptyOrWhiteSpace()) && (Directory.Exists(guess)));
            }
            else
                return true;
        }

        NOCProperty<string> _displayNameKey;
        public string DisplayNameKey
        {
            get => _displayNameKey.Value;
            set => _displayNameKey.Value = value;
        }

        NOCProperty<ObservableCollection<string>> _autoPaths;
        public ObservableCollection<string> AutoPaths
        {
            get => _autoPaths.Value;
            set => _autoPaths.Value = value;
        }

        NOCProperty<string> _autoPath;
        public string AutoPath
        {
            get => _autoPath.Value;
            set => _autoPath.Value = value;
        }

        NOCProperty<bool> _needsExplicitPath;
        public bool NeedsExplicitPath
        {
            get => _needsExplicitPath.Value;
            set => _needsExplicitPath.Value = value;
        }

        NOCProperty<bool> _useAutoPath;
        public bool UseAutoPath
        {
            get => _useAutoPath.Value;
            set => _useAutoPath.Value = value;
        }

        NOCProperty<string> _explicitPath;
        public string ExplicitPath
        {
            get => _explicitPath.Value;
            set => _explicitPath.Value = value;
        }

        public string Path
        {
            get
            {
                if ((!NeedsExplicitPath) && UseAutoPath)
                    return AutoPath;
                else
                    return ExplicitPath;
            }
        }

        public bool Ensure()
        {
            string outVal = Path;
            return (!outVal.IsNullOrEmptyOrWhiteSpace()) && Directory.Exists(outVal);
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            string coutput = $"{propertyName} changed";
            Debug.WriteLine(coutput);
            Console.WriteLine(coutput);
        }
    }
}
