using SporeMods.Core.Mods.ModIdentity.V1_0_X_XComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
    public abstract partial class MI1_0_X_XMod : ModIdentityBase, ICanInstallFromSporemodFile
    {
        public override bool HasExplicitVersion => false;

        public override Version ModVersion => new Version(0, 0, 0, 0);

        public override List<ModDependency> Dependencies => new List<ModDependency>();

        public override List<string> UpgradeTargets => new List<string>();

        public override bool IsUpgradeTo(ISporeMod mod)
            => Unique == mod.Unique;

        public override bool DependsOn(ISporeMod mod)
            => false;

        internal bool IsIncoming = false;


        ThreadSafeObservableCollection<ComponentBase> _allComponents = new ThreadSafeObservableCollection<ComponentBase>();
        public ThreadSafeObservableCollection<ComponentBase> AllComponents
        {
            get => _allComponents;
        }


        ThreadSafeObservableCollection<ComponentBase> _featureComponents = new ThreadSafeObservableCollection<ComponentBase>();
        public ThreadSafeObservableCollection<ComponentBase> FeatureComponents
        {
            get => _featureComponents;
        }

        public override Task<ModJobBatchEntryBase> EnsureCanInstall(ModJobBatchModEntry entry, List<ModJobBatchModEntry> otherEntries)
        {
            throw new NotImplementedException();
        }


        public override IViewLocatable GetSettingsViewModel(bool postInstall)
            => new MI1_0_X_XModSettingsViewModel(this);

        protected virtual Version EnsureIdentityVersion(XDocument doc)
        {
            var versionAttr = doc.Root.Attribute("installerSystemVersion");
            Version identityVersion = new Version(0, 0, 0, 0);
            if (versionAttr == null)
                throw new FormatException(Externals.GetLocalizedText("Mods!Error!Identity!MissingSysVersion"));
            else if (Version.TryParse(versionAttr.Value, out identityVersion))
            {
                if (!ModUtils.IsID_1_0_X_X(identityVersion))
                    throw new FormatException(Externals.GetLocalizedText("Mods!Error!Identity!UnsupportedSysVersion").Replace("%VERSION%", identityVersion.ToString()));

                /*if (ModUtils.IsID_1_0_1_X(identityVersion))
                    IsMI1_0_0_0 = false;
                else
                    IsMI1_0_0_0 = true;*/
            }
            else
                throw new FormatException(Externals.GetLocalizedText("Mods!Error!Identity!InvalidAttributeValue").Replace("%ATTRIBUTE%", "installerSystemVersion").Replace("%VALUE%", versionAttr.Value).Replace("%TYPE%", "Version"));
            
            return identityVersion;
        }
    }
}
