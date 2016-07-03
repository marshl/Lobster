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
    using System.Security;
    using System.Threading;
    using Properties;

    /// <summary>
    /// Used to define how a database should be connected to, and where the ClobTypes are stored for it.
    /// </summary>
    public class DatabaseConnection : IDisposable
    {
        /// <summary>
        /// The file watcher for the directory where the clob types are stored.
        /// </summary>
        private FileSystemWatcher clobTypeFileWatcher;

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
        /// The directory of the CodeSource folder.
        /// </summary>
        private DirectoryInfo codeSourceDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnection"/> class.
        /// </summary>
        /// <param name="config">The configuration file to base this connection off.</param>
        /// <param name="password">The password to connect to the database with.</param>
        public DatabaseConnection(DatabaseConfig config, SecureString password)
        {
            this.Config = config;
            this.IsAutoUpdateEnabled = this.Config.AllowAutomaticUpdates;
            this.Password = password;

            bool result = Utils.DeserialiseXmlFileUsingSchema("LobsterSettings/MimeTypes.xml", null, out this.mimeTypeList);

            this.ClobTypeDirectory = new DirectoryInfo(this.Config.ClobTypeDirectory);

            this.codeSourceDirectory = new DirectoryInfo(config.CodeSource);
            if (!this.codeSourceDirectory.Exists)
            {
                throw new SetConnectionException($"Could not find CodeSource directory: {this.codeSourceDirectory.FullName}");
            }

            this.clobTypeFileWatcher = new FileSystemWatcher(this.Config.ClobTypeDirectory);
            this.clobTypeFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            this.clobTypeFileWatcher.Changed += new FileSystemEventHandler(this.OnClobTypeChangeEvent);
            this.clobTypeFileWatcher.Created += new FileSystemEventHandler(this.OnClobTypeChangeEvent);
            this.clobTypeFileWatcher.Deleted += new FileSystemEventHandler(this.OnClobTypeChangeEvent);
            this.clobTypeFileWatcher.Renamed += new RenamedEventHandler(this.OnClobTypeChangeEvent);

            this.clobTypeFileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// The event for when all file event processing has finished.
        /// </summary>
        public event EventHandler<FileProcessingFinishedEventArgs> FileProcessingFinishedEvent;

        /// <summary>
        /// The event for when the first file change has ocurred within a block of events.
        /// </summary>
        public event EventHandler<FileChangeEventArgs> StartChangeProcessingEvent;

        /// <summary>
        /// The event for when a file update has completed (successfully or not).
        /// </summary>
        public event EventHandler<FileUpdateCompleteEventArgs> UpdateCompleteEvent;

        /// <summary>
        /// The event for when a Table selection is required by the user.
        /// </summary>
        public event EventHandler<TableRequestEventArgs> RequestTableEvent;

        /// <summary>
        /// The event for when a mime type is needed.
        /// </summary>
        public event EventHandler<MimeTypeRequestEventArgs> RequestMimeTypeEvent;

        /// <summary>
        /// The event for when a ClobType in the LobsterTypes directory has changed.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> ClobTypeChangedEvent;

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
        public SecureString Password { get; }

        /// <summary>
        /// Gets the directory of the clob types folder (wtihin the codeSourceDirectory)
        /// </summary>
        public DirectoryInfo ClobTypeDirectory { get; }

        /// <summary>
        /// Loads each of the xml files in the ClobTypeDir (if they are valid).
        /// </summary>
        /// <param name="errors">Any errors that are raised during loading.</param>
        public void LoadClobTypes(ref List<ClobTypeLoadException> errors)
        {
            this.ClobDirectoryList = new List<ClobDirectory>();
            if (!this.ClobTypeDirectory.Exists)
            {
                MessageLog.LogWarning($"The directory {ClobTypeDirectory} could not be found when loading connection {this.Config.Name}");
                errors.Add(new ClobTypeLoadException($"The directory {this.ClobTypeDirectory} could not be found when loading connection {this.Config.Name}"));
                return;
            }

            List<ClobType> clobTypes = ClobType.GetClobTypeList(this.Config.ClobTypeDirectory);
            clobTypes.ForEach(x => x.Initialise());

            foreach (ClobType clobType in clobTypes)
            {
                try
                {
                    ClobDirectory clobDir = new ClobDirectory(this.codeSourceDirectory, clobType);
                    clobDir.FileChangeEvent += this.OnClobDirectoryFileChangeEvent;
                    this.ClobDirectoryList.Add(clobDir);
                }
                catch (ClobTypeLoadException ex)
                {
                    MessageLog.LogError($"An error occurred when loading the ClobType {clobType.Directory}: {ex}");
                }
            }
        }

        /// <summary>
        /// Signal a request for a table type to the event subscribers.
        /// Used for finding which table a file that can be inserted into multiple tables should be inserted into.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="fullpath">The full name of the file being inserted.</param>
        /// <param name="tables">The list of table that the user can select from.</param>
        /// <returns>The user selected table (if any)</returns>
        public Table OnTableRequest(object sender, string fullpath, Table[] tables)
        {
            var handler = this.RequestTableEvent;
            if (handler != null)
            {
                var args = new TableRequestEventArgs(fullpath, tables);
                handler(sender, args);

                return args.SelectedTable;
            }

            return null;
        }

        /// <summary>
        /// Used to signal a mime type request to event listeners.
        /// For inserting a new record into a mime type controlled table.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="fullpath">The fullpath of the file that requires a mime type to be specified.</param>
        /// <param name="mimeTypes">The list of possible mime types that can be used.</param>
        /// <returns>The mime type to insert the new record as.</returns>
        public string OnMimeTypeRequest(object sender, string fullpath, string[] mimeTypes)
        {
            var handler = this.RequestMimeTypeEvent;
            if (handler != null)
            {
                var args = new MimeTypeRequestEventArgs(fullpath, mimeTypes);
                handler(sender, args);

                return args.SelectedMimeType;
            }

            return null;
        }

        /// <summary>
        /// Reloads the list of ClobTypes without destorying the connection.
        /// </summary>
        public void ReloadClobTypes()
        {
            List<ClobTypeLoadException> errors = new List<ClobTypeLoadException>();
            List<FileListRetrievalException> fileLoadErrors = new List<FileListRetrievalException>();
            this.LoadClobTypes(ref errors);
            Model.GetDatabaseFileLists(this, ref fileLoadErrors);
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

            if (this.clobTypeFileWatcher != null)
            {
                this.clobTypeFileWatcher.Dispose();
                this.clobTypeFileWatcher = null;
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
        /// The listener for when a file is changed within the clob type directory.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnClobTypeChangeEvent(object sender, FileSystemEventArgs args)
        {
            lock (this.fileEventQueue)
            {
                lock (this.fileEventProcessingSemaphore)
                {
                    var handler = this.ClobTypeChangedEvent;

                    if (handler != null)
                    {
                        handler(this, args);
                    }
                }
            }
        }

        /// <summary>
        /// The listener when a child ClobDirectory raises a file change event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        private void OnClobDirectoryFileChangeEvent(object sender, ClobDirectoryFileChangeEventArgs args)
        {
            Thread thread = new Thread(() => this.EnqueueFileEvent(args.ClobDir, args.Args));
            thread.Start();
        }

        /// <summary>
        /// Takes a single file event, and pushes it onto the event stack, before processing that event.
        /// </summary>
        /// <param name="clobDirectory">The directory from where the event originated.</param>
        /// <param name="args">The event arguments.</param>
        private void EnqueueFileEvent(ClobDirectory clobDirectory, FileSystemEventArgs args)
        {
            this.LogFileEvent($"File change event of type {args.ChangeType} for file {args.FullPath} with a write time of " + File.GetLastWriteTime(args.FullPath).ToString("yyyy-MM-dd HH:mm:ss.fff"));

            lock (this.fileEventQueue)
            {
                if (args.ChangeType != WatcherChangeTypes.Changed)
                {
                    this.fileTreeChangeInQueue = true;
                }

                // Ignore the event if it is already in the event list.
                // This often occurs, as most programs invoke several file change events when saving.
                if (this.fileEventQueue.Find(x => x.FullPath.Equals(args.FullPath, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    this.LogFileEvent("Ignoring event, as it is already in the list.");
                    return;
                }

                if (this.fileEventQueue.Count == 0)
                {
                    this.LogFileEvent("Event stack populated.");
                    this.OnEventProcessingStart();
                }

                this.fileEventQueue.Add(args);
            }

            this.LogFileEvent("Awaiting semaphore...");

            lock (this.fileEventProcessingSemaphore)
            {
                this.LogFileEvent("Lock achieved, processing event.");
                this.ProcessFileEvent(clobDirectory, args);

                lock (this.fileEventQueue)
                {
                    this.fileEventQueue.Remove(args);
                    if (this.fileEventQueue.Count == 0)
                    {
                        this.LogFileEvent("Event stack empty.");
                        this.OnFileProcessingFinished(this.fileTreeChangeInQueue);
                        this.fileTreeChangeInQueue = false;
                    }
                }
            }
        }

        /// <summary>
        /// Processes a single file event. Sending a file update to the database if necessary.
        /// </summary>
        /// <param name="clobDirectory">The clob directory that the event was raised from.</param>
        /// <param name="e">The event to process.</param>
        private void ProcessFileEvent(ClobDirectory clobDirectory, FileSystemEventArgs e)
        {
            this.LogFileEvent($"Processing file event of type {e.ChangeType} for file {e.FullPath}");

            FileInfo fileInfo = new FileInfo(e.FullPath);

            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                this.LogFileEvent($"Unsupported file event {e.ChangeType}");
                return;
            }

            if (!this.IsAutoUpdateEnabled)
            {
                this.LogFileEvent($"Automatic clobbing is disabled, ignoring event.");
                return;
            }

            if (!fileInfo.Exists)
            {
                this.LogFileEvent($"File could not be found {e.FullPath}");
                return;
            }

            if (fileInfo.IsReadOnly)
            {
                this.LogFileEvent($"File is read only and will be skipped {e.FullPath}");
                return;
            }

            if (!clobDirectory.ClobType.AllowAutomaticUpdates)
            {
                this.LogFileEvent("The ClobType does not allow autuomatic file updates, and the file will be skipped.");
                return;
            }

            // "IncludeSubDirectories" check
            if (!clobDirectory.ClobType.IncludeSubDirectories && fileInfo.Directory.FullName != clobDirectory.Directory.FullName)
            {
                this.LogFileEvent("The ClobType does not include sub directories, and the file will be skipped.");
                return;
            }

            DBClobFile clobFile = clobDirectory.GetDatabaseFileForFullpath(e.FullPath);
            if (clobFile == null)
            {
                this.LogFileEvent($"The file does not have a DBClobFile and will be skipped {e.FullPath}");
                return;
            }

            if (clobFile.LastUpdatedTime.AddMilliseconds(Settings.Default.FileUpdateTimeoutMilliseconds) > DateTime.Now)
            {
                this.LogFileEvent("The file was updated within the cooldown period, and will be skipped.");
                return;
            }

            this.LogFileEvent($"Auto-updating file {e.FullPath}");

            try
            {
                Model.SendUpdateClobMessage(this, clobDirectory, e.FullPath);
            }
            catch (FileUpdateException)
            {
                this.OnAutoUpdateComplete(e.FullPath, false);
                return;
            }

            this.OnAutoUpdateComplete(e.FullPath, true);
        }

        /// <summary>
        /// Logs the given text only if file events are enabled.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private void LogFileEvent(string message)
        {
            if (Settings.Default.LogFileEvents)
            {
                MessageLog.LogInfo(message);
            }
        }

        /// <summary>
        /// Signals a start in file processing to the event subscribers.
        /// </summary>
        private void OnEventProcessingStart()
        {
            var handler = this.StartChangeProcessingEvent;

            if (handler != null)
            {
                FileChangeEventArgs args = new FileChangeEventArgs();
                handler(this, args);
            }
        }

        /// <summary>
        /// Signals that a file automatic update has finished.
        /// </summary>
        /// <param name="filename">The path of the file that was updated.</param>
        /// <param name="success">Whether the update was a success or not.</param>
        private void OnAutoUpdateComplete(string filename, bool success)
        {
            var handler = this.UpdateCompleteEvent;

            if (handler != null)
            {
                FileUpdateCompleteEventArgs args = new FileUpdateCompleteEventArgs(filename, success);
                handler(this, args);
            }
        }

        /// <summary>
        /// Signals that all file events within a file processing block have finished.
        /// </summary>
        /// <param name="fileTreeChange">Whether or not the structure of the tree has changed.</param>
        private void OnFileProcessingFinished(bool fileTreeChange)
        {
            var handler = this.FileProcessingFinishedEvent;

            if (handler != null)
            {
                var args = new FileProcessingFinishedEventArgs(fileTreeChange);
                handler(this, args);
            }
        }
    }
}
