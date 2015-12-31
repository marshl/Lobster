﻿//-----------------------------------------------------------------------
// <copyright file="DatabaseConfigView.cs" company="marshl">
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
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using LobsterModel;
    using Microsoft.WindowsAPICodePack.Dialogs;

    /// <summary>
    /// The view of a DatabaseConfig object.
    /// </summary>
    public class DatabaseConfigView : INotifyPropertyChanged
    {
        /// <summary>
        /// The model object.
        /// </summary>
        private DatabaseConfig databaseConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfigView"/> class.
        /// </summary>
        /// <param name="databaseConfig">The database config to use as the model of this view.</param>
        public DatabaseConfigView(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the name of the connection. This is for display purposes only.
        /// </summary>
        public string Name
        {
            get
            {
                return this.databaseConfig.Name;
            }

            set
            {
                this.databaseConfig.Name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the host of the database.
        /// </summary>
        public string Host
        {
            get
            {
                return this.databaseConfig.Host;
            }

            set
            {
                this.databaseConfig.Host = value;
                this.NotifyPropertyChanged("Host");
            }
        }

        /// <summary>
        /// Gets or sets the port the database is listening on. Usually 1521 for Oracle.
        /// </summary>
        public string Port
        {
            get
            {
                return this.databaseConfig.Port;
            }

            set
            {
                this.databaseConfig.Port = value;
                this.NotifyPropertyChanged("Port");
            }
        }

        /// <summary>
        /// Gets or sets the Oracle System ID of the database.
        /// </summary>
        public string SID
        {
            get
            {
                return this.databaseConfig.SID;
            }

            set
            {
                this.databaseConfig.SID = value;
                this.NotifyPropertyChanged("SID");
            }
        }

        /// <summary>
        /// Gets or sets the name of the user/schema to connect as.
        /// </summary>
        public string Username
        {
            get
            {
                return this.databaseConfig.Username;
            }

            set
            {
                this.databaseConfig.Username = value;
                this.NotifyPropertyChanged("Username");
            }
        }

        /// <summary>
        /// Gets or sets the password to connect with.
        /// </summary>
        public string Password
        {
            get
            {
                return this.databaseConfig.Password;
            }

            set
            {
                this.databaseConfig.Password = value;
                this.NotifyPropertyChanged("Password");
            }
        }

        /// <summary>
        /// Gets or sets the location of the CodeSource directory that is used for this database.
        /// </summary>
        public string CodeSource
        {
            get
            {
                return this.databaseConfig.CodeSource;
            }

            set
            {
                this.databaseConfig.CodeSource = value;
                this.NotifyPropertyChanged("CodeSource");
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
                return this.databaseConfig.UsePooling;
            }

            set
            {
                this.databaseConfig.UsePooling = value;
                this.NotifyPropertyChanged("UsePooling");
            }
        }

        /// <summary>
        /// Gets or sets the directory name where ClobTypes are stored.
        /// </summary>
        public string ClobTypeDir
        {
            get
            {
                return this.databaseConfig.ClobTypeDir;
            }

            set
            {
                this.databaseConfig.ClobTypeDir = value;
                this.NotifyPropertyChanged("ClobTypeDir");
            }
        }

        /// <summary>
        /// Gets or sets the file from which this DatabaseConfig was loaded.
        /// </summary>
        public string FileLocation
        {
            get
            {
                return this.databaseConfig.FileLocation;
            }

            set
            {
                this.databaseConfig.FileLocation = value;
                this.NotifyPropertyChanged("FileLocation");
            }
        }

        /// <summary>
        /// Tests the connection.
        /// </summary>
        /// <param name="ex">The exception tha was raised during connection testing, if any.</param>
        /// <returns>True if the connection test was successful, otherwise false.</returns>
        public bool TestConnection(ref Exception ex)
        {
            return Model.TestConnection(this.databaseConfig, ref ex);
        }

        /// <summary>
        /// Opens a folder select dialog to let the user pick a new code source directory.
        /// </summary>
        public void SelectCodeSourceDirectory()
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = this.CodeSource;
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                this.CodeSource = dlg.FileName;
            }
        }

        /// <summary>
        /// Opens a folder select dialog to let the user pick a new clob type directory.
        /// </summary>
        public void SelectClobTypeDirectory()
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = this.ClobTypeDir;
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                this.ClobTypeDir = dlg.FileName;
            }
        }

        /// <summary>
        /// Writes the database config out to file, prompting the user for the file to save to if not already set.
        /// </summary>
        /// <param name="initialDirectory">The directory that the save file dialog will initially open t=to if a new file is made.</param>
        /// <returns>True if the changes could be applied, otherwise false.</returns>
        public bool ApplyChanges(string initialDirectory)
        {
            if (this.databaseConfig.FileLocation == null || !File.Exists(this.databaseConfig.FileLocation))
            {
                CommonSaveFileDialog dlg = new CommonSaveFileDialog();
                dlg.Filters.Add(new CommonFileDialogFilter("eXtensible Markup Language", "*.xml"));
                dlg.Title = "Save Lobster Connection As";
                dlg.DefaultFileName = this.Name?.Replace(" ", string.Empty) + "NewConnection.xml";
                dlg.InitialDirectory = initialDirectory;
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
                DatabaseConfig.SerialiseToFile(this.FileLocation, this.databaseConfig);
                return true;
            }
            catch (IOException)
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