using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Lobster
{
    class GitHubUpdater
    {
        public static bool RunUpdateCheck( string _user, string _repo )
        { 
            RestClient restClient = new RestClient( "https://api.github.com" );
            var restRequest = new RestRequest( "/repos/" + _user + "/" + _repo +"/releases/latest", Method.GET );
            IRestResponse response = restClient.Execute( restRequest );
            if ( response.StatusCode != HttpStatusCode.OK )
            {
                MessageLog.LogWarning( "An error occured when attempting to query GitHub: Http Status Code " + response.StatusCode );
                return false;
            }

            GitHubRelease release = JsonConvert.DeserializeObject<GitHubRelease>( response.Content );
            Regex regex = new Regex( "(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})" );
            Match match = regex.Match( release.published_at );
            if ( !match.Success )
            {
                MessageLog.LogWarning( "An error occurred when attempting to extract the publish date" );
                return false;
            }
            DateTime latestRelease = new DateTime( Int32.Parse( match.Groups["year"].Value ), Int32.Parse( match.Groups["month"].Value ), Int32.Parse( match.Groups["day"].Value ) );
            
            DateTime linkerDate = Common.RetrieveLinkerTimestamp();
            
            if ( latestRelease > linkerDate )
            {
                MessageBox.Show( "A newer version of Lobster is available. Please download it from https://www.github.com/marshl/lobster\n"
                    + "Your release: " + linkerDate.ToShortDateString() + "\n"
                    + "Latest release: " + latestRelease.ToShortDateString() );
            }
            return true;
        }

#pragma warning disable 649
        private class GitHubRelease
        {
            public string url;
            public string assets_url;
            public string created_at;
            public string published_at;

            public List<GitHubAsset> assets;
        }

        private class GitHubAsset
        {
            public string url;
        }
#pragma warning restore 649
    }
}
