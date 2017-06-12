//-----------------------------------------------------------------------
// <copyright file="DirectoryWatcher.cs" company="marshl">
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
//      For the moment there was no whispering or movement among the branches; 
//      but they all got an uncomfortable feeling that they were being watched 
//      with disapproval, deepening to dislike and even enmity.
//
//      [ _The Lord of the Rings_, I/vi: "The Old Forest"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class DirectoryWatcher
    {
        /// <summary>
        /// The file system event watcher for changes in this directory.
        /// </summary>
        private FileSystemWatcher fileWatcher;

        public DirectoryWatcher(string codeSourceDirectory, DirectoryDescriptor descriptor)
        {
            this.Descriptor = descriptor;
            this.DirectoryPath = Path.Combine(codeSourceDirectory, this.Descriptor.DirectoryName);

            if (!Directory.Exists(this.DirectoryPath))
            {
                throw new ClobTypeLoadException($"The ClobDirectory {this.DirectoryPath} could not be found.");
            }

            try
            {
                this.fileWatcher = new FileSystemWatcher(this.DirectoryPath);
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

            this.GetFiles();
        }

        public string DirectoryPath { get; }

        /// <summary>
        /// The event for when a mime type is needed.
        /// </summary>
        public event EventHandler<DirectoryWatcherFileChangeEventArgs> FileChangeEvent;

        /// <summary>
        /// Gets the ClobType that controls this directory
        /// </summary>
        public DirectoryDescriptor Descriptor { get; }

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
                var args = new DirectoryWatcherFileChangeEventArgs(this, eventArgs);
                handler(this, args);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        /// <param name="disposing">Whether this object is being disposed or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.fileWatcher != null)
            {
                this.fileWatcher.EnableRaisingEvents = false;
                this.fileWatcher.Dispose();
                this.fileWatcher = null;
            }
        }

        public void GetFiles()
        {
            List<string> files = SearchRule.GetFiles(this.DirectoryPath, this.Descriptor.SearchRules);
            List<string> directories = SearchRule.GetDirectories(this.DirectoryPath, this.Descriptor.SearchRules);

            this.RootDirectory = new WatchedDirectory(this.DirectoryPath, null);
            this.PopulateWatchedDirectory(this.RootDirectory, files, directories);
        }

        private void PopulateWatchedDirectory(WatchedDirectory watchedDir, List<string> validFiles, List<string> validDirectories)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(watchedDir.FilePath);
            if (!dirInfo.Exists)
            {
                return;
            }

            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                if (!validFiles.Contains(fileInfo.FullName))
                {
                    continue;
                }

                WatchedFile newFile = new WatchedFile(fileInfo.FullName, watchedDir);
                watchedDir.ChildNodes.Add(newFile);
            }

            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                if (!validDirectories.Contains(dir.FullName))
                {
                    continue;
                }

                WatchedDirectory newDir = new WatchedDirectory(dir.FullName, watchedDir);
                watchedDir.ChildNodes.Add(newDir);

                this.PopulateWatchedDirectory(newDir, validFiles, validDirectories);
            }
        }

        public WatchedDirectory RootDirectory { get; private set; }
    }
}
