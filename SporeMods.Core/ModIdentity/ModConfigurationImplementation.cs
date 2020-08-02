using SporeMods.Core.InstalledMods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SporeMods.Core.ModIdentity
{
    public abstract class ModConfiguration : INotifyPropertyChanged
    {
        ObservableCollection<ModComponent> _prerequisites = new ObservableCollection<ModComponent>();
        public ObservableCollection<ModComponent> Prerequisites
        {
            get => _prerequisites;
            set
            {
                _prerequisites = value;
                NotifyPropertyChanged(nameof(Prerequisites));
            }
        }


        ObservableCollection<ModComponent> _compatFiles = new ObservableCollection<ModComponent>();
        public ObservableCollection<ModComponent> CompatFiles
        {
            get => _compatFiles;
            set
            {
                _compatFiles = value;
                NotifyPropertyChanged(nameof(CompatFiles));
            }
        }


        ObservableCollection<ModComponent> _disabledFiles = new ObservableCollection<ModComponent>();
        public ObservableCollection<ModComponent> DisabledFiles
        {
            get => _disabledFiles;
            set
            {
                _disabledFiles = value;
                NotifyPropertyChanged(nameof(DisabledFiles));
            }
        }

        ObservableCollection<ModComponent> _components = new ObservableCollection<ModComponent>();
        public ObservableCollection<ModComponent> Components
        {
            get => _components;
            set
            {
                _components = value;
                NotifyPropertyChanged(nameof(Components));
            }
        }

        ObservableCollection<ModComponent> _removes = new ObservableCollection<ModComponent>();
        public ObservableCollection<ModComponent> Removes
        {
            get => _removes;
            set
            {
                _removes = value;
                NotifyPropertyChanged(nameof(Removes));
            }
        }

        public string ModName
        {
            get => _mod.DisplayName;
        }

        public string ModDescription
        {
            get => _mod.Description;
        }

        public abstract Task<bool> EnableMod();
        public abstract Task<bool> DisableMod();
        //public abstract Task<bool> UninstallMod();
        public abstract Task RemoveModFiles();

        public List<ModComponent> GetAllAdditiveComponents()
        {
            List<ModComponent> components = new List<ModComponent>();

            foreach (ModComponent m in Prerequisites)
                components.Add(m);
            foreach (ModComponent m in Components)
            {
                if (m.IsGroup)
                {
                    foreach (ModComponent c in m.SubComponents)
                    {
                        if ((c.FileNames.Length > 0) && (c.FileGames.Length > 0))
                        {
                            components.Add(c);
                            string componentOutput = "subComponent added: " + c.DisplayName;

                            foreach (string s in c.FileNames)
                                componentOutput += ", " + s;

                            foreach (string s in c.FileGames)
                                componentOutput += "; " + s;

                            //ModInstallation.DebugMessageBoxShow(componentOutput);
                        }
                    }
                }
                else if ((m.FileNames.Length > 0) && (m.FileGames.Length > 0))
                {
                    components.Add(m);
                    string componentOutput = "component added: " + m.DisplayName;

                    foreach (string s in m.FileNames)
                        componentOutput += ", " + s;

                    foreach (string s in m.FileGames)
                        componentOutput += "; " + s;
                    //ModInstallation.DebugMessageBoxShow(componentOutput);
                }
            }
            foreach (ModComponent m in CompatFiles)
                components.Add(m);
            foreach (ModComponent m in DisabledFiles)
                components.Add(m);

            return components;
        }

        public double GetComponentFileCount()
        {
            double fileCount = 0.0;

            foreach (ModComponent m in Prerequisites)
                fileCount += m.FileNames.Count();

            foreach (ModComponent m in Components)
            {
                if (m.IsGroup)
                {
                    foreach (ModComponent c in m.SubComponents)
                            fileCount += c.FileNames.Count();
                }
                else
                    fileCount += m.FileNames.Count();
            }

            foreach (ModComponent m in Removes)
                fileCount += m.FileNames.Count();

            foreach (ModComponent m in CompatFiles)
                fileCount += m.FileNames.Count();

            return fileCount;
        }

        public double GetEnabledComponentFileCount()
        {
            double fileCount = 0.0;

            foreach (ModComponent m in Prerequisites)
                fileCount += m.FileNames.Count();

            foreach (ModComponent m in Components)
            {
                if (m.IsGroup)
                {
                    foreach (ModComponent c in m.SubComponents)
                    {
                        if (c.IsEnabled)
                            fileCount += c.FileNames.Count();
                    }
                }
                else if (m.IsEnabled)
                    fileCount += m.FileNames.Count();
            }

            foreach (ModComponent m in Removes)
                fileCount += m.FileNames.Count();

            foreach (ModComponent m in CompatFiles)
                fileCount += m.FileNames.Count();

            return fileCount;
        }

        protected InstalledMod _mod;

        public ModConfiguration(InstalledMod mod)
        {
            _mod = mod;
        }

        public string GetStoragePath() => _mod._path;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ModConfigurationV1_0_0_0 : ModConfiguration
    {
        bool _v1_0_1_X = false;
        public ModConfigurationV1_0_0_0(InstalledMod mod, bool V1_0_1_X) : base(mod)
        {
            _v1_0_1_X = V1_0_1_X;
            foreach (XElement node in _mod.Document.Root.Elements())
            {
                if (node.Name.LocalName.ToLowerInvariant() == "prerequisite")
                {
                    if (!string.IsNullOrWhiteSpace(node.Value))
                        Prerequisites.Add(new ModComponentV1_0_0_0(node));
                    /*else
                        ModInstallation.DebugMessageBoxShow(node.Name.LocalName.ToLowerInvariant() + "\n" + node.ToString());*/
                }
                else if ((node.Name.LocalName.ToLowerInvariant() == "componentgroup") || (node.Name.LocalName.ToLowerInvariant() == "component"))
                {
                    if (!string.IsNullOrWhiteSpace(node.Value))
                        Components.Add(new ModComponentV1_0_0_0(node));
                    /*else if (node.Name.LocalName.ToLowerInvariant() == "component")
                        Components.Add(new ModComponentV1_0_0_0(node));*/
                }
                else if (node.Name.LocalName.ToLowerInvariant() == "compatfile")
                {
                    if (!string.IsNullOrWhiteSpace(node.Value))
                        CompatFiles.Add(new ModComponentV1_0_0_0(node));
                }
            }
        }

        public override async Task<bool> DisableMod()
        {
            await RemoveModFiles();
            return true;
        }

        public override async Task<bool> EnableMod()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Debug.WriteLine("ModConfigurationV1_0_0_0.EnableMod() called!");
            //Exception exc = null;
            //List<string> fileQueue = new List<string>(); //_mod.RemoveModFiles();
            int prerequisiteCount = 0;
            try
            {
                await RemoveModFiles();
                Task task = new Task(() =>
                {
                    _mod.FileCount = GetEnabledComponentFileCount();

                    foreach (ModComponentV1_0_0_0 c in Removes)
                    {
                        for (int i = 0; i < c.FileNames.Count(); i++)
                        {
                            try
                            {
                                DirectoryInfo info = null;

                                if (c.FileGames[i].ToLowerInvariant() == ComponentGameDir.galacticadventures.ToString())
                                    info = new DirectoryInfo(GameInfo.GalacticAdventuresData);
                                else if (c.FileGames[i].ToLowerInvariant() == ComponentGameDir.spore.ToString())
                                    info = new DirectoryInfo(GameInfo.CoreSporeData);
                                else if (!_v1_0_1_X)
                                    info = new DirectoryInfo(Settings.LegacyLibsPath);
                                else
                                    info = new DirectoryInfo(Settings.ModLibsPath);

                                List<FileInfo> files = info.EnumerateFiles(c.FileNames[i]).ToList();

                                foreach (FileInfo f in files)
                                {
                                    if (File.Exists(f.FullName))
                                        FileWrite.SafeDeleteFile(f.FullName);
                                    //File.Delete(f.FullName);
                                }
                                _mod.Progress++;
                            }
                            catch (Exception ex)
                            {
                                MessageDisplay.RaiseError(new ErrorEventArgs(ex));
                            }
                        }
                    }

                    foreach (ModComponentV1_0_0_0 c in Prerequisites)
                    {
                        for (int i = 0; i < c.FileNames.Count(); i++)
                        {
                            try
                            {
                                FileWrite.SafeCopyFile(Path.Combine(_mod._path, c.FileNames[i]), FileWrite.GetFileOutputPath(c.FileGames[i], c.FileNames[i], !_v1_0_1_X)); //fileQueue.Add(c.FileNames[i] + "?" + c.FileGames[i].ToLowerInvariant());
                                _mod.Progress++;
                                prerequisiteCount++;
                            }
                            catch (Exception ex)
                            {
                                MessageDisplay.RaiseError(new ErrorEventArgs(ex));
                                MessageDisplay.DebugShowMessageBox("PREREQUISITE FAILED!\n" + ex.GetType().ToString() + "\n" + ex.Message + "\n" + ex.StackTrace);
                            }
                            //ModInstallation.DebugMessageBoxShow("prerequisiteCount: " + prerequisiteCount.ToString());
                        }
                    }

                    foreach (ModComponentV1_0_0_0 c in Components)
                    {
                        if (c.IsGroup)
                        {
                            foreach (ModComponent b in c.SubComponents)
                            {
                                if (b.IsEnabled)
                                {
                                    for (int i = 0; i < b.FileNames.Count(); i++)
                                    {
                                        try
                                        {
                                            FileWrite.SafeCopyFile(Path.Combine(_mod._path, b.FileNames[i]), FileWrite.GetFileOutputPath(b.FileGames[i], b.FileNames[i], !_v1_0_1_X)); //fileQueue.Add(b.FileNames[i] + "?" + b.FileGames[i].ToLowerInvariant());
                                            _mod.Progress++;
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageDisplay.RaiseError(new ErrorEventArgs(ex));
                                            string title = string.Empty;
                                            if (c.FileNames.Length > 0)
                                                title += ", " + c.FileNames[0];


                                            if (c.FileGames.Length > 0)
                                                title += ", " + c.FileGames[0];

                                            title += ", " + c.FileNames.Length.ToString() + ", " + c.FileGames.Length.ToString();

                                            MessageDisplay.RaiseError(new ErrorEventArgs(title, ex));
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        else if (c.IsEnabled)
                        {
                            for (int i = 0; i < c.FileNames.Count(); i++)
                            {
                                try
                                {
                                    FileWrite.SafeCopyFile(Path.Combine(_mod._path, c.FileNames[i]), FileWrite.GetFileOutputPath(c.FileGames[i], c.FileNames[i], !_v1_0_1_X)); //fileQueue.Add(c.FileNames[i] + "?" + c.FileGames[i].ToLowerInvariant());
                                    _mod.Progress++;
                                }
                                catch (Exception ex)
                                {
                                    MessageDisplay.RaiseError(new ErrorEventArgs(ex));
                                    string title = c.Unique;
                                    if (c.FileNames.Length > 0)
                                        title += " " + c.FileNames[0];


                                    if (c.FileGames.Length > 0)
                                        title += " " + c.FileGames[0];

                                    title += " " + c.FileNames.Length.ToString() + " " + c.FileGames.Length.ToString();

                                    MessageDisplay.RaiseError(new ErrorEventArgs(title, ex));
                                }
                            }
                        }
                    }

                    foreach (ModComponentV1_0_0_0 c in CompatFiles)
                    {
                        /*for (int i = 0; i < c.FileNames.Count(); i++)
                        {
                            string data = c.FileNames[i] + "?" + c.FileGames[i].ToLowerInvariant() + "?compat<";
                            for (int j = 0; j < c.CompatFileNames.Count(); i++)
                            {
                                data += c.CompatFileNames[j] + "|" + c.CompatFileGames[j].ToLowerInvariant();
                                if (j < (c.CompatFileNames.Count() - 1))
                                    data += ":";
                            }

                            fileQueue.Add(data);
                        }*/
                    }
                });
                task.Start();
                await task;
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                MessageDisplay.RaiseError(new ErrorEventArgs(ex/*ex.GetType() + "\n\n" + ex.StackTrace*/));
                tcs.TrySetResult(false);
            }

            //string queuePath = Path.Combine(Settings.ModQueuePath, _mod.RealName);
            //File.WriteAllLines(queuePath, fileQueue.ToArray());
            return await tcs.Task;
        }

        /*public override async Task<bool> UninstallMod()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            try
            {
                /*List<string> fileQueue = _mod.RemoveModFiles();
                string queuePath = Path.Combine(Settings.ModQueuePath, _mod.RealName);
                File.WriteAllLines(queuePath, fileQueue.ToArray());
                return null;*
                await RemoveModFiles();
                /*string queuePath = Path.Combine(Settings.ModQueuePath, _mod.RealName);
                File.WriteAllLines(queuePath, fileQueue.ToArray());*

                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                ModInstallation.InvokeErrorOccurred(new ErrorEventArgs(ex));
                tcs.TrySetResult(false);
            }
            return await tcs.Task;
        }*/

        public override async Task RemoveModFiles()
        {
            Task task = new Task(() =>
            {
                foreach (ModComponent m in GetAllAdditiveComponents())
                {
                    for (int i = 0; i < m.FileNames.Length; i++)
                        FileWrite.SafeDeleteFile(FileWrite.GetFileOutputPath(m.FileGames[i], m.FileNames[i], !_v1_0_1_X));
                }
            });
            task.Start();
            await task;
        }
    }
}
