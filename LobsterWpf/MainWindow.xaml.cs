//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="marshl">
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
namespace LobsterWpf
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Media;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
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
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// The delegate for all file operation events.
        /// </summary>
        /// <param name="filename">The full path of the file.</param>
        /// <param name="success">Whether the operation was a success or not.</param>
        private delegate void FileOperationDelegate(string filename, bool success);

        /// <summary>
        /// Gets or sets the primary model 
        /// </summary>
        public Model PrimaryModel { get; set; }

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
        /// Opens a connection dialog window.
        /// </summary>
        private void OpenConnectionDialog()
        {
            ConnectionListWindow window = new ConnectionListWindow(this.PrimaryModel);
            window.Owner = this;
            bool? result = window.ShowDialog();
            if (result.HasValue && result.Value)
            {
                this.ConnectionContainer.DataContext = this.connectionView = new ConnectionView(this.PrimaryModel.CurrentConnection);
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
            this.OpenConnectionDialog();
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

            ClobDirectory clobDir = this.PrimaryModel.CurrentConnection.ClobDirectoryList[this.clobTypeListBox.SelectedIndex];

            this.connectionView.PopulateFileTreeForClobDirectory(clobDir);
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
                    this.PrimaryModel.SendUpdateClobMessage(filename);
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
                string downloadedFile = this.PrimaryModel.SendDownloadClobDataToFileMessage(filename);
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
                    bool result = this.PrimaryModel.SendInsertClobMessage(filename, ref databaseFile);

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
            SystemSounds.Question.Play();
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
                this.PrimaryModel.UpdateClobWithExternalFile(fileBackup.OriginalFilename, fileBackup.BackupFilename);
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

            this.PrimaryModel.GetDatabaseFileLists(ref errorList);
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
    }
}
