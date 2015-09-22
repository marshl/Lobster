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
namespace Lobster
{
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
        /// The list all of the files in every node under this directory
        /// </summary>
        public List<ClobFile> FileList { get; private set; }

        /// <summary>
        /// The list of files that Lobster has found on the database for this directory
        /// </summary>
        public List<DBClobFile> DatabaseFileList { get; set; }

        /// <summary>
        /// The list of files that don't have a local file connection. These files are not found in FileList.
        /// </summary>
        public List<ClobFile> DatabaseOnlyFiles { get; private set; }

        /// <summary>
        /// The ClobNOde that represents this directory on the file system. The root node can have any number of child nodes, representing subfolders under the main directory.
        /// </summary>
        public ClobNode RootClobNode { get; set; }

        /// <summary>
        /// Recursively traverses the directory specified by the ClobType for all subdirectories.
        /// Files are found using the GetLocalFiles function.
        /// </summary>
        /// <returns>False if the directory in the ClobType could not be found, otherwise true.</returns>
        public bool BuildDirectoryTree()
        {
            DirectoryInfo info = new DirectoryInfo(Path.Combine(this.ClobType.ParentConnection.CodeSource, this.ClobType.Directory));
            if (!info.Exists)
            {
                MessageLog.LogWarning(info.FullName + " could not be found.");
                LobsterMain.OnErrorMessage("Folder not found", "Folder \"" + info.FullName + "\" could not be found for ClobType " + this.ClobType.Name);
                return false;
            }

            this.RootClobNode = new ClobNode(info, this);
            if (this.ClobType.IncludeSubDirectories)
            {
                this.PopulateClobNodeDirectories_r(this.RootClobNode);
            }

            return true;
        }

        /// <summary>
        /// Gets all editable files within this directory, and stores them in the given list.
        /// </summary>
        /// <param name="workingFileList">The list of files to populate.</param>
        public void GetWorkingFiles(ref List<ClobFile> workingFileList)
        {
            this.RootClobNode.GetWorkingFiles(ref workingFileList);
        }

        /// <summary>
        /// Finds all files in all directories under this directory, and links them to the database files.
        /// </summary>
        public void GetLocalFiles()
        {
            this.FileList = new List<ClobFile>();
            this.RootClobNode.RepopulateFileLists_r();
            this.LinkLocalAndDatabaseFiles();

            // The UI will have to be refreshed
            LobsterMain.instance.UpdateUIThread();
        }

        /// <summary>
        /// Recursively finds all directories under the given ClobNode and adds them to that node.
        /// </summary>
        /// <param name="clobNode">The ClobNode to operate on.</param>
        private void PopulateClobNodeDirectories_r(ClobNode clobNode)
        {
            DirectoryInfo[] subDirs = clobNode.DirInfo.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs)
            {
                ClobNode childNode = new ClobNode(subDir, this);
                this.PopulateClobNodeDirectories_r(childNode);
                clobNode.ChildNodes.Add(childNode);
            }
        }

        /// <summary>
        /// Links all database files to their corresponding local files.
        /// Files that are not found locally are stored as "database-only".
        /// </summary>
        private void LinkLocalAndDatabaseFiles()
        {
            // Reset the database only list
            this.DatabaseOnlyFiles = new List<ClobFile>();

            // Break any existing connections to clob files
            this.FileList.ForEach(x => x.DatabaseFile = null);

            foreach (DBClobFile file in this.DatabaseFileList)
            {
                List<ClobFile> matchingFiles = this.FileList.FindAll(x => x.LocalFile.FileInfo.Name.ToLower() == file.Filename.ToLower());

                // Link all matching local files to that database file
                if (matchingFiles.Count > 0)
                {
                    matchingFiles.ForEach(x => x.DatabaseFile = file);
                    if (matchingFiles.Count > 1)
                    {
                        MessageLog.LogWarning("Multiple local files have been found for the database file " + file.Filename + " from the table " + file.ParentTable.FullName);
                        matchingFiles.ForEach(x => MessageLog.LogWarning(x.LocalFile.FileInfo.FullName));
                        MessageLog.LogWarning("Updating any of those files will update the same database file.");
                    }
                }
                else
                {
                    // If it has no local file to link it, then add it to the database only list
                    ClobFile databaseOnlyFile = new ClobFile(this.RootClobNode);
                    databaseOnlyFile.DatabaseFile = file;
                    this.DatabaseOnlyFiles.Add(databaseOnlyFile);
                }
            }
        }
    }
}
