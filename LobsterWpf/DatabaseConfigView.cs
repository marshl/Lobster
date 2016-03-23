//-----------------------------------------------------------------------
// <copyright file="DatabaseConfigView.cs" company="marshl">
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
//      The yells and yammering, croaking, jibbering and jabbering; howls,
//      growls and curses; shrieking and skriking, that followed were beyond
//      description.
//
//      [ _The Hobbit_, IV: "Over Hill and Under Hill"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using LobsterModel;
    using Microsoft.WindowsAPICodePack.Dialogs;

    /// <summary>
    /// The view of a DatabaseConfig object.
    /// </summary>
    public class DatabaseConfigView : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfigView"/> class.
        /// </summary>
        /// <param name="databaseConfig">The database config to use as the model of this view.</param>
        public DatabaseConfigView(DatabaseConfig databaseConfig)
        {
            this.BaseConfig = databaseConfig;
        }

        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the underlying model object.
        /// </summary>
        public DatabaseConfig BaseConfig { get; }

        /// <summary>
        /// Gets or sets a value indicating whether changes have been made to any of the fields in this config.
        /// </summary>
        public bool ChangesMade { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the connection. This is for display purposes only.
        /// </summary>
        public string Name
        {
            get
            {
                return this.BaseConfig.Name;
            }

            set
            {
                this.BaseConfig.Name = value;
                this.NotifyPropertyChanged("Name");
                this.ChangesMade = true;
            }
        }

        /// <summary>
        /// Gets or sets the host of the database.
        /// </summary>
        public string Host
        {
            get
            {
                return this.BaseConfig.Host;
            }

            set
            {
                this.BaseConfig.Host = value;
                this.NotifyPropertyChanged("Host");
                this.ChangesMade = true;
            }
        }

        /// <summary>
        /// Gets or sets the port the database is listening on. Usually 1521 for Oracle.
        /// </summary>
        public string Port
        {
            get
            {
                return this.BaseConfig.Port;
            }

            set
            {
                this.BaseConfig.Port = value;
                this.NotifyPropertyChanged("Port");
                this.ChangesMade = true;
            }
        }

        /// <summary>
        /// Gets or sets the Oracle System ID of the database.
        /// </summary>
        public string SID
        {
            get
            {
                return this.BaseConfig.SID;
            }

            set
            {
                this.BaseConfig.SID = value;
                this.NotifyPropertyChanged("SID");
                this.ChangesMade = true;
            }
        }

        /// <summary>
        /// Gets or sets the name of the user/schema to connect as.
        /// </summary>
        public string Username
        {
            get
            {
                return this.BaseConfig.Username;
            }

            set
            {
                this.BaseConfig.Username = value;
                this.NotifyPropertyChanged("Username");
                this.ChangesMade = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether pooling is enabled or not. 
        /// When enabled, Oracle will remember new connections for a time, and reuse it if the same computer connects using the same connection string.
        /// </summary>
        public bool UsePooling
        {
            get
            {
                return this.BaseConfig.UsePooling;
            }

            set
            {
                this.BaseConfig.UsePooling = value;
                this.NotifyPropertyChanged("UsePooling");
                this.ChangesMade = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether files in the connection can be automatically clobbed when updated.
        /// </summary>
        public bool AllowAutomaticUpdates
        {
            get
            {
                return this.BaseConfig.AllowAutomaticUpdates;
            }

            set
            {
                this.BaseConfig.AllowAutomaticUpdates = value;
                this.NotifyPropertyChanged("AllowAutomaticUpdates");
                this.ChangesMade = true;
            }
        }

        /// <summary>
        /// Gets the ClobType directory for the base config.
        /// </summary>
        public string ClobTypeDirectory
        {
            get
            {
                return this.BaseConfig.ClobTypeDirectory;
            }
        }

        /// <summary>
        /// Gets or sets the file from which this DatabaseConfig was loaded.
        /// </summary>
        public string FileLocation
        {
            get
            {
                return this.BaseConfig.FileLocation;
            }

            set
            {
                this.BaseConfig.FileLocation = value;
                this.NotifyPropertyChanged("FileLocation");
                this.ChangesMade = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the xml the config was loaded from was valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return this.BaseConfig.IsValid;
            }
        }

        /// <summary>
        /// Tests the connection.
        /// </summary>
        /// <param name="password">The password to test the connection with.</param>
        /// <param name="ex">The exception tha was raised during connection testing, if any.</param>
        /// <returns>True if the connection test was successful, otherwise false.</returns>
        public bool TestConnection(string password, ref Exception ex)
        {
            return Model.TestConnection(this.BaseConfig, password, ref ex);
        }

        /// <summary>
        /// Writes the database config out to file, prompting the user for the file to save to if not already set.
        /// </summary>
        /// <returns>True if the changes could be applied, otherwise false.</returns>
        public bool ApplyChanges()
        {
            if (this.BaseConfig.FileLocation == null || !File.Exists(this.BaseConfig.FileLocation))
            {
                CommonSaveFileDialog dlg = new CommonSaveFileDialog();
                dlg.Filters.Add(new CommonFileDialogFilter("eXtensible Markup Language", "*.xml"));
                dlg.Title = "Save Lobster Connection As";
                dlg.DefaultFileName = string.IsNullOrEmpty(this.Name) ? "NewConnection.xml" : this.Name.Replace(" ", string.Empty) + ".xml";

                CommonFileDialogResult result = dlg.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    this.FileLocation = dlg.FileName;
                }
                else
                {
                    return false;
                }
            }

            try
            {
                DatabaseConfig.SerialiseToFile(this.FileLocation, this.BaseConfig);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return false;
            }
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
    }
}
