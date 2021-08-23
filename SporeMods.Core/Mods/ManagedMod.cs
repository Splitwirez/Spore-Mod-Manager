using SporeMods.Core.ModTransactions;
using SporeMods.Core.ModTransactions.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SporeMods.Core.Mods
{
	/// <summary>
	/// A generic mod installed through the Mod Manager.
	/// </summary>
	public class ManagedMod : IInstalledMod, INotifyPropertyChanged
	{
		public static readonly string MOD_INFO = "ModInfo.xml";
		public static readonly string MOD_CONFIG = "Config.xml";
		public static readonly string PATH_USELEGACYDLLS = "UseLegacyDLLs";


		// True if the mod has its files in the SMM storage, false if not (for example just before it is installed)
		// When it is false, _xmlPath, _configPath, StoragePath must be ignored
		bool _hasStoredFiles;
		// Only valid when we are installing
		ZipArchive _zipArchive;

		string _xmlPath;
		string _configPath;
		bool _isLegacy;
		bool _copyAllFiles;

		/// <summary>
		/// Creates a non-stored managed mod, which is only temporary. This mod is linked to a mod identity,
		/// but it has no extracted files inside SMM and many features cannot be used.
		/// Optionally, the original zip archive of the mod can be provided: this will be used to extract resources for the configurator.
		/// </summary>
		/// <param name="isEnabledByDefault"></param>
		/// <param name="identity"></param>
		/// <param name="zip"></param>
		public ManagedMod(bool isEnabledByDefault, ModIdentity identity, ZipArchive zip = null)
        {
			_zipArchive = zip;
			_hasStoredFiles = false;
			Identity = identity;
			Identity.ParentMod = this;

			Configuration = new ModConfiguration(this);
			Configuration.IsEnabled = isEnabledByDefault;
			Configuration.Tags.AddRange(Identity.Tags);
			PopulateEnabledUniques(Identity);
		}

		/// <summary>
		/// Creates a managed mod instance for a mod whose files are present in SMM.
		/// Optionally, a predefined configuration can be specified; if not, it will use the configuration
		/// existing in the folder, or create a new one.
		/// </summary>
		/// <param name="unique">The unique tag, used as folder name</param>
		/// <param name="isEnabledByDefault"></param>
		/// <param name="configuration"></param>
		public ManagedMod(string unique, bool isEnabledByDefault, ModConfiguration configuration = null)
		{
			StoragePath = Path.Combine(Settings.ModConfigsPath, unique);
			_xmlPath = Path.Combine(StoragePath, MOD_INFO);
			_configPath = Path.Combine(StoragePath, MOD_CONFIG);

			var document = XDocument.Load(_xmlPath);
			Version xmlVersion = XmlModIdentity.ParseXmlVersion(document);

			if (xmlVersion.Major == 1)
			{
				Identity = XmlModIdentityV1.ParseModIdentity(this, document.Root);
			}

			if (configuration == null)
			{
				PrepareConfiguration(isEnabledByDefault);
			}
			else
            {
				Configuration = configuration;
				Configuration.IsEnabled = isEnabledByDefault;
				Configuration.Save(_configPath);
			}

			string isLegacyPath = Path.Combine(StoragePath, PATH_USELEGACYDLLS);
			if (xmlVersion == ModIdentity.XmlModIdentityVersion1_0_0_0)
			{
				File.WriteAllText(isLegacyPath, string.Empty);
				Permissions.GrantAccessFile(isLegacyPath);
			}

			_isLegacy = File.Exists(isLegacyPath);

			var attr = document.Root.Attribute("copyAllFiles");
			if (attr != null && (bool.TryParse(attr.Value, out bool value)))
				_copyAllFiles = value;
			else
				_copyAllFiles = false;

			if (IsProgressing)
			{
				NotifyPropertyChanged(nameof(IsProgressing));
				IsProgressingChanged?.Invoke(this, null);
				RaiseAnyModIsProgressingChanged(this, false, true);
			}

			_hasStoredFiles = true;
		}

		private void PopulateEnabledUniques(BaseModComponent component)
		{
			if (component is ModComponent c)
			{
				if (Configuration.UserSetComponents.ContainsKey(c.Unique))
					Configuration.UserSetComponents.Remove(c.Unique);
				
				Configuration.UserSetComponents.Add(c.Unique, c.EnabledByDefault);
			}
			foreach (var child in component.SubComponents)
			{
				PopulateEnabledUniques(child);
			}
		}

		private void PrepareConfiguration(bool isEnabledByDefault)
		{
			Configuration = new ModConfiguration(this);
			if (File.Exists(_configPath))
			{
				Configuration.Load(_configPath);
			}
			else
			{
				Configuration.IsEnabled = isEnabledByDefault;
				Configuration.Tags.AddRange(Identity.Tags);
				PopulateEnabledUniques(Identity);

				Configuration.Save(_configPath);
			}
		}

		public ModIdentity Identity { get; }
		public ModConfiguration Configuration;

		/// <summary>
		/// True if the mod uses the old duplicated DLLs system.
		/// </summary>
		public bool IsLegacy { get => _isLegacy; }

		public bool MustCopyAllFiles { get => _copyAllFiles; }

		public bool HasConfigsDirectory { get { return true; } }

		public string StoragePath { get; }

		public string Unique => Identity.Unique;
		public string DisplayName => Identity.DisplayName;
		public string Description => Identity.Description;
		public bool HasDescription
		{
			get => !Description.IsNullOrEmptyOrWhiteSpace();
		}

		public List<string> Tags => Configuration.Tags;
		
		public string RealName { get { return Path.GetFileName(StoragePath); } }

		public Version ModVersion => Identity.ModVersion;

		/// <summary>
		/// Whether or not the mod exposes a version.
		/// </summary>
		public bool ModHasVersion
		{
			get => ModVersion != ModIdentity.UNKNOWN_MOD_VERSION;
		}

		/// <summary>
		/// The mod's icon, if any, or null if no icon is provided.
		/// </summary>
		public System.Drawing.Icon Icon
		{
			get
			{
				return null;
				//TODO: Restore for Mod Identity 2
				/*string iconPath = Path.Combine(StoragePath, "Icon.ico");
				if (File.Exists(iconPath))
					return new System.Drawing.Icon(iconPath);
				else
					return null;*/
			}
		}


		string LogoPath => _hasStoredFiles ? Path.Combine(StoragePath, "Branding.png") : null; 
		
		public bool HasLogo
		{
			get {
				if (_hasStoredFiles)
				{
					return File.Exists(LogoPath) && TryGetLogo(LogoPath, out System.Drawing.Image img);
				}
				else if (_zipArchive != null)
                {
					return _zipArchive.GetEntry("Branding.png") != null;
                }
				else
                {
					return false;
                }
			}
		}

		/// <summary>
		/// The mod's logo, if any, or null if no logois provided.
		/// </summary>
		public System.Drawing.Image Logo
		{
			get
			{

				if (HasLogo && TryGetLogo(LogoPath, out System.Drawing.Image img))
					return img;
				return null;

			}
		}

		bool TryGetLogo(string path, out System.Drawing.Image img)
		{
			try
			{
				System.Drawing.Image logo = null;
				if (path != null)
                {
					using (FileStream stream = new FileStream(LogoPath, FileMode.Open, FileAccess.Read))
					{
						stream.Seek(0, SeekOrigin.Begin);
						logo = System.Drawing.Image.FromStream(stream);
					}
				}
				else if (_zipArchive != null)
                {
					using (Stream stream = _zipArchive.GetEntry("Branding.png").Open())
					{
						stream.Seek(0, SeekOrigin.Begin);
						logo = System.Drawing.Image.FromStream(stream);
					}
				}
				img = logo;
				return true;
			}
			catch { }
			img = null;
			return false;
		}

		/// <summary>
		/// Whether or not this mod has a configurator.
		/// </summary>
		public bool HasConfigurator { get { return Identity.HasCustomInstaller; } }

		/// <summary>
		/// Gets a list of all of the files in the mod
		/// </summary>
		/// <returns></returns>
		public List<string> GetModFileNames()
		{
			List<string> paths = Directory.EnumerateFiles(StoragePath).Where(x => ModsManager.IsModFile(x)).ToList();
			List<string> names = new List<string>();
			foreach (string p in paths)
				names.Add(Path.GetFileName(p));

			return names;
		}

		bool _isProgressing = false;
		/// <summary>
		/// Whether or not this mod is currently being installed/reconfigured/removed.
		/// </summary>
		public bool IsProgressing
		{
			get => _isProgressing;
			set
			{
				bool oldVal = _isProgressing;
				_isProgressing = value;
				NotifyPropertyChanged(nameof(IsProgressing));
				IsProgressingChanged?.Invoke(this, null);
				RaiseAnyModIsProgressingChanged(this, oldVal, _isProgressing);
			}
		}

		double _progress = 0.0;
		/// <summary>
		/// The current configuration progress for this mod.
		/// </summary>
		public double Progress
		{
			get => _progress;
			set
			{
				double oldVal = _progress;
				_progress = value;
				NotifyPropertyChanged(nameof(Progress));
				/*if ((FileCount > 0) && (Progress >= FileCount) && (IsProgressing))
				{
					IsProgressing = false;
					_watcher.EnableRaisingEvents = false;
					Progress = 0.0;
				}*/
				AnyModProgressChanged?.Invoke(this, new ModProgressChangedEventArgs(this, _progress - oldVal));
			}
		}

		/// <summary>
		/// [PARTIAL NYI]Queues all of this mod's files for removal, then deletes its configuration
		/// </summary>
		public async Task<ModTransactionCommitException> UninstallModAsync()
		{
			return await ModTransactionManager.ExecuteAsync(new UninstallManagedModTransaction(this));
		}

		/// <summary>
		/// Returns a list of the paths to all the mod-related files that can be removed.
		/// If the mod has a configurator, those are all the files that the mod can add.
		/// If the mod does not have a configurator, it's all the .package and .dll files.
		/// </summary>
		/// <returns></returns>
		public List<string> GetFilePathsToRemove()
        {
			var result = new List<string>();

			if (HasConfigurator)
			{
				bool isLegacyPath = Identity.InstallerSystemVersion == ModIdentity.XmlModIdentityVersion1_0_0_0;

				foreach (var file in Identity.GetAllFilesToAdd())
				{
					result.Add(FileWrite.GetFileOutputPath(file.GameDir, file.Name, isLegacyPath));
				}
			}
			else
			{
				foreach (string s in GetModFileNames())
				{
					if (Path.GetExtension(s).ToLowerInvariant() == ".package")
						result.Add(FileWrite.GetFileOutputPath(ComponentGameDir.GalacticAdventures, s, _isLegacy));
					else if (Path.GetExtension(s).ToLowerInvariant() == ".dll")
						result.Add(FileWrite.GetFileOutputPath(ComponentGameDir.ModAPI, s, _isLegacy));
				}
			}

			return result;
		}

		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler IsProgressingChanged;

		public static event EventHandler<ModIsProgressingChangedEventArgs> AnyModIsProgressingChanged;
		public static event EventHandler<ModProgressChangedEventArgs> AnyModProgressChanged;

		internal static void RaiseAnyModIsProgressingChanged(IInstalledMod mod, bool oldVal, bool newVal)
		{
			if (oldVal != newVal)
				AnyModIsProgressingChanged?.Invoke(mod, new ModIsProgressingChangedEventArgs(mod, newVal));
		}

		public override string ToString()
		{
			return DisplayName;
		}
	}

	public class ModProgressChangedEventArgs : EventArgs
	{
		public ManagedMod Mod { get; private set; } = null;
		public double Change { get; private set; } = 0.0;

		public ModProgressChangedEventArgs(ManagedMod mod, double change)
		{
			Mod = mod;
			Change = change;
		}
	}

	public class ModIsProgressingChangedEventArgs : EventArgs
	{
		public IInstalledMod Mod { get; private set; } = null;
		public bool IsNowProgressing { get; private set; } = false;

		public ModIsProgressingChangedEventArgs(IInstalledMod mod, bool isNowProgressing)
		{
			Mod = mod;
			IsNowProgressing = isNowProgressing;
		}
	}
}
