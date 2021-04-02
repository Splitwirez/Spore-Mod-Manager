using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        string _xmlPath;
        string _configPath;
        bool _isLegacy;
        bool _copyAllFiles;

        public ManagedMod(string name, bool isEnabledByDefault)
        {
            StoragePath = Path.Combine(Settings.ModConfigsPath, Path.GetFileName(name));
            _xmlPath = Path.Combine(StoragePath, "ModInfo.xml");
            _configPath = Path.Combine(StoragePath, "Config.xml");

            var document = XDocument.Load(_xmlPath);
            Version xmlVersion = ParseXmlVersion(document);

            if (xmlVersion.Major == 1)
            {
                Identity = XmlModIdentityV1.ParseModIdentity(this, document.Root);
            }

            PrepareConfiguration(isEnabledByDefault);

            string isLegacyPath = Path.Combine(StoragePath, "UseLegacyDLLs");
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
        }

        private Version ParseXmlVersion(XDocument document)
        {
            var xmlVersionAttr = document.Root.Attribute("installerSystemVersion");
            if (xmlVersionAttr != null)
            {
                if (Version.TryParse(xmlVersionAttr.Value, out Version version))
                {
                    return version;
                }
                else
                    throw new FormatException("Mod identity 'installerSystemVersion': '" + xmlVersionAttr.Value + "' is not a valid version");
            }
            else
            {
                throw new FormatException("Mod identity 'installerSystemVersion': '" + xmlVersionAttr.Value + "' is not a valid version");
            }
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

        public bool IsEnabled 
        {
            get { return Configuration.IsEnabled; }
            set
            {
                if (value != Configuration.IsEnabled)
                    EnableOrDisable(value);
            }
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


        string LogoPath => Path.Combine(StoragePath, "Branding.png"); 
        
        public bool HasLogo
        {
            get => File.Exists(LogoPath) && TryGetLogo(LogoPath, out System.Drawing.Image img);
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
                using (FileStream stream = new FileStream(LogoPath, FileMode.Open, FileAccess.Read))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    logo = System.Drawing.Image.FromStream(stream);
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
        /// Shows the configurator for this mod, if it has one.
        /// </summary>
        public async Task ConfigureMod()
        {
            try
            {
                if (HasConfigurator)
                {
                    //ModsManager.Instance.AddToTaskCount(1);
                    await ModInstallation.RegisterSporemodModWithInstallerAsync(this.RealName);
                    //await EnableMod();
                }
                else
                {
                    throw new InvalidOperationException("Cannot configure a mod which does not have a configurator");
                }
            }
            catch (Exception ex)
            {
                MessageDisplay.RaiseError(new ErrorEventArgs(ex));
                ModsManager.InstalledMods.Add(new InstallError(ex));
            }
        }

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
        /// Removes all the files in the mod, increasing the 'Progress' property.
        /// </summary>
        /// <param name="progressRange">How much Progress increases after removing all files</param>
        private void RemoveModFiles(double progressRange)
        {
            if (HasConfigurator)
            {
                bool isLegacyPath = Identity.InstallerSystemVersion == ModIdentity.XmlModIdentityVersion1_0_0_0;
                var files = Identity.GetAllFilesToAdd();
                
                foreach (var file in files)
                {
                    FileWrite.SafeDeleteFile(FileWrite.GetFileOutputPath(file.GameDir, file.Name, isLegacyPath));
                    Progress += progressRange / files.Count;
                }
            }
            else
            {
                var files = GetModFileNames();
                foreach (string s in files)
                {
                    if (Path.GetExtension(s).ToLowerInvariant() == ".package")
                        FileWrite.SafeDeleteFile(FileWrite.GetFileOutputPath(ComponentGameDir.GalacticAdventures, s, _isLegacy));
                    else if (Path.GetExtension(s).ToLowerInvariant() == ".dll")
                        FileWrite.SafeDeleteFile(FileWrite.GetFileOutputPath(ComponentGameDir.ModAPI, s, _isLegacy));

                    Progress += progressRange / files.Count;
                }
            }
        }

        /// <summary>
        /// [PARTIAL NYI]Queues all of this mod's files for removal, then deletes its configuration
        /// </summary>
        public async Task<bool> UninstallModAsync()
        {
            var task = new Task<bool>(() =>
            {
                try
                {
                    Progress = 0;
                    IsProgressing = true;

                    RemoveModFiles(80.0);

                    Directory.Delete(StoragePath, true);

                    ModsManager.RunOnMainSyncContext(state => ModsManager.InstalledMods.Remove(this));

                    Progress = 0;
                    IsProgressing = false;
                    return true;
                }
                catch (Exception ex)
                {
                    MessageDisplay.RaiseError(new ErrorEventArgs(ex));
                    ModsManager.InstalledMods.Add(new InstallError(ex));

                    return false;
                }
            });
            task.Start();
            return await task;
        }

        /// <summary>
        /// Queues all of this mod's enabled files for installation
        /// </summary>
        public async Task<bool> EnableMod()
        {
            var task = new Task<bool>(() =>
            {
                try
                {
                    // It is possible that this is called RegisterSporemodModAsync, and some progress has already happened
                    double totalProgress = 100.0;

                    bool startsHere = !IsProgressing;
                    if (startsHere)
                    {
                        Progress = 0;
                        IsProgressing = true;
                    }
                    else
                    {
                        totalProgress = 100.0 - Progress;
                    }

                    RemoveModFiles(totalProgress * 0.1);

                    if (_copyAllFiles)
                    {
                        var files = Directory.EnumerateFiles(StoragePath).Where(x => ModsManager.IsModFile(x));
                        var progressInc = totalProgress * 0.9 / files.Count();

                        foreach (string s in files)
                        {
                            if (Path.GetExtension(s).ToLowerInvariant() == ".package")
                                FileWrite.SafeCopyFile(s, FileWrite.GetFileOutputPath(ComponentGameDir.GalacticAdventures, Path.GetFileName(s), _isLegacy));
                            else if (Path.GetExtension(s).ToLowerInvariant() == ".dll")
                                FileWrite.SafeCopyFile(s, FileWrite.GetFileOutputPath(ComponentGameDir.ModAPI, Path.GetFileName(s), _isLegacy));

                            Progress += progressInc;
                        }
                    }
                    else
                    {
                        EnableModAdvanced(totalProgress * 0.9);
                    }

                    Progress = 0;
                    IsProgressing = false;

                    Configuration.IsEnabled = true;

                    Configuration.Save(_configPath);

                    return true;
                }
                catch (Exception ex)
                {
                    MessageDisplay.RaiseError(new ErrorEventArgs(ex));
                    ModsManager.InstalledMods.Add(new InstallError(ex));
                    return false;
                }
            });
            task.Start();
            return await task;
        }

        private int GetEnabledComponentFileCount(BaseModComponent component)
        {
            int count = component.Files.Count;
            foreach (var child in component.SubComponents)
            {
                count += GetEnabledComponentFileCount(child);
            }
            return count;
        }

        /// <summary>
        /// Returns how many files have to be added or removed by the enabled components of this mod.
        /// </summary>
        /// <returns></returns>
        private int GetEnabledComponentFileCount()
        {
            int count = Identity.FilesToRemove.Count;
            foreach (var fix in Identity.CompatibilityFixes)
            {
                count += fix.FilesToAdd.Count;
                count += fix.FilesToRemove.Count;
            }
            return count + GetEnabledComponentFileCount(Identity);
        }

        private event EventHandler CompatibilityProgressIncreased;

        //TODO: Implement this for XML Mod Identity v2.0.0.0, in a generalized way for components
        private void EnableModAdvanced(double progressRange)
        {
            int fileCount = GetEnabledComponentFileCount();
            double progressIncrease = progressRange / fileCount;

            foreach (var file in Identity.FilesToRemove)
            {
                DirectoryInfo info = new DirectoryInfo(FileWrite.GetGameDirectory(file.GameDir, _isLegacy));
                foreach (FileInfo f in info.EnumerateFiles(file.Name))
                {
                    if (File.Exists(f.FullName))
                        FileWrite.SafeDeleteFile(f.FullName);
                }

                if (file.GameDir == ComponentGameDir.ModAPI && (!_isLegacy))
                {
                    foreach (FileInfo f2 in new DirectoryInfo(Settings.LegacyLibsPath).EnumerateFiles(file.Name))
                    {
                        string path = Path.Combine(Settings.LegacyLibsPath, f2.Name);
                        if (File.Exists(path))
                            FileWrite.SafeDeleteFile(path);
                    }
                }

                Progress += progressIncrease;
            }

            foreach (var file in Identity.Files)
            {
                FileWrite.SafeCopyFile(Path.Combine(StoragePath, file.Name), FileWrite.GetFileOutputPath(file.GameDir, file.Name, _isLegacy));
                Progress += progressIncrease;
            }

            foreach (var component in Identity.SubComponents)
            {
                if (component.IsGroup)
                {
                    foreach (var subComponent in component.SubComponents)
                    {
                        if (subComponent.IsEnabled)
                        {
                            foreach (var file in subComponent.Files)
                            {
                                FileWrite.SafeCopyFile(Path.Combine(StoragePath, file.Name), FileWrite.GetFileOutputPath(file.GameDir, file.Name, _isLegacy));
                                Progress += progressIncrease;
                            }
                            break;
                        }
                    }
                }
                else if (component.IsEnabled)
                {
                    foreach (var file in component.Files)
                    {
                        FileWrite.SafeCopyFile(Path.Combine(StoragePath, file.Name), FileWrite.GetFileOutputPath(file.GameDir, file.Name, _isLegacy));
                        Progress += progressIncrease;
                    }
                }
            }

            void CompatibilityProgressIncreasedHandler(object sender, EventArgs e)
            {
                Progress += progressIncrease;
            }
            CompatibilityProgressIncreased += CompatibilityProgressIncreasedHandler;
            EvaluateCompatibilityFixes();
            CompatibilityProgressIncreased -= CompatibilityProgressIncreasedHandler;
        }

        public void EvaluateCompatibilityFixes()
        {
            if (_copyAllFiles)
            {
                foreach (var fix in Identity.CompatibilityFixes)
                {
                    bool proceed = true;
                    foreach (var file in fix.RequiredFiles)
                    {
                        bool fileDoesntExist = !File.Exists(FileWrite.GetFileOutputPath(file.GameDir, file.Name, _isLegacy));
                        if ((!_isLegacy) && (!fileDoesntExist) && (file.GameDir == ComponentGameDir.ModAPI))
                            fileDoesntExist = !File.Exists(FileWrite.GetFileOutputPath(file.GameDir, file.Name, true));
                        if (fileDoesntExist)
                        {
                            proceed = false;
                            break;
                        }
                    }
                    if (proceed)
                    {
                        foreach (var file in fix.FilesToRemove)
                        {
                            DirectoryInfo info = new DirectoryInfo(FileWrite.GetGameDirectory(file.GameDir, _isLegacy));
                            foreach (FileInfo f in info.EnumerateFiles(file.Name))
                            {
                                if (File.Exists(f.FullName))
                                    FileWrite.SafeDeleteFile(f.FullName);
                            }

                            if ((!_isLegacy) && (file.GameDir == ComponentGameDir.ModAPI))
                            {
                                foreach (FileInfo f in new DirectoryInfo(Settings.LegacyLibsPath).EnumerateFiles(file.Name))
                                {
                                    string path = Path.Combine(Settings.LegacyLibsPath, file.Name);
                                    if (File.Exists(path))
                                        FileWrite.SafeDeleteFile(path);
                                }
                            }

                            CompatibilityProgressIncreased?.Invoke(this, null);
                        }
                        foreach (var file in fix.FilesToAdd)
                        {
                            FileWrite.SafeCopyFile(Path.Combine(StoragePath, file.Name), FileWrite.GetFileOutputPath(file.GameDir, file.Name, _isLegacy));
                            CompatibilityProgressIncreased?.Invoke(this, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// [PARTIAL NYI]Queues all of this mod's enabled files for removal, and all disabled files for installation
        /// </summary>
        public async Task<bool> DisableMod()
        {
            var task = new Task<bool>(() =>
            {
                try
                {
                    Progress = 0;
                    IsProgressing = true;
                    RemoveModFiles(100.0);

                    Progress = 0;
                    IsProgressing = false;

                    Configuration.IsEnabled = true;

                    return true;
                }
                catch (Exception ex)
                {
                    ModsManager.InstalledMods.Add(new InstallError(ex));
                    return false;
                }
            });
            task.Start();
            return await task;
        }

        public async Task EnableOrDisable(bool isEnabling)
        {
            if (isEnabling)
                await EnableMod();
            else
                await DisableMod();

            NotifyPropertyChanged(nameof(IsEnabled));
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
