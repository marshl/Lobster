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
    public class ClobDirectory
    {
        /// <summary>
        /// The event for when a mime type is needed.
        /// </summary>
        public event EventHandler<ClobDirectoryFileChangeEventArgs> FileChangeEvent;


        private FileSystemWatcher fileWatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClobDirectory"/> class
        /// </summary>
        /// <param name="clobType">The ClobType that points to this directory.</param>
        public ClobDirectory(DirectoryInfo codeSourceDirectory, ClobType clobType)
        {
            this.ClobType = clobType;
            this.directory = new DirectoryInfo(Path.Combine(codeSourceDirectory.FullName, this.ClobType.Directory));

            try
            {
                this.fileWatcher = new FileSystemWatcher(this.directory.FullName);
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

        public class ClobDirectoryFileChangeEventArgs : EventArgs
        {
            public ClobDirectoryFileChangeEventArgs(ClobDirectory clobDir, FileSystemEventArgs args)
            {
                this.ClobDir = clobDir;
                this.Args = args;
            }

            public ClobDirectory ClobDir { get; }
            public FileSystemEventArgs Args { get; }
        }

        private void OnFileSystemEvent(object sender, FileSystemEventArgs eventArgs)
        {
            var handler = this.FileChangeEvent;

            if (handler != null)
            {
                var args = new ClobDirectoryFileChangeEventArgs(this, eventArgs);
                handler(this, args);
            }
        }

        public DirectoryInfo directory { get; }

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
        /// <param name="connection">The connection parent of this directory.</param>
        /// <param name="fullpath">The file to test for.</param>
        /// <returns>Whether the file is in this directory or not.</returns>
        public bool IsLocalFileInDirectory(string fullpath)
        {
            return Array.Exists(this.directory.GetFiles(".", SearchOption.AllDirectories), x => x.FullName.Equals(fullpath, StringComparison.OrdinalIgnoreCase));
        }
    }
}
