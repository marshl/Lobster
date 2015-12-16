using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using LobsterModel;

namespace LobsterWpf
{
    public abstract class FileNodeView : INotifyPropertyChanged
    {
        protected static string FullDirectoryUrl = @"Resources\Images\Folder_stuffed.ico";
        protected static string EmptyDirectoryUrl = @"Resources\Images\folder_open.ico";
        protected static string LockedFileUrl = @"Resources\Images\SecurityLock.ico";
        protected static string NormalFileUrl = @"Resources\Images\Generic_Document.ico";
        protected static string FileNotFoundUrl = @"Resources\Images\Annotate_Blocked_large.ico";

        public abstract string GetFileSize();

        protected ObservableCollection<FileBackup> _fileBackupList;
        public ObservableCollection<FileBackup> FileBackupList
        {
            get
            {
                Application.Current.FindResource("FolderImageSource");
                return this._fileBackupList;
            }
            set
            {
                this._fileBackupList = value;
                this.NotifyPropertyChanged("FileBackupList");
            }
        }

        protected ConnectionView parentConnectionView;
        public FileNodeView(ConnectionView connectionView)
        {
            this.parentConnectionView = connectionView;
        }

        public abstract string FullName { get; set; }

        public abstract string Name { get; }

        public ObservableCollection<FileNodeView> Children { get; set; }

        private DateTime _lastWriteTime;
        public DateTime LastWriteTime
        {
            get
            {
                return this._lastWriteTime;
            }

            set
            {
                this._lastWriteTime = value;
                this.NotifyPropertyChanged("LastWriteTime");
            }
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ImageUrl
        {
            get
            {
                return this.GetImageUrl();
            }
        }

        protected abstract string GetImageUrl();

        public abstract bool IsReadOnly { get; }

        public abstract void Refresh();

        //private DBClobFile databaseFile;
        public abstract DBClobFile DatabaseFile { get; set; }

        public abstract bool CanBeUpdated { get; }

        public abstract bool CanBeDiffed { get; }

        public abstract bool CanBeInserted { get; }

        public abstract bool CanBeExploredTo { get; }

        public abstract string ForegroundColour { get; }
    }
}
