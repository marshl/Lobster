//-----------------------------------------------------------------------
// <copyright file="ConnectionView.cs" company="marshl">
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
//      "Bless us and splash us, my precioussss! I guess it's a choice feast; at least a
//      tasty morsel it'd make us, gollum!"
//          -- CHARACTER
//
//      [ _The Lord of the Rings_, V/i: "Chapter"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using LobsterModel;

    /// <summary>
    /// The ViewModel for a model DatabaseConnection object.
    /// </summary>
    public sealed class ConnectionView : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Whether this connection is currently enabled in the interface or not.
        /// </summary>
        private bool isEnabled = true;

        public WatchedNodeView SelectedNode { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionView"/> class.
        /// </summary>
        /// <param name="con">The model connection this object is viewing.</param>
        public ConnectionView(DatabaseConnection con)
        {
            this.Connection = con;
            this.ExpandedDirectoryNames = new List<string>();

            this.DirectoryWatcherViews = new ObservableCollection<DirectoryWatcherView>();
            foreach (DirectoryWatcher watchedDir in con.DirectoryWatcherList)
            {
                this.DirectoryWatcherViews.Add(new DirectoryWatcherView(watchedDir));
            }
        }

        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the list of directory names that have been expanded.
        /// </summary>
        public List<string> ExpandedDirectoryNames { get; }

        /// <summary>
        /// Gets the connection model for this view.
        /// </summary>
        public DatabaseConnection Connection { get; private set; }

        /// <summary>
        /// Gets or sets the root level file for the currently selected clob directory.
        /// </summary>
        public WatchedNodeView RootFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether read only files should be displayed or not.
        /// </summary>
        public bool ShowReadOnlyFiles { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of clob types currently found for this connection.
        /// </summary>
        //public ObservableCollection<ClobDirectoryView> ClobDirectories { get; set; } = new ObservableCollection<ClobDirectoryView>();

        /// <summary>
        /// Gets or sets a value indicating whether this connection is currently enabled.
        /// The connection is disabled while events are processed.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return this.isEnabled;
            }

            set
            {
                this.isEnabled = value;
                this.NotifyPropertyChanged("IsEnabled");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the underlying connection has automatic file updates enabled.
        /// </summary>
        public bool IsAutoUpdateEnabled
        {
            get
            {
                return this.Connection.IsAutomaticClobbingEnabled;
            }

            set
            {
                this.Connection.IsAutomaticClobbingEnabled = value;
                this.NotifyPropertyChanged("IsAutoUpdateEnabled");
            }
        }

        public ObservableCollection<DirectoryWatcherView> DirectoryWatcherViews { get; }

        public void PopulateFileTreeForDirectoryWatcher(DirectoryWatcher dirWatcher)
        {
            this.RootFile = null;

            if (dirWatcher == null)
            {
                return;
            }
        }
        /*
        /// <summary>
        /// Populates the root file with files from the given clob directory.
        /// </summary>
        /// <param name="clobDir">The directory to populate the file list for.</param>
        public void PopulateFileTreeForClobDirectory(ClobDirectory clobDir)
        {
            if (clobDir == null)
            {
                if (this.RootFile != null)
                {
                    this.RootFile.Children = null;
                }

                this.RootFile = null;
                return;
            }

            if (!clobDir.Directory.Exists)
            {
                return;
            }

            if (this.CurrentDisplayMode == DisplayMode.LocalFiles)
            {
                this.RootFile = new LocalFileView(this, clobDir, clobDir.Directory.FullName, clobDir.ClobType.IncludeSubDirectories);
            }
            else if (this.CurrentDisplayMode == DisplayMode.DatabaseFiles)
            {
                this.RootFile = new DatabaseFileView(this, null, null);
                this.RootFile.Children = new ObservableCollection<FileNodeView>();

                FileInfo[] files = clobDir.Directory.GetFiles(".", SearchOption.AllDirectories);

                foreach (DBClobFile df in clobDir.DatabaseFileList)
                {
                    FileInfo fileInfo = Array.Find(files, x => x.Name.Equals(df.Filename, StringComparison.OrdinalIgnoreCase));
                    if (!this.ShowReadOnlyFiles && fileInfo != null && fileInfo.IsReadOnly)
                    {
                        continue;
                    }

                    DatabaseFileView dfv = new DatabaseFileView(this, df, fileInfo?.FullName);
                    this.RootFile.Children.Add(dfv);
                }
            }

            this.NotifyPropertyChanged("RootFile");
        }*/

        /// <summary>
        /// Reloads the ClobTypes for the current connection.
        /// </summary>
        public void ReloadClobTypes()
        {
            /*this.Connection.ReloadClobTypes();
            this.ClobDirectories.Clear();

            foreach (ClobDirectory clobDir in this.Connection.ClobDirectoryList)
            {
                this.ClobDirectories.Add(new ClobDirectoryView(this.Connection, clobDir));
            }*/
        }

        /// <summary>
        /// Disposes of the connection for this view.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of this object.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources or not.</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (this.Connection != null)
            {
                this.Connection.Dispose();
                this.Connection = null;
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
