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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
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
            this.DataContext = this;

            // Select the first CodeSource by default
            if (this.CodeSourceConfigList.Count > 0)
            {
                this.codeSourceListBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// The event for when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the list of loaded CodeSource configuration files.
        /// </summary>
        public ObservableCollection<CodeSourceConfigView> CodeSourceConfigList { get; set; }

        /// <summary>
        /// Gets the currently selected connection config from the connection list.
        /// </summary>
        public ConnectionConfigView SelectedConnectionConfig
        {
            get
            {
                return (ConnectionConfigView)this.connectionListBox.SelectedItem;
            }
        }

        /// <summary>
        /// Gets the currently selected CodeSourceConfigView in teh CodeSOurce list.
        /// </summary>
        public CodeSourceConfigView SelectedCodeSourceConfig
        {
            get
            {
                return (CodeSourceConfigView)this.codeSourceListBox.SelectedItem;
            }
        }

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
                this.NotifyPropertyChanged("IsEditButtonEnabled");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the edit button should be enabled or not.
        /// </summary>
        public bool IsEditButtonEnabled
        {
            get
            {
                return this.SelectedConnectionConfig != null && !this.IsEditingConfig;
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
        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Loads the CodeSOurce configuration files that Lobster currently knows of.
        /// </summary>
        private void LoadCodeSourceConfigs()
        {
            CodeSourceConfigLoader configLoader = new CodeSourceConfigLoader();
            configLoader.CodeSourceLoadErrorEvent += this.OnCodeSourceConfigLoadError;
            configLoader.Load();

            List<CodeSourceConfigView> configViews = configLoader.CodeSourceConfigList.Select(item => new CodeSourceConfigView(item)).ToList();
            this.CodeSourceConfigList = new ObservableCollection<CodeSourceConfigView>(configViews);
            this.NotifyPropertyChanged("CodeSourceConfigList");
            this.NotifyPropertyChanged("SelectedConnectionConfig");
        }

        /// <summary>
        /// Handles the event of an error occurring when loading a CodeSourceConfig file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void OnCodeSourceConfigLoadError(object sender, CodeSourceConfigLoader.CodeSourceLoadErrorEventArgs e)
        {
            MessageBox.Show($"An error occurred when loading the CodeSourceConfig file '{e.FilePath}': {e.ExceptionObj?.Message}");
        }

        /// <summary>
        /// Displays a prompt to the user, confirming that they want to cancel changes to the currently selected database config.
        /// </summary>
        /// <returns>True if the user is Ok with cancelling the changes, otherwise false.</returns>
        private bool ConfirmCancelChanges()
        {
            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to cancel any unsaved changes to {(string.IsNullOrEmpty(this.SelectedConnectionConfig.Name) ? "Unnamed" : this.SelectedConnectionConfig.Name)}?",
                "Cancel",
                MessageBoxButton.OKCancel);

            if (result != MessageBoxResult.OK)
            {
                return false;
            }

            if (this.isEditingNewConfig)
            {
                this.RemoveSelectedConnection();
                this.isEditingNewConfig = false;
            }

            return true;
        }

        /// <summary>
        /// The event when the connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedConnectionConfig == null)
            {
                return;
            }

            this.TryConnectWithConfig(this.SelectedConnectionConfig.BaseConfig);
        }

        /// <summary>
        /// The event called when the user removes a connection.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void RemoveConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedConnectionConfig == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove this connection?", "Confirm Connection Removal", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                this.RemoveSelectedConnection();
            }
        }

        /// <summary>
        /// Adds a new connection to the connection list of the currently selected CodeSource, and begins the edit process.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void AddConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedCodeSourceConfig == null)
            {
                return;
            }

            this.IsEditingConfig = true;
            this.isEditingNewConfig = true;
            ConnectionConfig newConfig = new ConnectionConfig();
            this.SelectedCodeSourceConfig.BaseConfig.ConnectionConfigList.Add(newConfig);
            ConnectionConfigView newView = new ConnectionConfigView(newConfig);
            this.SelectedCodeSourceConfig.ConnectionConfigViewList.Add(newView);
            this.connectionListBox.SelectedItem = newView;
        }

        /// <summary>
        /// Removes the currently selected connection from the connection list.
        /// </summary>
        private void RemoveSelectedConnection()
        {
            if (this.SelectedConnectionConfig == null)
            {
                return;
            }

            this.SelectedCodeSourceConfig.BaseConfig.ConnectionConfigList.Remove(this.SelectedConnectionConfig.BaseConfig);
            this.SelectedCodeSourceConfig.BaseConfig.SerialiseToFile();

            this.SelectedCodeSourceConfig.ConnectionConfigViewList.Remove(this.SelectedConnectionConfig);
            this.connectionListBox.SelectedItem = null;
            this.NotifyPropertyChanged("CodeSourceConfigList");
            this.NotifyPropertyChanged("ConnectionConfigList");
            this.NotifyPropertyChanged("SelectedConnectionConfig");
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
                bool loadSuccess = true;

                var databaseConnection = DatabaseConnection.CreateDatabaseConnection(config, password);
                databaseConnection.ConnectionLoadErrorEvent += (object sender, DatabaseConnection.ConnectionLoadErrorEventArgs e) =>
                {
                    MessageBox.Show(e.ErrorMessage);
                    loadSuccess = false;
                };

                databaseConnection.LoadDirectoryDescriptors();

                if (loadSuccess)
                {
                    this.DatabaseConnection = databaseConnection;
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (CreateConnectionException ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        /// <summary>
        /// The event that is called when the test connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordPromptWindow win = new PasswordPromptWindow(this.SelectedConnectionConfig.Name, this.SelectedConnectionConfig.Username);
            win.ShowDialog();
            if (!win.DialogResult.GetValueOrDefault(false))
            {
                return;
            }

            SecureString password = win.textField.SecurePassword;
            Exception ex = null;
            bool result = this.SelectedConnectionConfig.TestConnection(password, ref ex);
            string message = result ? "Connection test successful" : "Connection test unsuccessful:\n" + ex.Message;
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
                if (this.SelectedConnectionConfig.ChangesMade && !this.ConfirmCancelChanges())
                {
                    return;
                }
            }

            this.IsEditingConfig = false;
            this.LoadCodeSourceConfigs();
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
            Debug.Assert(this.IsEditingConfig, "The save button should be usable while not in edit mode");
            try
            {
                this.SelectedCodeSourceConfig.BaseConfig.SerialiseToFile();
                this.IsEditingConfig = false;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                MessageBox.Show($"An error occurred when attempting to save the configuration file:\n{ex.Message}");
            }
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

        /// <summary>
        /// The event handler for when the user double clicks a record in the connection list, opening the connection.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ConnectionListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConnectionConfigView configView = ((ListBoxItem)sender).Content as ConnectionConfigView;
            this.TryConnectWithConfig(configView.BaseConfig);
        }

        /// <summary>
        /// The event handler for when the user selects a different item of the CodeSource list, changing which connections should be displayed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CodeSourceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.NotifyPropertyChanged("SelectedCodeSourceConfig");
        }

        /// <summary>
        /// The event handler for when the Add CodeSource button is pressed, giving the user the ability to choose how they want to add a CodeSource directory.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void AddCodeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddCodeSourceWindow();
            bool? result = window.ShowDialog();
            if (!(result ?? false))
            {
                return;
            }

            switch (window.UserSelection)
            {
                case AddCodeSourceWindow.Selection.AddPreparedCodeSource:
                    this.AddExistingCodeSource();
                    break;
                case AddCodeSourceWindow.Selection.PrepareNewCodeSource:
                    this.PrepareNewCodeSource();
                    break;
                default:
                    return;
            }

            this.NotifyPropertyChanged("CodeSourceConfigList");
        }

        /// <summary>
        /// Prompts the user for an existing CodeSource folder to select.
        /// </summary>
        private void AddExistingCodeSource()
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
            {
                return;
            }

            string errorMessage = null;
            if (!CodeSourceConfigLoader.ValidateCodeSourceLocation(dlg.FileName, ref errorMessage))
            {
                MessageBox.Show(errorMessage);
                return;
            }

            CodeSourceConfigLoader.AddCodeSourceDirectory(dlg.FileName);
            this.LoadCodeSourceConfigs();
        }

        /// <summary>
        /// Prompts the user for a directory to select and initialise as a new CodeSource directory.
        /// </summary>
        private void PrepareNewCodeSource()
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
            {
                return;
            }

            string errorMessage = null;
            if (!CodeSourceConfigLoader.ValidateNewCodeSourceLocation(dlg.FileName, ref errorMessage))
            {
                MessageBox.Show(errorMessage);
                return;
            }

            var nameWindow = new CodeSourceNameWindow();
            bool nameResult = nameWindow.ShowDialog() ?? false;
            if (!nameResult)
            {
                return;
            }

            CodeSourceConfig codeSourceConfig = null;
            if (!CodeSourceConfigLoader.InitialiseCodeSourceDirectory(dlg.FileName, nameWindow.CodeSourceName, ref codeSourceConfig))
            {
                return;
            }

            this.LoadCodeSourceConfigs();
        }

        /// <summary>
        /// The event handler for when the Remove CodeSource button is clicked, removing the selected CodeSource.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void RemoveCodeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.codeSourceListBox.SelectedItem != null)
            {
                CodeSourceConfigLoader.RemoveCodeSource(this.SelectedCodeSourceConfig.BaseConfig.CodeSourceDirectory);
                this.CodeSourceConfigList.Remove((CodeSourceConfigView)this.codeSourceListBox.SelectedItem);
            }
        }
    }
}
