//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="marshl">
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
//      'I think that in
//      the end, if all else is conquered, Bombadil will fall, Last as he was First; and
//      then Night will come.'
//          -- Glorfindel
//
//      [ _The Lord of the Rings_, II/iii: "The Ring Goes South"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Media;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using LobsterModel;
    using ViewModels;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable
    {
        /// <summary>
        /// The view of the current connection, if crrently connected.
        /// </summary>
        private ConnectionView connectionView;

        /// <summary>
        /// The sound played when an automatic update is successful.
        /// </summary>
        private SoundPlayer successSound;

        /// <summary>
        /// The sound played when an automatic update fails.
        /// </summary>
        private SoundPlayer failureSound;

        /// <summary>
        /// The system tray icon that can be used to minimise and maximise the window.
        /// </summary>
        private NotifyIcon notifyIcon;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            this.successSound = new SoundPlayer(Path.Combine(Environment.CurrentDirectory, @"Resources\Audio\success.wav"));
            this.failureSound = new SoundPlayer(Path.Combine(Environment.CurrentDirectory, @"Resources\Audio\failure.wav"));
        }

        /// <summary>
        /// Disposes of this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The method that is called when the window is initialised.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                Icon icon = new Icon("Resources/Images/cartoon-lobster.ico");

                this.notifyIcon = new NotifyIcon();
                this.notifyIcon.Click += new EventHandler(this.NotifyIcon_Click);
                this.notifyIcon.Icon = icon;
            }
            catch (IOException)
            {
                // Swallow icon not found exception
            }
        }

        /// <summary>
        /// Opens a connection dialog window.
        /// </summary>
        private void OpenConnectionDialog()
        {
            ConnectionListWindow window = new ConnectionListWindow();
            window.Owner = this;
            bool? result = window.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (this.connectionView != null)
                {
                    this.connectionView.Dispose();
                }

                this.ConnectionContainer.DataContext = this.connectionView = new ConnectionView(window.DatabaseConnection);
                this.RepopulateFileListView(true);

                this.connectionView.BaseConnection.FileProcessingFinishedEvent += this.OnFileProcessingFinished;
                this.connectionView.BaseConnection.StartChangeProcessingEvent += this.OnEventProcessingStart;
                this.connectionView.BaseConnection.UpdateCompleteEvent += this.OnAutoUpdateComplete;
                this.connectionView.BaseConnection.ClobTypeChangedEvent += this.ReloadLobsterTypesMenuItem_Click;
            }
        }

        /// <summary>
        /// The event for when the Connections meu item is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ConnectionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.OpenConnectionDialog();
        }

        /// <summary>
        /// The event for when the window is loaded.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.OpenConnectionDialog();
            this.notifyIcon.Visible = true;

#if !DEBUG
            this.UpdateCheck();
#endif
        }

        /// <summary>
        /// PErforms an update check, and closes the program if an update is found.
        /// </summary>
        private void UpdateCheck()
        {
            GitHubUpdater updater = new GitHubUpdater("marshl", "lobster");
            Thread thread = new Thread(() =>
            {
                bool updateExists = updater.RunUpdateCheck();
                if (!updateExists)
                {
                    return;
                }

                DialogResult result = System.Windows.Forms.MessageBox.Show("A newer version is available. Would you like to update now?", "Update Available", MessageBoxButtons.OKCancel);
                if (result != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                updater.PrepareUpdate();

                // The DownloadUpdateWindow begins the update once opened
                this.Dispatcher.Invoke((Action)delegate
                {
                    DownloadUpdateWindow window = new DownloadUpdateWindow(updater);
                    window.Owner = this;
                    updater.DownloadClient.DownloadFileCompleted += DownloadClient_DownloadFileCompleted;
                    window.ShowDialog();
                });
            });

            thread.Start();
        }

        /// <summary>
        /// The event called when the update download has finished.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The completed event arguments.</param>
        private void DownloadClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // If there was an error, print it out
            if (e.Error != null)
            {
                System.Windows.MessageBox.Show(e.Error.ToString(), "Update Error");
                return;
            }

            // If the event wasn't cancelled, then close the program
            if (!e.Cancelled)
            {
                this.Dispatcher.Invoke((Action)delegate
                {
                    this.Close();
                });
            }
        }

        /// <summary>
        /// The event that is called when the exit menu button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// The event that is called when the a different clob type is selected.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DirectoryWatcherListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RepopulateFileListView(true);
        }

        /// <summary>
        /// Clears the file list and populates with the files of the currently selected clob directory.
        /// </summary>
        /// <param name="fullRebuild">WHether a full rebuild of the file list needs to be performed.</param>
        private void RepopulateFileListView(bool fullRebuild)
        {
            if (this.directoryWatcherListBox.SelectedIndex == -1)
            {
                this.connectionView.ChangeCurrentDirectoryWatcher(null);
                //this.fileTree.ItemsSource = null;
                return;
            }

            if (this.connectionView == null)
            {
                return;
            }

            DirectoryWatcherView dirWatcherView = (DirectoryWatcherView)this.directoryWatcherListBox.SelectedItem;

            this.connectionView.ChangeCurrentDirectoryWatcher(dirWatcherView);
            if (this.connectionView.RootDirectoryView != null)
            {
                this.fileTree.ItemsSource = this.connectionView.RootDirectoryView.ChildNodes;
            }
        }

        /// <summary>
        /// The event that is called when the show/hide readonly files checkbox is checked or unchecked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments</param>
        private void HideReadonlyCheckbox_Toggled(object sender, RoutedEventArgs e)
        {
            this.RepopulateFileListView(true);
        }

        /// <summary>
        /// The event that is called when the push file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void PushButton_Click(object sender, RoutedEventArgs e)
        {
            /*if (this.connectionView.SelectedFileNode == null)
            {
                return;
            }

            var clobDirView = (ClobDirectoryView)this.clobTypeListBox.SelectedItem;
            string filename = this.connectionView.SelectedFileNode.FilePath;

            FileInfo fi = new FileInfo(filename);
            if (fi.IsReadOnly)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    $"{this.connectionView.SelectedFileNode.DisplayName} is locked. Are you sure you want to clob it?",
                    "File is Locked",
                    MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

            try
            {
                this.connectionView.Connection.SendUpdateClobMessage(clobDirView.BaseClobDirectory, filename);
            }
            catch (FileUpdateException)
            {
                this.DisplayUpdateNotification(filename, false);
                return;
            }

            this.DisplayUpdateNotification(filename, true);
            this.connectionView.SelectedFileNode.RefreshBackupList();*/
        }

        /// <summary>
        /// The event that is called when the pull file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void PullButton_Click(object sender, RoutedEventArgs e)
        {
            /*if (this.connectionView?.SelectedFileNode?.DatabaseFile == null)
            {
                return;
            }

            try
            {
                string resultPath = this.connectionView.Connection.SendDownloadClobDataToFileMessage(this.connectionView.SelectedFileNode.DatabaseFile);
                Process.Start(resultPath);
            }
            catch (FileDownloadException ex)
            {
                System.Windows.MessageBox.Show($"The file download failed: {ex}");
            }*/
        }

        /// <summary>
        /// The event that is called when the 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            /*if (this.connectionView?.SelectedFileNode?.DatabaseFile == null)
            {
                return;
            }

            try
            {
                string filename = this.connectionView.SelectedFileNode.FilePath;
                string downloadedFile = this.connectionView.Connection.SendDownloadClobDataToFileMessage(this.connectionView.SelectedFileNode.DatabaseFile);
                string args = string.Format(
                    Settings.Default.DiffProgramArguments,
                    downloadedFile,
                    filename);

                Process.Start(Settings.Default.DiffProgramName, args);
            }
            catch (FileDownloadException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }*/
        }

        /// <summary>
        /// The event that is called when the explore button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ExploreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionView.SelectedNode != null)
            {
                Utils.OpenFileInExplorer(this.connectionView.SelectedNode.BaseNode.FilePath);
            }
        }

        /// <summary>
        /// The event that is called when the insert file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionView.SelectedNode != null)
            {
                var watcherView = (DirectoryWatcherView)this.directoryWatcherListBox.SelectedItem;
                string filename = this.connectionView.SelectedNode.BaseNode.FilePath;
                try
                {
                    this.connectionView.BaseConnection.InsertFile(watcherView.Watcher, filename);

                    this.DisplayUpdateNotification(filename, true);
                }
                catch (FileInsertException ex)
                {
                    System.Windows.MessageBox.Show($"The file insert failed: {ex}");
                    this.DisplayUpdateNotification(filename, false);
                }
            }
        }

        /// <summary>
        /// The event that is called when a different file is selected.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void FileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.connectionView.SelectedNode = (WatchedNodeView)e.NewValue;
        }

        /// <summary>
        /// Displays a notification window showing the status of a file update.
        /// </summary>
        /// <param name="filename">The full path of the file that was updated.</param>
        /// <param name="success">Whether the file was updated successfulyl or not.</param>
        private void DisplayUpdateNotification(string filename, bool success)
        {
            string message = $"File update {(success ? "succeeded" : "failed")} for {Path.GetFileName(filename)}";
            NotificationWindow nw = new NotificationWindow(message, success);
            nw.Show();

            try
            {
                (success ? this.successSound : this.failureSound).Play();
            }
            catch (FileNotFoundException ex)
            {
                MessageLog.LogError($"An error occurred when attempting to play a sound: {ex}");
            }
        }

        /// <summary>
        /// The event for when the push file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void PushBackupButton_Click(object sender, EventArgs e)
        {
            /*FileBackup fileBackup = ((FrameworkElement)sender).DataContext as FileBackup;
            var clobDirView = (ClobDirectoryView)this.clobTypeListBox.SelectedItem;
            try
            {
                this.connectionView.Connection.UpdateClobWithExternalFile(clobDirView.BaseClobDirectory, fileBackup.OriginalFilename, fileBackup.BackupFilename);
            }
            catch (FileUpdateException)
            {
                this.DisplayUpdateNotification(fileBackup.BackupFilename, false);
                return;
            }

            this.DisplayUpdateNotification(fileBackup.BackupFilename, true);*/
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
        /// The event for when the requery database menu item is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        [Obsolete]
        private void RequeryDatabaseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<FileListRetrievalException> errorList = new List<FileListRetrievalException>();

            this.connectionView.BaseConnection.GetDatabaseFileLists(ref errorList);
            this.RepopulateFileListView(true);
        }

        /// <summary>
        /// The event for when either of the display mode radio buttons are clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnToggleViewModeRadioClicked(object sender, RoutedEventArgs e)
        {
            /*if (this.connectionView == null)
            {
                return;
            }

            this.connectionView.CurrentDisplayMode = this.LocalOnlyFilesRadio.IsChecked.Value ? ConnectionView.DisplayMode.LocalFiles : ConnectionView.DisplayMode.DatabaseFiles;
            this.RepopulateFileListView(true);*/
        }

        /// <summary>
        /// The event called when the settings button is clicked, opening a new settings window.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new SettingsWindow();
            window.Owner = this;
            bool? result = window.ShowDialog();
        }

        /// <summary>
        /// The event called when the settings button is clicked, opening a new settings window.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void MessagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageListWindow.Instance != null)
            {
                MessageListWindow.Instance.Activate();
            }
            else
            {
                MessageListWindow window = new MessageListWindow();
                window.Show();
            }
        }

        /// <summary>
        /// The event that is called when the tray icon is clicked, toggling the display of the window.
        /// </summary>
        /// <param name="sender">Tjhe sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                this.WindowState = WindowState.Minimized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        /// <summary>
        /// The event that is called when the state of the window is changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Th event arguments.</param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.notifyIcon != null)
            {
                this.ShowInTaskbar = this.WindowState != WindowState.Minimized;
            }
        }

        /// <summary>
        /// The event that is raised when the window is closed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageListWindow.Instance != null)
            {
                MessageListWindow.Instance.Close();
            }

            this.Dispose();
        }

        /// <summary>
        /// Disposes of this object.
        /// </summary>
        /// <param name="disposing">Whether to dispose of managed resources or not.</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (this.connectionView != null)
            {
                this.connectionView.Dispose();
                this.connectionView = null;
            }

            if (this.failureSound != null)
            {
                this.failureSound.Dispose();
                this.failureSound = null;
            }

            if (this.successSound != null)
            {
                this.successSound.Dispose();
                this.successSound = null;
            }

            if (this.notifyIcon != null)
            {
                this.notifyIcon.Dispose();
                this.notifyIcon = null;
            }
        }

        /// <summary>
        /// The callback for when a automatic file update is completed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments</param>
        private void OnAutoUpdateComplete(object sender, FileUpdateCompleteEventArgs args)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                this.DisplayUpdateNotification(args.Fullpath, args.Success);
            });
        }

        /// <summary>
        /// The callback for when the user needs to select a table to insert a file into.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments</param>
        [Obsolete]
        private void PromptForTable(object sender, TableRequestEventArgs args)
        {
            TableSelectorWindow tsw = new TableSelectorWindow(args.FullPath, args.Tables);
            tsw.Owner = this;
            bool? result = tsw.ShowDialog();
            if (result ?? false)
            {
                args.SelectedTable = tsw.SelectedTable;
            }
        }

        /// <summary>
        /// The callback for when the user is needed to select a mime type for a file to be inserted as.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments</param>
        [Obsolete]
        private void PromptForMimeType(object sender, MimeTypeRequestEventArgs args)
        {
            MimeTypeSelectorWindow msw = new MimeTypeSelectorWindow(args.FullPath, args.MimeTypes);
            msw.Owner = this;
            bool? result = msw.ShowDialog();
            if (result ?? false)
            {
                args.SelectedMimeType = msw.SelectedMimeType;
            }
        }

        /// <summary>
        /// The callback for when a file event is initially received.
        /// </summary>\
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments</param>
        private void OnEventProcessingStart(object sender, FileChangeEventArgs args)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                this.connectionView.IsEnabled = false;
            });
        }

        /// <summary>
        /// The callback for when the last file event in the event queue is processed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments</param>
        private void OnFileProcessingFinished(object sender, FileProcessingFinishedEventArgs args)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                this.connectionView.IsEnabled = true;
                this.RepopulateFileListView(args.FileTreeChange);
            });
        }

        /// <summary>
        /// The handler for when the Reload Lobster Types menu item is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ReloadLobsterTypesMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                this.connectionView.ReloadClobTypes();
                this.connectionView.SelectedNode = null;
                this.directoryWatcherListBox.SelectedIndex = -1;
                this.RepopulateFileListView(true);
            });
        }
    }
}
