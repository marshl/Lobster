//-----------------------------------------------------------------------
// <copyright file="FileNodeView.cs" company="marshl">
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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;
    using LobsterModel;

    /// <summary>
    /// 
    /// </summary>
    public abstract class FileNodeView : INotifyPropertyChanged
    {
        /*
        protected static string FullDirectoryUrl = @"Resources\Images\Folder_stuffed.ico";
        protected static string EmptyDirectoryUrl = @"Resources\Images\folder_open.ico";
        protected static string LockedFileUrl = @"Resources\Images\SecurityLock.ico";
        protected static string NormalFileUrl = @"Resources\Images\Generic_Document.ico";
        protected static string FileNotFoundUrl = @"Resources\Images\Annotate_Blocked_large.ico";
        */

        public abstract string FileSize { get; }

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

        public abstract ImageSource ImageUrl { get; }

        public abstract bool IsReadOnly { get; }

        public abstract void Refresh();

        public abstract DBClobFile DatabaseFile { get; set; }

        public abstract bool CanBeUpdated { get; }

        public abstract bool CanBeDiffed { get; }

        public abstract bool CanBeInserted { get; }

        public abstract bool CanBeExploredTo { get; }

        public abstract string ForegroundColour { get; }
    }
}
