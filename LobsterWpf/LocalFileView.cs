using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using LobsterModel;

namespace LobsterWpf
{
    public class LocalFileView : FileNodeView
    {
        public bool IsDirectory { get; }

        public override bool CanBeUpdated
        {
            get
            {
                return !this.IsDirectory && this.DatabaseFile != null;
            }
        }

        public override bool CanBeDiffed
        {
            get
            {
                return !this.IsDirectory && this.DatabaseFile != null;
            }
        }

        public override bool CanBeInserted
        {
            get
            {
                return !this.IsDirectory && this.DatabaseFile == null;
            }
        }

        public override bool CanBeExploredTo
        {
            get
            {
                return true;
            }
        }

        public override DBClobFile DatabaseFile
        {
            get
            {
                try
                {
                    ClobDirectory clobDir = this.parentConnectionView.Connection.GetClobDirectoryForFile(this.FullName);
                    return clobDir.GetDatabaseFileForFullpath(this.FullName);
                }
                catch (ClobFileLookupException)
                {
                    return null;
                }
            }

            set
            {
                this.NotifyPropertyChanged("DatabaseFile");
                this.NotifyPropertyChanged("IsLocalOnly");
                this.NotifyPropertyChanged("CanBeUpdated");
                this.NotifyPropertyChanged("CanBeDiffed");
                this.NotifyPropertyChanged("CanBeInserted");
            }
        }

        public override string FullName { get; set; }

        public override string Name
        {
            get
            {
                return Path.GetFileName(this.FullName);
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return (File.GetAttributes(this.FullName) & FileAttributes.ReadOnly) != 0;
            }
        }

        public override string ForegroundColour
        {
            get
            {
                return (this.IsDirectory || this.DatabaseFile != null ? Colors.Black : Colors.LimeGreen).ToString();
            }
        }

        public LocalFileView(ConnectionView connection, string path) : base(connection)
        {
            this.FullName = path;

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
                    FileNodeView node = new LocalFileView(parentConnectionView, subDir.FullName);
                    this.Children.Add(node);
                }

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    if (this.parentConnectionView.ShowReadOnlyFiles || !file.IsReadOnly)
                    {
                        FileNodeView node = new LocalFileView(this.parentConnectionView, file.FullName);
                        this.Children.Add(node);
                    }
                }
            }

            this.Refresh();
        }

        public override string FileSize
        {
            get
            {
                return this.IsDirectory ? null : Utils.BytesToString(new FileInfo(this.FullName).Length);
            }
        }


        public override ImageSource ImageUrl
        {
            get
            {
                string resourceName;

                if (this.IsDirectory)
                {
                    resourceName = this.Children?.Count > 0 ? "FullDirectoryImageSource" : "EmptyDirectoryImageSource";
                }
                else
                {
                    try
                    {
                        resourceName = this.IsReadOnly ? "LockedFileImageSource" : "NormalFileImageSource";
                    }
                    catch (IOException)
                    {
                        // In case the file was not found
                        resourceName = "FileNotFoundImageSource";
                    }
                }

                return (ImageSource)App.Current.FindResource(resourceName);
            }
        }

        public override void Refresh()
        {
            this.NotifyPropertyChanged("FileSize");

            if (!this.IsDirectory)
            {
                List<FileBackup> fileBackups = this.parentConnectionView.Connection.ParentModel.FileBackupLog.GetBackupsForFile(this.FullName);
                if (fileBackups != null)
                {
                    this.FileBackupList = new ObservableCollection<FileBackup>(fileBackups.OrderByDescending(backup => backup.DateCreated));
                }
            }
        }
    }
}
