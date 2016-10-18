//-----------------------------------------------------------------------
// <copyright file="ClobDirectory.cs" company="marshl">
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

    /// <summary>
    /// A ClobDirectory maps to a single ClobType, and represents the directory on the file system where the files for that ClobType are found.
    /// </summary>
    [Obsolete]
    public class ClobDirectory
    {
        /// <summary>
        /// The file system event watcher for changes in this directory.
        /// </summary>
        private FileSystemWatcher fileWatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClobDirectory"/> class
        /// </summary>
        /// <param name="codeSourceDirectory">The directory that the code source is located in.</param>
        /// <param name="clobType">The ClobType that points to this directory.</param>
        public ClobDirectory(string codeSourceDirectory, ClobType clobType)
        {
            this.ClobType = clobType;
            this.Directory = new DirectoryInfo(Path.Combine(codeSourceDirectory, this.ClobType.Directory));

            if (!this.Directory.Exists)
            {
                throw new ClobTypeLoadException($"The ClobDirectory {this.Directory.FullName} could not be found.");
            }

            try
            {
                this.fileWatcher = new FileSystemWatcher(this.Directory.FullName);
            }
            catch (ArgumentException ex)
            {
                throw new ClobTypeLoadException("An error occurred when creating the FileSystemWatcher", ex);
            }

            this.fileWatcher.IncludeSubdirectories = true;
            this.fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Attributes;
            this.fileWatcher.Changed += new FileSystemEventHandler(this.OnFileSystemEvent);
            this.fileWatcher.Created += new FileSystemEventHandler(this.OnFileSystemEvent);
            this.fileWatcher.Deleted += new FileSystemEventHandler(this.OnFileSystemEvent);
            this.fileWatcher.Renamed += new RenamedEventHandler(this.OnFileSystemEvent);

            this.fileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// The event for when a mime type is needed.
        /// </summary>
        public event EventHandler<ClobDirectoryFileChangeEventArgs> FileChangeEvent;

        /// <summary>
        /// Gets the directory informaton for this directory.
        /// </summary>
        public DirectoryInfo Directory { get; }

        /// <summary>
        /// Gets the ClobType that controls this directory
        /// </summary>
        public ClobType ClobType { get; }

        /// <summary>
        /// Gets or sets the list of files that Lobster has found on the database for this directory
        /// </summary>
        public List<DBClobFile> DatabaseFileList { get; set; }

        /// <summary>
        /// Finds and returns the database file that matches the given local file.
        /// </summary>
        /// <param name="fullpath">The path of the file to find the database file for.</param>
        /// <returns>The database file, if it exists.</returns>
        public DBClobFile GetDatabaseFileForFullpath(string fullpath)
        {
            string filename = Path.GetFileName(fullpath);
            return this.DatabaseFileList.Find(x => x.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns whether the given file exists within the file system directory for this ClobDirectory.
        /// </summary>
        /// <param name="fullpath">The file to test for.</param>
        /// <returns>Whether the file is in this directory or not.</returns>
        public bool IsLocalFileInDirectory(string fullpath)
        {
            SearchOption so = this.ClobType.IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Array.Exists(this.Directory.GetFiles(".", so), x => x.FullName.Equals(fullpath, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// The event listener for the file system watcher.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The file system event arguments.</param>
        private void OnFileSystemEvent(object sender, FileSystemEventArgs eventArgs)
        {
            var handler = this.FileChangeEvent;

            if (handler != null)
            {
                var args = new ClobDirectoryFileChangeEventArgs(this, eventArgs);
                handler(this, args);
            }
        }
    }
}
