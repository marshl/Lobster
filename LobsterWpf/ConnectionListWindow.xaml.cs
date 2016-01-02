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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
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
        private ObservableCollection<DatabaseConfigView> databaseConfigList;

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

        private bool isEditingConfig = false;

        private bool isEdditingNewConfig = false;

        public bool IsEditingConfig
        {
            get
            {
                return this.isEditingConfig;
            }

            set
            {
                this.isEditingConfig = value;
                this.NotifyPropertyChanged("IsEditingConfig");
            }
        }
        
        public bool IsEditButtonEnabled
        {
            get
            {
                return this.connectionListBox.SelectedItem != null && !this.IsEditingConfig;
            }
        }

        public bool IsListAccessible
        {
            get
            {
                return !this.IsEditingConfig;
            }
        }

        public DatabaseConfigView CurrentConfigView
        {
            get
            {
                return this.connectionListBox.SelectedItem as DatabaseConfigView;
            }
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
        public ObservableCollection<DatabaseConfigView> DatabaseConfigList
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
            DatabaseConfigView configView = ((ListBoxItem)sender).Content as DatabaseConfigView;
            this.TryConnectWithConfig(configView.BaseConfig);
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

        private bool ConfirmCancelChanges()
        {
            MessageBoxResult result = MessageBox.Show("Cancel unsaved changes?", "Cancel", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                if (this.isEdditingNewConfig)
                {
                    this.databaseConfigList.Remove(this.CurrentConfigView);
                    this.isEdditingNewConfig = false;
                }
            }

            return result == MessageBoxResult.OK;
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

            DatabaseConfigView config = this.DatabaseConfigList[this.connectionListBox.SelectedIndex];

            this.TryConnectWithConfig(config.BaseConfig);
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

            this.databaseConfigList.Add(configView);
            this.connectionListBox.SelectedItem = configView;
            this.IsEditingConfig = true;
            this.isEdditingNewConfig = true;
        }

        /// <summary>
        /// Loads the database connections from disk, and refreshes the connection list.
        /// </summary>
        private void LoadDatabaseConnections()
        {
            List<DatabaseConfigView> configViews = DatabaseConfig.GetConfigList().Select(item => new DatabaseConfigView(item)).ToList();
            this.DatabaseConfigList = new ObservableCollection<DatabaseConfigView>(configViews);
        }

        /// <summary>
        /// The event that is called when the test connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            Exception ex = null;
            bool result = this.CurrentConfigView.TestConnection(ref ex);
            string message = result ? "Connection test successful" : "Connection test unsuccessful.\n" + ex;
            MessageBox.Show(message);
        }

        /// <summary>
        /// The event that is called when the codesource button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CodeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentConfigView.SelectCodeSourceDirectory();
            this.Focus();
        }

        /// <summary>
        /// The event that is called when the clobtype button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ClobTypeButton_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentConfigView.SelectClobTypeDirectory();
            this.Focus();
        }

        /// <summary>
        /// The event that is called when the edit clob type button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EditClobTypeButton_Click(object sender, RoutedEventArgs e)
        {
            ClobTypeListWindow window = new ClobTypeListWindow(this.CurrentConfigView.ClobTypeDir);
            window.Owner = this;
            bool? result = window.ShowDialog();
            this.Focus();
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsEditingConfig)
            {
                if (!ConfirmCancelChanges())
                {
                    return;
                }
            }

            this.IsEditingConfig = false;
            this.NotifyPropertyChanged("IsEditButtonEnabled");
            int selectedIndex = this.connectionListBox.SelectedIndex;
            this.LoadDatabaseConnections();
            this.connectionListBox.SelectedIndex = selectedIndex;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsEditingConfig && !this.ConfirmCancelChanges())
            {
                return;
            }

            this.Close();
        }

        private void SaveEditButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(this.IsEditingConfig);
            this.IsEditingConfig = false;
            bool result = this.CurrentConfigView.ApplyChanges(this.ConnectionDirectory);
        }

        private void StartEditButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(this.IsEditButtonEnabled);

            this.IsEditingConfig = true;
            this.NotifyPropertyChanged("IsEditButtonEnabled");
        }

        private void connectionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsEditingConfig)
            {
                e.Handled = true;
                return;
            }

            this.NotifyPropertyChanged("IsEditButtonEnabled");
        }
    }
}
