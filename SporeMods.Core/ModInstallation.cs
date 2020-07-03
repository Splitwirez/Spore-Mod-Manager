using Ionic.Zip;
using SporeMods.Core.InstalledMods;
using SporeMods.Core.ModIdentity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows;
using System.Xml.Linq;

namespace SporeMods.Core
{
    public static class ModInstallation
    {
        public static string InstallationQueuePath = Path.Combine(Settings.ProgramDataPath, "InstallQueue.info");

        public static string InstructionPath = Path.Combine(Settings.ProgramDataPath, "Instruction.info");

        static List<string> _installableMods = new List<string>();
        /*public static ErrorInfo[]*/

        public static void ClearQueues()
        {
            if (File.Exists(InstructionPath))
                File.Delete(InstructionPath);

            if (File.Exists(InstallationQueuePath))
                File.Delete(InstallationQueuePath);

            if (Directory.Exists(Settings.ModQueuePath))
            {
                string[] files = Directory.EnumerateFiles(Settings.ModQueuePath).ToArray();
                foreach (string s in files)
                {
                    if (File.Exists(s))
                        File.Delete(s);
                }
            }

            foreach (string d in Directory.EnumerateDirectories(Settings.ModConfigsPath))
            {
                string[] files = Directory.EnumerateFiles(d).ToArray();
                foreach (string s in files)
                {
                    if (Path.GetExtension(s).ToLowerInvariant() == ".completion")
                        File.Delete(s);
                }
            }
        }

        public static async Task InstallModsAsync(string[] modPaths)
        {
            for (int i = 0; i < modPaths.Length; i++)
            {
                string path = modPaths[i];
                try
                {
                    if (Path.GetExtension(path).ToLowerInvariant() == ".package")
                    {
                        await RegisterLoosePackageModAsync(path);
                    }
                    else if (Path.GetExtension(path).ToLowerInvariant() == ".sporemod")
                    {
                        await RegisterSporemodModAsync(path);
                    }
                }
                catch (Exception ex)
                {
                    MessageDisplay.RaiseError(new ErrorEventArgs(ex/*ex.Message + "\n" + ex.StackTrace*/));
                    ManagedMods.Instance.ModConfigurations.Add(new InstallError(ex));
                }
            }
        }

        public static async Task UninstallModsAsync(IInstalledMod[] modConfigurations)
        {
            List<string> modsToUninstall = new List<string>();

            foreach (IInstalledMod mod in modConfigurations)
            {
                try
                {
                    mod.UninstallMod();
                    modsToUninstall.Add(mod.RealName);
                }
                catch (Exception ex)
                {
                    MessageDisplay.RaiseError(new ErrorEventArgs(ex/*ex.Message + "\n" + ex.StackTrace*/));
                    ManagedMods.Instance.ModConfigurations.Add(new InstallError(ex));
                }
            }

            File.WriteAllLines(InstallationQueuePath, modsToUninstall.ToArray());
            File.WriteAllText(InstructionPath, "install");
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(int flags, bool inherit, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, out int lpdwSize);

        public static string GetExecutablePath(this Process process)
        {
            string returnValue = string.Empty;
            StringBuilder stringBuilder = new StringBuilder(1024);
            IntPtr hprocess = OpenProcess(0x1000, false, process.Id);

            if (hprocess != IntPtr.Zero)
            {
                int size = stringBuilder.Capacity;

                if (QueryFullProcessImageName(hprocess, 0, stringBuilder, out size))
                    returnValue = stringBuilder.ToString();
            }

            return returnValue;
        }

        /*public static void VerifyServantIsRunning()
        {
            try
            {
                if (Process.GetProcessesByName("SporeMods.InstallServant").Length == 0)
                {
                    string servantPath = Path.Combine(Settings.ManagerInstallLocationPath, "SporeMods.InstallServant.exe");

                    if (File.Exists(servantPath))
                    {
                        var info = new ProcessStartInfo(servantPath);
                        if (Permissions.IsAtleastWindowsVista())
                            info.Verb = "runas";
                        Process.Start(info);
                    }
                    else
                        throw new FileNotFoundException("Where's the Install Servant? We were expecting it here: " + servantPath);
                }
            }
            catch (Exception ex)
            {
                InvokeErrorOccurred(new ErrorEventArgs(ex.Message + "\n" + ex.StackTrace));
            }
        }*/

        /*public static void DoFirstRunVerification()
        {
            Debug.WriteLine("FIRST RUN VERIFICATION");
            if (Settings.IsFirstRun)
            {
                Debug.WriteLine("IS FIRST RUN");
                File.WriteAllText(InstructionPath, "verify");
                Settings.IsFirstRun = false;
            }
        }*/

        public static event EventHandler<ModRegistrationEventArgs> AddModProgress;

        static void InvokeAddModProgress(InstalledMod mod)
        {
            AddModProgress?.Invoke(null, new ModRegistrationEventArgs(mod));
        }

        public static async Task RegisterLoosePackageModAsync(string path)
        {
            InstalledMod mod = null;
            if (Settings.AllowVanillaIncompatibleMods)
            {
                string noExtensionName = string.Empty;
                try
                {
                    string name = Path.GetFileName(path);
                    noExtensionName = Path.GetFileNameWithoutExtension(path).Replace(".", "-");

                    string dir = Path.Combine(Settings.ModConfigsPath, noExtensionName);

                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    CreateModInfoXml(noExtensionName, dir, out XDocument document);
                    File.WriteAllText(Path.Combine(dir, "UseLegacyDLLs"), string.Empty);

                    mod = new InstalledMod(noExtensionName)
                    {
                        FileCount = 2,
                        Progress = 0,
                        IsProgressing = true
                    };

                    ManagedMods.Instance.AddMod(mod);
                    //mod = await ManagedMods.Instance.GetModConfigurationAsync(noExtensionName);
                    //mod.FileCount = 2;

                    Task task = new Task(() =>
                    {
                        File.Copy(path, Path.Combine(dir, name));
                    });
                    task.Start();
                    await task;
                    ManagedMods.RemoveMatchingManuallyInstalledFile(name, ComponentGameDir.galacticadventures);
                    mod.Progress++;

                    await mod.EnableMod();

                    _installableMods.Add(noExtensionName);
                }
                catch (Exception ex)
                {
                    MessageDisplay.RaiseError(new ErrorEventArgs(ex/*.Message + "\n" + ex.StackTrace*/), noExtensionName);
                    if ((mod != null) && ManagedMods.Instance.ModConfigurations.Contains(mod))
                    {
                        ManagedMods.Instance.ModConfigurations.Remove(mod);
                    }   
                    ManagedMods.Instance.ModConfigurations.Add(new InstallError(path, ex));
                }
            }
        }

        public static async Task RegisterSporemodModAsync(string path)
        {
            bool isUnique = true;
            string name = string.Empty;
            bool proceed = false;
            InstalledMod mod = null;
            try
            {
            name = Path.GetFileNameWithoutExtension(path).Replace(".", "-");
            using (ZipFile zip = new ZipFile(path))
            {
                string unique = name;

                string dir = Path.Combine(Settings.ModConfigsPath, name);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                XDocument document = null;

                Task validateModTask = new Task(() =>
                {
                    if (zip.ContainsEntry("ModInfo.xml"))
                    {
                        ZipEntry entry = zip["ModInfo.xml"];
                        entry.Extract(Settings.TempFolderPath, ExtractExistingFileAction.OverwriteSilently);
                        XDocument compareDocument = XDocument.Load(Path.Combine(Settings.TempFolderPath, "ModInfo.xml"));

                        var uniqueAttr = compareDocument.Root.Attribute("unique");
                        if (uniqueAttr != null)
                            unique = uniqueAttr.Value;

                        var vanillaCompatAttr = compareDocument.Root.Attribute("verifiedVanillaCompatible");
                        if (vanillaCompatAttr != null)
                        {
                            if (vanillaCompatAttr.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                                proceed = true;
                        }
                    }
                    else if (Settings.AllowVanillaIncompatibleMods)
                        CreateModInfoXml(name, Settings.TempFolderPath, out document);

                    if (proceed || Settings.AllowVanillaIncompatibleMods)
                    {
                        string[] modConfigDirs = Directory.EnumerateDirectories(Settings.ModConfigsPath).ToArray();
                        foreach (string s in modConfigDirs)
                        {
                            string xmlPath = Path.Combine(s, "ModInfo.xml");
                            if (File.Exists(xmlPath))
                            {
                                XDocument modDocument = XDocument.Load(xmlPath);
                                string modUnique = Path.GetFileName(s);
                                var modAttr = modDocument.Root.Attribute("unique");
                                if (modAttr != null)
                                    modUnique = modAttr.Value;
                                if (unique.ToLowerInvariant() == modUnique.ToLowerInvariant())
                                {
                                    MessageDisplay.DebugShowMessageBox("Unique identifier matched: " + unique + ", " + modUnique);
                                    isUnique = false;
                                    break;
                                }
                            }
                        }
                    }
                }/*, TaskCreationOptions.LongRunning*/);
                validateModTask.Start();
                await validateModTask;
                if (proceed || Settings.AllowVanillaIncompatibleMods)
                {
                    //ModInstallation.DebugMessageBoxShow("Evaluating uniqueness");
                    if (isUnique)
                    {
                        //ModInstallation.DebugMessageBoxShow("Mod is unique");
                        Task extractXMLTask = new Task(() =>
                        {
                            if (zip.ContainsEntry("ModInfo.xml"))
                            {
                                ZipEntry entry = zip["ModInfo.xml"];
                                entry.Extract(dir, ExtractExistingFileAction.OverwriteSilently);
                            }
                            else
                            {
                                File.WriteAllText(Path.Combine(dir, "UseLegacyDLLs"), string.Empty);
                                document.Save(Path.Combine(dir, "ModInfo.xml"));
                            }
                        });
                        extractXMLTask.Start();
                        await extractXMLTask;

                        //ModInstallation.DebugMessageBoxShow("Generating InstalledMod");
                        mod = new InstalledMod(name)
                        {
                            Progress = 0,
                            FileCount = 1,
                            IsProgressing = true
                        };// ManagedMods.Instance.GetModConfiguration(name);
                          //ManagedMods.Instance.AddMod(mod);

                        //ModInstallation.DebugMessageBoxShow("Adding InstalledMod");
                        ManagedMods.Instance.ModConfigurations.Add(mod);
                        //ModInstallation.DebugMessageBoxShow("InstalledMod should be added");

                        Task evaluateArchiveAndCountFilesTask = new Task(() =>
                        {
                            for (int i = 0; i < zip.Entries.Count; i++)
                            {
                                ZipEntry e = zip.Entries.ElementAt(i);

                                if (InstalledMod.IsModFile(Path.GetExtension(e.FileName)))
                                {
                                    mod.FileCount++;
                                }

                                string newFileName = e.FileName.Replace(@"/", @"\");
                                if ((!e.IsDirectory) && newFileName.Contains(@"\"))
                                {
                                    newFileName = newFileName.Substring(newFileName.LastIndexOf(@"\"));
                                    newFileName = newFileName.Replace(@"\", @"/");
                                    e.FileName = newFileName;
                                }
                            }
                        });
                        evaluateArchiveAndCountFilesTask.Start();
                        await evaluateArchiveAndCountFilesTask;

                        for (int i = 0; i < Directory.EnumerateDirectories(dir).Count(); i++)
                        {
                            string s = Directory.EnumerateDirectories(dir).ElementAt(i);
                            if ((Directory.EnumerateFiles(s).Count() == 0) && (Directory.EnumerateDirectories(s).Count() == 0) && s.ToLowerInvariant().Replace(dir.ToLowerInvariant(), string.Empty).TrimStart('\\').StartsWith(name.ToLowerInvariant()))
                                Directory.Delete(s);
                        }

                        //ModInstallation.DebugMessageBoxShow("Beginning file extraction");
                        Task extractFilesTask = new Task(() =>
                        {
                            foreach (ZipEntry e in zip.Entries)
                            {
                                bool isModInfo = e.FileName.ToLowerInvariant().EndsWith("modinfo.xml");
                                if ((!isModInfo) || (isModInfo && (document == null)))
                                {
                                    e.Extract(dir, ExtractExistingFileAction.OverwriteSilently);
                                    //remove corresponding manually-installed file

                                    if (!isModInfo)
                                        mod.Progress++;
                                }
                            }
                        });
                        extractFilesTask.Start();
                        await extractFilesTask;

                        if (mod.HasConfigurator)
                            await RegisterSporemodModWithInstallerAsync(name);
                        else
                            await mod.EnableMod();
                    }
                    else
                    {
                        Directory.Delete(dir, true);
                    }
                }
                else
                {
                    Directory.Delete(dir, true);
                }
            }

            if (isUnique && (proceed || Settings.AllowVanillaIncompatibleMods))
                _installableMods.Add(name);

            //return null;
            }
            catch (Exception ex)
            {
                MessageDisplay.RaiseError(new ErrorEventArgs(ex), path);
                if ((mod != null) && ManagedMods.Instance.ModConfigurations.Contains(mod))
                {
                    ManagedMods.Instance.ModConfigurations.Remove(mod);
                }
                ManagedMods.Instance.ModConfigurations.Add(new InstallError(path, ex));
            }
        }

        public static void CreateModInfoXml(string name, string dir, out XDocument document)
        {
            document = XDocument.Parse(@"<mod>
</mod>");
            document.Root.SetAttributeValue("unique", name);
            document.Root.SetAttributeValue("displayName", name);
            document.Root.SetAttributeValue("installerSystemVersion", InstalledMod.XmlModIdentityVersion1_1_0_0.ToString());
            document.Root.SetAttributeValue("copyAllFiles", true.ToString());
            document.Root.SetAttributeValue("canDisable", false.ToString());
            document.Root.SetAttributeValue("isEnabled", true.ToString());

            document.Save(Path.Combine(dir, "ModInfo.xml"));
        }

        public static async Task RegisterSporemodModWithInstallerAsync(string modName)
        {
            //DebugMessageBoxShow("Registering mod with installer");
            InstalledMod mod = await ManagedMods.Instance.GetModConfigurationAsync(modName);
            if (mod.HasConfigurator)
            {
                /*foreach (ModComponent m in mod.Configurator.Components)
                {
                    DebugMessageBoxShow("DisplayName: " + m.DisplayName + "\nUnique: " + m.Unique + "\nIsEnabled: " + m.IsEnabled);
                }*/

                //DebugMessageBoxShow("Component count: " + mod.Configurator.Components.Count + "\nXML Mod Identity Version: " + mod.XmlVersion);

                await ManagedMods.Instance.ShowModConfigurator(mod); // theMod would be a ModConfiguration
                                                                     // The properties on theMod were set in that event handler down there, so theMod now has the user's specified configuration
                List<string> files = new List<string>();
                foreach (ModComponent component in mod.Configurator.Prerequisites)
                {
                    string prer = "COMPONENT: ";
                    foreach (string file in component.FileNames)
                        prer += file + ", ";

                    files.Add(prer);
                }

                foreach (ModComponent component in mod.Configurator.Components)
                {
                    if (component.IsGroup)
                    {
                        foreach (ModComponent subComponent in component.SubComponents)
                        {
                            if (subComponent.IsEnabled)
                            {
                                string prer = "COMPONENT: ";
                                foreach (string file in subComponent.FileNames)
                                    prer += file + ", ";

                                files.Add(prer);
                            }
                        }
                    }
                    else if (component.IsEnabled)
                    {
                        string prer = "COMPONENT: ";
                        foreach (string file in component.FileNames)
                            prer += file + ", ";

                        files.Add(prer);
                    }
                }

                File.WriteAllLines(Path.Combine(Settings.ProgramDataPath, "installLog"), files.ToArray());

                /*Task task = new Task(() =>
                {

                    mod.Progress = 0.0;
                    mod.FileCount = mod.Configurator.GetComponentFileCount() + mod.Configurator.GetEnabledComponentFileCount();
                    MessageDisplay.DebugShowMessageBox("Progress: " + mod.Progress + "\nFileCount: " + mod.FileCount);
                });
                task.Start();
                await task;*/
                await mod.EnableMod();
            }
            else
            {
                MessageDisplay.RaiseError(new ErrorEventArgs(new Exception("This mod does not have a configurator!")));
            }
        }
    }

    public class ErrorInfo
    {
        public string RegistrationErrorData = null;
        public string RegistrationStackTrace = null;
        public string InstallationErrorData = null;
        public string InstallationStackTrace = null;
    }

    public class ModRegistrationEventArgs : EventArgs
    {
        public bool HasCustomInstaller { get; set; } = false;
        public InstalledMod ModConfiguration { get; set; } = null;
        public ModRegistrationEventArgs(InstalledMod config)
        {
            ModConfiguration = config;
            HasCustomInstaller = ModConfiguration.HasConfigurator;
        }
    }

    public class MessageBoxEventArgs : EventArgs
    {
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public MessageBoxEventArgs(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public MessageBoxEventArgs(string content)
        {
            Title = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            Content = content;
        }
    }


        public class ErrorEventArgs : EventArgs
    {
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public Exception Exception { get; set; } = null;

        /*public ErrorEventArgs(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public ErrorEventArgs(string content)
        {
            Title = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            Content = content;
        }*/

        public ErrorEventArgs(Exception ex)
        {
            Title = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            //Content = ex.Message + "\n" + ex.StackTrace;
            Exception = ex;
        }

        public ErrorEventArgs(string title, Exception ex)
        {
            Title = title;
            //Content = ex.Message + "\n" + ex.StackTrace;
            Exception = ex;
        }
    }

    /*public class MessageBoxEventArgs : EventArgs
    {
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public MessageBoxEventArgs(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }*/
}
