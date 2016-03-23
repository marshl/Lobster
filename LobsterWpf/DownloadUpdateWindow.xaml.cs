//-----------------------------------------------------------------------
// <copyright file="DownloadUpdateWindow.xaml.cs" company="marshl">
// Copyright 2016, Liam Marshall, marshl.
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
//      At first they made
//      fair progress, but as they went on, their passage became slower and more
//      dangerous.
//
//      [ _The Lord of the Rings_, I/xi: "A Knife in the Dark"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System.ComponentModel;
    using System.Net;
    using System.Windows;
    using System.Windows.Forms;

    /// <summary>
    /// Interaction logic for DownloadUpdateWindow.xaml
    /// </summary>
    public partial class DownloadUpdateWindow : Window
    {
        /// <summary>
        /// The auto-update instance
        /// </summary>
        private GitHubUpdater githubUpdater;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadUpdateWindow"/> class.
        /// </summary>
        /// <param name="gitHubUpdater">The update instance to use.</param>
        public DownloadUpdateWindow(GitHubUpdater gitHubUpdater)
        {
            this.InitializeComponent();

            this.githubUpdater = gitHubUpdater;

            this.githubUpdater.DownloadClient.DownloadProgressChanged += this.WebClient_DownloadProgressChanged;
            this.githubUpdater.DownloadClient.DownloadFileCompleted += this.DownloadClient_DownloadFileCompleted;
            this.githubUpdater.BeginUpdate();
        }

        /// <summary>
        /// The event called when the file download is completed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The completed event arguments.</param>
        private void DownloadClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.Close();
            }
        }

        /// <summary>
        /// The event called when the progress of the down
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The download progress arguments.</param>
        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                this.DownloadProgressBar.Value = e.BytesReceived;
                this.DownloadProgressBar.Maximum = e.TotalBytesToReceive;

                this.DownloadProgressLabel.Content = $"Downloaded {LobsterModel.Utils.BytesToString(e.BytesReceived)} of {LobsterModel.Utils.BytesToString(e.TotalBytesToReceive)}";
            });
        }

        /// <summary>
        /// The event called when the Cancel button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.githubUpdater.DownloadClient.CancelAsync();
            this.Close();
        }
    }
}
