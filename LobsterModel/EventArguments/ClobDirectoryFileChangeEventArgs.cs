//-----------------------------------------------------------------------
// <copyright file="ClobDirectoryFileChangeEventArgs.cs" company="marshl">
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
namespace LobsterModel
{
    using System;
    using System.IO;

    /// <summary>
    /// The event arguments for a change in any file within a <see cref="ClobDirectory"/>.
    /// </summary>
    [Obsolete]
    public class ClobDirectoryFileChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClobDirectoryFileChangeEventArgs"/> class.
        /// </summary>
        /// <param name="clobDir">The directory of the event.</param>
        /// <param name="args">The file system event arguments.</param>
        public ClobDirectoryFileChangeEventArgs(ClobDirectory clobDir, FileSystemEventArgs args)
        {
            this.ClobDir = clobDir;
            this.Args = args;
        }

        /// <summary>
        /// Gets the directory the event was raised in.
        /// </summary>
        public ClobDirectory ClobDir { get; }

        /// <summary>
        /// Gets the file system event arguments.
        /// </summary>
        public FileSystemEventArgs Args { get; }
    }
}
