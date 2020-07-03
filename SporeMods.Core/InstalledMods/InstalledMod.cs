using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SporeMods.Core.ModIdentity;

namespace SporeMods.Core.InstalledMods
{
    public class InstalledMod : IInstalledMod, INotifyPropertyChanged
    {
        internal string _path = null;
        string _xmlPath = null;
        public XDocument Document;
        bool _isLegacy = false;

        public InstalledMod(string name)
        {
            _path = Path.Combine(Settings.ModConfigsPath, Path.GetFileNameWithoutExtension(name));
            _xmlPath = Path.Combine(_path, "ModInfo.xml");
            Document = XDocument.Load(_xmlPath);
            string isLegacyPath = Path.Combine(_path, "UseLegacyDLLs");

            if (XmlVersion == XmlModIdentityVersion1_0_0_0)
                File.WriteAllText(isLegacyPath, string.Empty);

            _isLegacy = File.Exists(isLegacyPath);

            foreach (string s in Directory.EnumerateFiles(_path))
            {
                if (IsModFile(s))
                    FileCount++;
            }
        }

        /// <summary>
        /// Returns whether or not a given path or filename corresponds to a mod file which is to be installed.
        /// </summary>
        public static bool IsModFile(string path)
        {
            return (Path.GetExtension(path).ToLowerInvariant() == ".package") || (Path.GetExtension(path).ToLowerInvariant() == ".dll");
        }

        /// <summary>
        /// The mod's icon, if any, or null if no icon is provided.
        /// </summary>
        public System.Drawing.Icon Icon
        {
            get
            {
                string iconPath = Path.Combine(_path, "Icon.ico");
                if (File.Exists(iconPath))
                    return new System.Drawing.Icon(iconPath);
                else
                    return null;
            }
        }

        static string _displayName = "displayName";
        /// <summary>
        /// The name the user knows this mod by. Only used in UI.
        /// </summary>
        public string DisplayName
        {
            get
            {
                var attr = Document.Root.Attribute(_displayName);
                if (attr != null)
                    return attr.Value;
                else
                    return RealName;
            }
        }

        static string _description = "description";
        /// <summary>
        /// The mod's in-UI description.
        /// </summary>
        public string Description
        {
            get
            {
                var attr = Document.Root.Attribute(_description);
                if (attr != null)
                    return attr.Value;
                else
                    return string.Empty;
            }
        }
        public bool HasDescription
        {
            get => !Description.IsNullOrEmptyOrWhiteSpace();
        }

        static string _tags = "tags";
        public List<string> Tags
        {
            get
            {
                string tagsFilePath = Path.Combine(_path, _tags);
                if (!File.Exists(tagsFilePath))
                {
                    List<string> lines = new List<string>();
                    var el = Document.Root.Element(_tags);
                    if (el != null)
                    {
                        foreach (XElement cel in el.Elements("tag"))
                            lines.Add(cel.Value);
                    }
                    File.WriteAllLines(tagsFilePath, lines);
                }
                return File.ReadAllLines(tagsFilePath).Where(x => !(x.IsNullOrEmptyOrWhiteSpace())).ToList();
            }
        }

        string _copyAllFiles = "copyAllFiles";
        public bool CopyAllFiles
        {
            get
            {
                var attr = Document.Root.Attribute(_copyAllFiles);
                if ((attr != null) && (bool.TryParse(attr.Value, out bool returnValue)))
                    return returnValue;
                else
                    return false;
            }
        }

        public bool HasConfigsDirectory()
        {
            return true;
        }

        /// <summary>
        /// The name of the installation directory used by this mod.
        /// </summary>
        public string RealName
        {
            get => Path.GetFileName(_path);
        }

        static string _xmlVersion = "installerSystemVersion";
        /// <summary>
        /// The XML Mod Identity version used by this mod, if any.
        /// </summary>
        public Version XmlVersion
        {
            get => Version.Parse(Document.Root.Attribute(_xmlVersion).Value);
        }

        static string _modVersion = "modVersion";
        /// <summary>
        /// The installed version of this mod. Some mods do not expose a version.
        /// </summary>
        public Version ModVersion
        {
            get
            {
                var attr = Document.Root.Attribute(_modVersion);
                if ((attr != null) && (Version.TryParse(attr.Value, out Version returnValue)))
                    return returnValue;
                else
                    return UnknownModVersion;
            }
        }

        /// <summary>
        /// Whether or not the mod exposes a version.
        /// </summary>
        public bool ModHasVersion
        {
            get => ModVersion != UnknownModVersion;
        }

        static string _unique = "unique";
        /// <summary>
        /// The unique identifier for this mod. This value should never change between versions of a mod.
        /// </summary>
        public string Unique
        {
            get
            {
                var attr = Document.Root.Attribute(_unique);
                if (attr != null)
                    return attr.Value;
                else
                    return RealName;
            }
        }

        string _canDisable = "canDisableMod";
        /// <summary>
        /// Whether or not this mod can be disabled by the user.
        /// </summary>
        public bool CanDisable
        {
            get
            {
                var attr = Document.Root.Attribute(_canDisable);
                if ((attr != null) && (bool.TryParse(attr.Value, out bool returnValue)))
                    return returnValue;
                else
                    return false;
            }
        }

        string _isEnabled = "isEnabled";
        /// <summary>
        /// Whether or not this mod is enabled by the user.
        /// </summary>

        public bool IsEnabled
        {
            get
            {
                var attr = Document.Root.Attribute(_isEnabled);
                if ((attr != null) && (bool.TryParse(attr.Value, out bool returnValue)))
                    return returnValue;
                else
                    return true;
            }
            set
            {
                bool isEnabling = (value == true) && (value != IsEnabled);
                EnableOrDisable(isEnabling);
            }
        }

        public async Task EnableOrDisable(bool isEnabling)
        {
            bool result = false;
            if (isEnabling)
                result = await EnableMod();
            else
                result = await DisableMod();

            if (result)
                SafeSetAttributeValue(_isEnabled, isEnabling.ToString());

            NotifyPropertyChanged(nameof(IsEnabled));
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
                _isProgressing = value;
                NotifyPropertyChanged(nameof(IsProgressing));
                IsProgressingChanged?.Invoke(this, null);
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
                _progress = value;
                NotifyPropertyChanged(nameof(Progress));
                /*if ((FileCount > 0) && (Progress >= FileCount) && (IsProgressing))
                {
                    IsProgressing = false;
                    _watcher.EnableRaisingEvents = false;
                    Progress = 0.0;
                }*/
            }
        }

        double _fileCount = 0.0;
        /// <summary>
        /// The number of files this mod has queued for installation.
        /// </summary>
        public double FileCount
        {
            get => _fileCount;
            set
            {
                _fileCount = value;
                NotifyPropertyChanged(nameof(FileCount));
                //ModInstallation.DebugMessageBoxShow(RealName + " FILE COUNT: " + _fileCount);
            }
        }

        public static Version XmlModIdentityVersion1_0_0_0 = new Version(1, 0, 0, 0);
        public static Version XmlModIdentityVersion1_0_1_0 = new Version(1, 0, 1, 0);
        public static Version XmlModIdentityVersion1_0_1_1 = new Version(1, 0, 1, 1);
        public static Version XmlModIdentityVersion1_1_0_0 = new Version(1, 1, 0, 0);
        public static Version UnknownModVersion = new Version(0, 0, 0, 0);

        string _hasConfigurator = "hasCustomInstaller";
        /// <summary>
        /// Whether or not this mod has a configurator.
        /// </summary>
        public bool HasConfigurator
        {
            get
            {
                if (XmlVersion == XmlModIdentityVersion1_0_0_0)
                {

                    var attr = Document.Root.Attribute("compatOnly");
                    if ((attr != null) && bool.TryParse(attr.Value, out bool compatOnly))
                        return (!compatOnly);
                    else
                        return true;
                }
                else
                {
                    var attr = Document.Root.Attribute(_hasConfigurator);
                    if ((attr != null) && bool.TryParse(attr.Value, out bool configurator))
                        return configurator;
                    else
                        return false;
                }
            }
        }

        private ModConfiguration _configurator = null;
        /// <summary>
        /// The Configurator for this mod, if any. Returns null if there is none.
        /// </summary>
        public ModConfiguration Configurator
        {
            get
            {
                if (_configurator == null)
                {
                    if ((XmlVersion == XmlModIdentityVersion1_0_0_0) ||
                        (XmlVersion == XmlModIdentityVersion1_0_1_0) ||
                        (XmlVersion == XmlModIdentityVersion1_0_1_1))
                        _configurator = new ModConfigurationV1_0_0_0(this, (XmlVersion != XmlModIdentityVersion1_0_0_0));
                    else if (XmlVersion == XmlModIdentityVersion1_1_0_0)
                        _configurator = null; //NYI
                    else
                        _configurator = null;
                }
                return _configurator;
            }
        }

        /// <summary>
        /// Shows the configurator for this mod, if it has one.
        /// </summary>
        public async Task ConfigureMod()
        {
            try
            {
                if (HasConfigurator)
                {
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
                ManagedMods.Instance.ModConfigurations.Add(new InstallError(ex));
            }
        }

        /// <summary>
        /// Gets a list of all of the files in the mod
        /// </summary>
        /// <returns></returns>
        public List<string> GetModFileNames()
        {
            //Document.Root.Elements().Where(x => x.Name.LocalName.tol)
            List<string> paths = Directory.EnumerateFiles(_path).Where(x => IsModFile(x)).ToList();
            List<string> names = new List<string>();
            foreach (string p in paths)
                names.Add(Path.GetFileName(p));
            
            return names;
        }

        /// <summary>
        /// Queues all of this mod's enabled files for installation
        /// </summary>
        public async Task<bool> EnableMod()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            try
            {
            bool startsHere = !IsProgressing;

            if (startsHere)
            {
                Progress = 0;
                IsProgressing = true;
            }
            /*mod.Progress = 0.0;
                    mod.FileCount = mod.Configurator.GetComponentFileCount() + mod.Configurator.GetEnabledComponentFileCount();*/

            await RemoveModFiles();

            if (CopyAllFiles)
            {
                Task task = new Task(() =>
                {
                    if (startsHere)
                        FileCount = Directory.EnumerateFiles(_path).Where(x => IsModFile(x)).Count();

                    foreach (string s in Directory.EnumerateFiles(_path).Where(x => IsModFile(x)))
                    {
                        if (Path.GetExtension(s).ToLowerInvariant() == ".package")
                            FileWrite.SafeCopyFile(s, FileWrite.GetFileOutputPath(ComponentGameDir.galacticadventures.ToString(), Path.GetFileName(s), _isLegacy));
                        else if (Path.GetExtension(s).ToLowerInvariant() == ".dll")
                            FileWrite.SafeCopyFile(s, FileWrite.GetFileOutputPath(ComponentGameDir.modapi.ToString(), Path.GetFileName(s), _isLegacy));
                        Progress++;
                    }
                });
                task.Start();
                await task;
            }
            else
                await Configurator.EnableMod();
            IsProgressing = false;
            Progress = 0;
            tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                MessageDisplay.RaiseError(new ErrorEventArgs(ex));
                ManagedMods.Instance.ModConfigurations.Add(new InstallError(ex));
                tcs.TrySetResult(false);
            }
            return await tcs.Task;
        }

        /// <summary>
        /// [PARTIAL NYI]Queues all of this mod's enabled files for removal, and all disabled files for installation
        /// </summary>
        public async Task<bool> DisableMod()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            try
            {
                if (CopyAllFiles)
                {
                    await RemoveModFiles();
                    Task task = new Task(() =>
                    {
                    });

                    task.Start();
                    await task;
                }
                else
                    await Configurator.DisableMod();
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetResult(false);
                ManagedMods.Instance.ModConfigurations.Add(new InstallError(ex));
            }
            return await tcs.Task;
        }

        /// <summary>
        /// [PARTIAL NYI]Queues all of this mod's files for removal, then deletes its configuration
        /// </summary>
        public async Task UninstallMod()
        {
            try
            {
                await RemoveModFiles();

                Task task = new Task(() =>
                {
                    /*string[] paths = Directory.EnumerateFiles(_path).ToArray();
                    foreach (string s in paths)
                    {
                        if (IsModFile(s))
                            File.Delete(s);

                    }*/
                    Directory.Delete(_path, true);

                    ManagedMods.SyncContext.Send(state => ManagedMods.Instance.ModConfigurations.Remove(this), null);
                });
                task.Start();
                await task;
            }
            catch (Exception ex)
            {
                MessageDisplay.RaiseError(new ErrorEventArgs(ex));
                ManagedMods.Instance.ModConfigurations.Add(new InstallError(ex));
            }
        }

        public async Task RemoveModFiles()
        {
            Task task = new Task(() =>
            {
                if (HasConfigurator)
                {
                    Configurator.RemoveModFiles();
                }
                else
                {
                    foreach (string s in GetModFileNames()/*Directory.EnumerateFiles(_path).Where(x => IsModFile(x))*/)
                    {
                        if (Path.GetExtension(s).ToLowerInvariant() == ".package")
                            FileWrite.SafeDeleteFile(FileWrite.GetFileOutputPath(ComponentGameDir.galacticadventures.ToString(), s, _isLegacy));
                        else if (Path.GetExtension(s).ToLowerInvariant() == ".dll")
                            FileWrite.SafeDeleteFile(FileWrite.GetFileOutputPath(ComponentGameDir.modapi.ToString(), s, _isLegacy));
                    }
                }
            });
            task.Start();
            await task;
        }

        private void SafeSetAttributeValue(XName name, string value)
        {
            Document.Root.SetAttributeValue(name, value);
            Document.Save(_xmlPath);
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler IsProgressingChanged;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
