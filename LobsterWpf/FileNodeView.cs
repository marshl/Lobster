//-----------------------------------------------------------------------
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
namespace LobsterWpf
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
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
        /// Initializes a new instance of the <see cref="FileNodeView"/> class.
        /// </summary>
        /// <param name="connectionView">The parent connetion of the file.</param>
        public FileNodeView(ConnectionView connectionView, FileNodeView parentNode)
        {
            this.ParentNode = parentNode;
            this.parentConnectionView = connectionView;
        }

        /// <summary>
        /// The event for when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets a string representing the size of the local file, if it exists.
        /// </summary>
        public abstract string FileSize { get; }

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
        public abstract string FullName { get; set; }

        /// <summary>
        /// Gets the text that will be displayed in the file tree view.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets the child file nodes for this view.
        /// </summary>
        public ObservableCollection<FileNodeView> Children { get; set; }

        /// <summary>
        /// Gets the image source that is displayed next to this file in the file tree view.
        /// </summary>
        public abstract ImageSource ImageUrl { get; }

        /// <summary>
        /// Gets a value indicating whether the local file for this file is read only or not.
        /// </summary>
        public abstract bool IsReadOnly { get; }

        /// <summary>
        /// Gets or sets the database file.
        /// </summary>
        public abstract DBClobFile DatabaseFile { get; set; }

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
        /// Resets aspects of this file.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Implementation of the INotifyPropertyChange, to tell WPF when a data value has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        /// <remarks>This method is called by the Set accessor of each property.
        /// The CallerMemberName attribute that is applied to the optional propertyName
        /// parameter causes the property name of the caller to be substituted as an argument.</remarks>
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool isExpanded = false;

        public FileNodeView ParentNode { get; }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
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
                        this.parentConnectionView.ExpandedDirectoryNames.Add(this.FullName);
                    }
                    else
                    {
                        this.parentConnectionView.ExpandedDirectoryNames.Remove(this.FullName);
                    }
                }

                // Expand all the way up to the root.
                /*if (isExpanded && this.ParentNode != null)
                {
                    this.ParentNode.IsExpanded = true;
                }*/
            }
        }
    }
}
