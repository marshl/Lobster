﻿//-----------------------------------------------------------------------
// <copyright file="FileNodeView.cs" company="marshl">
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
//      Dori, Nori, On, Oin, and Gloin were more comfortable in a huge pine with regular
//      branches sticking out at intervals like the spokes of a wheel.
//          -- CHARACTER
//
//      [ _The Hobbit_, IX: "Out of the Frying Pan and Into the Fire"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using LobsterModel;

    /// <summary>
    /// An abstract node to display either a database or local file 
    /// </summary>
    public abstract class FileNodeView : INotifyPropertyChanged
    {
        /// <summary>
        /// The list of views of backup records (if the file is synchronised);
        /// </summary>
        private ObservableCollection<FileBackup> fileBackupList;

        /// <summary>
        /// The connectionview that created this file.
        /// </summary>
        private ConnectionView parentConnectionView;

        /// <summary>
        /// The last time the local file was written to.
        /// </summary>
        private DateTime lastWRiteTime;

        /// <summary>
        /// Whether or not the tree view item this node represents has been expanded.
        /// </summary>
        private bool isExpanded = false;

        /// <summary>
        /// A string representation of the size of the file, including unit (e.g. 12kb)
        /// </summary>
        private string fileSize;

        /// <summary>
        /// The complete file path of the 
        /// </summary>
        private string filePath;

        /// <summary>
        /// The string to use when displaying the file.
        /// </summary>
        private string displayName;

        /// <summary>
        /// Whether the file this node represents is read only.
        /// </summary>
        private bool isReadOnly;

        /// <summary>
        /// The database information about the file (if applicable).
        /// </summary>
        private DBClobFile databaseFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileNodeView"/> class.
        /// </summary>
        /// <param name="connectionView">The parent connetion of the file.</param>
        public FileNodeView(ConnectionView connectionView)
        {
            this.parentConnectionView = connectionView;
        }

        /// <summary>
        /// The event for when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets a string representing the size of this file.
        /// </summary>
        public string FileSize
        {
            get
            {
                return this.fileSize;
            }

            protected set
            {
                this.fileSize = value;
                this.NotifyPropertyChanged("FileSize");
            }
        }

        /// <summary>
        /// Gets or sets the collection of backup records.
        /// </summary>
        public ObservableCollection<FileBackup> FileBackupList
        {
            get
            {
                Application.Current.FindResource("FolderImageSource");
                return this.fileBackupList;
            }

            set
            {
                this.fileBackupList = value;
                this.NotifyPropertyChanged("FileBackupList");
            }
        }

        /// <summary>
        /// Gets or sets the full path of the local file.
        /// </summary>
        public string FilePath
        {
            get
            {
                return this.filePath;
            }

            protected set
            {
                this.filePath = value;
                this.NotifyPropertyChanged("FilePath");
            }
        }

        /// <summary>
        /// Gets or sets the text that will be displayed in the file tree view.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return this.displayName;
            }

            set
            {
                this.displayName = value;
                this.NotifyPropertyChanged("DisplayName");
            }
        }

        /// <summary>
        /// Gets or sets the child file nodes for this view.
        /// </summary>
        public ObservableCollection<FileNodeView> Children { get; set; }

        /// <summary>
        /// Gets the image source that is displayed next to this file in the file tree view.
        /// </summary>
        public abstract ImageSource ImageUrl { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this file is read only or not.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }

            protected set
            {
                this.isReadOnly = value;
                this.NotifyPropertyChanged("IsReadOnly");
            }
        }

        /// <summary>
        /// Gets or sets the database file.
        /// </summary>
        public DBClobFile DatabaseFile
        {
            get
            {
                return this.databaseFile;
            }

            set
            {
                this.databaseFile = value;
                this.NotifyPropertyChanged("DatabaseFile");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can update the database with its local file.
        /// </summary>
        public abstract bool CanBeUpdated { get; }

        /// <summary>
        /// Gets a value indicating whether this file can have its local and database files diffed.
        /// </summary>
        public abstract bool CanBeDiffed { get; }

        /// <summary>
        /// Gets a value indicating whether this file can be inserted into the database or not.
        /// </summary>
        public abstract bool CanBeInserted { get; }

        /// <summary>
        /// Gets a value indicating whether this file can be downloaded from the database.
        /// </summary>
        public abstract bool CanBePulled { get; }

        /// <summary>
        /// Gets a value indicating whether this file can be explored to or not.
        /// </summary>
        public abstract bool CanBeExploredTo { get; }

        /// <summary>
        /// Gets the colour to use for the Name of this file.
        /// </summary>
        public abstract string ForegroundColour { get; }

        /// <summary>
        /// Gets or sets the last trim the local file was written to.
        /// </summary>
        public DateTime LastWriteTime
        {
            get
            {
                return this.lastWRiteTime;
            }

            set
            {
                this.lastWRiteTime = value;
                this.NotifyPropertyChanged("LastWriteTime");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the TreeViewItem associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }

            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    this.NotifyPropertyChanged("IsExpanded");

                    if (this.isExpanded)
                    {
                        this.parentConnectionView.ExpandedDirectoryNames.Add(this.FilePath);
                    }
                    else
                    {
                        this.parentConnectionView.ExpandedDirectoryNames.Remove(this.FilePath);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this file is actually a directory.
        /// </summary>
        public bool IsDirectory { get; protected set; }

        /// <summary>
        /// Gets the parent connection view of this file.
        /// </summary>
        protected ConnectionView ParentConnectionView
        {
            get
            {
                return this.parentConnectionView;
            }

            private set
            {
                this.parentConnectionView = value;
            }
        }

        /// <summary>
        /// Refreshes any data relevant to the file.
        /// </summary>
        /// <param name="recurse">Whether all child files should also be refreshed.</param>
        public void RefreshFileInformation(bool recurse)
        {
            if (this.IsDirectory)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(this.FilePath);
                this.LastWriteTime = dirInfo.LastWriteTime;
                this.FileSize = string.Empty;
                this.IsReadOnly = false;

                if (recurse)
                {
                    foreach (var child in this.Children)
                    {
                        child.RefreshFileInformation(true);
                    }
                }
            }
            else if (this.FilePath != null)
            {
                FileInfo fileInfo = new FileInfo(this.FilePath);
                if (fileInfo.Exists)
                {
                    this.LastWriteTime = fileInfo.LastWriteTime;
                    this.FileSize = Utils.BytesToString(fileInfo.Length);
                    this.IsReadOnly = fileInfo.IsReadOnly;
                }
            }

            this.NotifyPropertyChanged();
        }

        /// <summary>
        /// Refreshes the cache of backup files from the backup directory.
        /// </summary>
        public void RefreshBackupList()
        {
            if (this.IsDirectory)
            {
                return;
            }

            // Don't retreive backups for database only files.
            if (this.FilePath == null)
            {
                return;
            }

            List<FileBackup> fileBackups = BackupLog.GetBackupsForFile(this.ParentConnectionView.Connection.Config.CodeSource, this.FilePath);
            if (fileBackups != null)
            {
                this.FileBackupList = new ObservableCollection<FileBackup>(fileBackups.OrderByDescending(backup => backup.DateCreated));
            }
        }

        /// <summary>
        /// Implementation of the INotifyPropertyChange, to tell WPF when a data value has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        /// <remarks>This method is called by the Set accessor of each property.
        /// The CallerMemberName attribute that is applied to the optional propertyName
        /// parameter causes the property name of the caller to be substituted as an argument.</remarks>
        protected void NotifyPropertyChanged(string propertyName = null)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}