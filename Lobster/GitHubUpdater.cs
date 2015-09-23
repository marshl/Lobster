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
namespace Lobster
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// Used to connect with the GitHub API and request release information.
    /// </summary>
    public abstract class GitHubUpdater
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
            var restRequest = new RestRequest("/repos/" + user + "/" + repo + "/releases/latest", Method.GET);
            IRestResponse response = restClient.Execute(restRequest);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageLog.LogWarning("An error occured when attempting to query GitHub: Http Status Code "
                    + response.StatusCode);
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
            DateTime linkerDate = Common.RetrieveLinkerTimestamp();

            if (latestRelease > linkerDate)
            {
                MessageBox.Show("A newer version is available. Please download it from "
                    + "https://www.github.com/" + user + "/" + repo + "\n"
                    + "Your release: " + linkerDate.ToShortDateString() + "\n"
                    + "Latest release: " + latestRelease.ToShortDateString());
                return true;
            }

            return false;
        }

#pragma warning disable 649
        /// <summary>
        /// Used to capture release information from the GitHub API.
        /// </summary>
        private class GitHubRelease
        {
            /// <summary>
            /// The date the release was created.
            /// </summary>
            [JsonProperty("created_at")]
            public string DateCreated { get; set; }

            /// <summary>
            /// The date the release was published.
            /// </summary>
            [JsonProperty("published_at")]
            public string DatePublished { get; set; }

            /// <summary>
            /// The list of assets for this release.
            /// </summary>
            [JsonProperty("assets")]
            public List<GitHubAsset> Assets { get; set; }
        }

        /// <summary>
        /// A single asset object.
        /// </summary>
        private class GitHubAsset
        {
            /// <summary>
            /// The Url of the asset.
            /// </summary>
            [JsonProperty("url")]
            public string Url { get; set; }
        }
#pragma warning restore 649
    }
}
