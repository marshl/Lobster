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
    using System.Linq;
    using System.Security;
    using System.Threading;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;
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
        /// Initializes a new instance of the <see cref="DatabaseConnection"/> class.
        /// </summary>
        /// <param name="config">The configuration file to base this connection off.</param>
        /// <param name="password">The password to connect to the database with.</param>
        private DatabaseConnection(ConnectionConfig config, SecureString password)
        {
            this.Config = config;
            this.IsAutomaticClobbingEnabled = this.Config.AllowAutomaticClobbing;
            this.Password = password;

            if (!Directory.Exists(this.Config.Parent.CodeSourceDirectory))
            {
                throw new CreateConnectionException($"Could not find CodeSource directory: {this.Config.Parent.CodeSourceDirectory}");
            }

            this.clobTypeFileWatcher = new FileSystemWatcher(this.Config.Parent.DirectoryDescriptorFolder);
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
        /// The event for when a ClobType in the LobsterTypes directory has changed.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> ClobTypeChangedEvent;

        /// <summary>
        /// Gets the list of temporary files that have been downloaded so far.
        /// These files are deleted when the model is disposed.
        /// </summary>
        public List<string> TempFileList { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the configuration file for this connection.
        /// </summary>
        public ConnectionConfig Config { get; }

        public List<DirectoryWatcher> DirectoryWatcherList { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database should be automatically updated when a file is changed.
        /// </summary>
        public bool IsAutomaticClobbingEnabled { get; set; }

        /// <summary>
        /// Gets the password the user entered for this connection (set on initialisation).
        /// </summary>
        public SecureString Password { get; }

        /// <summary>
        /// Sets the current connection to the given connection, if able.
        /// </summary>
        /// <param name="config">The connection to open.</param>
        /// <param name="password">The password to connect to the database with.</param>
        /// <returns>The successfully created database connection.</returns>
        public static DatabaseConnection CreateDatabaseConnection(ConnectionConfig config, SecureString password)
        {
            MessageLog.LogInfo($"Changing connection to {config.Name}");
            try
            {
                OracleConnection con = config.OpenSqlConnection(password);
                con.Close();
            }
            catch (ConnectToDatabaseException ex)
            {
                throw new CreateConnectionException($"A connection could not be made to the database: {ex.Message}", ex);
            }

            if (!Directory.Exists(config.Parent.DirectoryDescriptorFolder))
            {
                throw new CreateConnectionException($"The clob type directory {config.Parent.DirectoryDescriptorFolder} could not be found.");
            }

            DatabaseConnection databaseConnection = new DatabaseConnection(config, password);

            //TODO: Display these errors to the user somehow
            List<ClobTypeLoadException> errors = new List<ClobTypeLoadException>();
            List<FileListRetrievalException> fileLoadErrors = new List<FileListRetrievalException>();

            databaseConnection.LoadDirectoryDescriptors(ref errors);

            return databaseConnection;
        }

        public void LoadDirectoryDescriptors(ref List<ClobTypeLoadException> errors)
        {
            this.DirectoryWatcherList = new List<DirectoryWatcher>();
            if (!Directory.Exists(this.Config.Parent.DirectoryDescriptorFolder))
            {
                string errorMsg = $"The directory {this.Config.Parent.DirectoryDescriptorFolder} could not be found loading connection {this.Config.Name}";
                MessageLog.LogWarning(errorMsg);
                errors.Add(new ClobTypeLoadException(errorMsg));
                return;
            }

            var directoryDescriptors = DirectoryDescriptor.GetDirectoryDescriptorList(this.Config.Parent.DirectoryDescriptorFolder);
            foreach (var dirDesc in directoryDescriptors)
            {
                try
                {
                    DirectoryWatcher dirWatcher = new DirectoryWatcher(this.Config.Parent.CodeSourceDirectory, dirDesc);
                    //TODO: dirWatcher.FileChangeEvent += this.OnClobDirectoryFileChangeEvent;
                    dirWatcher.FileChangeEvent += this.OnDirectoryWatcherFileChangeEvent;
                    this.DirectoryWatcherList.Add(dirWatcher);
                }
                catch (ClobTypeLoadException ex)
                {
                    MessageLog.LogError($"An error occurred when loading the DirectoryDescriptor {dirDesc.Name}: {ex}");
                }
            }
        }

        public void UpdateDatabaseFile(DirectoryWatcher watcher, string filepath)
        {
            OracleConnection oracleConnection = this.Config.OpenSqlConnection(this.Password);
            if (Settings.Default.BackupEnabled)
            {
                this.BackupFile(oracleConnection, watcher, filepath);
            }

            OracleCommand oracleCommand = oracleConnection.CreateCommand();

            oracleCommand.CommandText = watcher.Descriptor.UpdateStatement;
            this.BindParametersToCommand(oracleConnection, oracleCommand, watcher, filepath);

            try
            {
                MessageLog.LogInfo($"Executing Update query: {oracleCommand.CommandText}");
                int rowsAffected = oracleCommand.ExecuteNonQuery();

                // The row check can't be relied upon when using a custom loader, as it may be in an anonymous block
                if (rowsAffected != 1)
                {
                    MessageLog.LogError($"In invalid number of rows ({rowsAffected}) were updated for command: {oracleCommand.CommandText}");
                    throw new FileUpdateException($"{rowsAffected} rows were affected during the update (expected only 1). The transaction has been rolled back.");
                }

                MessageLog.LogInfo($"Clob file update successful: {filepath}");
                return;
            }
            catch (Exception ex) when (ex is OracleException || ex is InvalidOperationException)
            {
                MessageLog.LogError($"Clob update failed for command: {oracleCommand.CommandText} {ex}");
                throw new FileUpdateException($"An invalid operation occurred when updating the database: {ex.Message}", ex);
            }
        }

        public void BackupFile(OracleConnection oracleConnection, DirectoryWatcher watcher, string filename)
        {
            FileInfo backupFile = BackupLog.AddBackup(this.Config.Parent.CodeSourceDirectory, filename);
            this.DownloadDatabaseFile(oracleConnection, watcher, filename, backupFile.FullName);
        }

        public void InsertFile(DirectoryWatcher watcher, string filepath)
        {
            OracleConnection oracleConnection = this.Config.OpenSqlConnection(this.Password);
            OracleCommand command = oracleConnection.CreateCommand();
            this.BindParametersToCommand(oracleConnection, command, watcher, filepath);

            try
            {
                command.ExecuteNonQuery();
                command.Dispose();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is OracleException || ex is IOException)
            {
                MessageLog.LogError($"Error creating new clob when executing command: {command.CommandText} {ex}");
                throw new FileInsertException("An exception ocurred when attempting to insert a file into the child table.", ex);
            }

            MessageLog.LogInfo($"Clob file creation successful: {filepath}");
        }

        public void DownloadDatabaseFile(OracleConnection connection, DirectoryWatcher watcher, string sourceFilename, string outputFile)
        {
            OracleCommand oracleCommand = connection.CreateCommand();
            this.BindParametersToCommand(connection, oracleCommand, watcher, sourceFilename);

            string dataType = this.GetDataTypeForFile(connection, watcher, sourceFilename);
            if (dataType == "BLOB")
            {
                oracleCommand.CommandText = watcher.Descriptor.FetchBinaryStatement;
            }
            else
            {
                oracleCommand.CommandText = watcher.Descriptor.FetchStatement;
            }

            try
            {
                OracleDataReader reader = oracleCommand.ExecuteReader();

                if (!reader.Read())
                {
                    string msg = $"No data found on clob retrieval of {sourceFilename} when executing command: {oracleCommand.CommandText}";
                    MessageLog.LogError(msg);
                    throw new FileDownloadException(msg);
                }

                using (FileStream fs = Utils.WaitForFile(outputFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                {
                    if (dataType == "BLOB")
                    {
                        byte[] b = new byte[reader.GetBytes(0, 0, null, 0, int.MaxValue)];
                        reader.GetBytes(0, 0, b, 0, b.Length);
                        fs.Write(b, 0, b.Length);
                    }
                    else
                    {
                        string result = reader.GetString(0);
                        using (StreamWriter streamWriter = new StreamWriter(fs))
                        {
                            streamWriter.NewLine = "\n";
                            streamWriter.Write(result);
                        }
                    }
                }

                if (!reader.Read())
                {
                    reader.Close();
                    return;
                }
                else
                {
                    // Too many rows
                    reader.Close();
                    MessageLog.LogError($"Too many rows found on clob retrieval of {sourceFilename}");
                    throw new FileDownloadException($"Too many rows were found for the given command {oracleCommand.CommandText}");
                }
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleException || e is OracleNullValueException || e is IOException)
            {
                MessageLog.LogError($"Error retrieving data when executing command {oracleCommand.CommandText} {e}");
                throw new FileDownloadException($"An exception ocurred when executing the command {oracleCommand.CommandText}");
            }
        }

        public bool IsFileSynchronised(DirectoryWatcher dirWatcher, WatchedFile watchedFile)
        {
            OracleConnection oracleConnection = this.Config.OpenSqlConnection(this.Password);

            OracleCommand oracleCommand = oracleConnection.CreateCommand();

            oracleCommand.CommandText = dirWatcher.Descriptor.DatabaseFileExistsStatement;
            this.BindParametersToCommand(oracleConnection, oracleCommand, dirWatcher, watchedFile.FilePath);

            try
            {
                MessageLog.LogInfo($"Executing file existence query: {oracleCommand.CommandText}");
                int result = (int)oracleCommand.ExecuteScalar();
                return result >= 1;
            }
            catch (Exception ex) when (ex is OracleException || ex is InvalidOperationException)
            {
                MessageLog.LogError($"File existence check failed for command: {oracleCommand.CommandText} {ex}");
                throw new FileUpdateException($"An exception occurred when determining whether {watchedFile.FilePath} is in the database: {ex.Message}", ex);
            }
        }

        private void BindParametersToCommand(OracleConnection connection, OracleCommand command, DirectoryWatcher watcher, string path)
        {
            command.BindByName = true;
            string dataType = this.GetDataTypeForFile(connection, watcher, path);

            if (command.ContainsParameter("filename"))
            {
                var param = new OracleParameter("filename", Path.GetFileName(path));
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter("filename_without_extension"))
            {
                var param = new OracleParameter("filename_without_extension", Path.GetFileName(path));
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter("file_extension"))
            {
                var param = new OracleParameter("file_extension", Path.GetExtension(path));
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter("relative_path"))
            {
                Uri baseUri = new Uri(watcher.DirectoryPath);
                Uri fileUri = new Uri(path);
                Uri relativeUri = baseUri.MakeRelativeUri(fileUri);
                var param = new OracleParameter("relative_path", relativeUri.OriginalString);
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter("parent_directory"))
            {
                var param = new OracleParameter("parent_directory", new FileInfo(path).Directory.Name);
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter("full_path"))
            {
                var param = new OracleParameter("full_path", path);
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter("data_type"))
            {
                var param = new OracleParameter();
                param.ParameterName = "data_type";

                param.Value = dataType;

                command.Parameters.Add(param);
            }

            if (command.ContainsParameter("file_data_clob"))
            {
                OracleParameter param = new OracleParameter();
                param.ParameterName = "file_data_clob";

                // Wait for the file to unlock
                using (FileStream fs = Utils.WaitForFile(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    // Binary mode
                    if (dataType == "BLOB")
                    {
                        byte[] fileData = new byte[fs.Length];
                        fs.Read(fileData, 0, Convert.ToInt32(fs.Length));

                        param.Value = fileData;
                        param.OracleDbType = OracleDbType.Blob;
                    }
                    else
                    {
                        // Text mode
                        var tr = new StreamReader(fs);

                        string contents = tr.ReadToEnd();

                        if (Settings.Default.AppendFooterToDatabaseFiles)
                        {
                            //TODO: Add Clob Footer Message
                            //contents += MimeTypeList.GetClobFooterMessage(mimeType);
                        }

                        param.Value = contents;
                        param.OracleDbType = OracleDbType.Clob;
                    }
                }
            }
        }

        private string GetDataTypeForFile(OracleConnection connection, DirectoryWatcher watcher, string path)
        {
            // Default to CLOB if there is no default or statement
            if (watcher.Descriptor.FileDataTypeStatement == null && watcher.Descriptor.DefaultDataType == null)
            {
                return "CLOB";
            }

            // Use the default if there is no statement (but there is a default)
            if (watcher.Descriptor.FileDataTypeStatement == null)
            {
                return watcher.Descriptor.DefaultDataType;
            }

            // Otherwise use the statement
            OracleCommand command = connection.CreateCommand();
            command.CommandText = watcher.Descriptor.FileDataTypeStatement;
            if (command.ContainsParameter("data_type"))
            {
                throw new InvalidOperationException("The FileDataTypeStatement cannot contain the \"data_type\" parameter");
            }

            this.BindParametersToCommand(connection, command, watcher, path);

            object result = command.ExecuteScalar();

            if (!(result is string))
            {
                throw new InvalidOperationException("The FileDataTypeStatement should return a single string");
            }

            return (string)result;
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

            if (this.DirectoryWatcherList != null)
            {
                foreach (DirectoryWatcher watcher in this.DirectoryWatcherList)
                {
                    watcher.Dispose();
                }

                this.DirectoryWatcherList = null;
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
                    this.ClobTypeChangedEvent?.Invoke(this, args);
                }
            }
        }

        private void OnDirectoryWatcherFileChangeEvent(object sender, DirectoryWatcherFileChangeEventArgs args)
        {
            Thread thread = new Thread(() => this.EnqueueFileEvent(args.Watcher, args.Args));
            thread.Start();
        }

        /// <summary>
        /// Takes a single file event, and pushes it onto the event stack, before processing that event.
        /// </summary>
        /// <param name="watcher">The directory from where the event originated.</param>
        /// <param name="args">The event arguments.</param>
        private void EnqueueFileEvent(DirectoryWatcher watcher, FileSystemEventArgs args)
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
                this.ProcessFileEvent(watcher, args);

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
        /// <param name="watcher">The clob directory that the event was raised from.</param>
        /// <param name="e">The event to process.</param>
        private void ProcessFileEvent(DirectoryWatcher watcher, FileSystemEventArgs e)
        {
            this.LogFileEvent($"Processing file event of type {e.ChangeType} for file {e.FullPath}");

            FileInfo fileInfo = new FileInfo(e.FullPath);

            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                this.LogFileEvent($"Unsupported file event {e.ChangeType}");
                return;
            }

            if (!this.IsAutomaticClobbingEnabled)
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

            if (!watcher.Descriptor.PushAutomatically)
            {
                this.LogFileEvent("The ClobType does not allow autuomatic file updates, and the file will be skipped.");
                return;
            }

            // TODO: Check whether the file will appear in the descriptor
            // "IncludeSubDirectories" check
            /*if (!watcher.Descriptor.IncludeSubDirectories && fileInfo.Directory.FullName != watcher.DirectoryPath)
            {
                this.LogFileEvent("The ClobType does not include sub directories, and the file will be skipped.");
                return;
            }*/
            /*
            DBClobFile clobFile = watcher.GetDatabaseFileForFullpath(e.FullPath);
            if (clobFile == null)
            {
                this.LogFileEvent($"The file does not have a DBClobFile and will be skipped {e.FullPath}");
                return;
            }*/

            /*if (clobFile.LastUpdatedTime.AddMilliseconds(Settings.Default.FileUpdateTimeoutMilliseconds) > DateTime.Now)
            {
                this.LogFileEvent("The file was updated within the cooldown period, and will be skipped.");
                return;
            }*/

            this.LogFileEvent($"Auto-updating file {e.FullPath}");

            try
            {
                //this.SendUpdateClobMessage(clobDirectory, e.FullPath);
                this.UpdateDatabaseFile(watcher, e.FullPath);
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
            FileChangeEventArgs args = new FileChangeEventArgs();
            handler?.Invoke(this, args);
        }

        /// <summary>
        /// Signals that a file automatic update has finished.
        /// </summary>
        /// <param name="filename">The path of the file that was updated.</param>
        /// <param name="success">Whether the update was a success or not.</param>
        private void OnAutoUpdateComplete(string filename, bool success)
        {
            var handler = this.UpdateCompleteEvent;
            FileUpdateCompleteEventArgs args = new FileUpdateCompleteEventArgs(filename, success);
            handler?.Invoke(this, args);
        }

        /// <summary>
        /// Signals that all file events within a file processing block have finished.
        /// </summary>
        /// <param name="fileTreeChange">Whether or not the structure of the tree has changed.</param>
        private void OnFileProcessingFinished(bool fileTreeChange)
        {
            var handler = this.FileProcessingFinishedEvent;
            var args = new FileProcessingFinishedEventArgs(fileTreeChange);
            handler?.Invoke(this, args);
        }
    }
}
