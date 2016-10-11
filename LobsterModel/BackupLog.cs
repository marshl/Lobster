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
    using System.Diagnostics;
    using System.IO;
    using Properties;

    /// <summary>
    /// The log for when a backup file is created.
    /// </summary>
    public static class BackupLog
    {
        /// <summary>
        /// Adds a backup for the given local file and backup file.
        /// </summary>
        /// <param name="startingDirectory">The root directory of the CodeSource folder.</param>
        /// <param name="originalFilename">The file to backup.</param>
        /// <returns>The file where the backup can be stored (not created).</returns>
        public static FileInfo AddBackup(string startingDirectory, string originalFilename)
        {
            DirectoryInfo dirInfo = GetBackupDirectoryForFile(startingDirectory, originalFilename);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            string filename = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}" + Path.GetExtension(originalFilename);
            string fullpath = Path.Combine(dirInfo.FullName, filename);
            return new FileInfo(fullpath);
        }

        /// <summary>
        /// Gets a list of backups for the given filename.
        /// </summary>
        /// <param name="rootDirectory">The root directory of thte CodeSource folder.</param>
        /// <param name="filename">The file to find the backups for.</param>
        /// <returns>The list of backups for the given file.</returns>
        public static List<FileBackup> GetBackupsForFile(string rootDirectory, string filename)
        {
            DirectoryInfo dirInfo = GetBackupDirectoryForFile(rootDirectory, filename);
            List<FileBackup> result = new List<FileBackup>();
            if (!dirInfo.Exists)
            {
                return result;
            }

            foreach (FileInfo child in dirInfo.GetFiles())
            {
                result.Add(new FileBackup(filename, child.FullName));
            }

            return result;
        }

        /// <summary>
        /// Deletes backup files that are older than the given number of days.
        /// </summary>
        /// <param name="daysOld">Files older than this number of days are deleted.</param>
        public static void DeleteOldBackupFiles(int daysOld)
        {
            DirectoryInfo backupDir = new DirectoryInfo(Settings.Default.BackupDirectory);
            if (!backupDir.Exists)
            {
                return;
            }

            foreach (FileInfo file in backupDir.GetFiles("*", SearchOption.AllDirectories))
            {
                if (file.LastWriteTime < DateTime.Now.AddDays(-daysOld))
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (IOException ex)
                    {
                        MessageLog.LogError($"An exception occurred when attempting to delete the out of date backup file {file.FullName}: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Finds the directory that would be used to store backups for the given file.
        /// This does not create the directory if it doesn't already exist.
        /// </summary>
        /// <param name="startingDirectory">The CodeSource directory the file is stored under.</param>
        /// <param name="filename">The file that is being backed up.</param>
        /// <returns>The directory where the backup would be stored.</returns>
        private static DirectoryInfo GetBackupDirectoryForFile(string startingDirectory, string filename)
        {
            Debug.Assert(new Uri(startingDirectory).IsBaseOf(new Uri(filename)), "The file must be a child of the starting directory.");
            DirectoryInfo backupDir = new DirectoryInfo(Settings.Default.BackupDirectory);
            if (!backupDir.Exists)
            {
                backupDir.Create();
            }

            Uri baseUri = new Uri(startingDirectory);
            Uri fileUri = new Uri(filename);
            Uri relativeUri = baseUri.MakeRelativeUri(fileUri);
            string backupDirectory = Path.Combine(backupDir.FullName, relativeUri.OriginalString);
            DirectoryInfo dirInfo = new DirectoryInfo(backupDirectory);
            return dirInfo;
        }
    }
}
