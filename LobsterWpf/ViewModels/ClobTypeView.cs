﻿//-----------------------------------------------------------------------
// <copyright file="ClobTypeView.cs" company="marshl">
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
//      ... but that very week orders began to pour out of Bag End for 
//      every kind of provision, commodity, or luxury that could be 
//      obtained in Hobbiton or Bywater or anywhere in the neighbourhood.
//
//      [ _The Lord of the Rings_, I/i: "A Long-expected Party"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using LobsterModel;
    using Microsoft.WindowsAPICodePack.Dialogs;

    /// <summary>
    /// A view for a model clob type.
    /// </summary>
    public class ClobTypeView : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClobTypeView"/> class.
        /// </summary>
        /// <param name="clobType">The clob type to use for this view.</param>
        public ClobTypeView(ClobType clobType)
        {
            this.ClobTypeObject = clobType;
        }

        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// Gets the model ClobType for this view.
        /// </summary>
        public ClobType ClobTypeObject { get; }

        /// <summary>
        /// Gets or sets the display name for this ClobType. This value has no functional impact, 
        /// and is used for display purposes only.
        /// </summary>
        public string Name
        {
            get
            {
                return this.ClobTypeObject.Name;
            }

            set
            {
                this.ClobTypeObject.Name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the name of the directory in CodeSource to be used for this ClobType. Directory separators can be used.
        /// </summary>
        public string Directory
        {
            get
            {
                return this.ClobTypeObject.Directory;
            }

            set
            {
                this.ClobTypeObject.Directory = value;
                this.NotifyPropertyChanged("Directory");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not all subdirectories under the specified folder should also be used.
        /// </summary>
        public bool IncludeSubdirectories
        {
            get
            {
                return this.ClobTypeObject.IncludeSubDirectories;
            }

            set
            {
                this.ClobTypeObject.IncludeSubDirectories = value;
                this.NotifyPropertyChanged("IncludeSubDirectories");
            }
        }
        
        /// <summary>
        /// Gets or sets the tables that files for this ClobType are stored in.
        /// ClobTypes usually have only a single table, but if there is more than one, then the user will be asked which to use when inserting a new file.
        /// </summary>
        public List<Table> Tables
        {
            get
            {
                return this.ClobTypeObject.Tables;
            }

            set
            {
                this.ClobTypeObject.Tables = value;
                this.NotifyPropertyChanged("Tables");
            }
        }

        /// <summary>
        /// Gets or sets the path of the file from where this ClobType was deserialised.
        /// </summary>
        public string FilePath
        {
            get
            {
                return this.ClobTypeObject.FilePath;
            }

            set
            {
                this.ClobTypeObject.FilePath = value;
                this.NotifyPropertyChanged("FilePath");
            }
        }

        /// <summary>
        /// Gets the filename of the the location where the clob type is stored.
        /// </summary>
        public string Filename
        {
            get
            {
                return Path.GetFileName(this.FilePath);
            }
        }

        /// <summary>
        /// Saves this ClobType out to its file, prompting for a new file if it doesn't already have one.
        /// </summary>
        /// <param name="initialDirectory">The directory to start in if this ClobType is new.</param>
        /// <returns>True if the save was successful, otherwise false.</returns>
        public bool ApplyChanges(string initialDirectory)
        {
            if (this.FilePath == null || !File.Exists(this.FilePath))
            {
                CommonSaveFileDialog dlg = new CommonSaveFileDialog();
                dlg.Filters.Add(new CommonFileDialogFilter("eXtensible Markup Language", "*.xml"));
                dlg.Title = "Save Clob Type As";
                dlg.DefaultFileName = this.Name?.Replace(" ", string.Empty) + ".xml";
                dlg.InitialDirectory = initialDirectory;
                CommonFileDialogResult result = dlg.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    this.FilePath = dlg.FileName;
                }
                else
                {
                    return false;
                }
            }

            try
            {
                ClobType.Serialise(this.FilePath, this.ClobTypeObject);
                return true;
            }
            catch (IOException ex)
            {
                MessageLog.LogError($"An error occurred when serialising ClobType {this.Name} to {this.FilePath}: {ex}");
                MessageBox.Show($"An error occurred when saving the file: {ex.Message}");
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