using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.Mods
{
    interface IConfigurableMod : ISporeMod
    {
        bool HasSettings { get; }

        string GetViewTypeName();
    }
}
