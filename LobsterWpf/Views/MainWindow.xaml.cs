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
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Media;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using LobsterModel;
    using Properties;
    using ViewModels;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable
    {
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
        /// A list of the connection controls within the connection tab list.
        /// </summary>
        private List<ConnectionControl> connectionControlList = new List<ConnectionControl>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            try
            {
                this.successSound = new SoundPlayer(Path.Combine(Environment.CurrentDirectory, Settings.Default.SuccessSoundFile));
                this.failureSound = new SoundPlayer(Path.Combine(Environment.CurrentDirectory, Settings.Default.FailureSoundFile));
            }
            catch (Exception ex)
            {
                MessageLog.LogError("An error occurred when loading the sound effects: " + ex);
            }
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
        /// Plays a success sound
        /// </summary>
        public void PlaySuccessSound()
        {
            try
            {
                this.successSound?.Play();
            }
            catch (Exception ex)
            {
                MessageLog.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Plays a failure sound
        /// </summary>
        public void PlayFailureSound()
        {
            try
            {
                this.failureSound?.Play();
            }
            catch (Exception ex)
            {
                MessageLog.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Closes the given connection and removes it from the list of tabs
        /// </summary>
        /// <param name="connectionControl">The connectio ncontrol to close</param>
        public void CloseConnection(ConnectionControl connectionControl)
        {
            connectionControl.Dispose();
            this.ConnectionTabControl.Items.Remove(connectionControl.TabItem);
            this.connectionControlList.Remove(connectionControl);
            connectionControl.Dispose();
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
                ConnectionView connectionView = new ConnectionView(window.DatabaseConnection);
                ConnectionControl control = new ConnectionControl(connectionView, this);

                this.connectionControlList.Add(control);
                var tabItem = new TabItem()
                {
                    Content = control,
                    Header = connectionView.BaseConnection.Config.Name,
                    IsSelected = true
                };
                this.ConnectionTabControl.Items.Add(tabItem);
                control.TabItem = tabItem;
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
            this.UpdateCheck(true);
#endif
        }

        /// <summary>
        /// Performs an update check, and closes the program if an update is found.
        /// </summary>
        /// <param name="backgroundCheck">WHether this update should be run in the background and hide the failure warning from the user.</param>
        private void UpdateCheck(bool backgroundCheck)
        {
            GitHubUpdater updater = new GitHubUpdater("marshl", "lobster");
            Thread thread = new Thread(() =>
            {
                try
                {
                    bool updateExists = updater.RunUpdateCheck();
                    if (!updateExists)
                    {
                        if (!backgroundCheck)
                        {
                            System.Windows.Forms.MessageBox.Show("No update available");
                        }

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
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Error", $"An error occurred when checking for updates: {ex}");
                }
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
        /// The event for when the requery database menu item is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void RequeryDatabaseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<FileListRetrievalException> errorList = new List<FileListRetrievalException>();
            foreach (ConnectionControl control in this.connectionControlList)
            {
                control.RepopulateFileListView(true);
            }
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
        /// The event called when the check for updates button is clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The evnet arguments</param>
        private void CheckForUpdatesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateCheck(false);
        }

        /// <summary>
        /// The event called when the View Log button is clicked
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ViewLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageLog.OpenLogFile();
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

            foreach (ConnectionControl connectionControl in this.connectionControlList)
            {
                connectionControl.Dispose();
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
        /// The handler for when the Reload Lobster Types menu item is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ReloadLobsterTypesMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ConnectionControl control in this.connectionControlList)
            {
                control.ReloadLobsterTypes();
            }
        }
    }
}
