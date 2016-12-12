using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using LobsterModel;

namespace LobsterWpf.ViewModels
{
    class WatchedFileView : WatchedNodeView
    {
        public WatchedFileView(WatchedFile watchedFile) : base(watchedFile)
        {
            this.WatchedFile = watchedFile;
        }

        public WatchedFile WatchedFile { get; }

        public override void CheckFileSynchronisation(ConnectionView connectionView, DirectoryWatcherView watcherView)
        {
            var th = new Thread(delegate ()
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
            });

            th.Start();
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

        public FileSynchronisationCheckException LastSyncError { get; private set; }

        public enum SynchronisationStatus
        {
            Unknown,
            Synchronised,
            LocalOnly,
            Error,
        }

        private SynchronisationStatus syncStatus = SynchronisationStatus.Unknown;

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
            }
        }


        public override bool CanBeUpdated
        {
            get
            {
                return this.SyncStatus == SynchronisationStatus.Synchronised;
            }
        }

        public override bool CanBeDownloaded
        {
            get
            {
                return this.SyncStatus == SynchronisationStatus.Synchronised;
            }
        }

        public override bool CanBeCompared
        {
            get
            {
                return this.SyncStatus == SynchronisationStatus.Synchronised;
            }
        }

        public override bool CanBeDeleted
        {
            get
            {
                return this.SyncStatus == SynchronisationStatus.Synchronised;
            }
        }

        public override bool CanBeExploredTo
        {
            get
            {
                return true;
            }
        }
    }
}
