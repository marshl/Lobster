//-----------------------------------------------------------------------
// <copyright file="DirectoryWatcherView.cs" company="marshl">
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
namespace LobsterWpf.ViewModels
{
    using System;
    using LobsterModel;

    /// <summary>
    /// The view for the <see cref="DirectoryWatcher"/> object.
    /// </summary>
    public class DirectoryWatcherView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryWatcherView"/> class.
        /// </summary>
        /// <param name="watcher">The watcher model.</param>
        public DirectoryWatcherView(DirectoryWatcher watcher)
        {
            this.BaseWatcher = watcher;
        }

        /// <summary>
        /// Gets the base watcher model
        /// </summary>
        public DirectoryWatcher BaseWatcher { get; }
        
        /// <summary>
        /// Gets the name of the base watcher.
        /// </summary>
        public string Name
        {
            get
            {
                return this.BaseWatcher.Descriptor.Name;
            }
        }

        /// <summary>
        /// Gets the directory name of the directory descriptor.
        /// </summary>
        public string Directory
        {
            get
            {
                return this.BaseWatcher.Descriptor.DirectoryName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the directory of this clob directory exists on the files system or not.
        /// </summary>
        public bool DirectoryExists
        {
            get
            {
                return System.IO.Directory.Exists(this.BaseWatcher.DirectoryPath);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this directory allows files to be deleted
        /// </summary>
        public bool CanDelete
        {
            get
            {
                return !string.IsNullOrEmpty(this.BaseWatcher.Descriptor.DeleteStatement);
            }
        }
    }
}
