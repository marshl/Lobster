//-----------------------------------------------------------------------
// <copyright file="ClobNode.cs" company="marshl">
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
//      All about them as they lay hung the darkness, hollow and immense,
//      and they were oppressed by the loneliness and vastness of the dolven halls and
//      endlessly branching stairs and passages.
//
//      [ _The Lord of the Rings_, II/iv: "A Journey in the Dark"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// A ClobNode is a single directory in the file system that is controlled by a ClobType.
    /// The root ClobNode of a ClobType is stored in the corresponding ClobDirectory.
    /// Sub directories are stored in the ClobNode as additional ClobNodes.
    /// </summary>
    [Obsolete]
    public class ClobNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClobNode"/> class.
        /// </summary>
        /// <param name="dirInfo">The directory this node connects to.</param>
        /// <param name="baseDirectory">The directory object that contains this ClobNode.</param>
        public ClobNode(DirectoryInfo dirInfo, ClobDirectory baseDirectory)
        {
            this.DirInfo = dirInfo;
            this.BaseClobDirectory = baseDirectory;
        }

        /// <summary>
        /// The file system DirectoryInfo that corresponds to this node.
        /// </summary>
        public DirectoryInfo DirInfo { get; private set; }

        /// <summary>
        /// The root directory of this node. All nodes under a directory will point back to that same directory.
        /// </summary>
        public ClobDirectory BaseClobDirectory { get; private set; }

        /// <summary>
        /// A list of children under this node. Each one corresponds to a subdirectory under this node's directory.
        /// </summary>
        public List<ClobNode> ChildNodes { get; private set; } = new List<ClobNode>();

        /// <summary>
        /// A mapping of the names of the files in the file system directory to the internal ClobFile that represents it.
        /// The name of the file is converted to lower case before being used as the key.
        /// </summary>
        public Dictionary<string, ClobFile> ClobFileMap { get; set; }

        /// <summary>
        /// The watcher that tracks all change/create/delete/rename events for this folder (not including subdirectories). 
        /// </summary>
       // private FileSystemWatcher FileWatcher { get; set; }

        /// <summary>
        /// The watcher that tracks all attribute change events (such as file locks) for this folder (not including subdirectories).
        /// </summary>
        //private FileSystemWatcher FileAttributeWatcher { get; set; }

        /// <summary>
        /// This adds a file on the file system to the file dictionary on this node.
        /// </summary>
        /// <param name="filepath">The file to add.</param>
        public void AddLocalClobFile(string filepath)
        {
            ClobFile clobFile = new ClobFile(this);
            clobFile.LocalFile = new LocalClobFile(filepath);

            this.ClobFileMap.Add(Path.GetFileName(filepath).ToLower(), clobFile);
            this.BaseClobDirectory.FileList.Add(clobFile);
        }

        /// <summary>
        /// Adds all files in the directory for this node to the file list, then does the same for any child nodes.
        /// </summary>
        public void RepopulateFileLists_r()
        {
            this.ClobFileMap = new Dictionary<string, ClobFile>();
            foreach (FileInfo fileInfo in this.DirInfo.GetFiles())
            {
                this.AddLocalClobFile(fileInfo.FullName);
            }

            foreach (ClobNode child in this.ChildNodes)
            {
                child.RepopulateFileLists_r();
            }
        }

        /// <summary>
        /// Populates a list of all files in this directory, and all files under this directory, that are unlocked.
        /// </summary>
        /// <param name="fileList">The file list to populate</param>
        public void GetWorkingFiles(ref List<ClobFile> fileList)
        {
            foreach (KeyValuePair<string, ClobFile> pair in this.ClobFileMap)
            {
                ClobFile clobFile = pair.Value;
                if (clobFile.IsEditable)
                {
                    fileList.Add(pair.Value);
                }
            }

            foreach (ClobNode node in this.ChildNodes)
            {
                node.GetWorkingFiles(ref fileList);
            }
        }

        /// <summary>
        /// The callback for when a file has its content changed.
        /// This pushes the file to the database if it is synchronised and editable.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments for the event.</param>
        /*private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Ensure that the file changed exists and is not a directory
            if (!File.Exists(e.FullPath))
            {
                return;
            }

            ClobFile clobFile;
            if (this.ClobFileMap.TryGetValue(e.Name.ToLower(), out clobFile))
            {
                if (clobFile.IsSynced && clobFile.IsEditable)
                {
                    // This is a long operation, so the file watchers need to be disabled to prevent multiple change hits.
                    this.SetFileWatchersEnabled(false);
                    this.BaseClobDirectory.ClobType.ParentConnection.ParentModel.SendUpdateClobMessage(clobFile);
                    this.SetFileWatchersEnabled(true);
                }
            }
        }*/

        /// <summary>
        /// The callback for when a file is created.
        /// This clears out the Lobster file lists and traverses the directory tree again.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments for the event.</param>
        /*private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            this.SetFileWatchersEnabled(false);
            this.BaseClobDirectory.GetLocalFiles();
            this.SetFileWatchersEnabled(true);
        }*/

        /// <summary>
        /// The callback for when a file is deleted
        /// This clears out the Lobster file lists and traverses the directory tree again.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments for the event.</param>
        /*private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            this.SetFileWatchersEnabled(false);
            this.BaseClobDirectory.GetLocalFiles();
            this.SetFileWatchersEnabled(true);
        }*/

        /// <summary>
        /// The callback for when a file is renamed.
        /// This clears out the Lobster file lists and traverses the directory tree again.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments for the event.</param>
        /*private void OnFileRenamed(object sender, FileSystemEventArgs e)
        {
            this.SetFileWatchersEnabled(false);
            this.BaseClobDirectory.GetLocalFiles();
            this.SetFileWatchersEnabled(true);
        }*/
    }
}
