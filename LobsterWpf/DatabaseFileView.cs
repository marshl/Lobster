//-----------------------------------------------------------------------
// <copyright file="DatabaseFileView.cs" company="marshl">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Media;
    using LobsterModel;

    /// <summary>
    /// The view for a file that is on the database, and may also be found locally.
    /// </summary>
    public class DatabaseFileView : FileNodeView
    {
        /// <summary>
        /// The underlying database file this view is working on.
        /// </summary>
        private DBClobFile databaseFile;

        /// <summary>
        /// The file path of the local file for this view.
        /// </summary>
        private string localFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseFileView"/> class.
        /// </summary>
        /// <param name="connection">The parent connection of this file.</param>
        /// <param name="databaseFile">The database file </param>
        /// <param name="localFile">The local equivalent of this file, if it exists.</param>
        public DatabaseFileView(ConnectionView connection, DBClobFile databaseFile, string localFile) : base(connection)
        {
            this.databaseFile = databaseFile;
            this.localFilePath = localFile;
        }

        /// <summary>
        /// Gets a value indicating whether this file can be diffed with the local version.
        /// </summary>
        public override bool CanBeDiffed
        {
            get
            {
                return this.localFilePath != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the file can be directly shown in explorer.
        /// </summary>
        public override bool CanBeExploredTo
        {
            get
            {
                return this.localFilePath != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file is new and can be inserted into the database.
        /// </summary>
        public override bool CanBeInserted
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be used to update the database.
        /// </summary>
        public override bool CanBeUpdated
        {
            get
            {
                return this.localFilePath != null;
            }
        }

        /// <summary>
        /// Gets or sets the database file this view is used for.
        /// </summary>
        public override DBClobFile DatabaseFile
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
        /// Get sor sets the name of the local file.
        /// </summary>
        public override string FullName
        {
            get
            {
                return this.localFilePath;
            }

            set
            {
                this.localFilePath = value;
                this.NotifyPropertyChanged("FullName");
            }
        }

        /// <summary>
        /// Gets a value indicating whether th local file (if it exists) is read only.
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                if (this.localFilePath == null)
                {
                    return false;
                }

                return (File.GetAttributes(this.FullName) & FileAttributes.ReadOnly) != 0;
            }
        }

        /// <summary>
        /// Gets the name that should be displayed in the file list.
        /// </summary>
        public override string Name
        {
            get
            {
                return this.databaseFile.Filename;
            }
        }

        /// <summary>
        /// Gets the colour that should be used for the text of the Name in the file list.
        /// </summary>
        public override string ForegroundColour
        {
            get
            {
                return (this.localFilePath != null ? Colors.Black : Colors.DodgerBlue).ToString();
            }
        }

        /// <summary>
        /// Gets the image that should be displayed next to the name in the file list.
        /// </summary>
        public override ImageSource ImageUrl
        {
            get
            {
                string resourceName = this.IsReadOnly ? "LockedFileImageSource" : "NormalFileImageSource";
                return (ImageSource)App.Current.FindResource(resourceName);
            }
        }

        /// <summary>
        /// Gets the size of the local file connecting to the database file, if it exists.
        /// </summary>
        public override string FileSize
        {
            get
            {
                if (this.databaseFile != null)
                {
                    return null;
                }

                return Utils.BytesToString(new FileInfo(this.FullName).Length);
            }
        }

        /// <summary>
        /// Performs a data refresh of this view, if necessary.
        /// </summary>
        public override void Refresh()
        {
            List<FileBackup> fileBackups = this.ParentConnectionView.Connection.ParentModel.FileBackupLog.GetBackupsForFile(this.FullName);
            if (fileBackups != null)
            {
                this.FileBackupList = new ObservableCollection<FileBackup>(fileBackups.OrderByDescending(backup => backup.DateCreated));
            }
        }
    }
}
