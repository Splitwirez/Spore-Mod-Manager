using SporeMods.BaseTypes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SporeMods.Core
{
    public class SmmInfo : NOCSingleInstanceObject<SmmInfo>
    {
        [DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern string GetWineVersion();

        private static bool? IsRunningUnderWine(out Version version)
        {
            version = new Version(0, 0, 0, 0);
            try
            {
                string wineVer = GetWineVersion();
                if (!Version.TryParse(wineVer, out version))
                    return null;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public SmmInfo()
            : base()
        {
            _mgrLastVersion = AddProperty(nameof(MgrLastVersion), SmmStorage.EnsureInstance().ReadSetting(_mgrLastVersionName, new Version()));

            var isWine = IsRunningUnderWine(out Version wineVersion);
            _nonVitalIsInWine = AddProperty(nameof(NonVitalIsInWine), SmmStorage.EnsureInstance().ReadSetting(_nonVitalIsInWineName, isWine.HasValue ? isWine.Value : false));
        }


        readonly Version _mgrVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public Version MgrVersion
        {
            get => _mgrVersion;
        }


        string _targetFramework = "dotnet-core--" + Environment.Version;
        public string TargetFramework
        {
            get => _targetFramework;
        }


        string _mgrLastVersionName = "LastMgrVersion";
        NOCProperty<Version> _mgrLastVersion;
        public Version MgrLastVersion
        {
            get => _mgrLastVersion.Value;
            set => _mgrLastVersion.Value = value;
        }


        string _nonVitalIsInWineName = "WineMode";
        NOCProperty<bool> _nonVitalIsInWine;
        public bool NonVitalIsInWine
        {
            get => _nonVitalIsInWine.Value;
            set => _nonVitalIsInWine.Value = value;
        }
    }
}
