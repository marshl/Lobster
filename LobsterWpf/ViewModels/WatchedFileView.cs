//-----------------------------------------------------------------------
// <copyright file="WatchedFileView.cs" company="marshl">
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
namespace LobsterWpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using LobsterModel;

    /// <summary>
    /// The view for the <see cref="WatchedFile"/>  object.
    /// </summary>
    public class WatchedFileView : WatchedNodeView
    {
        /// <summary>
        /// The current synchronisation status
        /// </summary>
        private SynchronisationStatus syncStatus = SynchronisationStatus.Unknown;

        /// <summary>
        /// Initializes a new instance of the <see cref="WatchedFileView"/> classs.
        /// </summary>
        /// <param name="watchedFile">The base model.</param>
        public WatchedFileView(WatchedFile watchedFile) : base(watchedFile)
        {
            this.WatchedFile = watchedFile;
        }

        /// <summary>
        /// The different synchronisation status options
        /// </summary>
        public enum SynchronisationStatus
        {
            /// <summary>
            /// Used if the file synchronisation hasn't been checked yet
            /// </summary>
            Unknown,

            /// <summary>
            /// used if the file is synchronised with the database.
            /// </summary>
            Synchronised,

            /// <summary>
            /// Used if the file does not have a database equivalent
            /// </summary>
            LocalOnly,

            /// <summary>
            /// used if there was an error checking the synchronisation status
            /// </summary>
            Error,
        }

        /// <summary>
        /// Gets the base model object.
        /// </summary>
        public WatchedFile WatchedFile { get; }

        /// <summary>
        /// Gets the last write time of the watched file (if possible)
        /// </summary>
        public override DateTime? LastWriteTime
        {
            get
            {
                FileInfo fileInfo = new FileInfo(this.FilePath);
                return fileInfo.Exists ? (DateTime?)fileInfo.LastWriteTime : null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file is read only or not
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return this.WatchedFile.IsReadOnly;
            }
        }

        /// <summary>
        /// Gets the image tha is used to represent this file.
        /// </summary>
        public override ImageSource ImageUrl
        {
            get
            {
                string resourceName = this.IsReadOnly ? "LockedFileImageSource" : "NormalFileImageSource";
                return (ImageSource)Application.Current.FindResource(resourceName);
            }
        }

        /// <summary>
        /// Gets the colour to use for the Name of this file.
        /// </summary>
        public override string ForegroundColour
        {
            get
            {
                switch (this.SyncStatus)
                {
                    case SynchronisationStatus.Synchronised:
                        return Colors.White.ToString();
                    case SynchronisationStatus.LocalOnly:
                        return Colors.Green.ToString();
                    case SynchronisationStatus.Error:
                        return Colors.Red.ToString();
                    case SynchronisationStatus.Unknown:
                        return Colors.Gray.ToString();
                    default:
                        throw new ArgumentException($"Unknown SynchronisationStatus {this.SyncStatus}");
                }
            }
        }

        /// <summary>
        /// Gets the last synchronisation error that occurred when checking the sync status of this file.
        /// </summary>
        public FileSynchronisationCheckException LastSyncError { get; private set; }

        /// <summary>
        /// Gets the synchronisation status
        /// </summary>
        public SynchronisationStatus SyncStatus
        {
            get
            {
                return this.syncStatus;
            }

            private set
            {
                this.syncStatus = value;
                this.NotifyPropertyChanged("SyncStatus");
                this.NotifyPropertyChanged("ForegroundColour");

                this.NotifyPropertyChanged("CanBeInserted");
                this.NotifyPropertyChanged("CanBeUpdated");
                this.NotifyPropertyChanged("CanBeDownloaded");
                this.NotifyPropertyChanged("CanBeCompared");
                this.NotifyPropertyChanged("CanBeDeleted");
                this.NotifyPropertyChanged("CanBeExplored");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be inserted into the database
        /// </summary>
        public override bool CanBeInserted
        {
            get
            {
                return this.SyncStatus == SynchronisationStatus.LocalOnly;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be updated
        /// </summary>
        public override bool CanBeUpdated
        {
            get
            {
                return this.SyncStatus == SynchronisationStatus.Synchronised;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be downloaded.
        /// </summary>
        public override bool CanBeDownloaded
        {
            get
            {
                return this.SyncStatus == SynchronisationStatus.Synchronised;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be compared with its database version
        /// </summary>
        public override bool CanBeCompared
        {
            get
            {
                return this.SyncStatus == SynchronisationStatus.Synchronised;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be deleted from the database
        /// </summary>
        public override bool CanBeDeleted
        {
            get
            {
                return this.SyncStatus == SynchronisationStatus.Synchronised;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be explored to
        /// </summary>
        public override bool CanBeExploredTo
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Checks the synchronisation status of this file with the database.
        /// </summary>
        /// <param name="connectionView">The connection to the database.</param>
        /// <param name="watcherView">The parent directory watcher.</param>
        public void UpdateSyncStatus(ConnectionView connectionView, DirectoryWatcherView watcherView)
        {
            try
            {
                this.LastSyncError = null;
                this.SyncStatus = SynchronisationStatus.Unknown;
                bool result = this.WatchedFile.IsSynchonisedWithDatabase(connectionView.BaseConnection, watcherView.BaseWatcher);
                this.SyncStatus = result ? SynchronisationStatus.Synchronised : SynchronisationStatus.LocalOnly;
            }
            catch (FileSynchronisationCheckException ex)
            {
                this.LastSyncError = ex;
                this.SyncStatus = SynchronisationStatus.Error;
            }
        }

        /// <summary>
        /// Checks the synchronisation status of this file with the database.
        /// </summary>
        /// <param name="connectionView">The connection to the database.</param>
        /// <param name="watcherView">The parent directory watcher.</param>
        public override void CheckFileSynchronisation(ConnectionView connectionView, DirectoryWatcherView watcherView)
        {
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                UpdateSyncStatus(connectionView, watcherView);
            });
        }

        /// <summary>
        /// Refreshes the cache of backup files from the backup directory.
        /// </summary>
        /// <param name="connectionView">The connection view of this directory</param>
        public override void RefreshBackupList(ConnectionView connectionView)
        {
            // Don't retreive backups for database only files.
            if (this.FilePath == null)
            {
                return;
            }

            List<FileBackup> fileBackups = BackupLog.GetBackupsForFile(connectionView.BaseConnection.Config.Parent.CodeSourceDirectory, this.FilePath);
            if (fileBackups != null)
            {
                this.FileBackupList = new ObservableCollection<FileBackup>(fileBackups.OrderByDescending(backup => backup.DateCreated));
            }
        }
    }
}
