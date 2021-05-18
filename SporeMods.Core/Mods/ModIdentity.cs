using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
	/// <summary>
	/// Represents the structure of a mod. It is uniquely associated with a ManagedMod.
	/// A mod identity is fixed and it doesn't depend on the current configuration of the mod.
	/// </summary>
	public class ModIdentity
		: BaseModComponent 
	{
		public static readonly Version ModIdentityVersion1_0_0_0 = new Version(1, 0, 0, 0);
		static readonly Version ModIdentityVersion1_0_1_0 = new Version(1, 0, 1, 0);
		static readonly Version ModIdentityVersion1_0_1_1 = new Version(1, 0, 1, 1);
		public static readonly Version GrandfatheredModIdentityVersion = new Version(1, 1, 0, 0);
		public static readonly Version UNKNOWN_MOD_VERSION = new Version(0, 0, 0, 0);

		public static bool IsLauncherKitCompatibleXmlModIdentityVersion(Version identityVersion)
		{
			return (identityVersion == ModIdentity.ModIdentityVersion1_0_0_0) ||
					(identityVersion == ModIdentity.ModIdentityVersion1_0_1_0) ||
					(identityVersion == ModIdentity.ModIdentityVersion1_0_1_1);
		}

		public static bool IsSmmCompatibleModIdentityVersion(Version identityVersion, bool includeGrandfathered = false)
		{
			return IsLauncherKitCompatibleXmlModIdentityVersion(identityVersion) || (includeGrandfathered && (identityVersion == GrandfatheredModIdentityVersion));
		}

		/*public static bool IsValidUnique(string inputUnique)
		{
			bool returnValue = true;

			foreach (char c in System.IO.Path.GetInvalidPathChars())
			{
				if (inputUnique.Contains(c))
				{
					returnValue = false;
					break;
				}
			}
			return returnValue;
		}

		public static bool IsValidUnique(string inputUnique, out string validUnique)
		{
			string value = inputUnique;

			bool returnValue = true;

			foreach (char c in System.IO.Path.GetInvalidPathChars())
			{
				if (value.Contains(c))
				{
					returnValue = false;
					value = value.Replace(c.ToString(), string.Empty);
					//break;
				}
			}
			validUnique = value;

			return returnValue;
		}*/

		public ModIdentity(ManagedMod mod, string uniqueTag)
			: base(null, uniqueTag)
		{
			ParentMod = mod;
		}

		public ManagedMod ParentMod { get; }

		/// <summary>
		/// The version of XML Mod Identity used by this mod.
		/// </summary>
		public Version InstallerSystemVersion { get; set; }

		/// <summary>
		/// The minimum build of the Spore ModAPI DLLs required by the mod; the last digit is ignored.
		/// </summary>
		public Version DllsBuild { get; set; }

		/// <summary>
		/// If explicitly set to true, the mod will show a configuration dialog during installation, 
		/// allowing the user to select what they want among whatever components and/or componentGroups you have specified.
		/// </summary>
		public bool HasCustomInstaller { get; set; }

		/// <summary>
		/// If explicitly set to true, the user will receive a warning, telling them that the mod is still 
		/// experimental and may have undesirable side effects, along with the option to abort installation 
		/// if they do not wish to proceed after learning of this. This value should generally be used for 
		/// mods which are still in testing.
		/// </summary>
		public bool IsExperimental { get; set; }

		/// <summary>
		/// If explicitly set to true, the user will receive a warning, telling them that the mod requires a 
		/// Galaxy reset to take full effect, along an order to perform the Galaxy reset themselves, and the 
		/// option to abort installation if they do not wish to proceed after learning of this. 
		/// Many gameplay-related mods require a Galaxy reset.
		/// </summary>
		public bool RequiresGalaxyReset { get; set; }

		/// <summary>
		/// If explicitly set to true, the user will receive a warning, telling them that the mod will cause 
		/// save data dependency, and an explanation that their save planets may become corrupted or otherwise 
		/// inaccessible or be adversely affected in some other way if the mod is uninstalled, along with the 
		/// option to abort installation if they do not wish to proceed after learning of this. This applies to
		/// many gameplay mods, including those which add new Simulator objects. 
		/// </summary>
		public bool CausesSaveDataDependency { get; set; }

		//TODO for XML Mod Identity 2.0: move this to BaseModComponent
		/// <summary>
		/// Files to be removed when installing this mod.
		/// </summary>
		public List<ModFile> FilesToRemove { get; } = new List<ModFile>();

		//TODO for XML Mod Identity 2.0: move this to BaseModComponent
		/// <summary>
		/// A list of fixes that can be applied to ensure compatiblity between mods.
		/// </summary>
		public List<ModCompatibilityFix> CompatibilityFixes { get; } = new List<ModCompatibilityFix>();

		/// <summary>
		/// The version of this mod. Some mods do not expose a version.
		/// </summary>
		public Version ModVersion { get; set; } = UNKNOWN_MOD_VERSION;

		/// <summary>
		/// Whether or not the mod exposes a version.
		/// </summary>
		public bool ModHasVersion
		{
			get => ModVersion != UNKNOWN_MOD_VERSION;
		}

		/// <summary>
		/// Whether or not this mod can be disabled by the user.
		/// </summary>
		public bool CanDisable { get; set; }

		public List<string> Tags { get; } = new List<string>();


		private BaseModComponent GetComponentInternal(BaseModComponent component, string unique)
		{
			if (component.Unique == unique) return component;
			foreach (var child in component.SubComponents)
			{
				var result = GetComponentInternal(component, unique);
				if (result != null) return result;
			}
			return null;
		}

		/// <summary>
		/// Returns the mod component that is identified with the given 'unique'  string.
		/// </summary>
		/// <param name="unique"></param>
		/// <returns></returns>
		public BaseModComponent GetComponent(string unique)
		{
			return GetComponentInternal(this, unique);
		}

		private void GetAllFilesToAddInternal(BaseModComponent component, List<ModFile> list)
		{
			list.AddRange(component.Files);
			foreach (var sub in component.SubComponents)
			{
				GetAllFilesToAddInternal(sub, list);
			}
		}

		/// <summary>
		/// Gets all the files that could potentially be added by this mod. This will consider every component that adds files,
		/// regardless of whether it's enabled or not. This also includes files that could be added through ModCompatibilityFix objects.
		/// </summary>
		/// <returns></returns>
		public List<ModFile> GetAllFilesToAdd()
		{
			var list = new List<ModFile>();

			GetAllFilesToAddInternal(this, list);
			foreach (var compat in CompatibilityFixes)
			{
				list.AddRange(compat.FilesToAdd);
			}

			return list;
		}


		public static bool IsValidModIdentity(XDocument modInfo, IEnumerable<string> modFileNames, out string invalidReason, bool allowSmmExclusiveIdentities = true, bool allowGrandfatheredIdentities = false)
        {
			Version modIdentitySysVersion;
			if (modInfo == null)
            {
				invalidReason = "Mod identity was null.";
				return false;
            }

			if (modInfo.Root.Name.LocalName != "mod")
            {
				invalidReason = "Root element has wrong tag name.";
				return false;
            }

			XAttribute attr = modInfo.Root.Attribute("installerSystemVersion");
			if (attr == null)
            {
				invalidReason = "Mod 'installerSystemVersion' was not specified.";
				return false;
			}
			else if (Version.TryParse(attr.Value, out modIdentitySysVersion))
            {
				if (allowSmmExclusiveIdentities)
				{
					if (!IsSmmCompatibleModIdentityVersion(modIdentitySysVersion, allowGrandfatheredIdentities))
					{
						invalidReason = "This mod requires a newer version of the Spore Mod Manager.";
						return false;
					}
				}
				else
				{
					if (!IsLauncherKitCompatibleXmlModIdentityVersion(modIdentitySysVersion))
                    {
						invalidReason = "This mod requires the Spore Mod Manager.";
						return false;
					}
				}
			}
			else
            {
				invalidReason = "This mod might require a newer version of the Spore Mod Manager, or it might just be broken.";
				return false;
            }

			attr = modInfo.Root.Attribute("unique");
			if (attr == null)
			{
				invalidReason = "Mod 'unique' was not specified.";
				return false;
			}

			if (modIdentitySysVersion > ModIdentityVersion1_0_0_0)
			{
				attr = modInfo.Root.Attribute("dllsBuild");
				if (attr == null)
				{
					invalidReason = "Mod 'dllsBuild' was not specified.";
					return false;
				}
			}

			string hasConfiguratorAttrName = "hasCustomInstaller";
			if (modIdentitySysVersion == ModIdentityVersion1_0_0_0)
				hasConfiguratorAttrName = "compatOnly";
			attr = modInfo.Root.Attribute(hasConfiguratorAttrName);
			if (attr != null)
            {
				if (!bool.TryParse(attr.Value, out bool lmao))
                {
					invalidReason = "Mod '" + hasConfiguratorAttrName + "' was invalid.";
					return false;
                }
            }


			attr = modInfo.Root.Attribute("isExperimental");
			if (attr != null)
			{
				if (!bool.TryParse(attr.Value, out bool lmao))
				{
					invalidReason = "Mod 'isExperimental' was invalid.";
					return false;
				}
			}

			attr = modInfo.Root.Attribute("requiresGalaxyReset");
			if (attr != null)
			{
				if (!bool.TryParse(attr.Value, out bool lmao))
				{
					invalidReason = "Mod 'requiresGalaxyReset' was invalid.";
					return false;
				}
			}


			attr = modInfo.Root.Attribute("causesSaveDataDependency");
			if (attr != null)
			{
				if (!bool.TryParse(attr.Value, out bool lmao))
				{
					invalidReason = "Mod 'causesSaveDataDependency' was invalid.";
					return false;
				}
			}

			if (IsLauncherKitCompatibleXmlModIdentityVersion(modIdentitySysVersion))
            {
				return XmlModIdentityV1.IsValidModIdentity(modInfo, modFileNames, out invalidReason);
            }


			invalidReason = null;
			return true;
        }
	}
}
