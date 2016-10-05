//-----------------------------------------------------------------------
// <copyright file="ConnectionListWindow.xaml.cs" company="marshl">
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
//      'Who are you, and who is your lord?' asked Frodo.
//
//      [ _The Lord of the Rings_, I/iii: "Three is Company"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Security;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using LobsterModel;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using ViewModels;

    /// <summary>
    /// Interaction logic for ConnectionListWindow.xaml
    /// </summary>
    public partial class ConnectionListWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<CodeSourceConfigView> CodeSourceConfigList { get; set; }

        /// <summary>
        /// The internal value for DatabaseConfigList.
        /// </summary>
        public ObservableCollection<ConnectionConfigView> DatabaseConfigList { get; set; }

        /// <summary>
        /// Whether the user is currently editing a config file or not.
        /// </summary>
        private bool isEditingConfig = false;

        /// <summary>
        /// Whether the config that the user is currently editing is new or not.
        /// </summary>
        private bool isEditingNewConfig = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListWindow"/> class.
        /// </summary>
        public ConnectionListWindow()
        {
            this.InitializeComponent();

            this.LoadCodeSourceConfigs();
            //this.LoadDatabaseConnections();
            this.DataContext = this;
        }

        private void LoadCodeSourceConfigs()
        {
            List<CodeSourceConfigView> configViews = CodeSourceConfig.GetConfigList().Select(item => new CodeSourceConfigView(item)).ToList();
            this.CodeSourceConfigList = new ObservableCollection<CodeSourceConfigView>(configViews);
        }

        /// <summary>
        /// The event for when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently editing a config file or not.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether the edit button should be enabled or not.
        /// </summary>
        public bool IsEditButtonEnabled
        {
            get
            {
                return this.connectionListBox.SelectedItem != null && !this.IsEditingConfig;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the config list is accessible to the user or not.
        /// </summary>
        public bool IsListAccessible
        {
            get
            {
                return !this.IsEditingConfig;
            }
        }

        /// <summary>
        /// Gets the <see cref="DatabaseConfigView"/> that is currently selected in the config list.
        /// </summary>
        public ConnectionConfigView CurrentConnectionConfigView
        {
            get
            {
                return this.connectionListBox.SelectedItem as ConnectionConfigView;
            }
        }

        public CodeSourceConfigView CurrentCodeSourceConfigView
        {
            get
            {
                return (CodeSourceConfigView)this.codeSourceListBox.SelectedItem;
            }
        }

        /// <summary>
        /// Gets the connection that was made by the last connection call (acce
        /// </summary>
        public DatabaseConnection DatabaseConnection { get; private set; }

        /// <summary>
        /// Implementation of the INotifyPropertyChange, to tell WPF when a data value has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        /// <remarks>This method is called by the Set accessor of each property.
        /// The CallerMemberName attribute that is applied to the optional propertyName
        /// parameter causes the property name of the caller to be substituted as an argument.</remarks>
        private void NotifyPropertyChanged(string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Displays a prompt to the user, confirming that they want to cancel changes to the currently selected database config.
        /// </summary>
        /// <returns>True if the user is Ok with cancelling the changes, otherwise false.</returns>
        private bool ConfirmCancelChanges()
        {
            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to cancel any unsaved changes to {(string.IsNullOrEmpty(this.CurrentConnectionConfigView.Name) ? "Unnamed" : this.CurrentConnectionConfigView.Name)}?",
                "Cancel",
                MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                if (this.isEditingNewConfig)
                {
                    this.DatabaseConfigList.Remove(this.CurrentConnectionConfigView);
                    this.isEditingNewConfig = false;
                }
            }

            return result == MessageBoxResult.OK;
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

            ConnectionConfigView config = this.DatabaseConfigList[this.connectionListBox.SelectedIndex];

            this.TryConnectWithConfig(config.BaseConfig);
        }

        /// <summary>
        /// The event called when the user removes a connection.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void RemoveConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            /*MessageBoxResult result = MessageBox.Show("Are you sure you want to forget this connection? (This will not affect any files on disk)", "Confirm", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                DatabaseConfig.RemoveCodeSource(this.CurrentConfigView.BaseConfig.ParentCodeSourceConfig.CodeSourceDirectory);
                this.databaseConfigList.Remove(this.CurrentConfigView);
            }*/
        }

        /// <summary>
        /// Attempts to change the current connection to the input config. If successful, the window is closed.
        /// </summary>
        /// <param name="config">The config to connect with.</param>
        private void TryConnectWithConfig(ConnectionConfig config)
        {
            PasswordPromptWindow win = new PasswordPromptWindow(config.Name, config.Username);
            win.Owner = this;
            win.ShowDialog();
            if (!(win.DialogResult ?? false))
            {
                return;
            }

            SecureString password = win.textField.SecurePassword;

            try
            {
                this.DatabaseConnection = DatabaseConnection.CreateDatabaseConnection(config, password);
                this.DialogResult = true;
                this.Close();
            }
            catch (SetConnectionException ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        /// <summary>
        /// The event called when the new connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void NewConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            /*CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
            {
                return;
            }

            string errorMessage = null;
            if (!CodeSourceConfig.ValidateNewCodeSourceLocation(dlg.FileName, ref errorMessage))
            {
                MessageBox.Show(errorMessage);
                return;
            }

            CodeSourceConfig codeSourceConfig = null;
            if (!CodeSourceConfig.InitialiseCodeSourceDirectory(dlg.FileName, ref codeSourceConfig))
            {
                return;
            }

            CodeSourceConfigView configView = new CodeSourceConfigView(codeSourceConfig);

            this.databaseConfigList.Add(configView);
            this.connectionListBox.SelectedItem = configView;
            this.IsEditingConfig = true;
            this.isEditingNewConfig = true;*/
        }

        /// <summary>
        /// The event called when the new connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void AddExistingButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
            {
                return;
            }

            string errorMessage = null;
            if (!CodeSourceConfig.ValidateCodeSourceLocation(dlg.FileName, ref errorMessage))
            {
                MessageBox.Show(errorMessage);
                return;
            }

            CodeSourceConfig.AddCodeSourceDirectory(dlg.FileName);

            this.LoadDatabaseConnections();
        }

        /// <summary>
        /// Loads the database connections from disk, and refreshes the connection list.
        /// </summary>
        private void LoadDatabaseConnections()
        {
            /*List<DatabaseConfigView> configViews = DatabaseConfig.GetConfigList().Select(item => new DatabaseConfigView(item)).ToList();
            this.DatabaseConfigList = new ObservableCollection<DatabaseConfigView>(configViews);*/
        }

        /// <summary>
        /// The event that is called when the test connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordPromptWindow win = new PasswordPromptWindow(this.CurrentConnectionConfigView.Name, this.CurrentConnectionConfigView.Username);
            win.ShowDialog();
            if (!win.DialogResult.GetValueOrDefault(false))
            {
                return;
            }

            SecureString password = win.textField.SecurePassword;
            Exception ex = null;
            bool result = this.CurrentConnectionConfigView.TestConnection(password, ref ex);
            string message = result ? "Connection test successful" : "Connection test unsuccessful.\n" + ex;
            MessageBox.Show(message);
        }

        /// <summary>
        /// The event that is called when the edit clob type button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EditClobTypeButton_Click(object sender, RoutedEventArgs e)
        {
            /*ClobTypeListWindow window = new ClobTypeListWindow(this.CurrentConfigView.ClobTypeDirectory);
            window.Owner = this;
            bool? result = window.ShowDialog();
            this.Focus();*/
        }

        /// <summary>
        /// The event that is called when the Cancel button for the config settings is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments</param>
        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsEditingConfig)
            {
                if (this.CurrentConnectionConfigView.ChangesMade && !this.ConfirmCancelChanges())
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

        /// <summary>
        /// The event that is called when the Close button is pressed, prompting the user for unsaved changes, then clsoing the window.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsEditingConfig && !this.ConfirmCancelChanges())
            {
                return;
            }

            this.Close();
        }

        /// <summary>
        /// The event that is called when the Save button for the config options panel is clicked, saving changes to the config.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void SaveEditButton_Click(object sender, RoutedEventArgs e)
        {
            /*Debug.Assert(this.IsEditingConfig, "The save button should be usable while not in edit mode");

            bool result = this.CurrentConfigView.ApplyChanges();
            if (!result)
            {
                MessageBox.Show("Changes could not be saved.");
            }
            else
            {
                this.IsEditingConfig = false;
                this.NotifyPropertyChanged("IsEditButtonEnabled");
            }*/
        }

        /// <summary>
        /// The event that is called when the Edit button is clicked for the config options panel, 
        /// starting an edit to the currently selected config.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void StartEditButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEditingConfig = true;
            this.NotifyPropertyChanged("IsEditButtonEnabled");
        }

        /// <summary>
        /// The event that is called when the selected item of the connection list box changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ConnectionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsEditingConfig)
            {
                e.Handled = true;
                return;
            }

            this.NotifyPropertyChanged("IsEditButtonEnabled");
        }

        private void codeSourceListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void connectionListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConnectionConfigView configView = ((ListBoxItem)sender).Content as ConnectionConfigView;
            this.TryConnectWithConfig(configView.BaseConfig);
        }

        private void codeSourceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
