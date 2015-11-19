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
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Properties;

    /// <summary>
    /// Used to define how a database should be connected to, and where the ClobTypes are stored for it.
    /// </summary>
    [XmlType("DatabaseConfig")]
    public class DatabaseConnection
    {
        /// <summary>
        /// The thread used to process file events when a new event is triggered.
        /// Only a single event thread is used at any one time.
        /// </summary>
        private Thread fileEventThread;

        /// <summary>
        /// The file watcher for the entire CodeSource directory. 
        /// FIles that are not covered by any ClobTypes filtered out when processed.
        /// </summary>
        private FileSystemWatcher fileWatcher;

        /// <summary>
        /// The queue of events that have triggered. 
        /// Events are popped off and processed one at a time by the fileEventThread.
        /// </summary>
        private Queue<FileSystemEventArgs> fileEventQueue = new Queue<FileSystemEventArgs>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnection"/> class.
        /// </summary>
        /// <param name="parentModel">The model that is parent to this connection.</param>
        /// <param name="config">The configuration file to base this connection off.</param>
        public DatabaseConnection(Model parentModel, DatabaseConfig config)
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

        /// <summary>
        /// Gets the configuration file for this connection.
        /// </summary>
        public DatabaseConfig Config { get; private set; }

        /// <summary>
        /// Gets the Lobster model that is the parent of this connection.
        /// </summary>
        public Model ParentModel { get; private set; }

        /// <summary>
        /// Gets the list of ClobDirectories for each ClobType located in directory in the configuration settings.
        /// </summary>
        public List<ClobDirectory> ClobDirectoryList { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database should be automatically updated when a file is changed.
        /// </summary>
        public bool IsAutoClobEnabled { get; set; } = true;

        /// <summary>
        /// Returns the ClobDirectory that would contain the given fullpath, if applicable.
        /// Whether the file exists or not is irrelevent, only that it would be contained in the returned ClobDirectory.
        /// </summary>
        /// <param name="fullpath">The file to return the directory for.</param>
        /// <returns>The matching clobDirectory, if one exists.</returns>
        public ClobDirectory GetClobDirectoryForFile(string fullpath)
        {
            string absolutePath = Path.GetFullPath(fullpath);
            List<ClobDirectory> clobDirList = this.ClobDirectoryList.FindAll(x => absolutePath.Contains(x.ClobType.Fullpath));
            if (clobDirList.Count == 0)
            {
                throw new ClobDirectoryNotFoundForFileException();
            }
            else if (clobDirList.Count > 1)
            {
                throw new MultipleClobDirectoriesFoundForFileException();
            }

            return clobDirList[0];
        }

        /// <summary>
        /// Loads each of the xml files in the ClobTypeDir (if they are valid).
        /// </summary>
        /// <param name="errors">Any errors that are raised during loading.</param>
        public void LoadClobTypes(out List<ClobTypeLoadException> errors)
        {
            errors = new List<ClobTypeLoadException>();

            this.ClobDirectoryList = new List<ClobDirectory>();
            DirectoryInfo dirInfo = new DirectoryInfo(this.Config.ClobTypeDir);
            if (!dirInfo.Exists)
            {
                MessageLog.LogWarning("The directory " + dirInfo + " could not be found when loading connection " + this.Config.Name);
                return;
            }

            foreach (FileInfo file in dirInfo.GetFiles("*.xml"))
            {
                try
                {
                    MessageLog.LogInfo("Loading ClobType file " + file.FullName);
                    ClobType clobType = Utils.DeserialiseXmlFileUsingSchema<ClobType>(file.FullName, Settings.Default.ClobTypeSchemaFilename);

                    clobType.Initialise(this);
                    clobType.FilePath = file.FullName;

                    ClobDirectory clobDir = new ClobDirectory(clobType);
                    this.ClobDirectoryList.Add(clobDir);
                }
                catch (Exception e) when (e is InvalidOperationException || e is XmlException || e is XmlSchemaValidationException || e is IOException)
                {
                    errors.Add(new ClobTypeLoadException("ClobType load error", e));
                    MessageLog.LogError("An error occurred when loading the ClobType " + file.Name + " " + e);
                }
            }
        }

        /// <summary>
        /// Finds all currently unlocked files in all clob types and populates he input list with them.
        /// </summary>
        /// <param name="workingFileList">The file list to populate.</param>
        public void GetWorkingFiles(ref List<string> workingFileList)
        {
            foreach (ClobDirectory clobDir in this.ClobDirectoryList)
            {
                clobDir.GetWorkingFiles(ref workingFileList);
            }
        }

        /// <summary>
        /// The event raised when a file is changed within the CodeSource directory.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnFileChangeEvent(object sender, FileSystemEventArgs e)
        {
            MessageLog.LogInfo($"File change event of type {e.ChangeType} for file {e.FullPath}");

            if (!this.IsAutoClobEnabled)
            {
                MessageLog.LogInfo($"Automatic clobbing is disabled, ignoring event.");
                return;
            }

            lock (this.fileEventQueue)
            {
                this.fileEventQueue.Enqueue(e);
            }

            if (this.fileEventThread == null || this.fileEventThread.ThreadState != ThreadState.Running)
            {
                MessageLog.LogInfo("Starting file event thread");
                this.fileEventThread = new Thread(new ThreadStart(this.ProcessAllFileEvents));
                this.fileEventThread.Start();
            }
        }

        /// <summary>
        /// Used from the worked thread to process any file events on the queue.
        /// </summary>
        private void ProcessAllFileEvents()
        {
            while (this.fileEventQueue.Count > 0)
            {
                FileSystemEventArgs e;
                lock (this.fileEventQueue)
                {
                    e = this.fileEventQueue.Dequeue();
                }

                this.ProcessFileEvent(e);
            }

            MessageLog.LogInfo("File event stack empty");
        }

        /// <summary>
        /// Processes a single file event. Sending a file update to the database if necessary.
        /// </summary>
        /// <param name="e">The event to process.</param>
        private void ProcessFileEvent(FileSystemEventArgs e)
        {
            MessageLog.LogInfo($"Processing file event of type {e.ChangeType} for file {e.FullPath}");
            FileInfo fileInfo = new FileInfo(e.FullPath);

            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                if (!fileInfo.Exists)
                {
                    MessageLog.LogInfo($"File could not be found {e.FullPath}");
                    return;
                }

                ClobDirectory clobDir;
                try
                {
                    clobDir = this.GetClobDirectoryForFile(e.FullPath);
                }
                catch (Exception ex) when (ex is ClobDirectoryNotFoundForFileException || ex is MultipleClobDirectoriesFoundForFileException)
                {
                    MessageLog.LogInfo($"The file does not belong to any ClobDirectory and will be skipped {e.FullPath}");
                    return;
                }

                DBClobFile clobFile = clobDir.GetDatabaseFileForFullpath(e.FullPath);
                if (clobFile == null)
                {
                    MessageLog.LogInfo($"The file does not have a DBClobFile and will be skipped {e.FullPath}");
                    return;
                }

                if (fileInfo.IsReadOnly)
                {
                    MessageLog.LogInfo($"File is read only and will be skipped {e.FullPath}");
                    return;
                }

                MessageLog.LogInfo($"Auto-updating file {e.FullPath}");
                this.ParentModel.SendUpdateClobMessage(e.FullPath);
                this.ParentModel.EventListener.OnAutoUpdateComplete(e.FullPath);
            }
            else
            {
                MessageLog.LogInfo($"Unsupported change type {e.ChangeType} for {e.FullPath}");
            }
        }
    }
}
