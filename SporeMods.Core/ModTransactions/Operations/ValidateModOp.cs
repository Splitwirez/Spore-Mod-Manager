using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.ModTransactions.Operations
{
    public class UnsupportedXmlModIdentityVersionException : Exception
    {
        public Version BadVersion { get; } = null;

        public UnsupportedXmlModIdentityVersionException(Version badVersion)
        {
            BadVersion = badVersion;
        }
    }

    public class UnsupportedDllsBuildException : Exception
    {
        public Version BadVersion { get; } = null;

        public UnsupportedDllsBuildException(Version badVersion)
        {
            BadVersion = badVersion;
        }
    }

    public class MissingXmlModIdentityAttributeException : Exception
    {
        public string Attribute { get; } = null;

        public MissingXmlModIdentityAttributeException(string attribute)
        {
            Attribute = attribute;
            _message = $"The provided identity was missing the '{Attribute}' attribute";
        }

        string _message = string.Empty;
        public override string Message
        {
            get => _message;
        }
    }

    public class ModAlreadyInstalledException : Exception
    { }

    public class UserRefusedConditionsException : Exception
    { }

    public class ValidateModOp : IModSyncOperation
    {
        public static event Func<string, bool> InstallingExperimentalMod;
        public static event Func<string, bool> InstallingRequiresGalaxyResetMod;
        public static event Func<string, bool> InstallingSaveDataDependencyMod;

        public readonly ModIdentity identity;
        public bool verifiedVanillaCompatible;

        public ValidateModOp(ModIdentity identity)
        {
            this.identity = identity;
        }

        public bool Do()
        {
            if (identity.InstallerSystemVersion == null)
            {
                throw new MissingXmlModIdentityAttributeException(null);
            }
            if (!ModIdentity.IsLauncherKitCompatibleXmlModIdentityVersion(identity.InstallerSystemVersion))
            {
                throw new UnsupportedXmlModIdentityVersionException(identity.InstallerSystemVersion);
            }

            if (identity.InstallerSystemVersion > ModIdentity.XmlModIdentityVersion1_0_0_0)
            {
                if (identity.DllsBuild == null)
                {
                    throw new MissingXmlModIdentityAttributeException("dllsBuild");
                }
                if (identity.DllsBuild > Settings.CurrentDllsBuild)
                {
                    throw new UnsupportedDllsBuildException(identity.DllsBuild);
                }
            }

            if (!Settings.AllowVanillaIncompatibleMods && !identity.VerifiedVanillaCompatible)
            {
                return false;
            }

            if (identity.IsExperimental)
            {
                if (!InstallingExperimentalMod(identity.DisplayName))
                    throw new UserRefusedConditionsException();
            }
            if (identity.RequiresGalaxyReset)
            {
                if (!InstallingRequiresGalaxyResetMod(identity.DisplayName))
                    throw new UserRefusedConditionsException();
            }
            if (identity.CausesSaveDataDependency)
            {
                if (!InstallingSaveDataDependencyMod(identity.DisplayName))
                    throw new UserRefusedConditionsException();
            }

            return true;
        }

        public void Undo()
        {
        }
    }
}
