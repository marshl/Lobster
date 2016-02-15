//-----------------------------------------------------------------------
// <copyright file="LocalFileView.cs" company="marshl">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Media;
    using LobsterModel;
    using Properties;

    /// <summary>
    /// A view representing a single local file, with a possible database file connection.
    /// </summary>
    public class LocalFileView : FileNodeView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileView"/> class.
        /// </summary>
        /// <param name="connection">The parent connection of this file view.</param>
        /// <param name="filename">The full path of the file this view will represent.</param>
        public LocalFileView(ConnectionView connection, FileNodeView parentNode, string filename) : base(connection, parentNode)
        {
            this.FullName = filename;

            FileInfo fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                this.LastWriteTime = fileInfo.LastWriteTime;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(filename);
            this.IsDirectory = dirInfo.Exists;

            if (this.IsDirectory)
            {
                this.LastWriteTime = dirInfo.LastWriteTime;
                this.Children = new ObservableCollection<FileNodeView>();

                foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
                {
                    FileNodeView node = new LocalFileView(ParentConnectionView, this, subDir.FullName);
                    this.Children.Add(node);
                }

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    if (connection.ShowReadOnlyFiles || !file.IsReadOnly)
                    {
                        FileNodeView node = new LocalFileView(this.ParentConnectionView, this, file.FullName);
                        this.Children.Add(node);
                    }
                }

                this.IsExpanded = this.ParentConnectionView.ExpandedDirectoryNames.Contains(this.FullName);
            }

            this.Refresh();
        }

        /// <summary>
        /// Gets a value indicating whether this file is a directory or not.
        /// </summary>
        public bool IsDirectory { get; }

        /// <summary>
        /// Gets a value indicating whether this file can be updated or not.
        /// </summary>
        public override bool CanBeUpdated
        {
            get
            {
                return !this.IsDirectory && this.DatabaseFile != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be diffed or not.
        /// </summary>
        public override bool CanBeDiffed
        {
            get
            {
                if (this.IsDirectory || this.DatabaseFile == null)
                {
                    return false;
                }

                string extension = Path.GetExtension(this.Name);
                return Settings.Default.DiffableExtensions.Contains(extension);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can inserted or not.
        /// </summary>
        public override bool CanBeInserted
        {
            get
            {
                return !this.IsDirectory && this.DatabaseFile == null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be explored to or not.
        /// </summary>
        public override bool CanBeExploredTo
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the database file for this local file.
        /// </summary>
        public override DBClobFile DatabaseFile
        {
            get
            {
                try
                {
                    ClobDirectory clobDir = this.ParentConnectionView.Connection.GetClobDirectoryForFile(this.FullName);
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

        /// <summary>
        /// Gets or sets the full path of the file for this view.
        /// </summary>
        public override string FullName { get; set; }

        /// <summary>
        /// Gets the text that is displayed in the file tree view for this local file.
        /// </summary>
        public override string Name
        {
            get
            {
                return Path.GetFileName(this.FullName);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file is read only or not.
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return !this.IsDirectory && (File.GetAttributes(this.FullName) & FileAttributes.ReadOnly) != 0;
            }
        }

        /// <summary>
        /// Gets a colour that is used as the foreground for the name.
        /// </summary>
        public override string ForegroundColour
        {
            get
            {
                return (this.IsDirectory || this.DatabaseFile != null ? Colors.White : Colors.LimeGreen).ToString();
            }
        }

        /// <summary>
        /// Gets a string representing the size of this file.
        /// </summary>
        public override string FileSize
        {
            get
            {
                return this.IsDirectory ? null : Utils.BytesToString(new FileInfo(this.FullName).Length);
            }
        }

        /// <summary>
        /// Gets the image tha is used to represent this file.
        /// </summary>
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

        /// <summary>
        /// Refreshes any data relevant to the file.
        /// </summary>
        public override void Refresh()
        {
            this.NotifyPropertyChanged("FileSize");

            if (!this.IsDirectory)
            {
                List<FileBackup> fileBackups = BackupLog.GetBackupsForFile(this.ParentConnectionView.Connection.Config.CodeSource, this.FullName);
                if (fileBackups != null)
                {
                    this.FileBackupList = new ObservableCollection<FileBackup>(fileBackups.OrderByDescending(backup => backup.DateCreated));
                }
            }
        }
    }
}
