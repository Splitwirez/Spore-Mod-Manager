using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.InstalledMods
{
    public interface IInstalledMod
    {
        Task UninstallMod();

        bool HasConfigsDirectory();

        string DisplayName
        {
            get;
        }

        string Unique
        {
            get;
        }
        
        string RealName
        {
            get;
        }

        bool IsProgressing
        {
            get;
        }
    }
}
