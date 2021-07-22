using Microsoft.Win32;
using SporeMods.BaseTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SporeMods.Core.Context
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
            _autoPath = AddProperty(new NOCRespondProperty<string>(nameof(AutoPath), allInstallPaths.Count == 1 ? allInstallPaths[0] : string.Empty)
            { 
                ValueChangeResponse = (x, o, n) =>
                {
                    if (((AppPath)x).UseAutoPath)
                        ((AppPath)x).NotifyPropertyChanged(nameof(Path));
                }
            });


            _needsExplicitPath = new NOCProperty<bool>(nameof(NeedsExplicitPath), allInstallPaths.Count != 1);
            _useAutoPath = new NOCProperty<bool>(nameof(UseAutoPath), !NeedsExplicitPath);

            _explicitPath = AddProperty<string>(nameof(ExplicitPath), allInstallPaths.Count > 0 ? allInstallPaths[0] : string.Empty); //TODO: Get properly
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

        NOCRespondProperty<string> _autoPath;
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
    }
}
