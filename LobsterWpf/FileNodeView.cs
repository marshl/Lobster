using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LobsterModel;

namespace LobsterWpf
{
    class FileNodeView : INotifyPropertyChanged
    {
        public string FileSize
        {
            get
            {
                return this.IsDirectory ? null : Utils.BytesToString(new FileInfo(this.FullName).Length);
            }
        }

        private ObservableCollection<FileBackup> _fileBackupList;
        public ObservableCollection<FileBackup> FileBackupList
        {
            get
            {
                return this._fileBackupList;
            }
            set
            {
                this._fileBackupList = value;
                this.NotifyPropertyChanged("FileBackupList");
            }
        }

        private ConnectionView parentConnectionView;
        public FileNodeView(ConnectionView connectionView, string path)
        {
            this.FullName = path;
            this.parentConnectionView = connectionView;

            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                this.LastWriteTime = fileInfo.LastWriteTime;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            this.IsDirectory = dirInfo.Exists;

            if (this.IsDirectory)
            {
                this.LastWriteTime = dirInfo.LastWriteTime;
                this.Children = new ObservableCollection<FileNodeView>();

                foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
                {
                    FileNodeView node = new FileNodeView(connectionView, subDir.FullName);
                    this.Children.Add(node);
                }

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    if (this.parentConnectionView.ShowReadOnlyFiles || !this.IsReadOnly)
                    {
                        FileNodeView node = new FileNodeView(connectionView, file.FullName);
                        this.Children.Add(node);
                    }
                }
            }

            this.Refresh();
        }

        public string FullName { get; private set; }

        public string Name
        {
            get
            {
                return Path.GetFileName(this.FullName);
            }
        }

        public bool IsDirectory { get; private set; }

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

        private void NotifyPropertyChanged(string propertyName = "")
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
                if (this.IsDirectory)
                {
                    return this.Children?.Count > 0 ? @"Resources\Images\Folder_stuffed.ico" : @"Resources\Images\folder_open.ico";
                }

                try
                {
                    return this.IsReadOnly ? @"Resources\Images\SecurityLock.ico" : @"Resources\Images\Generic_Document.ico";
                }
                catch (IOException)
                {
                    // In case the file was not found
                    return @"Resources\Images\Annotate_Blocked_large.ico";
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return (File.GetAttributes(this.FullName) & FileAttributes.ReadOnly) != 0;
            }
        }

        public bool IsVisible
        {
            get
            {
                if (this.IsDirectory)
                {
                    return true;
                }
                try
                {
                    return this.parentConnectionView.ShowReadOnlyFiles || !this.IsReadOnly;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }
        }

        public void Refresh()
        {
            this.NotifyPropertyChanged("FileSize");

            if (!this.IsDirectory)
            {
                List<FileBackup> fileBackups = this.parentConnectionView.connection.ParentModel.FileBackupLog.GetBackupsForFile(this.FullName);
                if (fileBackups != null)
                {
                    this.FileBackupList = new ObservableCollection<FileBackup>(fileBackups.OrderByDescending(backup => backup.DateCreated));
                }
            }
        }
    }
}
