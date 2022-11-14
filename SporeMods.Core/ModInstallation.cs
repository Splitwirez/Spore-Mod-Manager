//using Ionic.Zip;
using SporeMods.Core.Mods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
		internal static bool IS_INSTALLING_MODS = false;
		internal static Dictionary<string, Exception> INSTALL_FAILURES = new Dictionary<string, Exception>();

		internal static bool IS_UNINSTALLING_MODS = false;

		internal static bool IS_RECONFIGURING_MODS = false;

		static readonly string MOD_INFO = "ModInfo.xml";


		[DllImport("shlwapi.dll")]
		static extern bool PathIsNetworkPath(string pszPath);

		public static event Func<string, bool> InstallingExperimentalMod;
		public static event Func<string, bool> InstallingRequiresGalaxyResetMod;
		public static event Func<string, bool> InstallingSaveDataDependencyMod;

		public static event Func<IEnumerable<string>, bool> UninstallingSaveDataDependencyMod;


		static List<string> _installableMods = new List<string>();
		/*public static ErrorInfo[]*/

		public static void ClearQueues()
		{
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

		public static string AnyInstallActivitiesPath = Path.Combine(Settings.TempFolderPath, "InstallingSomething");
		static int _installActivitiesCounter = 0;
		internal static int InstallActivitiesCounter
		{
			get => _installActivitiesCounter;
			set
			{
				_installActivitiesCounter = value;

				if ((_installActivitiesCounter > 0) && (!File.Exists(AnyInstallActivitiesPath)))
					File.Create(AnyInstallActivitiesPath).Close();
				else if ((_installActivitiesCounter <= 0) && File.Exists(AnyInstallActivitiesPath))
					File.Delete(AnyInstallActivitiesPath);
			}
		}

		public static async Task<ModInstallationStatus> InstallModsAsync(string[] modPaths)
		{
			IS_INSTALLING_MODS = true;
			//ModsManager.Instance.AddToTaskCount(modPaths.Count(x => !PathIsNetworkPath(x)));
			//InstallActivitiesCounter++;
			var retVal = new ModInstallationStatus();
			//Task<ModInstallationStatus> task = new Task<ModInstallationStatus>(())
			for (int i = 0; i < modPaths.Length; i++)
			{
				string path = modPaths[i];
				bool validExtension = true;
				Exception result = null;
				
				if (PathIsNetworkPath(path))
				{
					INSTALL_FAILURES.Add(Path.GetFileName(path), new Exception("Cannot install mods from network locations. Please move the mod(s) to local storage and try again from there."));
				}
				else if (Path.GetExtension(path).ToLowerInvariant() == ".package")
				{
					/*result = await */RegisterLoosePackageModAsync(path);
				}
				else if (Path.GetExtension(path).ToLowerInvariant() == ".sporemod")
				{
					/*result = await */RegisterSporemodModAsync(path);
				}
				else
				{
					validExtension = false;
				}
				/*catch (Exception ex)
				{
					MessageDisplay.RaiseError(new ErrorEventArgs(ex/*ex.Message + "\n" + ex.StackTrace*));
					ModsManager.InstalledMods.Add(new InstallError(ex));
				}*/
				if (result != null)
					retVal.Failures.Add(Path.GetFileName(path), result);
				else if (validExtension)
					retVal.Successes.Add(Path.GetFileName(path));
			}

			//InstallActivitiesCounter--;
			return retVal;
		}

		public static void UninstallModsAsync(IInstalledMod[] modConfigurations)
		{
			IS_UNINSTALLING_MODS = true;
			//ModsManager.Instance.AddToTaskCount(modConfigurations.Length);
			//InstallActivitiesCounter++;

			List<IInstalledMod> modsToUninstall = modConfigurations.ToList();
			List<IInstalledMod> modsToThinkTwiceBeforeUninstalling = new List<IInstalledMod>();

			foreach (IInstalledMod mod in modsToUninstall.Where(x => (x is ManagedMod xm) && xm.Identity.CausesSaveDataDependency))
				modsToThinkTwiceBeforeUninstalling.Add(mod);

			if (modsToThinkTwiceBeforeUninstalling.Count() > 0)
			{
				List<string> modNames = new List<string>();
				foreach (IInstalledMod mod in modsToThinkTwiceBeforeUninstalling)
					modNames.Add(mod.DisplayName);

				if (!UninstallingSaveDataDependencyMod(modNames))
				{
					foreach (IInstalledMod mod in modsToThinkTwiceBeforeUninstalling)
						modsToUninstall.Remove(mod);
				}
			}


			foreach (IInstalledMod mod in modsToUninstall)
			{
				// This function doesn't throw exceptions, the code inside must handle it
				mod.UninstallModAsync();
			}

			//InstallActivitiesCounter--;
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

		static void InvokeAddModProgress(ManagedMod mod)
		{
			AddModProgress?.Invoke(null, new ModRegistrationEventArgs(mod));
		}

		public static async Task<Exception> RegisterLoosePackageModAsync(string path)
		{
			try
			{
				ManagedMod mod = null;
				if (Settings.AllowVanillaIncompatibleMods)
				{
					string noExtensionName = string.Empty;

					string name = Path.GetFileName(path);
					noExtensionName = Path.GetFileNameWithoutExtension(path).Replace(".", "-");

					string dir = Path.Combine(Settings.ModConfigsPath, noExtensionName);

					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);

					CreateModInfoXml(noExtensionName, dir, out XDocument document);
					string legacyPath = Path.Combine(dir, "UseLegacyDLLs");
					File.WriteAllText(legacyPath, string.Empty);
					Permissions.GrantAccessFile(legacyPath);

					mod = new ManagedMod(noExtensionName, true)
					{
						Progress = 0,
						IsProgressing = true
					};

					Task task = new Task(() =>
					{
						var prevMod = ModsManager.GetManagedMod(noExtensionName);
						if (prevMod != null)
						{
							int prevModIndex = ModsManager.InstalledMods.IndexOf(prevMod);
							ModsManager.RemoveMod(prevMod);
							ModsManager.InsertMod(prevModIndex, mod);
						}
						else
							ModsManager.AddMod(mod);
						//mod = await ManagedMods.Instance.GetModConfigurationAsync(noExtensionName);
						//mod.FileCount = 2;

						FileWrite.SafeCopyFile(path, Path.Combine(dir, name));
					});
					task.Start();
					await task;

					ModsManager.RemoveMatchingManuallyInstalledFile(name, ComponentGameDir.GalacticAdventures);
					mod.Progress++;

					await mod.EnableMod();

					_installableMods.Add(noExtensionName);
					/*}
					catch (Exception ex)
					{
						MessageDisplay.RaiseError(new ErrorEventArgs(ex/*.Message + "\n" + ex.StackTrace*), noExtensionName);
						if ((mod != null) && ModsManager.InstalledMods.Contains(mod))
						{
							ModsManager.InstalledMods.Remove(mod);
						}	
						ModsManager.InstalledMods.Add(new InstallError(path, ex));
					}*/
				}
				return null;
			}
			catch (Exception ex)
			{
				INSTALL_FAILURES.Add(Path.GetFileName(path), ex);
				return ex;
			}
		}

		public static async Task<Exception> RegisterSporemodModAsync(string path)
		{
			bool isUnique = true;
			string name = string.Empty;
			bool proceed = false;
			ManagedMod mod = null;
			string compareUniquePath = string.Empty;
			try
			{
				name = Path.GetFileNameWithoutExtension(path).Replace(".", "-");
				using (ZipArchive zip = ZipFile.OpenRead(path)) //ZipFile zip = new ZipFile(path))
				{
					string unique = name;

					string dir = Path.Combine(Settings.ModConfigsPath, name);
					string prevConfigDirPath = null;
					int prevModIndex = 0;
					ManagedMod prevMod = null;

					XDocument document = null;



					if (Settings.DebugMode)
					{
						string entryNames = string.Empty;
						foreach (var entry in zip.Entries)
							entryNames += "NAME: " + entry.Name + "\n	  FULLNAME: " + entry.FullName + "\n\n";

						MessageDisplay.ShowMessageBox(entryNames);
						//return null;
					}

					Task validateModTask = new Task(() =>
					{
						if (zip.TryGetEntry(MOD_INFO, out ZipArchiveEntry entry))
						{
							//ZipArchiveEntry entry = zip.GetEntry(MOD_INFO);
							//compareUniquePath = Path.Combine(Settings.TempFolderPath, name + ".xml");
							//entry.OpenReader()
							//entry.Extract(Settings.TempFolderPath, ExtractExistingFileAction.OverwriteSilently);
							XDocument compareDocument = null; //XDocument.Load(compareUniquePath);
							using (Stream strm = entry.Open() /*FileStream stream = new FileStream(compareUniquePath, FileMode.OpenOrCreate)*/)
							{
								compareDocument = XDocument.Load(strm); //compareUniquePath);
								//entry.Extract(stream);
							}
							//Permissions.GrantAccessFile(compareUniquePath);

							Version identityVersion = ModIdentity.UNKNOWN_MOD_VERSION;
							var xmlModIdentityVersionAttr = compareDocument.Root.Attribute("installerSystemVersion");
							if (xmlModIdentityVersionAttr != null)
							{
								if (Version.TryParse(xmlModIdentityVersionAttr.Value, out identityVersion))
								{
									if (!ModIdentity.IsLauncherKitCompatibleXmlModIdentityVersion(identityVersion))
									{
										throw new UnsupportedXmlModIdentityVersionException(identityVersion);
									}
									else
										XmlModIdentityV1.ParseModIdentity(null, compareDocument.Root);
								}
								else
									throw new UnsupportedXmlModIdentityVersionException(ModIdentity.UNKNOWN_MOD_VERSION);
							}
							else
								throw new MissingXmlModIdentityAttributeException(null);

							Version dllsBuild = null;
							if (identityVersion > ModIdentity.XmlModIdentityVersion1_0_0_0)
							{
								var dllsBuildAttr = compareDocument.Root.Attribute("dllsBuild");
								if (dllsBuildAttr != null)
								{
									if (Version.TryParse(dllsBuildAttr.Value + ".0", out dllsBuild))
									{
										if (dllsBuild > Settings.CurrentDllsBuild)
											throw new UnsupportedDllsBuildException(dllsBuild);
									}
									else
										throw new UnsupportedDllsBuildException(ModIdentity.UNKNOWN_MOD_VERSION);
								}
								else
									throw new MissingXmlModIdentityAttributeException("dllsBuild");
							}

							var uniqueAttr = compareDocument.Root.Attribute("unique");
							if (uniqueAttr != null)
							{
								unique = uniqueAttr.Value;

								foreach (char c in Path.GetInvalidPathChars())
									unique = unique.Replace(c.ToString(), string.Empty);

								dir = Path.Combine(Settings.ModConfigsPath, unique);
								name = unique;
							}

							/*if (
									   (identityVersion < ModIdentity.XmlModIdentityVersion1_0_1_2)
									&& (dllsBuild != null)
									&& (dllsBuild > ModIdentity.MAX_DLLS_BUILD_PRE_MI1_0_1_2)
								)
							{
								if
								(!(
									   (dllsBuild == ModIdentity.PRE_MI1_0_1_2_EXCLUDE_FROM_DLLS_BUILD_CUTOFF_LOCKED_DLLS_BUILD)
									&& ModIdentity.PRE_MI1_0_1_2_EXCLUDE_FROM_DLLS_BUILD_CUTOFF_UNIQUES.Any(x => x == unique)
								))
								{
									throw new UnsupportedDllsBuildException(dllsBuild);
								}
							}*/
							if (!HasValidDllsVersion__MI1_0_1_2__Hack(identityVersion, dllsBuild, unique))
                            {
								throw new UnsupportedDllsBuildException(dllsBuild);
							}

							var vanillaCompatAttr = compareDocument.Root.Attribute("verifiedVanillaCompatible");
							if (vanillaCompatAttr != null)
							{
								if (vanillaCompatAttr.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
									proceed = true;
							}



							var isExperimentalAttr = compareDocument.Root.Attribute("isExperimental");
							if (isExperimentalAttr != null)
							{
								if (bool.TryParse(isExperimentalAttr.Value, out bool isExperimental) && isExperimental)
								{
									if (!InstallingExperimentalMod(name))
										throw new UserRefusedConditionsException();
								}
							}

							var requiresGalaxyResetAttr = compareDocument.Root.Attribute("requiresGalaxyReset");
							if (requiresGalaxyResetAttr != null)
							{
								if (bool.TryParse(requiresGalaxyResetAttr.Value, out bool requiresGalaxyReset) && requiresGalaxyReset)
								{
									if (!InstallingRequiresGalaxyResetMod(name))
										throw new UserRefusedConditionsException();
								}
							}

							var causesSaveDataDependencyAttr = compareDocument.Root.Attribute("causesSaveDataDependency");
							if (causesSaveDataDependencyAttr != null)
							{
								if (bool.TryParse(causesSaveDataDependencyAttr.Value, out bool causesSaveDataDependency) && causesSaveDataDependency)
								{
									if (!InstallingSaveDataDependencyMod(name))
										throw new UserRefusedConditionsException();
								}
							}
						}
						else if (Settings.AllowVanillaIncompatibleMods)
							CreateModInfoXml(name, Settings.TempFolderPath, out document);

						if (proceed || Settings.AllowVanillaIncompatibleMods)
						{
							string[] modConfigDirs = Directory.EnumerateDirectories(Settings.ModConfigsPath).ToArray();
							foreach (string s in modConfigDirs)
							{
								string xmlPath = Path.Combine(s, MOD_INFO);
								if (File.Exists(xmlPath))
								{
									XDocument modDocument = null;
									using (FileStream stream = new FileStream(xmlPath, FileMode.OpenOrCreate, FileAccess.Read))
									{
										modDocument = XDocument.Load(stream);
									}
									string modUnique = Path.GetFileName(s);
									var modAttr = modDocument.Root.Attribute("unique");
									if (modAttr != null)
										modUnique = modAttr.Value;
									if (unique.ToLowerInvariant() == modUnique.ToLowerInvariant())
									{
										MessageDisplay.DebugShowMessageBox("Unique identifier matched: " + unique + ", " + modUnique);

										prevMod = ModsManager.GetManagedMod(Path.GetFileName(s).ToLowerInvariant());
										if (prevMod != null)
											prevModIndex = ModsManager.InstalledMods.IndexOf(prevMod);

										isUnique = false;
										prevConfigDirPath = s;
										break;
									}
								}
							}
						}
						if (File.Exists(compareUniquePath))
							File.Delete(compareUniquePath);
					}/*, TaskCreationOptions.LongRunning*/);
					validateModTask.Start();
					await validateModTask;

					if (proceed || Settings.AllowVanillaIncompatibleMods)
					{
						try
						{
							//List<string> oldFiles = new List<string>();
							//List<string> newFiles = new List<string>();
							//ModInstallation.DebugMessageBoxShow("Evaluating uniqueness");

							Task ensureOldFilesTask = new Task(() =>
							{

								if (!isUnique)
								{
									if ((prevConfigDirPath != null) && Directory.Exists(prevConfigDirPath))
										Directory.Move(prevConfigDirPath, Path.Combine(Settings.TempFolderPath, Path.GetFileName(prevConfigDirPath)));
									//File.Move(Path.Combine(prevConfigDirPath, "ModInfo.xml"), Path.Combine(prevConfigDirPath, "OldModInfo.xml"));*/


									/*foreach (string s in Directory.EnumerateFiles(prevConfigDirPath))
										oldFiles.Add(s);*/

									ModsManager.RemoveMod(prevMod);
								}
							});
							ensureOldFilesTask.Start();
							await ensureOldFilesTask;


							if (!Directory.Exists(dir))
								Directory.CreateDirectory(dir);

							bool hasXmlInZip = false;
							//ModInstallation.DebugMessageBoxShow("Mod is unique");
							Task extractXMLTask = new Task(() =>
							{
								if (zip.TryGetEntry(MOD_INFO, out ZipArchiveEntry entry))
								{
									hasXmlInZip = true;
									string xmlOutPath = Path.Combine(dir, MOD_INFO);
									entry.ExtractToFile(xmlOutPath, true);
									Permissions.GrantAccessFile(xmlOutPath);
								}
								else
								{
									string legacyPath = Path.Combine(dir, "UseLegacyDLLs");
									string modInfoPath = Path.Combine(dir, "ModInfo.xml");
									File.WriteAllText(legacyPath, string.Empty);
									document.Save(modInfoPath);
									Permissions.GrantAccessFile(legacyPath);
									Permissions.GrantAccessFile(modInfoPath);
								}
							});
							extractXMLTask.Start();
							await extractXMLTask;

							//ModInstallation.DebugMessageBoxShow("Generating InstalledMod");
							mod = new ManagedMod(name, true)
							{
								Progress = 0,
								IsProgressing = true
							};// ManagedMods.Instance.GetModConfiguration(name);
							  //ManagedMods.Instance.AddMod(mod);

							//ModInstallation.DebugMessageBoxShow("Adding InstalledMod");
							ModsManager.AddMod(mod);
							//ModInstallation.DebugMessageBoxShow("InstalledMod should be added");

							/*Task evaluateArchiveAndCountFilesTask = new Task(() =>
							{
								for (int i = 0; i < zip.Entries.Count; i++)
								{
									ZipEntry e = zip.Entries.ElementAt(i);

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
							await evaluateArchiveAndCountFilesTask;*/

							for (int i = 0; i < Directory.EnumerateDirectories(dir).Count(); i++)
							{
								string s = Directory.EnumerateDirectories(dir).ElementAt(i);
								if ((Directory.EnumerateFiles(s).Count() == 0) && (Directory.EnumerateDirectories(s).Count() == 0) && s.ToLowerInvariant().Replace(dir.ToLowerInvariant(), string.Empty).TrimStart('\\').StartsWith(name.ToLowerInvariant()))
									Directory.Delete(s);
							}

							//ModInstallation.DebugMessageBoxShow("Beginning file extraction");
							// We don't increase all the progress, because EnableMod() will be called
							double totalProgress = 50.0;

							Task extractFilesTask = new Task(() =>
							{
								var fileEntries = zip.Entries.Where(x => !x.IsDirectory());
								int fileCount = fileEntries.Count();
								if (hasXmlInZip)
									fileCount--;
								
								foreach (ZipArchiveEntry e in fileEntries)
								{
									string name = e.Name.ToLowerInvariant();
									bool isModInfo = name.EndsWith(MOD_INFO.ToLowerInvariant()) || name.Contains(MOD_INFO.ToLowerInvariant()) || (name == MOD_INFO.ToLowerInvariant());
									//MessageDisplay.ShowMessageBox(name + " isModInfo: " + isModInfo + ", " + totalProgress + " / " + fileCount);

									if ((!isModInfo) || (isModInfo && document == null))
									{
										string outPath = Path.Combine(dir, e.Name);
										e.ExtractToFile(outPath, true); //.Extract(dir, ExtractExistingFileAction.OverwriteSilently);
										Permissions.GrantAccessFile(outPath);



										ModsManager.RemoveMatchingManuallyInstalledFile(e.Name, ComponentGameDir.GalacticAdventures);
										mod.Progress += totalProgress / fileCount;
									}
								}
								/*int fileCount = zip.Entries.Count;
								if (hasXmlInZip) fileCount--;
								//TODO
								foreach (ZipEntry e in zip.Entries)
								{
									bool isModInfo = e.FileName.ToLowerInvariant().EndsWith("modinfo.xml");
									if (!isModInfo || (isModInfo && document == null))
									{
										e.Extract(dir, ExtractExistingFileAction.OverwriteSilently);
										Permissions.GrantAccessFile(Path.Combine(dir, e.FileName));
										//remove corresponding manually-installed file
										if (!isModInfo)
											mod.Progress += totalProgress / fileCount;
									}
								}*/
							});
							extractFilesTask.Start();
							await extractFilesTask;

							if (mod.HasConfigurator)
								await RegisterSporemodModWithInstallerAsync(name);
							else
								await mod.EnableMod();
							/*}
							else
							{
								if (Directory.Exists(dir))
									Directory.Delete(dir, true);

								throw new ModAlreadyInstalledException();
							}*/

							if (!isUnique)
							{
								Task deletOldModFolderTask = new Task(() =>
								{
									Directory.Delete(Path.Combine(Settings.TempFolderPath, Path.GetFileName(prevConfigDirPath)), true);
								});
								deletOldModFolderTask.Start();
								await deletOldModFolderTask;
							}
						}
						catch (Exception x)
						{
							if (Directory.Exists(dir))
								Directory.Delete(dir, true);

							if (Directory.Exists(prevConfigDirPath))
								Directory.Move(Path.Combine(Settings.TempFolderPath, Path.GetFileName(prevConfigDirPath)), prevConfigDirPath);

							if (mod != null)
								ModsManager.RemoveMod(mod);

							if (prevMod != null)
							{
								try
								{
									ModsManager.InsertMod(prevModIndex, prevMod);
								}
								catch
								{
									ModsManager.AddMod(prevMod);
								}
							}

							MessageDisplay.ShowMessageBox(x.ToString());
							throw x;
						}
					}
					else
					{
						if (Directory.Exists(dir))
							Directory.Delete(dir, true);
					}
				}

				if (isUnique && (proceed || Settings.AllowVanillaIncompatibleMods))
					_installableMods.Add(name);

				return null;
			}
			catch (Exception ex)
			{
				/*MessageDisplay.RaiseError(new ErrorEventArgs(ex), path);
				if (mod != null && ModsManager.InstalledMods.Contains(mod))
				{
					ModsManager.InstalledMods.Remove(mod);
				}
				ModsManager.InstalledMods.Add(new InstallError(path, ex));*/
				//MessageDisplay.ShowMessageBox(ex.ToString());
				INSTALL_FAILURES.Add(Path.GetFileName(path), ex);
				return ex;
			}
		}

		public static void CreateModInfoXml(string name, string dir, out XDocument document)
		{
			/*document = XDocument.Parse(@"<mod>
</mod>");
			document.Root.SetAttributeValue("unique", name);
			document.Root.SetAttributeValue("displayName", name);
			document.Root.SetAttributeValue("installerSystemVersion", ModIdentity.XmlModIdentityVersion1_1_0_0.ToString());
			document.Root.SetAttributeValue("copyAllFiles", true.ToString());
			document.Root.SetAttributeValue("canDisable", false.ToString());

			document.Save(Path.Combine(dir, "ModInfo.xml"));*/
			CreateModInfoXml(name, name, dir, out document);
		}

		public static void CreateModInfoXml(string unique, string displayName, string dir, out XDocument document)
		{
			document = XDocument.Parse(@"<mod>
</mod>");
			document.Root.SetAttributeValue("unique", unique);
			document.Root.SetAttributeValue("displayName", displayName);
			document.Root.SetAttributeValue("installerSystemVersion", ModIdentity.XmlModIdentityVersion1_1_0_0.ToString());
			document.Root.SetAttributeValue("copyAllFiles", true.ToString());
			document.Root.SetAttributeValue("canDisable", false.ToString());

			document.Save(Path.Combine(dir, "ModInfo.xml"));
		}

		public static async Task RegisterSporemodModWithInstallerAsync(string modName)
		{
			//DebugMessageBoxShow("Registering mod with installer");
			ManagedMod mod = ModsManager.GetManagedMod(modName);
			Debug.Assert(mod != null);
			if (mod.HasConfigurator)
			{
				bool isProgressing = mod.IsProgressing;
				/*foreach (ModComponent m in mod.Configurator.Components)
				{
					DebugMessageBoxShow("DisplayName: " + m.DisplayName + "\nUnique: " + m.Unique + "\nIsEnabled: " + m.IsEnabled);
				}*/

				//DebugMessageBoxShow("Component count: " + mod.Configurator.Components.Count + "\nXML Mod Identity Version: " + mod.XmlVersion);

				if (await ModsManager.Instance.ShowModConfigurator(mod))
				{
					if (!isProgressing)
						IS_RECONFIGURING_MODS = true;
					await mod.EnableMod();
				}
				else if (!isProgressing)
					mod.Configuration.Reload();
			}
			else
			{
				MessageDisplay.RaiseError(new ErrorEventArgs(new Exception("This mod does not have a configurator!")));
			}
		}

		public static bool TryGetEntry(this ZipArchive archive, string entryName, out ZipArchiveEntry entry)
		{
			entry = archive.Entries.FirstOrDefault(x => x.FullName == entryName);
			return entry != null;
		}

		public static bool IsDirectory(this ZipArchiveEntry entry)
		{
			string eName = entry.Name;
			string eFull = entry.FullName;
			bool backSlash = eFull.EndsWith(@"\");
			bool foreSlash = eFull.EndsWith(@"/");
			bool blankName = string.IsNullOrEmpty(eName);

			return (backSlash || foreSlash) && blankName;
		}


		static bool HasValidDllsVersion__MI1_0_1_2__Hack(Version identityVersion, Version dllsBuild, string unique)
		{
			Version installerSystemVersion = identityVersion;
			Version requiredDllsVersion = dllsBuild;
			Version OldLauncherDllsBuild = ModIdentity.MAX_DLLS_BUILD_PRE_MI1_0_1_2;

			if (requiredDllsVersion > OldLauncherDllsBuild)
			{
				// Only allow these mods unless they have the newer installer version, to ensure they don't get released into old Launcher versions
				if (installerSystemVersion < ModIdentity.XmlModIdentityVersion1_0_1_2)
				{
					// Some mods already existed with newer DLL version but same installerSystemVersion
					// We make exceptions for them here
					if (!((ModIdentity.PRE_MI1_0_1_2_EXCLUDE_FROM_DLLS_BUILD_CUTOFF_UNIQUES.Contains(unique)) &&
						  requiredDllsVersion.Major == 2 && requiredDllsVersion.Minor == 5 && requiredDllsVersion.Build == 179))
						return false;
				}
			}

			/*if (requiredDllsVersion != null &&
				requiredDllsVersion > CurrentDllsBuild)
			{
				return false;
			}*/


			return true;
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
		public ManagedMod Mod { get; set; } = null;
		public ModRegistrationEventArgs(ManagedMod mod)
		{
			Mod = mod;
			HasCustomInstaller = Mod.HasConfigurator;
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

	public class ModInstallationStatus
	{
		public List<string> Successes = new List<string>();

		public bool AnySucceeded
		{
			get => Successes.Count > 0;
		}

		public Dictionary<string, Exception> Failures = new Dictionary<string, Exception>();

		public bool AnyFailed
		{
			get => Failures.Keys.Count > 0;
		}

		/*public ModInstallationStatus(List<IInstalledMod> successes, Dictionary<IInstalledMod, object> failures)
		{
			Successes = successes;
			Failures = failures;
		}*/
	}

	public class UnsupportedXmlModIdentityVersionException : Exception
	{
		Version _badVersion = null;
		public Version BadVersion
		{
			get => _badVersion;
		}

		public UnsupportedXmlModIdentityVersionException(Version badVersion)
		{
			_badVersion = badVersion;
		}
	}

	public class UnsupportedDllsBuildException : Exception
	{
		Version _badVersion = null;
		public Version BadVersion
		{
			get => _badVersion;
		}

		public UnsupportedDllsBuildException(Version badVersion)
		{
			_badVersion = badVersion;
		}
	}

	public class MissingXmlModIdentityAttributeException : Exception
	{
		string _attribute = null;
		public string Attribute
		{
			get => _attribute;
		}

		public MissingXmlModIdentityAttributeException(string attribute)
		{
			_attribute = attribute;
		}
	}

	public class ModAlreadyInstalledException : Exception
	{ }

	public class UserRefusedConditionsException : Exception
	{ }
}
