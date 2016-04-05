//-----------------------------------------------------------------------
// <copyright file="DatabaseConnection.cs" company="marshl">
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
    using Oracle.ManagedDataAccess.Client;
    using Properties;

    /// <summary>
    /// Used to define how a database should be connected to, and where the ClobTypes are stored for it.
    /// </summary>
    public class DatabaseConnection : IDisposable
    {
        /// <summary>
        /// The file watcher for the entire CodeSource directory. 
        /// FIles that are not covered by any ClobTypes filtered out when processed.
        /// </summary>
        private FileSystemWatcher fileWatcher;

        /// <summary>
        /// The queue of events that have triggered. 
        /// Events are popped off and processed one at a time by the fileEventThread.
        /// </summary>
        private List<FileSystemEventArgs> fileEventQueue = new List<FileSystemEventArgs>();

        /// <summary>
        /// A value indicating whether any of the current stack of file events require a file tree change (file delete/create/rename).
        /// </summary>
        private bool fileTreeChangeInQueue = false;

        /// <summary>
        /// A basic object to be locked when a thread is procesing a file event.
        /// </summary>
        private object fileEventProcessingSemaphore = new object();

        /// <summary>
        /// The internal mime type list.
        /// </summary>
        private MimeTypeList mimeTypeList;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnection"/> class.
        /// </summary>
        /// <param name="config">The configuration file to base this connection off.</param>
        /// <param name="password">The password to connect to the database with.</param>
        /// <param name="eventListener">The event listener that events will be sent to when files are automatically updated.</param>
        public DatabaseConnection(DatabaseConfig config, string password, IModelEventListener eventListener)
        {
            this.Config = config;
            this.IsAutoUpdateEnabled = this.Config.AllowAutomaticUpdates;
            this.Password = password;

            this.EventListener = eventListener;
            bool result = Utils.DeserialiseXmlFileUsingSchema<MimeTypeList>("LobsterSettings/MimeTypes.xml", null, out this.mimeTypeList);

            try
            {
                this.fileWatcher = new FileSystemWatcher(this.Config.CodeSource);
            }
            catch (ArgumentException ex)
            {
                throw new SetConnectionException("An error occurred when creating the FileSystemWatcher", ex);
            }

            this.fileWatcher.IncludeSubdirectories = true;
            this.fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Attributes;
            this.fileWatcher.Changed += new FileSystemEventHandler(this.OnFileChangeEvent);
            this.fileWatcher.Created += new FileSystemEventHandler(this.OnFileChangeEvent);
            this.fileWatcher.Deleted += new FileSystemEventHandler(this.OnFileChangeEvent);
            this.fileWatcher.Renamed += new RenamedEventHandler(this.OnFileRenameEvent);

            this.fileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Gets the event listener that is sent messages whenever a change is made to the model due to a FileSystemEvent.
        /// </summary>
        public IModelEventListener EventListener { get; private set; }

        /// <summary>
        /// Gets or sets the list of mime types that are used to translate from file names to database mnemonics and vice-sersa.
        /// </summary>
        public MimeTypeList MimeTypeList
        {
            get
            {
                return this.mimeTypeList;
            }

            set
            {
                this.mimeTypeList = value;
            }
        }

        /// <summary>
        /// Gets the list of temporary files that have been downloaded so far.
        /// These files are deleted when the model is disposed.
        /// </summary>
        public List<string> TempFileList { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the configuration file for this connection.
        /// </summary>
        public DatabaseConfig Config { get; private set; }

        /// <summary>
        /// Gets the list of ClobDirectories for each ClobType located in directory in the configuration settings.
        /// </summary>
        public List<ClobDirectory> ClobDirectoryList { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database should be automatically updated when a file is changed.
        /// </summary>
        public bool IsAutoUpdateEnabled { get; set; }

        /// <summary>
        /// Gets the password the user entered for this connection (set on initialisation).
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Returns the ClobDirectory that would contain the given fullpath, if applicable.
        /// Whether the file exists or not is irrelevent, only that it would be contained in the returned ClobDirectory.
        /// </summary>
        /// <param name="fullpath">The file to return the directory for.</param>
        /// <returns>The matching clobDirectory, if one exists.</returns>
        public ClobDirectory GetClobDirectoryForFile(string fullpath)
        {
            string absolutePath = Path.GetFullPath(fullpath);
            List<ClobDirectory> clobDirList = this.ClobDirectoryList.FindAll(x => absolutePath.Contains(x.GetFullPath(this)));
            if (clobDirList.Count == 0)
            {
                throw new ClobFileLookupException($"The file could not be found: {fullpath}");
            }
            else if (clobDirList.Count > 1)
            {
                throw new ClobFileLookupException($"The file was found in too many places: {fullpath}");
            }

            return clobDirList[0];
        }

        /// <summary>
        /// Loads each of the xml files in the ClobTypeDir (if they are valid).
        /// </summary>
        /// <param name="errors">Any errors that are raised during loading.</param>
        public void LoadClobTypes(ref List<ClobTypeLoadException> errors)
        {
            this.ClobDirectoryList = new List<ClobDirectory>();

            DirectoryInfo dirInfo = new DirectoryInfo(this.Config.ClobTypeDirectory);
            if (!dirInfo.Exists)
            {
                MessageLog.LogWarning($"The directory {dirInfo} could not be found when loading connection {this.Config.Name}");
                errors.Add(new ClobTypeLoadException($"The directory {dirInfo} could not be found when loading connection {this.Config.Name}"));
                return;
            }

            List<ClobType> clobTypes = ClobType.GetClobTypeList(this.Config.ClobTypeDirectory);

            foreach (ClobType clobType in clobTypes)
            {
                clobType.Initialise();

                ClobDirectory clobDir = new ClobDirectory(clobType);
                this.ClobDirectoryList.Add(clobDir);
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
                clobDir.GetWorkingFiles(this, ref workingFileList);
            }
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
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
            if (!disposing)
            {
                return;
            }

            if (this.fileWatcher != null)
            {
                this.fileWatcher.Dispose();
                this.fileWatcher = null;
            }

            foreach (string filename in this.TempFileList)
            {
                try
                {
                    MessageLog.LogInfo($"Deleting temporary file {filename}");
                    File.Delete(filename);
                }
                catch (IOException)
                {
                    MessageLog.LogInfo($"An error occurred when deleting temporary file {filename}");
                }
            }
        }

        /// <summary>
        /// The event raised when a file is renamed within the CodeSource directory.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnFileRenameEvent(object sender, RenamedEventArgs e)
        {
            this.OnFileChangeEvent(sender, e);
        }

        /// <summary>
        /// The event raised when a file is changed within the CodeSource directory.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnFileChangeEvent(object sender, FileSystemEventArgs e)
        {
            Thread thread = new Thread(() => this.EnqueueFileEvent(sender, e));
            thread.Start();
        }

        /// <summary>
        /// Takes a single file event, and pushes it onto the event stack, before processing that event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EnqueueFileEvent(object sender, FileSystemEventArgs e)
        {
            if (Settings.Default.LogFileEvents)
            {
                MessageLog.LogInfo($"File change event of type {e.ChangeType} for file {e.FullPath} with a write time of " + File.GetLastWriteTime(e.FullPath).ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }

            lock (this.fileEventQueue)
            {
                // Ignore the event if it is already in the event list.
                // This often occurs, as most programs invoke several file change events when saving.
                if (this.fileEventQueue.Find(x => x.FullPath.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    if (Settings.Default.LogFileEvents)
                    {
                        MessageLog.LogInfo("Ignoring event, as it is already in the list.");
                    }

                    return;
                }

                if (this.fileEventQueue.Count == 0)
                {
                    if (Settings.Default.LogFileEvents)
                    {
                        MessageLog.LogInfo("Event stack populated.");
                    }

                    if (e.ChangeType != WatcherChangeTypes.Changed)
                    {
                        this.fileTreeChangeInQueue = true;
                    }

                    this.EventListener.OnEventProcessingStart();
                }

                this.fileEventQueue.Add(e);
            }

            if (Settings.Default.LogFileEvents)
            {
                MessageLog.LogInfo("Awaiting semaphore...");
            }

            lock (this.fileEventProcessingSemaphore)
            {
                if (Settings.Default.LogFileEvents)
                {
                    MessageLog.LogInfo("Lock achieved, processing event.");
                }

                this.ProcessFileEvent(e);

                lock (this.fileEventQueue)
                {
                    this.fileEventQueue.Remove(e);
                    if (this.fileEventQueue.Count == 0)
                    {
                        if (Settings.Default.LogFileEvents)
                        {
                            MessageLog.LogInfo("Event stack empty.");
                        }

                        this.EventListener.OnFileProcessingFinished(this.fileTreeChangeInQueue);
                        this.fileTreeChangeInQueue = false;
                    }
                }
            }
        }

        /// <summary>
        /// Processes a single file event. Sending a file update to the database if necessary.
        /// </summary>
        /// <param name="e">The event to process.</param>
        private void ProcessFileEvent(FileSystemEventArgs e)
        {
            if (Settings.Default.LogFileEvents)
            {
                MessageLog.LogInfo($"Processing file event of type {e.ChangeType} for file {e.FullPath}");
            }

            FileInfo fileInfo = new FileInfo(e.FullPath);

            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            if (!this.IsAutoUpdateEnabled)
            {
                if (Settings.Default.LogFileEvents)
                {
                    MessageLog.LogInfo($"Automatic clobbing is disabled, ignoring event.");
                }

                return;
            }

            if (!fileInfo.Exists)
            {
                if (Settings.Default.LogFileEvents)
                {
                    MessageLog.LogInfo($"File could not be found {e.FullPath}");
                }

                return;
            }

            if (fileInfo.IsReadOnly)
            {
                if (Settings.Default.LogFileEvents)
                {
                    MessageLog.LogInfo($"File is read only and will be skipped {e.FullPath}");
                }

                return;
            }

            ClobDirectory clobDir;
            try
            {
                clobDir = this.GetClobDirectoryForFile(e.FullPath);
            }
            catch (Exception ex) when (ex is ClobFileLookupException)
            {
                if (Settings.Default.LogFileEvents)
                {
                    MessageLog.LogInfo($"The file does not belong to any ClobDirectory and will be skipped {e.FullPath}");
                }

                return;
            }

            if (!clobDir.ClobType.AllowAutomaticUpdates)
            {
                if (Settings.Default.LogFileEvents)
                {
                    MessageLog.LogInfo("The ClobType does not allow autuomatic file updates, and the file will be skipped.");
                }

                return;
            }

            // "IncludeSubDirectories" check
            if (!clobDir.ClobType.IncludeSubDirectories && fileInfo.Directory.FullName != clobDir.GetFullPath(this))
            {
                if (Settings.Default.LogFileEvents)
                {
                    MessageLog.LogInfo("The ClobType does not include sub directories, and the file will be skipped.");
                }

                return;
            }

            DBClobFile clobFile = clobDir.GetDatabaseFileForFullpath(e.FullPath);
            if (clobFile == null)
            {
                if (Settings.Default.LogFileEvents)
                {
                    MessageLog.LogInfo($"The file does not have a DBClobFile and will be skipped {e.FullPath}");
                }

                return;
            }

            if (clobFile.LastUpdatedTime.AddMilliseconds(Settings.Default.FileUpdateTimeoutMilliseconds) > DateTime.Now)
            {
                if (Settings.Default.LogFileEvents)
                {
                    MessageLog.LogInfo("The file was updated within the cooldown period, and will be skipped.");
                }

                return;
            }

            if (Settings.Default.LogFileEvents)
            {
                MessageLog.LogInfo($"Auto-updating file {e.FullPath}");
            }

            try
            {
                Model.SendUpdateClobMessage(this, e.FullPath);
            }
            catch (FileUpdateException)
            {
                this.EventListener.OnAutoUpdateComplete(e.FullPath, false);
                return;
            }

            this.EventListener.OnAutoUpdateComplete(e.FullPath, true);
        }
    }
}
