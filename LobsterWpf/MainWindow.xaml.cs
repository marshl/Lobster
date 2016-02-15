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
namespace LobsterWpf
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Media;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Media;
    using LobsterModel;
    using Properties;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IModelEventListener
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
        /// The delegate for all file operation events.
        /// </summary>
        /// <param name="filename">The full path of the file.</param>
        /// <param name="success">Whether the operation was a success or not.</param>
        private delegate void FileOperationDelegate(string filename, bool success);

        /// <summary>
        /// The callback for when a automatic file update is completed.
        /// </summary>
        /// <param name="filename">The full name of the file that was updated.</param>
        /// <param name="updateSuccess">Whether the update was a success or not.</param>
        void IModelEventListener.OnAutoUpdateComplete(string filename, bool updateSuccess)
        {
            this.Dispatcher.Invoke(new FileOperationDelegate(this.DisplayUpdateNotification), filename, updateSuccess);
        }

        /// <summary>
        /// The callback for when the user needs to select a table to insert a file into.
        /// </summary>
        /// <param name="filename">The full path of the file </param>
        /// <param name="tables">The tables that the user can select from.</param>
        /// <param name="table">The table that the user selected.</param>
        /// <returns>Whether the user selected a table to use or not.</returns>
        bool IModelEventListener.PromptForTable(string filename, Table[] tables, ref Table table)
        {
            TableSelectorWindow tsw = new TableSelectorWindow(filename, tables);
            tsw.Owner = this;
            bool? result = tsw.ShowDialog();
            if (result ?? false)
            {
                table = tsw.SelectedTable;
            }

            return result ?? false;
        }

        /// <summary>
        /// The callback for when the user is needed to select a mime type for a file to be inserted as.
        /// </summary>
        /// <param name="filename">The full path of the file</param>
        /// <param name="mimeTypes">The mime types that the user can select from.</param>
        /// <param name="mimeType">The mime type that the user selected.</param>
        /// <returns>True if the user selected a mime type, otherwise false.</returns>
        bool IModelEventListener.PromptForMimeType(string filename, string[] mimeTypes, ref string mimeType)
        {
            MimeTypeSelectorWindow msw = new MimeTypeSelectorWindow(filename, mimeTypes);
            msw.Owner = this;
            bool? result = msw.ShowDialog();
            if (result ?? false)
            {
                mimeType = msw.SelectedMimeType;
            }

            return result ?? false;
        }

        /// <summary>
        /// The callback for when a file event is initially received.
        /// </summary>
        void IModelEventListener.OnEventProcessingStart()
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                this.connectionView.IsEnabled = false;
            });
        }

        /// <summary>
        /// The callback for when the last file event in the event queue is processed.
        /// </summary>
        void IModelEventListener.OnFileProcessingFinished()
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                this.connectionView.IsEnabled = true;

                this.RepopulateFileListView();
            });
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
                Assembly assembly = Assembly.GetExecutingAssembly();
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
            ConnectionListWindow window = new ConnectionListWindow(this);
            window.Owner = this;
            bool? result = window.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if ( this.connectionView != null )
                {
                    this.connectionView.Dispose();
                }

                this.ConnectionContainer.DataContext = this.connectionView = new ConnectionView(window.DatabaseConnection);
                this.RepopulateFileListView();
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
            Thread thread = new Thread(() => this.UpdateCheck());
            thread.Start();

            this.OpenConnectionDialog();
            this.notifyIcon.Visible = true;
        }

        /// <summary>
        /// PErforms an update check, and closes the program if an update is found.
        /// </summary>
        private void UpdateCheck()
        {
            bool result = GitHubUpdater.RunUpdateCheck("marshl", "lobster");
            if (result)
            {
                this.Close();
                return;
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
        private void ClobTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RepopulateFileListView();
        }

        /// <summary>
        /// Clears the file list and populates with the files of the currently selected clob directory.
        /// </summary>
        private void RepopulateFileListView()
        {
            if (this.clobTypeListBox.SelectedIndex == -1)
            {
                return;
            }

            if (this.connectionView == null)
            {
                return;
            }

            ClobDirectoryView clobDirView = (ClobDirectoryView)this.clobTypeListBox.SelectedItem;

            this.connectionView.PopulateFileTreeForClobDirectory(clobDirView.BaseClobDirectory);
            this.localFileTreeView.ItemsSource = this.connectionView.RootFile?.Children;
        }

        /// <summary>
        /// The event that is called when the show/hide readonly files checkbox is checked or unchecked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments</param>
        private void HideReadonlyCheckbox_Toggled(object sender, RoutedEventArgs e)
        {
            this.RepopulateFileListView();
        }

        /// <summary>
        /// The event that is called when the push file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void PushButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionView.SelectedFileNode != null)
            {
                string filename = this.connectionView.SelectedFileNode.FullName;

                FileInfo fi = new FileInfo(filename);
                if (fi.IsReadOnly)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show(
                        $"{this.connectionView.SelectedFileNode.Name} is locked. Are you sure you want to clob it?",
                        "File is Locked",
                        MessageBoxButton.OKCancel);

                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                }

                try
                {
                    Model.SendUpdateClobMessage(this.connectionView.Connection, filename);
                }
                catch (FileUpdateException)
                {
                    this.DisplayUpdateNotification(filename, false);
                    return;
                }

                this.DisplayUpdateNotification(filename, true);
                this.connectionView.SelectedFileNode.Refresh();
            }
        }

        /// <summary>
        /// The event that is called when the 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionView.SelectedFileNode == null)
            {
                return;
            }

            string filename = this.connectionView.SelectedFileNode.FullName;

            try
            {
                string downloadedFile = Model.SendDownloadClobDataToFileMessage(this.connectionView.Connection, filename);
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
            }
        }

        /// <summary>
        /// The event that is called when the explore button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ExploreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionView.SelectedFileNode != null)
            {
                Utils.OpenFileInExplorer(this.connectionView.SelectedFileNode.FullName);
            }
        }

        /// <summary>
        /// The event that is called when the insert file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionView.SelectedFileNode != null)
            {
                string filename = this.connectionView.SelectedFileNode.FullName;
                try
                {
                    DBClobFile databaseFile = null;
                    bool result = Model.SendInsertClobMessage(this.connectionView.Connection, filename, ref databaseFile);

                    if (result)
                    {
                        this.DisplayUpdateNotification(filename, true);
                        this.connectionView.SelectedFileNode.DatabaseFile = databaseFile;
                    }
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
        private void LocalFileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.connectionView.SelectedFileNode = (FileNodeView)e.NewValue;
        }

        /// <summary>
        /// Displays a notification window showing the status of a file update.
        /// </summary>
        /// <param name="filename">The full path of the file that was updated.</param>
        /// <param name="success">Whether the file was updated successfulyl or not.</param>
        private void DisplayUpdateNotification(string filename, bool success)
        {
            string message = "File update " + (success ? "succeeded" : "failed") + " for " + System.IO.Path.GetFileName(filename);
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
            FileBackup fileBackup = ((FrameworkElement)sender).DataContext as FileBackup;

            try
            {
                Model.UpdateClobWithExternalFile(this.connectionView.Connection, fileBackup.OriginalFilename, fileBackup.BackupFilename);
            }
            catch (FileUpdateException)
            {
                this.DisplayUpdateNotification(fileBackup.BackupFilename, false);
                return;
            }

            this.DisplayUpdateNotification(fileBackup.BackupFilename, true);
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
        private void RequeryDatabaseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<FileListRetrievalException> errorList = new List<FileListRetrievalException>();

            Model.GetDatabaseFileLists(this.connectionView.Connection, ref errorList);
            this.RepopulateFileListView();
        }

        /// <summary>
        /// The event for when either of the display mode radio buttons are clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnToggleViewModeRadioClicked(object sender, RoutedEventArgs e)
        {
            if (this.connectionView == null)
            {
                return;
            }

            this.connectionView.CurrentDisplayMode = this.LocalOnlyFilesRadio.IsChecked.Value ? ConnectionView.DisplayMode.LocalFiles : ConnectionView.DisplayMode.DatabaseFiles;
            this.RepopulateFileListView();
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
                MessageListWindow.Instance.BringIntoView();
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.notifyIcon.Dispose();
            if (MessageListWindow.Instance != null)
            {
                MessageListWindow.Instance.Close();
            }
        }
    }
}
