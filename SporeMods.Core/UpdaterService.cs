using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace SporeMods.Core
{
    /// <summary>
    /// This class manages updates for the program and the core ModAPI DLLs.
    /// Updates are checked from repositories in GitHub.
    /// </summary>
    public static class UpdaterService
    {
        private static readonly string GITHUB_API_URL = "https://api.github.com";

        private class GithubReleaseAsset
        {
            public string name;
            public string browser_download_url;
        }

        private class GithubRelease
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
        public static bool HasDllsUpdate()
        {
            var release = GetLatestGithubRelease("emd4600", "spore-modapi");
            var updateVersion = ParseGithubVersion(release.tag_name);

            return updateVersion > Settings.CurrentDllsBuild;
        }

        public static bool HasProgramUpdate()
        {
            var release = GetLatestGithubRelease("Splitwirez", "Spore-Mod-Manager");
            var updateVersion = ParseGithubVersion(release.tag_name);

            return updateVersion > Settings.ModManagerVersion;
        }
    }
}
