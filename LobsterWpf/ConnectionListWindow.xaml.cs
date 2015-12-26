//-----------------------------------------------------------------------
// <copyright file="ConnectionListWindow.xaml.cs" company="marshl">
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
//      'Who are you, and who is your lord?' asked Frodo.
//
//      [ _The Lord of the Rings_, I/iii: "Three is Company"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using LobsterModel;
    using Microsoft.WindowsAPICodePack.Dialogs;

    /// <summary>
    /// Interaction logic for ConnectionListWindow.xaml
    /// </summary>
    public partial class ConnectionListWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// The internal value for DatabaseConfigList.
        /// </summary>
        private ObservableCollection<DatabaseConfig> databaseConfigList;

        /// <summary>
        /// The event listener that is used to populate new connections.
        /// </summary>
        private IModelEventListener eventListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListWindow"/> class.
        /// </summary>
        /// <param name="listener">The event listener sed to create new connections.</param>
        public ConnectionListWindow(IModelEventListener listener)
        {
            this.InitializeComponent();

            this.LoadDatabaseConnections();
            this.DataContext = this;
            this.eventListener = listener;
        }

        /// <summary>
        /// The event for when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the connection that was made by the last connection call (acce
        /// </summary>
        public DatabaseConnection DatabaseConnection { get; private set; }

        /// <summary>
        /// Gets or sets the directory that connections are found in.
        /// </summary>
        public string ConnectionDirectory
        {
            get
            {
                return Model.ConnectionDirectory;
            }

            set
            {
                Model.ConnectionDirectory = value;
                this.NotifyPropertyChanged("ConnectionDirectory");
            }
        }

        /// <summary>
        /// Gets or sets the list of configuation files for the current connection directory.
        /// </summary>
        public ObservableCollection<DatabaseConfig> DatabaseConfigList
        {
            get
            {
                return this.databaseConfigList;
            }

            set
            {
                this.databaseConfigList = value;
                this.NotifyPropertyChanged("DatabaseConfigList");
            }
        }

        /// <summary>
        /// The event when a connection config item is double clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DatabaseConfig config = ((ListBoxItem)sender).Content as DatabaseConfig;
            this.TryConnectWithConfig(config);
        }

        /// <summary>
        /// Implementation of the INotifyPropertyChange, to tell WPF when a data value has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        /// <remarks>This method is called by the Set accessor of each property.
        /// The CallerMemberName attribute that is applied to the optional propertyName
        /// parameter causes the property name of the caller to be substituted as an argument.</remarks>
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// The event for when the cancel button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// The evenet for when the change directory method is clicked, prompting the user for the new directory.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ChangeDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            CommonFileDialogResult result = dlg.ShowDialog();
            this.Focus();
            if (result == CommonFileDialogResult.Ok)
            {
                this.ConnectionDirectory = dlg.FileName;
                this.LoadDatabaseConnections();
            }
        }

        /// <summary>
        /// The event when the connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionListBox.SelectedIndex == -1)
            {
                return;
            }

            DatabaseConfig config = this.DatabaseConfigList[this.connectionListBox.SelectedIndex];

            this.TryConnectWithConfig(config);
        }

        /// <summary>
        /// Attempts to change the current connection to the input config. If successful, the window is closed.
        /// </summary>
        /// <param name="config">The config to connect with.</param>
        private void TryConnectWithConfig(DatabaseConfig config)
        {
            try
            {
                this.DatabaseConnection = Model.SetDatabaseConnection(config, this.eventListener);
                this.DialogResult = true;
                this.Close();
            }
            catch (SetConnectionException ex)
            {
                MessageBox.Show($"An error occurred when attempting to connect to the database: \n{ex}");
            }
        }

        /// <summary>
        /// The event called when the new connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void NewConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            DatabaseConfig config = new DatabaseConfig();
            DatabaseConfigView configView = new DatabaseConfigView(config);
            EditConnectionWindow ecw = new EditConnectionWindow(configView, this.ConnectionDirectory);
            ecw.Owner = this;
            bool? result = ecw.ShowDialog();
        }

        /// <summary>
        /// The event for when the edit connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EditConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionListBox.SelectedIndex == -1)
            {
                return;
            }

            DatabaseConfig config = this.DatabaseConfigList[this.connectionListBox.SelectedIndex];
            DatabaseConfigView configView = new DatabaseConfigView(config);
            EditConnectionWindow ecw = new EditConnectionWindow(configView, this.ConnectionDirectory);
            ecw.Owner = this;
            bool? result = ecw.ShowDialog();
            this.LoadDatabaseConnections();
        }

        /// <summary>
        /// Loads the database connections from disk, and refreshes the connection list.
        /// </summary>
        private void LoadDatabaseConnections()
        {
            this.DatabaseConfigList = new ObservableCollection<DatabaseConfig>(DatabaseConfig.GetConfigList());
        }
    }
}
