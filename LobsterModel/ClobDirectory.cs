//-----------------------------------------------------------------------
// <copyright file="ClobDirectory.cs" company="marshl">
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
        /// Initializes a new instance of the <see cref="ClobDirectory"/> class
        /// </summary>
        /// <param name="clobType">The ClobType that points to this directory.</param>
        public ClobDirectory(ClobType clobType)
        {
            this.ClobType = clobType;
        }

        /// <summary>
        /// The ClobType that controls this directory
        /// </summary>
        public ClobType ClobType { get; private set; }

        /// <summary>
        /// The list of files that Lobster has found on the database for this directory
        /// </summary>
        public List<DBClobFile> DatabaseFileList { get; set; }

        /// <summary>
        /// Gets all editable files within this directory, and stores them in the given list.
        /// </summary>
        /// <param name="workingFileList">The list of files to populate.</param>
        public void GetWorkingFiles(ref List<string> workingFileList)
        {
            string directoryPath = Path.Combine(this.ClobType.ParentConnection.Config.CodeSource, this.ClobType.Directory);
            string[] files = Directory.GetFiles(directoryPath, ".", SearchOption.AllDirectories);
            workingFileList.AddRange(files);
        }

        /// <summary>
        /// FInds and returns the database file that matches the given local file.
        /// </summary>
        /// <param name="fullpath">The path of the file to find the database file for.</param>
        /// <returns>The database file, if it exists.</returns>
        public DBClobFile GetDatabaseFileForFullpath(string fullpath)
        {
            string filename = Path.GetFileName(fullpath).ToLower();
            return this.DatabaseFileList.Find(x => x.Filename.ToLower() == filename);
        }

        /// <summary>
        /// Returns whether the given file exists within the file system directory for this ClobDirectory.
        /// </summary>
        /// <param name="fullpath">The file to test for.</param>
        /// <returns>Whether the file is in this directory or not.</returns>
        public bool IsLocalFileInDirectory(string fullpath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(this.ClobType.Fullpath);
            return Array.Exists(dirInfo.GetFiles(".", SearchOption.AllDirectories), x => x.FullName == fullpath);
        }
    }
}
