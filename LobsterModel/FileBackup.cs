//-----------------------------------------------------------------------
// <copyright file="FileBackup.cs" company="marshl">
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
//     'We still remember, we who dwell
//      In this far land beneath the trees
//      The starlight on the Western Seas.'
//      
//      [ _The Lord of the Rings_, VI/ix: "The Grey Havens"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.IO;

    /// <summary>
    /// A single copy of a backed-up file.
    /// </summary>
    public class FileBackup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileBackup"/> class.
        /// </summary>
        /// <param name="originalFIlename">The name of the original file.</param>
        /// <param name="backupFilename">The name of the backup file.</param>
        public FileBackup(string originalFIlename, string backupFilename)
        {
            this.OriginalFilename = originalFIlename;
            this.BackupFilename = backupFilename;
            this.DateCreated = new FileInfo(this.BackupFilename).LastWriteTime;
        }

        /// <summary>
        /// Gets the name of the original file.
        /// </summary>
        public string OriginalFilename { get; }

        /// <summary>
        /// Gets the name of the backup file.
        /// </summary>
        public string BackupFilename { get; }

        /// <summary>
        /// Gets the time the backup was created.
        /// </summary>
        public DateTime DateCreated { get; }
    }
}
