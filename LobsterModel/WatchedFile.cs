//-----------------------------------------------------------------------
// <copyright file="WatchedFile.cs" company="marshl">
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
//      A box without hinges, key, or lid,
//      Yet golden treasure inside is hid,
//
//      [ _The Hobbit_, V: "Riddles in the Dark"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// A subclass of <see cref="WatchedNode"/> used only for files (but not directories).
    /// </summary>
    public class WatchedFile : WatchedNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WatchedFile"/> class.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="parent">The parent directory of the file.</param>
        public WatchedFile(string path, WatchedDirectory parent) : base(path, parent)
        {
            FileInfo fileInfo = new FileInfo(this.FilePath);

            if (fileInfo.Exists)
            {
                this.FileLength = fileInfo.Length;
                this.IsReadOnly = fileInfo.IsReadOnly;
            }
        }

        /// <summary>
        /// Gets or sets the last time that this file was updated.
        /// </summary>
        public DateTime LastUpdateDateTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets the list of backup files that have ever been taken for this file.
        /// </summary>
        public List<FileBackup> FileBackupList { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the file this node represents is read only.
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Gets the number of bytes in this file.
        /// </summary>
        public long FileLength { get; private set; }

        /// <summary>
        /// Refreshes the cache of backup files from the backup directory.
        /// </summary>
        /// <param name="codeSource">The configuration for this file.</param>
        public void RefreshBackupList(CodeSourceConfig codeSource)
        {
            List<FileBackup> fileBackups = BackupLog.GetBackupsForFile(codeSource.CodeSourceDirectory, this.FilePath);
            this.FileBackupList = new List<FileBackup>(fileBackups.OrderByDescending(backup => backup.DateCreated));
        }

        /// <summary>
        /// Gets a value indicating whether this file is stored with the database or not.
        /// </summary>
        /// <param name="connection">The connection to check syncronisation against.</param>
        /// <param name="dirWatcher">The directory watcher that this file falls within.</param>
        /// <returns>True if this file is stored in the database, otherwise false.</returns>
        public bool IsSynchonisedWithDatabase(DatabaseConnection connection, DirectoryWatcher dirWatcher)
        {
            return connection.IsFileSynchronised(dirWatcher, this);
        }
    }
}
