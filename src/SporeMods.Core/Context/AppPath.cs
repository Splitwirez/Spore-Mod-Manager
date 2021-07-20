using SporeMods.BaseTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SporeMods.Core.Context
{
    public class AppPath : NOCObjectBase
    {
        public AppPath()
            : base()
        {
            _autoPaths = AddProperty(nameof(AutoPaths), new ObservableCollection<string>());
            _autoPath = AddProperty(nameof(AutoPath), string.Empty);
            _explicitPath = AddProperty(nameof(ExplicitPath), string.Empty);
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

        NOCProperty<string> _explicitPath;
        public string ExplicitPath
        {
            get => _explicitPath.Value;
            set => _explicitPath.Value = value;
        }
    }
}
