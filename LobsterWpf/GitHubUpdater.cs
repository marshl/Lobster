﻿//-----------------------------------------------------------------------
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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Ionic.Zip;
    using LobsterModel;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// Used to connect with the GitHub API and request release information.
    /// </summary>
    public class GitHubUpdater
    {
        /// <summary>
        /// The name of the GitHUb user to search for.
        /// </summary>
        private string username;

        /// <summary>
        /// The repository of the user to query
        /// </summary>
        private string repository;

        /// <summary>
        /// The release information, if found.
        /// </summary>
        private GitHubRelease release;

        /// <summary>
        /// The URL the release zip can be downloaded from
        /// </summary>
        private string releaseDownloadUrl;

        /// <summary>
        /// The file to download the zip into.
        /// </summary>
        private string fileDownloadLocation;

        /// <summary>
        /// The location the zip file is exracted to (before copying it over the top of the current Lobster install)
        /// </summary>
        private string zipUnpackDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubUpdater"/> class.       
        /// </summary>
        /// <param name="user">There user that owns the repository.</param>
        /// <param name="repo">The repository name.</param>
        public GitHubUpdater(string user, string repo)
        {
            this.username = user;
            this.repository = repo;
        }

        /// <summary>
        /// Gets the client the release file will be downloaded with.
        /// </summary>
        public WebClient DownloadClient { get; private set; }

        /// <summary>
        /// Validates that the currently running program was built on the same day or after the latest GitHub release.
        /// </summary>
        /// <returns>True if there is a newer release available.</returns>
        public bool RunUpdateCheck()
        {
            RestClient restClient = new RestClient("https://api.github.com");
            var restRequest = new RestRequest($"/repos/{this.username}/{this.repository}/releases/latest", Method.GET);
            IRestResponse response = restClient.Execute(restRequest);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageLog.LogWarning($"An error occured when attempting to query GitHub: Http Status Code {response.StatusCode}");
                return false;
            }

            this.release = JsonConvert.DeserializeObject<GitHubRelease>(response.Content);
            Regex regex = new Regex("(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})");
            Match match = regex.Match(this.release.DatePublished);
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

            if (MessageLog.Instance == null)
            {
                return false;
            }

            MessageLog.LogInfo($"Release comparison: local={linkerDate} github={latestRelease}");
            return latestRelease > linkerDate;
        }

        /// <summary>
        /// Prepares the web client for downloading the release zip;
        /// </summary>
        public void PrepareUpdate()
        {
            MessageLog.LogInfo("Proceeding with update.");

            this.releaseDownloadUrl = this.release.Assets[0].BrowserDownloadUrl;
            this.fileDownloadLocation = $"{Path.GetTempPath()}\\{Path.GetFileName(this.releaseDownloadUrl)}";
            this.zipUnpackDirectory = $"{Path.GetTempPath()}\\{Path.GetFileNameWithoutExtension(this.releaseDownloadUrl)}";

            MessageLog.LogInfo($"Downloading file from {this.releaseDownloadUrl} to {this.fileDownloadLocation}");

            this.DownloadClient = new WebClient();
            this.DownloadClient.DownloadFileCompleted += this.Client_DownloadFileCompleted;
        }

        /// <summary>
        /// Begins the download of the release zip.
        /// </summary>
        public void BeginUpdate()
        {
            this.DownloadClient.DownloadFileAsync(new Uri(this.releaseDownloadUrl), this.fileDownloadLocation);
        }

        /// <summary>
        /// The event called when the download has completed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The completed event arguments.</param>
        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }

            if (e.Error != null)
            {
                return;
            }

            MessageLog.LogInfo($"Extracting file from {this.fileDownloadLocation} to {this.zipUnpackDirectory}");
            ZipFile zipFile = ZipFile.Read(this.fileDownloadLocation);
            zipFile.ExtractAll(this.zipUnpackDirectory, ExtractExistingFileAction.OverwriteSilently);

            string dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Go up one directory
            dirName = new DirectoryInfo(dirName).Parent.FullName;
            string script = this.CreateAutoUpdateScript(this.zipUnpackDirectory, dirName);

            MessageLog.LogInfo($"Starting batch script {script}");
            Process.Start(script);
        }

        /// <summary>
        /// Creates a .bat script that, when executed, will copy the contents of the update over the top of the current installation.
        /// The script waits for Lobster to close before executing.
        /// </summary>
        /// <param name="source">Where the program is currently located.</param>
        /// <param name="destination">The the new copy of the program is located.</param>
        /// <returns>The file location of the .bat script.</returns>
        private string CreateAutoUpdateScript(string source, string destination)
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
start ""{Assembly.GetExecutingAssembly().Location}""
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
