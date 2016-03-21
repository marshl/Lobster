using System;
using System.Net;
using System.Windows;
using System.Windows.Forms;

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for DownloadUpdateWindow.xaml
    /// </summary>
    public partial class DownloadUpdateWindow : Window
    {
        private GitHubUpdater githubUpdater;

        public DownloadUpdateWindow(GitHubUpdater gitHubUpdater)
        {
            this.InitializeComponent();

            this.githubUpdater = gitHubUpdater;

            this.githubUpdater.DownloadClient.DownloadProgressChanged += this.WebClient_DownloadProgressChanged;
            this.githubUpdater.BeginUpdate();
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                this.DownloadProgressBar.Value = e.BytesReceived;
                this.DownloadProgressBar.Maximum = e.TotalBytesToReceive;

                this.DownloadProgressLabel.Content = $"Downloaded {LobsterModel.Utils.BytesToString(e.BytesReceived)} of {LobsterModel.Utils.BytesToString(e.TotalBytesToReceive)}";
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.githubUpdater.DownloadClient.CancelAsync();
            this.Close();
        }
    }
}
