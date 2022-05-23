using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.Mods
{
    public interface IConfigurableMod : ISporeMod
    {
        object GetSettingsViewModel(bool postInstall);

        string GetSettingsViewTypeName(bool postInstall);
    }
}
