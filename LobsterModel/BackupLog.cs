//-----------------------------------------------------------------------
// <copyright file="BackupLog.cs" company="marshl">
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
    using System.IO;
    using System.Linq;
    using System.Text;
    using Properties;

    /// <summary>
    /// The log for when a backup file is created.
    /// </summary>
    public static class BackupLog
    {
        /// <summary>
        /// The mapping of local files to their backups.
        /// </summary>
        //private Dictionary<string, FileBackup> backupDictionary = new Dictionary<string, FileBackup>();

        /// <summary>
        /// Adds a backup for the given local file and backup file.
        /// </summary>
        /// <param name="originalFilename">The original file.</param>
        /// <param name="backupFilename">The backed up file.</param>
        public static FileInfo AddBackup(string startingDirectory, string originalFilename)
        {
            //FileBackup fileBackup;
            //if (!this.backupDictionary.TryGetValue(originalFilename, out fileBackup))
            //{

            DirectoryInfo dirInfo = GetBackupDirectoryForFile(startingDirectory, originalFilename);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            //fileBackup = new FileBackup(originalFilename, dirInfo.FullName);
            // }

            string filename = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now) + Path.GetExtension(originalFilename);
            string fullpath = Path.Combine(dirInfo.FullName, filename);
            return new FileInfo(fullpath);

            //return fileBackup.CreateBackup();
        }

        private static DirectoryInfo GetBackupDirectoryForFile(string startingDirectory, string filename)
        {
            DirectoryInfo backupDir = new DirectoryInfo(Settings.Default.BackupDirectory);
            if (!backupDir.Exists)
            {
                backupDir.Create();
            }

            Uri baseUri = new Uri(startingDirectory);
            Uri fileUri = new Uri(filename);
            Uri relativeUri = baseUri.MakeRelativeUri(fileUri);
            //Uri backupDirectory = new Uri(new Uri(backupDir.FullName), relativeUri);
            string backupDirectory = Path.Combine(backupDir.FullName, relativeUri.OriginalString);
            DirectoryInfo dirInfo = new DirectoryInfo(backupDirectory);
            return dirInfo;
        }

        /// <summary>
        /// Gets a list of backups for the given filename.
        /// </summary>
        /// <param name="filename">The file to find the backups for.</param>
        /// <returns>The list of backups for the given file.</returns>
        public static List<FileBackup> GetBackupsForFile(string startingDirectory, string filename)
        {
            DirectoryInfo dirInfo = GetBackupDirectoryForFile(startingDirectory, filename);
            List<FileBackup> result = new List<FileBackup>();
            if ( !dirInfo.Exists)
            {
                return result;
            }

            foreach (FileInfo child in dirInfo.GetFiles())
            {
                result.Add(new FileBackup(filename, child.FullName));
            }
            
            ///this.backupDictionary.TryGetValue(filename, out result);
            return result;
        }
    }
}
