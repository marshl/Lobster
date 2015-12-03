//-----------------------------------------------------------------------
// <copyright file="BackupLog.cs" company="marshl">
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
//
//      'I wish it need not have happened in my time,' said Frodo.
//      'So do I,' said Gandalf, 'and so do all who live to see such times.
//       But that is not for them to decide. All we have to decide is 
//       what to do with the time that is given, us.'
//
//      [ _The Lord of the Rings_, I/ii: "The Showdow of the Past"]
//
//-----------------------------------------------------------------------

namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The log for when a backup file is created.
    /// </summary>
    public class BackupLog
    {
        /// <summary>
        /// The mapping of local files to their backups.
        /// </summary>
        private Dictionary<string, List<FileBackup>> backupDictionary = new Dictionary<string, List<FileBackup>>();

        /// <summary>
        /// Adds a backup for the given local file and backup file.
        /// </summary>
        /// <param name="originalFilename">The original file.</param>
        /// <param name="backupFilename">The backed up file.</param>
        public void AddBackup(string originalFilename, string backupFilename)
        {
            FileBackup file = new FileBackup(originalFilename, backupFilename);

            List<FileBackup> files = this.GetBackupsForFile(originalFilename);
            if (files == null)
            {
                files = new List<FileBackup>();
                this.backupDictionary.Add(originalFilename, files);
            }

            files.Add(file);
        }

        /// <summary>
        /// Gets a list of backups for the given filename.
        /// </summary>
        /// <param name="filename">The file to find the backups for.</param>
        /// <returns>The list of backups for the given file.</returns>
        public List<FileBackup> GetBackupsForFile(string filename)
        {
            List<FileBackup> result;
            this.backupDictionary.TryGetValue(filename, out result);
            return result;
        }
    }
}
