//-----------------------------------------------------------------------
// <copyright file="DatabaseConnection.cs" company="marshl">
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
//      'What did the Men of old use them for?' asked Pippin, ...
//      'To see far off, and to converse in thought with one another,' said Gandalf
//
//      [ _The Lord of the Rings_, III/xi: "The Palantir"]
// 
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Properties;

    /// <summary>
    /// Used to define how a database should be connected to, and where the ClobTypes are stored for it.
    /// </summary>
    [XmlType("DatabaseConfig")]
    public class DatabaseConnection : ICloneable
    {
        public DatabaseConnection(LobsterModel parentModel, DatabaseConfig config)
        {
            this.ParentModel = parentModel;
            this.Config = config;

            this.fileWatcher = new FileSystemWatcher(this.Config.CodeSource);
            this.fileWatcher.IncludeSubdirectories = true;
            this.fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Attributes;
            this.fileWatcher.Changed += new FileSystemEventHandler(this.OnFileChangeEvent);
            this.fileWatcher.Created += new FileSystemEventHandler(this.OnFileChangeEvent);
            this.fileWatcher.Deleted += new FileSystemEventHandler(this.OnFileChangeEvent);

            this.fileWatcher.EnableRaisingEvents = true;
        }

        private void OnFileChangeEvent(object sender, FileSystemEventArgs e)
        {
            MessageLog.LogInfo($"File change event of type {e.ChangeType} for file {e.FullPath}");
            lock (this.fileEventStack)
            {
                this.fileEventStack.Push(e);

                if (this.fileEventThread == null || this.fileEventThread.ThreadState != ThreadState.Running)
                {
                    MessageLog.LogInfo("Starting file event thread");
                    this.fileEventThread = new Thread(new ThreadStart(this.ProcessFileEvents));
                    this.fileEventThread.Start();
                }
            }
        }

        private void ProcessFileEvents()
        {
            while (this.fileEventStack.Count > 0)
            {

                FileSystemEventArgs e;
                lock (this.fileEventStack)
                {
                    e = this.fileEventStack.Pop();
                }
                MessageLog.LogInfo($"Processing file event of type {e.ChangeType} for file {e.FullPath}");
                Console.WriteLine(e.FullPath);

                FileInfo fileInfo = new FileInfo(e.FullPath);

                if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    if (!fileInfo.Exists || fileInfo.IsReadOnly)
                    {
                        return;
                    }

                    DBClobFile clobFile = this.FindDatabaseFileForFullpath(e.FullPath);

                    if (clobFile == null)
                    {
                        return;
                    }

                    this.ParentModel.SendUpdateClobMessage(e.FullPath);
                }
            }
        }

        public DBClobFile FindDatabaseFileForFullpath(string fullpath)
        {
            foreach (KeyValuePair<ClobType, ClobDirectory> pair in this.ClobTypeToDirectoryMap)
            {
                DBClobFile clobFile = pair.Value.GetDatabaseFileForFullpath(fullpath);
                if (clobFile != null)
                {
                    return clobFile;
                }
            }
            return null;
        }

        public ClobDirectory GetClobDirectoryForFile(string fullpath)
        {
            foreach (KeyValuePair<ClobType, ClobDirectory> pair in this.ClobTypeToDirectoryMap)
            {
                if (fullpath.Contains(Path.Combine(this.Config.CodeSource, pair.Value.ClobType.Directory)))
                {
                    return pair.Value;
                }
            }
            return null;
        }

        private Thread fileEventThread;

        private FileSystemWatcher fileWatcher;

        private Stack<FileSystemEventArgs> fileEventStack = new Stack<FileSystemEventArgs>();

        public DatabaseConfig Config { get; set; }

        /// <summary>
        /// The Lobster model that is the parent of this connection.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public LobsterModel ParentModel { get; private set; }

        /// <summary>
        /// The name of the file where this was loaded from.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public string ConfigFilepath { get; set; }

        /// <summary>
        /// The list of Clob Types that are loaded from the files location in the directory defined by ClobTypeDir.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public List<ClobType> ClobTypeList { get; set; }

        /// <summary>
        /// A mapping of the ClobTypes to the ClobDirectories that are created when the database is connected to.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public Dictionary<ClobType, ClobDirectory> ClobTypeToDirectoryMap { get; set; }

        /// <summary>
        /// Loads each of the xml files in the ClobTypeDir (if they are valid).
        /// </summary>
        public void LoadClobTypes()
        {
            this.ClobTypeList = new List<ClobType>();
            DirectoryInfo dirInfo = new DirectoryInfo(this.Config.ClobTypeDir);
            if (!dirInfo.Exists)
            {
                MessageBox.Show(this.Config.ClobTypeDir + " could not be found.", "ClobType Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                MessageLog.LogWarning("The directory " + dirInfo + " could not be found when loading connection " + this.Config.Name);
                return;
            }

            foreach (FileInfo file in dirInfo.GetFiles("*.xml"))
            {
                try
                {
                    MessageLog.LogInfo("Loading ClobType file " + file.FullName);
                    ClobType clobType = Common.DeserialiseXmlFileUsingSchema<ClobType>(file.FullName, Settings.Default.ClobTypeSchemaFilename);

                    clobType.Initialise(this);
                    clobType.FilePath = file.FullName;
                    this.ClobTypeList.Add(clobType);
                }
                catch (Exception e)
                {
                    if (e is InvalidOperationException || e is XmlException || e is XmlSchemaValidationException || e is IOException)
                    {
                        MessageBox.Show(
                            "The ClobType " + file.FullName + " failed to load. Check the log for more information.",
                            "ClobType Load Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1);
                        MessageLog.LogError("An error occurred when loading the ClobType " + file.Name + " " + e);
                        continue;
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Creates the ClobDirectories for each of the ClobTypes in this connection.
        /// </summary>
        public void PopulateClobDirectories()
        {
            this.ClobTypeToDirectoryMap = new Dictionary<ClobType, ClobDirectory>();
            foreach (ClobType clobType in this.ClobTypeList)
            {
                ClobDirectory clobDir = new ClobDirectory(clobType);
                this.ClobTypeToDirectoryMap.Add(clobType, clobDir);
            }
        }

        /// <summary>
        /// Finds all currently unlocked files in all clob types and populates he input list with them.
        /// </summary>
        /// <param name="workingFileList">The file list to populate.</param>
        public void GetWorkingFiles(ref List<string> workingFileList)
        {
            foreach (KeyValuePair<ClobType, ClobDirectory> pair in this.ClobTypeToDirectoryMap)
            {
                pair.Value.GetWorkingFiles(ref workingFileList);
            }
        }

        /// <summary>
        /// Creates a deep copy of this DatabaseConnection and returns it.
        /// </summary>
        /// <returns>The new clone.</returns>
        public object Clone()
        {
            DatabaseConnection other = new DatabaseConnection(this.ParentModel, (DatabaseConfig)this.Config.Clone());

            other.ConfigFilepath = this.ConfigFilepath;
            other.ParentModel = this.ParentModel;

            other.ClobTypeList = new List<ClobType>();
            if (this.ClobTypeList == null)
            {
                this.ClobTypeList = new List<ClobType>();
            }

            foreach (ClobType clobType in this.ClobTypeList)
            {
                other.ClobTypeList.Add((ClobType)clobType.Clone());
            }

            return other;
        }
    }
}
