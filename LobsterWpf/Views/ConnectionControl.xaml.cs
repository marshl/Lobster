//-----------------------------------------------------------------------
// <copyright file="ConnectionControl.xaml.cs" company="marshl">
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
namespace LobsterWpf.Views
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using LobsterModel;
    using Properties;
    using ViewModels;

    /// <summary>
    /// Interaction logic for ConnectionControl.xaml
    /// </summary>
    public partial class ConnectionControl : System.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionControl"/> class.
        /// </summary>
        /// <param name="connectionView">The connection.</param>
        /// <param name="parentWindow">The parent <see cref="MainWindow"/> of this connection</param>
        public ConnectionControl(ConnectionView connectionView, MainWindow parentWindow)
        {
            this.InitializeComponent();

            this.ParentWindow = parentWindow;
            this.ConnectionView = connectionView;

            this.ConnectionView.BaseConnection.FileProcessingFinishedEvent += this.OnFileProcessingFinished;
            this.ConnectionView.BaseConnection.StartChangeProcessingEvent += this.OnEventProcessingStart;
            this.ConnectionView.BaseConnection.UpdateCompleteEvent += this.OnAutoUpdateComplete;
            this.ConnectionView.BaseConnection.DirectoryDescriptorChangeEvent += this.OnReloadDirectoryDescriptors;

            this.DataContext = this.ConnectionView;
            this.ReloadDirectoryDescriptors();
        }

        /// <summary>
        /// Gets the view of the current connection, if crrently connected.
        /// </summary>
        public ConnectionView ConnectionView { get; }

        /// <summary>
        /// Gets the parent window of this connection
        /// </summary>
        public MainWindow ParentWindow { get; }

        /// <summary>
        /// Gets or sets the <see cref="TabItem"/> that this connection control is part of
        /// </summary>
        public TabItem TabItem { get; set; }

        /// <summary>
        /// Clears the file list and populates with the files of the currently selected clob directory.
        /// </summary>
        /// <param name="fullRebuild">WHether a full rebuild of the file list needs to be performed.</param>
        public void RepopulateFileListView(bool fullRebuild)
        {
            this.ConnectionView.PopulateRootDirectory();
            if (this.ConnectionView.RootDirectoryView != null)
            {
                this.fileTree.ItemsSource = this.ConnectionView.RootDirectoryView.ChildNodes;
            }
        }

        /// <summary>
        /// Reloads the LobsterTypes for this connection
        /// </summary>
        public void ReloadDirectoryDescriptors()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.ConnectionView.SelectedNode = null;
                this.directoryWatcherListBox.SelectedIndex = -1;
                this.ConnectionView.ReloadDirectoryDescriptors();

                this.fileTree.ItemsSource = null;
            });
        }

        /// <summary>
        /// Disposes of this object.
        /// </summary>
        public void Dispose()
        {
            if (this.ConnectionView != null)
            {
                this.ConnectionView.Dispose();
            }
        }

        /// <summary>
        /// The handler of the event when the "Reload Types" menu button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ReloadDirectoryDescriptorsMenuItem_Click(object sender, EventArgs e)
        {
            this.ReloadDirectoryDescriptors();
        }

        /// <summary>
        /// The handler of the event when the Disconnect button is clicked
        /// </summary>
        /// <param name="sender">Teh sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(
                   $"Are you sure you want to disconnect from {this.ConnectionView.BaseConnection.Config.Name}?",
                   "Disconnect",
                   MessageBoxButton.OKCancel);

            if (result != MessageBoxResult.OK)
            {
                return;
            }

            this.ParentWindow.CloseConnection(this);
        }

        /// <summary>
        /// The event that is called when the a different directory descriptor is selected.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DirectoryWatcherListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var watcherView = (DirectoryWatcherView)this.directoryWatcherListBox.SelectedItem;
            this.ConnectionView.ChangeCurrentDirectoryWatcher(watcherView);
            if (this.ConnectionView.RootDirectoryView != null)
            {
                this.fileTree.ItemsSource = this.ConnectionView.RootDirectoryView.ChildNodes;
            }
        }

        /// <summary>
        /// The event that is called when the push file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void PushButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectionView.SelectedNode == null || this.ConnectionView.SelectedNode is WatchedDirectoryView)
            {
                return;
            }

            var fileView = (WatchedFileView)this.ConnectionView.SelectedNode;
            string filepath = this.ConnectionView.SelectedNode.FilePath;

            if (fileView.IsReadOnly)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    $"{Path.GetFileName(filepath)} is read-only. Are you sure you want to push it?",
                    "File is Read-Only",
                    MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

            try
            {
                this.ConnectionView.BaseConnection.UpdateDatabaseFile(this.ConnectionView.SelectedDirectoryWatcher.BaseWatcher, filepath);
            }
            catch (FileUpdateException ex)
            {
                MessageBox.Show($"An exception occurred when pushing the file: {ex}");
                this.DisplayUpdateNotification(filepath, false);
                return;
            }

            this.DisplayUpdateNotification(filepath, true);

            fileView.RefreshBackupList(this.ConnectionView);
        }

        /// <summary>
        /// The event that is called when the pull file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void PullButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectionView?.SelectedNode == null || !this.ConnectionView.SelectedNode.CanBeDownloaded)
            {
                return;
            }

            var watcherView = (DirectoryWatcherView)this.directoryWatcherListBox.SelectedItem;

            try
            {
                string tempPath = Utils.GetTempFilepath(this.ConnectionView.SelectedNode.FileName);
                this.ConnectionView.BaseConnection.DownloadDatabaseFile(watcherView.BaseWatcher, this.ConnectionView.SelectedNode.FilePath, tempPath);
                Process.Start(tempPath);
            }
            catch (FileDownloadException ex)
            {
                MessageBox.Show($"The file download failed: {ex}");
            }
        }

        /// <summary>
        /// The event that is called when the 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectionView?.SelectedNode == null || !this.ConnectionView.SelectedNode.CanBeCompared)
            {
                return;
            }

            string tempPath = null;

            try
            {
                var watcherView = (DirectoryWatcherView)this.directoryWatcherListBox.SelectedItem;

                tempPath = Utils.GetTempFilepath(this.ConnectionView.SelectedNode.FileName);
                this.ConnectionView.BaseConnection.DownloadDatabaseFile(watcherView.BaseWatcher, this.ConnectionView.SelectedNode.FilePath, tempPath);
            }
            catch (FileDownloadException ex)
            {
                MessageBox.Show($"The file download failed: {ex}");
            }

            try
            {
                string args = string.Format(
                    Settings.Default.DiffProgramArguments,
                    tempPath,
                    this.ConnectionView.SelectedNode.FilePath);

                Process.Start(Settings.Default.DiffProgramName, args);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The diff program \"{Settings.Default.DiffProgramName}\" couldn't be started: {ex}");
            }
        }

        /// <summary>
        /// The event that is called when the explore button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ExploreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectionView.SelectedNode != null)
            {
                Utils.OpenFileInExplorer(this.ConnectionView.SelectedNode.BaseNode.FilePath);
            }
        }

        /// <summary>
        /// The event that is called when the insert file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectionView?.SelectedNode == null)
            {
                return;
            }

            var watcherView = (DirectoryWatcherView)this.directoryWatcherListBox.SelectedItem;
            string filename = this.ConnectionView.SelectedNode.BaseNode.FilePath;
            try
            {
                this.ConnectionView.BaseConnection.InsertFile(watcherView.BaseWatcher, filename);
                this.DisplayUpdateNotification(filename, true);
                this.ConnectionView.SelectedNode.CheckFileSynchronisation(this.ConnectionView, watcherView);
            }
            catch (FileInsertException ex)
            {
                MessageBox.Show($"The file insert failed: {ex}");
                this.DisplayUpdateNotification(filename, false);
            }
        }

        /// <summary>
        /// The handler for when the user clicks on the Delete button
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectionView?.SelectedNode == null)
            {
                return;
            }

            var watcherView = (DirectoryWatcherView)this.directoryWatcherListBox.SelectedItem;
            var fileView = (WatchedFileView)this.ConnectionView.SelectedNode;

            try
            {
                this.ConnectionView.BaseConnection.DeleteDatabaseFile(watcherView.BaseWatcher, fileView.WatchedFile);
                this.ConnectionView.SelectedNode.CheckFileSynchronisation(this.ConnectionView, watcherView);
            }
            catch (FileDeleteException ex)
            {
                MessageBox.Show($"The file delete failed: {ex}");
            }
        }

        /// <summary>
        /// The event that is called when a different file is selected.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void FileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.ConnectionView.SelectedNode = (WatchedNodeView)e.NewValue;
            this.ConnectionView.SelectedNode?.RefreshBackupList(this.ConnectionView);
        }

        /// <summary>
        /// Displays a notification window showing the status of a file update.
        /// </summary>
        /// <param name="filename">The full path of the file that was updated.</param>
        /// <param name="success">Whether the file was updated successfulyl or not.</param>
        private void DisplayUpdateNotification(string filename, bool success)
        {
            string message = $"File update {(success ? "succeeded" : "failed")} for {Path.GetFileName(filename)}";

            if (success)
            {
                this.ParentWindow.PlaySuccessSound();
            }
            else
            {
                this.ParentWindow.PlayFailureSound();
            }

            NotificationWindow nw = new NotificationWindow(message, success);
            nw.Show();
        }
        
        /// <summary>
        /// The callback for when the last file event in the event queue is processed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments</param>
        private void OnFileProcessingFinished(object sender, FileProcessingFinishedEventArgs args)
        {
            this.Dispatcher.Invoke(() => 
            {
                this.ConnectionView.IsEnabled = true;
                this.RepopulateFileListView(args.FileTreeChange);
            });
        }

        /// <summary>
        /// The callback for when a file event is initially received.
        /// </summary>\
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments</param>
        private void OnEventProcessingStart(object sender, FileChangeEventArgs args)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.ConnectionView.IsEnabled = false;
            });
        }

        /// <summary>
        /// The callback for when a automatic file update is completed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments</param>
        private void OnAutoUpdateComplete(object sender, FileUpdateCompleteEventArgs args)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.DisplayUpdateNotification(args.Fullpath, args.Success);
            });

            if (args.ExceptionThrown != null)
            {
                MessageBox.Show($"An exception occurred when updating the file automatically ({args.Fullpath}): {args.ExceptionThrown}");
            }
        }

        /// <summary>
        /// The callback for when all lobsters need to be reloaded
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="args">The file system arguments (which file was updated)</param>
        private void OnReloadDirectoryDescriptors(object sender, FileSystemEventArgs args)
        {
            this.ReloadDirectoryDescriptors();
        }

        /// <summary>
        /// The event for when the open file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OpenBackupButton_Click(object sender, EventArgs e)
        {
            FileBackup fileBackup = ((FrameworkElement)sender).DataContext as FileBackup;
            Utils.OpenFileInExplorer(fileBackup.BackupFilename);
        }

        /// <summary>
        /// The event for thwne the toggle auto-updating button is clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void OnToggleAutoUpdateButton_Click(object sender, EventArgs e)
        {
            if (this.ConnectionView.CanAutoUpdate)
            {
                this.ConnectionView.IsAutoUpdateEnabled = !this.ConnectionView.IsAutoUpdateEnabled;
            }
        }
    }
}
