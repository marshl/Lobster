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

    public class WatchedFile : WatchedNode
    {
        public DateTime LastUpdateDateTime { get; set; } = DateTime.MinValue;

        public List<FileBackup> FileBackupList { get; private set; }

        public WatchedFile(string path, WatchedDirectory parent) : base(path, parent)
        {
            FileInfo fileInfo = new FileInfo(this.Path);

            if (fileInfo.Exists)
            {
                this.FileLength = fileInfo.Length;
                this.IsReadOnly = fileInfo.IsReadOnly;
            }
        }

        /// <summary>
        /// Whether the file this node represents is read only.
        /// </summary>
        public bool IsReadOnly { get; private set; }

        public long FileLength { get; private set; }

        /// <summary>
        /// Refreshes the cache of backup files from the backup directory.
        /// </summary>
        public void RefreshBackupList(CodeSourceConfig codeSource)
        {
            List<FileBackup> fileBackups = BackupLog.GetBackupsForFile(codeSource.CodeSourceDirectory, this.Path);
            this.FileBackupList = new List<FileBackup>(fileBackups.OrderByDescending(backup => backup.DateCreated));
        }
    }
}
