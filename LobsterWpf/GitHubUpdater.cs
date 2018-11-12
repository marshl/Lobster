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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text.RegularExpressions;
    using LobsterModel;
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
        private GitHubReleaseRoot release;

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
            // specify to use TLS 1.2 as default connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            RestClient restClient = new RestClient("https://api.github.com");
            var restRequest = new RestRequest($"/repos/{this.username}/{this.repository}/releases/latest", Method.GET);
            IRestResponse response = restClient.Execute(restRequest);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageLog.LogWarning($"An error occurred when attempting to query GitHub: Http Status Code {response.StatusCode}");
                return false;
            }

            var serialiser = new DataContractJsonSerializer(typeof(GitHubReleaseRoot));
            using (MemoryStream stream = new MemoryStream())
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(response.Content);
                sw.Flush();
                stream.Position = 0;
                this.release = (GitHubReleaseRoot)serialiser.ReadObject(stream);
            }

            Regex regex = new Regex("(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})");
            Match match = regex.Match(this.release.CreatedAt);
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

            this.releaseDownloadUrl = this.release.AssetList[0].BrowserDownloadUrl;
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

            FileStream stream = File.OpenRead(this.fileDownloadLocation);
            ZipArchive archive = new ZipArchive(stream);

            if (Directory.Exists(this.zipUnpackDirectory))
            {
                Directory.Delete(this.zipUnpackDirectory, true);
            }

            archive.ExtractToDirectory(this.zipUnpackDirectory);

            string dirName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
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

xcopy /s/e/y ""{source}\\Lobster"" ""{destination}""
start """" ""{Assembly.GetExecutingAssembly().Location}""
";

            using (StreamWriter stream = new StreamWriter(scriptFilePath))
            {
                stream.Write(scriptContents);
            }

            MessageLog.LogInfo($"Batch script {scriptFilePath} created with contents:\n{scriptContents}");
            return scriptFilePath;
        }

        /// <summary>
        /// The root object of the GiHub release Json object
        /// </summary>
        [DataContract]
        public class GitHubReleaseRoot
        {
            /// <summary>
            /// Gets or sets the name of the release.
            /// </summary>
            [DataMember(Name = "url")]
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets the Url of the aseets.
            /// </summary>
            [DataMember(Name = "assets_url")]
            public string AssetsUrl { get; set; }

            /// <summary>
            /// Gets or sets the Url where it was uploaded.
            /// </summary>
            [DataMember(Name = "upload_url")]
            public string UploadUrl { get; set; }

            /// <summary>
            /// Gets or sets the HTML URL of the release.
            /// </summary>
            [DataMember(Name = "html_url")]
            public string HtmlUrl { get; set; }

            /// <summary>
            /// Gets or sets the ID of the release.
            /// </summary>
            [DataMember(Name = "id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the tag for the release.
            /// </summary>
            [DataMember(Name = "tag_name")]
            public string TagName { get; set; }

            /// <summary>
            /// Gets or sets the commitish value that determines where the Git tag is created from. Can be any branch or commit SHA. Unused if the Git tag already exists. Default: the repository's default branch (usually master).
            /// </summary>
            [DataMember(Name = "target_commitish")]
            public string TargetCommitish { get; set; }

            /// <summary>
            /// Gets or sets the name of the release.
            /// </summary>
            [DataMember(Name = "name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the release is a draft or not.
            /// </summary>
            [DataMember(Name = "draft")]
            public bool Draft { get; set; }

            /// <summary>
            /// Gets or sets author of the release.
            /// </summary>
            [DataMember(Name = "author")]
            public GitHubUser Author { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this is a pre-release or not.
            /// </summary>
            [DataMember(Name = "prerelease")]
            public bool PreRelease { get; set; }

            /// <summary>
            /// Gets or sets the date at which this release was created.
            /// </summary>
            [DataMember(Name = "created_at")]
            public string CreatedAt { get; set; }

            /// <summary>
            /// Gets or sets the date at which this release was published. 
            /// </summary>
            [DataMember(Name = "published_at")]
            public string PublishedAt { get; set; }

            /// <summary>
            /// Gets or sets the list of assets for this release.
            /// </summary>
            [DataMember(Name = "assets")]
            public GitHubAsset[] AssetList { get; set; }

            /// <summary>
            /// Gets or sets the Url for the tarball.
            /// </summary>
            [DataMember(Name = "tarball_url")]
            public string TarballUrl { get; set; }

            /// <summary>
            /// Gets or sets the url for the zipball.
            /// </summary>
            [DataMember(Name = "zipball_url")]
            public string ZipballUrl { get; set; }

            /// <summary>
            /// Gets or sets the text describing the contents of the tag.
            /// </summary>
            [DataMember(Name = "body")]
            public string Body { get; set; }
        }

        /// <summary>
        /// A user of GitHub, used for the Authors of releases and assets.
        /// </summary>
        [DataContract]
        public class GitHubUser
        {
            /// <summary>
            /// Gets or sets the login of this user.
            /// </summary>
            [DataMember(Name = "login")]
            public string Login { get; set; }

            /// <summary>
            /// Gets or sets the unique ID of this user. 
            /// </summary>
            [DataMember(Name = "id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the URL for the avatar image of this user.
            /// </summary>
            [DataMember(Name = "avatar_url")]
            public string AvatarUrl { get; set; }

            /// <summary>
            /// Gets or sets the gravatar ID of the user.
            /// </summary>
            [DataMember(Name = "gravatar_id")]
            public string GravaterID { get; set; }

            /// <summary>
            /// Gets or sets the API URL of the user profile.
            /// </summary>
            [DataMember(Name = "url")]
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets the HTML of the user profile.
            /// </summary>
            [DataMember(Name = "html_url")]
            public string HtmlUrl { get; set; }

            /// <summary>
            /// Gets or sets the API URL of the followers of the user.
            /// </summary>
            [DataMember(Name = "followers_url")]
            public string FollowersUrl { get; set; }

            /// <summary>
            /// Gets or sets the API URL of the users that this user is following.
            /// </summary>
            [DataMember(Name = "following_url")]
            public string FollowingUrl { get; set; }

            /// <summary>
            /// Gets or sets the API URL of the Gists of this user.
            /// </summary>
            [DataMember(Name = "gists_url")]
            public string GistsUrl { get; set; }

            /// <summary>
            /// Gets or sets API URL of the repositories this user has starred.
            /// </summary>
            [DataMember(Name = "starred_url")]
            public string StarredUrl { get; set; }

            /// <summary>
            /// Gets or sets the API URL of the subscriptions of this user.
            /// </summary>
            [DataMember(Name = "subscriptions_url")]
            public string SubscriptionsUrl { get; set; }

            /// <summary>
            /// Gets or sets the API URL of the organisations this user is in.
            /// </summary>
            [DataMember(Name = "organizations_url")]
            public string OrganisationsUrl { get; set; }

            /// <summary>
            /// Gets or sets the API URL of the repositories of this user.
            /// </summary>
            [DataMember(Name = "repos_url")]
            public string RepositoriesUrl { get; set; }

            /// <summary>
            /// Gets or sets the API url of the events the user has created.
            /// </summary>
            [DataMember(Name = "events_url")]
            public string EventsUrl { get; set; }

            /// <summary>
            /// Gets or sets the API URL of the events the user has received.
            /// </summary>
            [DataMember(Name = "received_events_url")]
            public string ReceivedEventsUrl { get; set; }

            /// <summary>
            /// Gets or sets the type of user.
            /// </summary>
            [DataMember(Name = "type")]
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the user is an administrator.
            /// </summary>
            [DataMember(Name = "site_admin")]
            public bool SiteAdmin { get; set; }
        }

        /// <summary>
        /// An asset a user has uploaded in a <see cref="GitHubReleaseRoot"/>.
        /// </summary>
        [DataContract]
        public class GitHubAsset
        {
            /// <summary>
            /// Gets or sets the api url of this asset.
            /// </summary>
            [DataMember(Name = "url")]
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets the unique ID of this asset.
            /// </summary>
            [DataMember(Name = "id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of this asset.
            /// </summary>
            [DataMember(Name = "name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the an alternative short name for the asset.
            /// </summary>
            [DataMember(Name = "label")]
            public object Label { get; set; }

            /// <summary>
            /// Gets or sets the user that uploaded this asset.
            /// </summary>
            [DataMember(Name = "uploader")]
            public GitHubUser Uploader { get; set; }

            /// <summary>
            /// Gets or sets the content type of this asset.
            /// </summary>
            [DataMember(Name = "content_type")]
            public string ContentType { get; set; }

            /// <summary>
            /// Gets or sets the upload status of the asset.
            /// </summary>
            [DataMember(Name = "state")]
            public string State { get; set; }

            /// <summary>
            /// Gets or sets the size of this asset in bytes.
            /// </summary>
            [DataMember(Name = "size")]
            public int Size { get; set; }

            /// <summary>
            /// Gets or sets the number of times this asset has been downloaded.
            /// </summary>
            [DataMember(Name = "download_count")]
            public int DownloadCount { get; set; }

            /// <summary>
            /// Gets or sets the date this asset was created.
            /// </summary>
            [DataMember(Name = "created_at")]
            public string CreatedAt { get; set; }

            /// <summary>
            /// Gets or sets the date the asset was uploaded.
            /// </summary>
            [DataMember(Name = "updated_at")]
            public string UploadedAt { get; set; }

            /// <summary>
            /// Gets or sets a Url where this asset can be downloaded from. 
            /// </summary>
            [DataMember(Name = "browser_download_url")]
            public string BrowserDownloadUrl { get; set; }
        }
    }
}
