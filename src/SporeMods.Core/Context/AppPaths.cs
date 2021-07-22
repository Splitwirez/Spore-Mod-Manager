using SporeMods.BaseTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.Context
{
    public class AppPaths : NOCSingleInstanceObject<AppPaths>
    {
        NOCProperty<AppPath> _coreSporeData;
        public AppPath CoreSporeData
        {
            get => _coreSporeData.Value;
        }

        NOCProperty<AppPath> _galacticAdventuresData;
        public AppPath GalacticAdventuresData
        {
            get => _galacticAdventuresData.Value;
            set => _galacticAdventuresData.Value = value;
        }

        NOCProperty<AppPath> _sporebinEP1;
        public AppPath SporebinEP1
        {
            get => _sporebinEP1.Value;
            internal set => _sporebinEP1.Value = value;
        }

        public AppPaths()
            : base()
        {
            _coreSporeData = AddProperty(nameof(CoreSporeData), new AppPath(true, ExpansionPack.None, "Settings!Folders!AutoCoreData", "ExplicitCoreSporeData"));
            _galacticAdventuresData = AddProperty(nameof(GalacticAdventuresData), new AppPath(true, ExpansionPack.GalacticAdventures, "Settings!Folders!AutoGaData", "ExplicitGalacticAdventuresData"));
            _sporebinEP1 = AddProperty(nameof(SporebinEP1), new AppPath(false, ExpansionPack.GalacticAdventures, "Settings!Folders!AutoSporebinEp1", "ExplicitSporebinEP1"));
        }
    }
}
