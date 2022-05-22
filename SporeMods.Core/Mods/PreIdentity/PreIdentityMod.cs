using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using SporeMods.Core.Transactions;

namespace SporeMods.Core.Mods
{
    public partial class PreIdentityMod : NotifyPropertyChangedBase, ISporeMod
    {
        IModText _displayName = null;
        public IModText DisplayName
        {
            get => _displayName;
            protected set
            {
                _displayName = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasExplicitUnique => false;


        string _unique = string.Empty;
        public string Unique
        {
            get => _unique;
            protected set
            {
                _unique = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasInlineDescription => false;

        public IModText InlineDescription => null;

        public bool HasExplicitVersion => false;

        public Version ModVersion => new Version(0, 0, 0, 0);

        public List<ModDependency> Dependencies => new List<ModDependency>();

        public List<string> UpgradeTargets => new List<string>();

        public bool IsExperimental => false;

        public bool CausesSaveDataDependency => false;

        public bool RequiresGalaxyReset => false;

        bool _usesCodeInjection = false;
        public bool UsesCodeInjection
        {
            get => _usesCodeInjection;
            protected set
            {
                _usesCodeInjection = value;
                NotifyPropertyChanged();
            }
        }

        public bool GuaranteedVanillaCompatible => false;

        
        bool _knownHazardousMod = false;
        public bool KnownHazardousMod
        {
            get => _knownHazardousMod;
            protected set
            {
                _knownHazardousMod = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasSettings
        {
            get => false;
        }


        public bool IsUpgradeTo(ISporeMod mod)
            => Unique == mod.Unique;

        public bool DependsOn(ISporeMod mod)
            => false;

        
        private PreIdentityMod(string recordDirName, XDocument identityDoc = null)
        {
            RecordDirName = recordDirName;
            LoadFromRecordDir(identityDoc);
        }


        private void LoadFromRecordDir(XDocument identityDoc = null)
        {
            string location = Path.Combine(Settings.ModConfigsPath, RecordDirName);
            XDocument doc = identityDoc;
            if (doc == null)
            {
                string identityRaw = File.ReadAllText(Path.Combine(location, ModConstants.ID_XML_FILE_NAME));
                doc = XDocument.Parse(identityRaw, ModConstants.ID_XML_LOAD_OPTIONS);
            }

            List<string> packageNames = new List<string>();
            List<string> dllNames = new List<string>();

            foreach (string s in Directory.EnumerateFiles(location))
            {
                string fileName = Path.GetFileName(s);
                if (Path.GetExtension(fileName).Equals(ModConstants.MOD_FILE_EX_DBPF, StringComparison.OrdinalIgnoreCase))
                    packageNames.Add(fileName);
                else if (Path.GetExtension(fileName).Equals(ModConstants.MOD_SUBFILE_EX_DLL, StringComparison.OrdinalIgnoreCase))
                    dllNames.Add(fileName);
            }

            string displayName = doc.Root.Attribute(ModConstants.AT_DISP_NAME).Value;
            string unique = doc.Root.Attribute(ModConstants.AT_UNIQUE).Value;

            Unique = unique;
            DisplayName = new FixedModText(displayName);
            PackageNames = packageNames;
            DllNames = dllNames;
        }

        private PreIdentityMod(
              string recordDirName
            , string unique
            , string displayName
            , List<string> packageNames
            , List<string> dllNames = null
        )
        {
            RecordDirName = recordDirName;
            Unique = unique;
            DisplayName = new FixedModText(displayName);

            if (packageNames != null)
                PackageNames = packageNames;
            if (dllNames != null)
            {
                DllNames = dllNames;
                UsesCodeInjection = dllNames.Count > 0;
            }
        }

        string _recordDirName = string.Empty;
        public string RecordDirName
        {
            get => _recordDirName;
            protected set
            {
                _recordDirName = value;
                NotifyPropertyChanged();
            }
        }
        IEnumerable<string> _packageNames = new List<string>();
        public IEnumerable<string> PackageNames
        {
            get => _packageNames;
            protected set
            {
                _packageNames = value;
                NotifyPropertyChanged();
            }
        }
        IEnumerable<string> _dllNames = new List<string>();
        public IEnumerable<string> DllNames
        {
            get => _dllNames;
            protected set
            {
                _dllNames = value;
                NotifyPropertyChanged();
            }
        }
        /*private PreIdentityMod(string location, string unique, IModText displayName, List<string> fileNames)
        {
            _recordDirName = string.Empty;
            Unique = unique;
            DisplayName = displayName;

            _fileNames = fileNames;
        }*/


        /// <summary>
        /// Attempt to load information about an already-installed mod from a given subfolder of the <see cref="SporeMods.Core.Settings.ModConfigsPath"/>
        /// </summary>
        /// <param name="location">Complete path to the subfolder to load from</param>
        public static ISporeMod FromRecordDir(string location, XDocument doc)
        {
            try
            {
                /*List<string> packageNames = new List<string>();
                List<string> dllNames = new List<string>();

                foreach (string s in Directory.EnumerateFiles(location))
                {
                    string fileName = Path.GetFileName(s);
                    if (Path.GetExtension(fileName).Equals(ModConstants.MOD_FILE_EX_DBPF, StringComparison.OrdinalIgnoreCase))
                        packageNames.Add(fileName);
                    else if (Path.GetExtension(fileName).Equals(ModConstants.MOD_SUBFILE_EX_DLL, StringComparison.OrdinalIgnoreCase))
                        dllNames.Add(fileName);
                }

                string displayName = doc.Root.Attribute(ModConstants.AT_DISP_NAME).Value;
                string unique = doc.Root.Attribute(ModConstants.AT_UNIQUE).Value;*/
                return new PreIdentityMod(
                    recordDirName: location
                    , identityDoc: doc
                );
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /*protected bool ReadIdentity()
        {

        }*/

        public static async Task<ISporeMod> AnalyzeFromSporemodAsync(string inPath, ZipArchive archive)
        {
            /*await Task.Run(() =>
            {
                string debugText1 = "\n\n\nBegin Entries\n";
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    debugText1 += $"\t\t{entry.FullName}\n";
                }
                debugText1 += "End Entries\n\n\n";
                MessageDisplay.ShowMessageBox(debugText1);
            });*/

            return await Task<ISporeMod>.Run(() =>
            {
                /*using (StreamReader reader = new StreamReader(archive.GetEntry(ModConstants.ID_XML_FILE_NAME).Open()))
                {
                    string text = reader.ReadToEnd();
                    XDocument doc = XDocument.Parse(text);
                    return new PreIdentityMod(null, doc.Root.Attribute("unique").Value, new FixedModText(doc.Root.Attribute("displayName").Value), null);
                }*/

                string fileName = Path.GetFileNameWithoutExtension(inPath);
                string modConfigSubfolderName = fileName;
                
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    modConfigSubfolderName = modConfigSubfolderName.Replace(c, '-');
                }

                //List<string> fileNames = new List<string>();
                
                List<string> dllNames = new List<string>();
                List<string> packageNames = new List<string>();
                if (archive != null)
                {
                    foreach (var h in archive.Entries)
                    {
                        string hName = Path.GetFileName(h.FullName);
                        if (Path.GetExtension(hName).Equals(ModConstants.MOD_FILE_EX_DBPF, StringComparison.OrdinalIgnoreCase))
                            packageNames.Add(hName);
                        else if (Path.GetExtension(hName).Equals(ModConstants.MOD_SUBFILE_EX_DLL, StringComparison.OrdinalIgnoreCase))
                            dllNames.Add(hName);
                        else if (hName.Equals(ModConstants.ID_XML_FILE_NAME))
                            return null;
                    }
                }
                else if (Path.GetExtension(inPath).Equals(ModConstants.MOD_FILE_EX_DBPF))
                {
                    packageNames.Add(Path.GetFileName(inPath));
                }


                PreIdentityMod mod = new PreIdentityMod(
                      recordDirName: modConfigSubfolderName
                    , unique: fileName
                    , displayName: fileName
                    , packageNames: packageNames
                    , dllNames: dllNames
                );

                return mod;
            });
        }

        public static async Task<ISporeMod> AnalyzeFromLoosePackageAsync(string inPath)
        {
            return await Task<ISporeMod>.Run(() =>
            {
                string fileNamewithExtension = Path.GetFileName(inPath);
                string fileName = Path.GetFileNameWithoutExtension(inPath);
                string modConfigSubfolderName = fileName;

                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    modConfigSubfolderName = modConfigSubfolderName.Replace(c, '-');
                }

                PreIdentityMod mod = new PreIdentityMod(
                      recordDirName: modConfigSubfolderName
                    , unique: fileName
                    , displayName: fileName
                    , packageNames: new List<string>() { fileNamewithExtension }
                );

                return mod;
            });
        }

        public async Task<ModJobBatchEntryBase> EnsureCanInstall(ModJobBatchModEntry entry, List<ModJobBatchModEntry> otherEntries)
        {
            ModJobBatchModEntry newEntry = entry;
            List<ModDependency> unfulfilledDeps = new List<ModDependency>();

            await Task.Run(() =>
            {
                foreach (ModDependency dep in Dependencies)
                {
                    unfulfilledDeps.Add(dep);
                }


                foreach (ISporeMod mod in ModsManager.InstalledMods)
                {
                    if (IsUpgradeTo(mod))
                    {
                        newEntry.UpgradingFrom.Add(mod);
                    }
                    foreach (ModDependency dep in unfulfilledDeps)
                    {
                        if (dep.IsFulfilledBy(mod))
                        {
                            unfulfilledDeps.Remove(dep);
                            break;
                        }
                    }
                }

                if (unfulfilledDeps.Count > 0)
                {
                    foreach (var entry in otherEntries)
                    {
                        foreach (ModDependency dep in unfulfilledDeps)
                        {
                            if (dep.IsFulfilledBy(entry.Mod))
                            {
                                unfulfilledDeps.Remove(dep);
                                break;
                            }
                        }
                    }
                }
            });



            if (unfulfilledDeps.Count > 0)
            {
                return new ModJobBatchErrorEntry(entry.ModPath, "Identity!MissingDependency", null);
            }
            else
            {
                return newEntry;
            }
            /*if (Dependencies.All(dp => ModsManager.InstalledMods.Any(mod => dp.IsFulfilledBy(mod)) || otherEntries.Any(ent => dp.IsFulfilledBy(ent.Mod))))
            {
                return entry;
            }
            else
            {
                return new ModJobBatchErrorEntry(entry.ModPath, "Identity!MissingDependency", null);
            }*/
        }


        public override string ToString()
        => DisplayName.ToString();
    }
}
