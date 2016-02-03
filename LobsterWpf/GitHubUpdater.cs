//-----------------------------------------------------------------------
// <copyright file="GitHubUpdater.cs" company="marshl">
// Copyright 2015, Liam Marshall, marshl.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------
//
//      Rumours of strange events had by now spread all over the field, but Frodo would
//      only say no doubt everything will be cleared up in the morning.
// 
//      [ _The Lord of the Rings_, I/i: "A Long-expected Party"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using Ionic.Zip;
    using LobsterModel;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// Used to connect with the GitHub API and request release information.
    /// </summary>
    public static class GitHubUpdater
    {
        /// <summary>
        /// Validates that the currently running program was built on the same day or after the latest GitHub release.
        /// </summary>
        /// <param name="user">There user that owns the repository.</param>
        /// <param name="repo">The repository name.</param>
        /// <returns>True if there is a newer release available.</returns>
        public static bool RunUpdateCheck(string user, string repo)
        {
            RestClient restClient = new RestClient("https://api.github.com");
            var restRequest = new RestRequest($"/repos/{user}/{repo}/releases/latest", Method.GET);
            IRestResponse response = restClient.Execute(restRequest);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageLog.LogWarning($"An error occured when attempting to query GitHub: Http Status Code {response.StatusCode}");
                return false;
            }

            GitHubRelease release = JsonConvert.DeserializeObject<GitHubRelease>(response.Content);
            Regex regex = new Regex("(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})");
            Match match = regex.Match(release.DatePublished);
            if (!match.Success)
            {
                MessageLog.LogWarning("An error occurred when attempting to extract the publish date");
                return false;
            }

            // Get the day that the release was published.
            // Only day precision is required, as the release will always be published at least 
            // some time after the link time.
            int year = int.Parse(match.Groups["year"].Value);
            int month = int.Parse(match.Groups["month"].Value);
            int day = int.Parse(match.Groups["day"].Value);
            DateTime latestRelease = new DateTime(year, month, day);
            DateTime linkerDate = Utils.RetrieveLinkerTimestamp();

            MessageLog.LogInfo($"Release comparison: local={linkerDate} github={latestRelease}");

            if (latestRelease > linkerDate)
            {
                DialogResult result = MessageBox.Show("A newer version is available. Would you like to update now?", "Update Available", MessageBoxButtons.OKCancel);

                if (result != DialogResult.OK)
                {
                    return false;
                }

                MessageLog.LogInfo("Proceeding with update.");

                string downloadUrl = release.Assets[0].BrowserDownloadUrl;
                string sourceFile = $"{Path.GetTempPath()}\\{Path.GetFileName(downloadUrl)}";
                string destinationPath = $"{Path.GetTempPath()}\\{Path.GetFileNameWithoutExtension(downloadUrl)}";

                using (WebClient client = new WebClient())
                {
                    MessageLog.LogInfo($"Downloading file from {downloadUrl} to {sourceFile}");
                    client.DownloadFile(downloadUrl, sourceFile);
                }

                MessageLog.LogInfo($"Extracting file from {sourceFile} to {destinationPath}");
                ZipFile zipFile = ZipFile.Read(sourceFile);
                zipFile.ExtractAll(destinationPath, ExtractExistingFileAction.OverwriteSilently);

                string dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                // Go up one directory
                dirName = new DirectoryInfo(dirName).Parent.FullName;
                string script = CreateAutoUpdateScript(destinationPath, dirName);

                MessageLog.LogInfo($"Starting batch script {script}");
                Process.Start(script);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a .bat script that, when executed, will copy the contents of the update over the top of the current installation.
        /// </summary>
        /// <param name="source">Where the program is currently located.</param>
        /// <param name="destination">The the new copy of the program is located.</param>
        /// <returns>The file location of the .bat script.</returns>
        public static string CreateAutoUpdateScript(string source, string destination)
        {
            MessageLog.LogInfo("Creating batch script.");
            string scriptFilePath = $"{Path.GetTempPath()}\\lobster_update.bat";
            int processID = Process.GetCurrentProcess().Id;
            string scriptContents = $@"
:loop
tasklist | find "" {processID} "" >nul
if not errorlevel 1 (
    timeout /t 1 >nul
    goto :loop
)

xcopy /s/e/y ""{source}"" ""{destination}""
call ""{destination}\Lobster.exe""
";

            using (StreamWriter stream = new StreamWriter(scriptFilePath))
            {
                stream.Write(scriptContents);
            }

            MessageLog.LogInfo($"Batch script {scriptFilePath} created with contents:\n{scriptContents}");
            return scriptFilePath;
        }

        /// <summary>
        /// Used to capture release information from the GitHub API.
        /// </summary>
        public class GitHubRelease
        {
            /// <summary>
            /// Gets or sets the date the release was created.
            /// </summary>
            [JsonProperty("created_at")]
            public string DateCreated { get; set; }

            /// <summary>
            /// Gets or sets the date the release was published.
            /// </summary>
            [JsonProperty("published_at")]
            public string DatePublished { get; set; }

            /// <summary>
            /// Gets or sets the list of assets for this release.
            /// </summary>
            [JsonProperty("assets")]
            public List<GitHubAsset> Assets { get; set; }
        }

        /// <summary>
        /// A single asset object.
        /// </summary>
        public class GitHubAsset
        {
            /// <summary>
            /// Gets or sets the Url of the asset.
            /// </summary>
            [JsonProperty("url")]
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets the Url that the asset can be downloaded from.
            /// </summary>
            [JsonProperty("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }
        }
    }
}
