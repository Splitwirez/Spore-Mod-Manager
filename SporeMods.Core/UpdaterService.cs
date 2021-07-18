using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;
//using Ionic.Zip;
using Newtonsoft.Json;

namespace SporeMods.Core
{
	/// <summary>
	/// This class manages updates for the program and the core ModAPI DLLs.
	/// Updates are checked from repositories in GitHub.
	/// </summary>
	public static class UpdaterService
	{
		public static string UpdaterPath => Path.Combine(Settings.TempFolderPath, "smmUpdater.exe"); //"SporeModManagerSetup.exe");

		public static readonly string IgnoreUpdatesArg = "-ignoreUpdates";

		private static readonly string GITHUB_API_URL = "https://api.github.com";

		public class GithubReleaseAsset
		{
			public string name;
			public string browser_download_url;
		}

		public class GithubRelease
		{
			public string tag_name;
			public string html_url;
			public GithubReleaseAsset[] assets;
		}

		private static string GithubRequestGET(string uri)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			request.Method = "GET";
			request.Accept = "application/vnd.github.v3+json";
			request.UserAgent = "request";

			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		private static GithubRelease GetLatestGithubRelease(string repoUser, string repoName)
		{
			string data = GithubRequestGET(GITHUB_API_URL + "/repos/" + repoUser + "/" + repoName + "/releases/latest");

			var errors = new List<string>();
			var settings = new JsonSerializerSettings();
			settings.Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
			{
				errors.Add(args.ErrorContext.Error.Message);
				args.ErrorContext.Handled = true;
			};
			if (errors.Any())
			{
				Console.Error.WriteLine("Found " + errors.Count + " errors while parsing JSON for " + repoUser + "/" + repoName);
				foreach (var error in errors)
				{
					Console.Error.WriteLine(error);
				}
			}
			return JsonConvert.DeserializeObject<GithubRelease>(data);
		}

		/// <summary>
		/// Parses a version from Github, which is something like "v1.3.3" or "1.3.3"
		/// </summary>
		/// <returns></returns>
		private static Version ParseGithubVersion(string str)
		{
			if (str.StartsWith("v")) return new Version(str.Substring(1));
			else return new Version(str);
		}

		/// <summary>
		/// Returns whether there is an update available for the core ModAPI DLLs.
		/// </summary>
		/// <returns></returns>
		public static bool HasDllsUpdate(out GithubRelease release)
		{
			release = GetLatestGithubRelease("emd4600", "Spore-ModAPI");
			var updateVersion = ParseGithubVersion(release.tag_name);

			return updateVersion > Settings.CurrentDllsBuild;
		}

		/// <summary>
		/// Returns whether there is an update available for the program.
		/// </summary>
		/// <returns></returns>
		public static bool HasProgramUpdate(out GithubRelease release)
		{
			release = GetLatestGithubRelease("Splitwirez", "Spore-Mod-Manager");
			//release = GetLatestGithubRelease("emd4600", "sporemodder-fx");
			var updateVersion = ParseGithubVersion(release.tag_name);

			return updateVersion > Settings.ModManagerVersion;
		}

		static readonly string[] DLL_NAMES = { "SporeModAPI.disk.dll", "SporeModAPI.march2017.dll", "SporeModAPI.lib" };

		public class UpdateProgressEventArgs : EventArgs
		{
			public float Progress { get; set; }
		}

		/// <summary>
		/// How much of the progress is spent on download (the rest on copying the files)
		/// </summary>
		static readonly float DOWNLOAD_PROGRESS = 0.6f;

		/// <summary>
		/// Downloads the update from the given Github release and applies it, extracting all the necessary files.
		/// A progress handler can be passed to react when the operation progress (in the range [0, 100]) changes.
		/// Throws an InvalidOperationException if the update is not valid.
		/// </summary>
		/// <param name="release"></param>
		/// <param name="progressHandler"></param>
		public static void UpdateDlls(GithubRelease release, ProgressChangedEventHandler progressHandler)
		{
			var asset = Array.Find(release.assets, a => a.name.ToLower() == "sporemodapidlls.zip");
			if (asset == null)
			{
				throw new InvalidOperationException("Invalid update: no 'SporeModAPIdlls.zip' asset");
			}
			using (var client = new WebClient())
			{
				client.DownloadProgressChanged += (s, e) =>
				{
					var args = new ProgressChangedEventArgs((int)(e.ProgressPercentage * DOWNLOAD_PROGRESS), null);
					if (progressHandler != null) progressHandler.Invoke(null, args);
				};

				string zipName = Path.GetTempFileName();
				client.DownloadFile(asset.browser_download_url, zipName);

				using (var zip = ZipFile.Open(zipName, ZipArchiveMode.Read))
				{
					int filesExtracted = 0;
					foreach (string name in DLL_NAMES)
					{
						var entry = zip.Entries.FirstOrDefault(x => x.Name == name);
						if (entry == null)
						{
							throw new InvalidOperationException("Invalid update: missing " + name + " in zip file");
						}
						string outPath = Path.Combine(Settings.CoreLibsPath, name);
						entry.ExtractToFile(outPath, true);
						Permissions.GrantAccessFile(outPath);
						++filesExtracted;

						double progress = DOWNLOAD_PROGRESS + filesExtracted * (1.0f - DOWNLOAD_PROGRESS) / (float)(DLL_NAMES.Length);
						var args = new ProgressChangedEventArgs((int)(progress * 100.0), null);
						if (progressHandler != null) progressHandler.Invoke(null, args);
					}
				}

				File.Delete(zipName);
			}
		}

		public static event EventHandler UpdateDownloadCompleted;

		/// <summary>
		/// Downloads the update from the given Github release, and returns the path to the updater program. 
		/// A progress handler can be passed to react when the operation progress (in the range [0, 100]) changes.
		/// Throws an InvalidOperationException if the update is not valid.
		/// </summary>
		/// <param name="release"></param>
		/// <param name="progressHandler"></param>
		/// <returns></returns>
		public static bool UpdateProgram(GithubRelease release, ProgressChangedEventHandler progressHandler)
		{
			string targetFramework = Settings.TargetFramework.ToLowerInvariant();
			string fileName = Path.Combine(Settings.TempFolderPath, "smmUpdater.zip");

			/*var asset = Array.Find(release.assets, a => a.name.ToLowerInvariant() == "updater--" + targetFramework + ".zip");
			if (asset == null)
			{*/
				fileName = Path.Combine(Settings.TempFolderPath, "smmUpdater.exe");
				var asset = Array.Find(release.assets, a => a.name.ToLowerInvariant() == "sporemodmanagersetup.exe");
				if (asset == null)
					throw new InvalidOperationException("Invalid update: no 'SporeModManagerSetup.exe' or 'updater--" + targetFramework + ".zip' asset");
			//}
			using (var client = new WebClient())
			{
				client.DownloadProgressChanged += (s, e) =>
				{
					if (progressHandler != null) progressHandler.Invoke(null, e);
				};
				
				client.DownloadFile(asset.browser_download_url, fileName);
				client.DownloadFileCompleted += (sneder, args) =>
				{
					/*if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
						using (ZipArchive archive = ZipFile.Open(fileName, ZipArchiveMode.Read))
                        {
							archive.ExtractToDirectory(Settings.ManagerInstallLocationPath);
                        }

						foreach (string s in Directory.EnumerateFiles(Settings.ManagerInstallLocationPath))
                        {
							string updaterExe = Path.GetFileName(s);
							if (updaterExe.StartsWith("updater-", StringComparison.OrdinalIgnoreCase) && updaterExe.StartsWith(".exe", StringComparison.OrdinalIgnoreCase))
							{
								fileName = s;
                            }
                        }
                    }*/
					UpdateDownloadCompleted?.Invoke(fileName, null);
				};
			}
			return true;
		}
	}
}
