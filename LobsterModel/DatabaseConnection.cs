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
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Threading;
    using Oracle.ManagedDataAccess.Client;
    using Properties;

    /// <summary>
    /// Used to manage a connection from on code source to one database, with all the methods for comparing local files with those on the database and making changes to the database
    /// </summary>
    public class DatabaseConnection : IDisposable
    {
        /// <summary>
        /// The number of characters of a database parameter that will be logged (all the rest will be truncated)
        /// </summary>
        private const int ParameterLogLength = 255;

        /// <summary>
        /// The parameter for the name of the file (including the file extension)
        /// </summary>
        private const string FilenameParameterName = "p_filename";

        /// <summary>
        /// The parameter for the name of the file without the extension
        /// </summary>
        private const string FilenameWithoutExtensionParameterName = "p_filename_without_extension";

        /// <summary>
        /// The parameter for the extension of the file (exlcuding the .)
        /// </summary>
        private const string FileExtensionParameterName = "p_file_extension";

        /// <summary>
        /// The parameter for the path of the file, relative to the CodeSource directory
        /// </summary>
        private const string RelativePathParameterName = "p_relative_path";

        /// <summary>
        /// The parameter for the parent directory of the file
        /// </summary>
        private const string ParentDirectoryParameterName = "p_parent_directory";

        /// <summary>
        /// The parameter for the full path of the file
        /// </summary>
        private const string FullPathParameterName = "p_full_path";

        /// <summary>
        /// The parameter for the mime type of the file (has to be computed using another SQL statement)
        /// </summary>
        private const string MimeTypeParameterName = "p_mime_type";

        /// <summary>
        /// The parameter for the data type of the file (has to be computed using another SQL statement)
        /// </summary>
        private const string DataTypeParameterName = "p_data_type";

        /// <summary>
        /// The parameter for the CLOB data of the file
        /// </summary>
        private const string FileContentClobParameterName = "p_file_content_clob";

        /// <summary>
        /// The parameter for the BLOB data of the file
        /// </summary>
        private const string FileContentBlobParameterName = "p_file_content_blob";

        /// <summary>
        /// The parameter for the footer message placed at the bottom of some files
        /// </summary>
        private const string FooterMessageParameterName = "p_footer_message";

        /// <summary>
        /// The file watcher for the directory where the directory descriptors are stored.
        /// </summary>
        private FileSystemWatcher directoryDescriptorFileWatcher;

        /// <summary>
        /// The queue of events that have triggered. 
        /// Events are popped off and processed one at a time by the fileEventThread.
        /// </summary>
        private List<FileChangeEvent> fileEventQueue = new List<FileChangeEvent>();

        /// <summary>
        /// A value indicating whether any of the current stack of file events require a file tree change (file delete/create/rename).
        /// </summary>
        private bool fileTreeChangeInQueue = false;

        /// <summary>
        /// A basic object to be locked when a thread is procesing a file event.
        /// </summary>
        private object fileEventProcessingSemaphore = new object();

        /// <summary>
        /// The cached connection to the database.
        /// </summary>
        private OracleConnection storedConnection;

        /// <summary>
        /// The thread which is delayed after a time after the last file change event, and which processes the changes.
        /// </summary>
        private Timer eventProcessingTimer;

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

            this.directoryDescriptorFileWatcher = new FileSystemWatcher(this.Config.Parent.DirectoryDescriptorFolder);
            this.directoryDescriptorFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            this.directoryDescriptorFileWatcher.Changed += new FileSystemEventHandler(this.OnDirectoryDescriptorChangeEvent);
            this.directoryDescriptorFileWatcher.Created += new FileSystemEventHandler(this.OnDirectoryDescriptorChangeEvent);
            this.directoryDescriptorFileWatcher.Deleted += new FileSystemEventHandler(this.OnDirectoryDescriptorChangeEvent);
            this.directoryDescriptorFileWatcher.Renamed += new RenamedEventHandler(this.OnDirectoryDescriptorChangeEvent);

            this.directoryDescriptorFileWatcher.EnableRaisingEvents = true;

            this.OpenConnection();
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
        /// The event for when a DirectoryDescriptor in their folder has changed.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> DirectoryDescriptorChangeEvent;

        /// <summary>
        /// The event for when an error occurs when loading the directory descriptors
        /// </summary>
        public event EventHandler<ConnectionLoadErrorEventArgs> ConnectionLoadErrorEvent;

        /// <summary>
        /// Gets the list of temporary files that have been downloaded so far.
        /// These files are deleted when the model is disposed.
        /// </summary>
        public List<string> TempFileList { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the configuration file for this connection.
        /// </summary>
        public ConnectionConfig Config { get; }

        /// <summary>
        /// Gets the list of watchers for this connection.
        /// </summary>
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

            if (!Directory.Exists(config.Parent.DirectoryDescriptorFolder))
            {
                throw new CreateConnectionException($"The Directory Descriptor directory {config.Parent.DirectoryDescriptorFolder} could not be found.");
            }

            try
            {
                OracleConnection con = config.OpenSqlConnection(password);
                con.Close();
            }
            catch (ConnectToDatabaseException ex)
            {
                throw new CreateConnectionException($"A test connection could not be made to the database: {ex.Message}", ex);
            }

            DatabaseConnection databaseConnection = new DatabaseConnection(config, password);
            databaseConnection.LoadDirectoryDescriptors();

            return databaseConnection;
        }

        /// <summary>
        /// Opens a connection to the database.
        /// </summary>
        /// <returns>The connection to the database.</returns>
        public OracleConnection OpenConnection()
        {
            if (this.storedConnection == null || this.storedConnection.State == ConnectionState.Closed || this.storedConnection.State == ConnectionState.Broken)
            {
                this.storedConnection = this.Config.OpenSqlConnection(this.Password);
            }

            return this.storedConnection;
        }

        /// <summary>
        /// Loads the <see cref="DirectoryDescriptor"/>s in the CodeSource folder of this connection.
        /// </summary>
        public void LoadDirectoryDescriptors()
        {
            if (this.DirectoryWatcherList != null)
            {
                foreach (DirectoryWatcher watcher in this.DirectoryWatcherList)
                {
                    watcher.Dispose();
                }
            }

            this.DirectoryWatcherList = new List<DirectoryWatcher>();
            if (!Directory.Exists(this.Config.Parent.DirectoryDescriptorFolder))
            {
                string errorMsg = $"The directory {this.Config.Parent.DirectoryDescriptorFolder} could not be found loading connection {this.Config.Name}";
                MessageLog.LogWarning(errorMsg);
                this.OnConnectionLoadError(errorMsg);
                return;
            }

            var dirDescLoader = new DirectoryDescriptorLoader(this.Config.Parent.DirectoryDescriptorFolder);
            dirDescLoader.DirectoryDescriptorLoadError += (object sender, DirectoryDescriptorLoader.DirectoryDescriptorLoadErrorEventArgs e) =>
            {
                this.OnConnectionLoadError($"An error occurred when loading DirectoryDescriptor {e.FilePath}: {e.RaisedException}");
            };

            var dirDescList = dirDescLoader.GetDirectoryDescriptorList();

            foreach (var dirDesc in dirDescList)
            {
                try
                {
                    DirectoryWatcher dirWatcher = new DirectoryWatcher(this.Config.Parent.CodeSourceDirectory, dirDesc);
                    dirWatcher.FileChangeEvent += this.OnDirectoryWatcherFileChangeEvent;
                    this.DirectoryWatcherList.Add(dirWatcher);
                }
                catch (DirectoryDescriptorLoadException ex)
                {
                    MessageLog.LogError($"An error occurred when loading the DirectoryDescriptor {dirDesc.Name}: {ex}");
                }
            }
        }

        /// <summary>
        /// Updates a file in the database using its synchronised local version.
        /// </summary>
        /// <param name="watcher">The watcher of the file to update.</param>
        /// <param name="filepath">The file to update.</param>
        public void UpdateDatabaseFile(DirectoryWatcher watcher, string filepath)
        {
            OracleConnection oracleConnection = this.OpenConnection();

            if (Settings.Default.BackupEnabled)
            {
                try
                {
                    this.BackupFile(watcher, filepath);
                }
                catch (FileDownloadException ex)
                {
                    MessageLog.LogError($"An exception occurred when attempting to backup the file: {ex}");
                    throw new FileUpdateException("An exception occurred when backing up the file.", ex);
                }
            }

            OracleTransaction trans = oracleConnection.BeginTransaction();
            OracleCommand oracleCommand = oracleConnection.CreateCommand();

            oracleCommand.CommandText = watcher.Descriptor.UpdateStatement;
            this.BindParametersToCommand(oracleConnection, oracleCommand, watcher, filepath);

            try
            {
                MessageLog.LogInfo($"Executing Update query: {oracleCommand.CommandText}");
                int rowsAffected = oracleCommand.ExecuteNonQuery();

                // Ensure that only one row was affected by the uopdate.
                // If the update statement was an anonymous block, then -1 is returned, which can be ignored
                if (rowsAffected != 1 && rowsAffected != -1)
                {
                    trans.Rollback();
                    MessageLog.LogError($"In invalid number of rows ({rowsAffected}) were updated for command: {oracleCommand.CommandText}");
                    throw new FileUpdateException($"{rowsAffected} rows were affected during the update (expected only 1). The transaction has been rolled back.");
                }

                trans.Commit();
                MessageLog.LogInfo($"Clob file update successful: {filepath}");
            }
            catch (Exception ex)
            {
                MessageLog.LogError($"Clob update failed for command: {oracleCommand.CommandText} {ex}");
                throw new FileUpdateException($"An invalid operation occurred when updating the database: {ex.Message}", ex);
            }
            finally
            {
                trans.Dispose();
            }
        }

        /// <summary>
        /// Backs up a single file using its watcher
        /// </summary>
        /// <param name="watcher">The watcher that manages the file to be backed up.</param>
        /// <param name="filename">The file to backup.</param>
        public void BackupFile(DirectoryWatcher watcher, string filename)
        {
            FileInfo backupFile = BackupLog.AddBackup(this.Config.Parent.CodeSourceDirectory, filename);
            this.DownloadDatabaseFile(watcher, filename, backupFile.FullName);
        }

        /// <summary>
        /// Inserts a single file into the database
        /// </summary>
        /// <param name="watcher">The watcer that manages the file to be inserted.</param>
        /// <param name="filepath">The path od the file to insert.</param>
        public void InsertFile(DirectoryWatcher watcher, string filepath)
        {
            OracleConnection oracleConnection = this.OpenConnection();
            OracleCommand command = oracleConnection.CreateCommand();
            command.CommandText = watcher.Descriptor.InsertStatement;
            this.BindParametersToCommand(oracleConnection, command, watcher, filepath);

            try
            {
                command.ExecuteNonQuery();
                command.Dispose();
            }
            catch (Exception ex)
            {
                MessageLog.LogError($"Error creating new clob when executing command: {command.CommandText} {ex}");
                throw new FileInsertException("An exception ocurred when attempting to insert a file.", ex);
            }

            MessageLog.LogInfo($"Clob file creation successful: {filepath}");
        }

        /// <summary>
        /// Downloads the contents of a file stored in the database to a local directory.
        /// </summary>
        /// <param name="watcher">The directory watcher of the file to downlaod.</param>
        /// <param name="sourceFilename">The file to download the data for.</param>
        /// <param name="outputFile">The file to download the data to.</param>
        public void DownloadDatabaseFile(DirectoryWatcher watcher, string sourceFilename, string outputFile)
        {
            OracleConnection connection = this.OpenConnection();

            MessageLog.LogInfo($"Downlloading the database file for {sourceFilename}{(sourceFilename != outputFile ? " to " + outputFile : "")}");
            OracleCommand oracleCommand = connection.CreateCommand();

            try
            {
                string dataType = this.GetDataTypeForFile(watcher, sourceFilename);
                if (dataType == "BLOB")
                {
                    oracleCommand.CommandText = watcher.Descriptor.FetchBinaryStatement;
                }
                else
                {
                    oracleCommand.CommandText = watcher.Descriptor.FetchStatement;
                }

                this.BindParametersToCommand(connection, oracleCommand, watcher, sourceFilename);

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
                        try
                        {
                            string result = reader.GetString(0);
                            using (StreamWriter streamWriter = new StreamWriter(fs))
                            {
                                streamWriter.NewLine = "\n";
                                streamWriter.Write(result);
                            }
                        }
                        catch (InvalidCastException ex)
                        {
                            MessageLog.LogError($"There was an error casting the result to a string: {ex}");
                            throw new FileDownloadException("An exception occurred when casting the result to a string", ex);
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
            catch (Exception ex)
            {
                MessageLog.LogError($"An error occured when retrieving data when executing command {oracleCommand.CommandText} for file {sourceFilename}: {ex}");
                throw new FileDownloadException($"An exception ocurred when downloading the file: {ex}");
            }
        }

        /// <summary>
        /// Deletes a watched file from the database.
        /// </summary>
        /// <param name="dirWatcher">The directory watcher that manages the file.</param>
        /// <param name="watchedFile">The file to delete.</param>
        public void DeleteDatabaseFile(DirectoryWatcher dirWatcher, WatchedFile watchedFile)
        {
            MessageLog.LogInfo($"Deleing the database file of {watchedFile.FilePath}");

            OracleConnection connection = this.OpenConnection();
            OracleCommand oracleCommand = connection.CreateCommand();

            oracleCommand.CommandText = dirWatcher.Descriptor.DeleteStatement;

            this.BindParametersToCommand(connection, oracleCommand, dirWatcher, watchedFile.FilePath);

            try
            {
                oracleCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageLog.LogError($"An error occurred when deleting the database file for the file {watchedFile.FilePath}");
                throw new FileDeleteException($"An exception occurred when deleting the file {watchedFile.FilePath}", ex);
            }
        }

        /// <summary>
        /// Gets whether the given file is synchronised with the database or not.
        /// </summary>
        /// <param name="dirWatcher">The directory watcher that manages the file.</param>
        /// <param name="watchedFile">The file to check for synchronisation.</param>
        /// <returns>True if the file exists on the database, otherwise false.</returns>
        public bool IsFileSynchronised(DirectoryWatcher dirWatcher, WatchedFile watchedFile)
        {
            MessageLog.LogInfo($"Checking file synchronisation of {watchedFile.FilePath}");
            try
            {
                OracleConnection oracleConnection = this.OpenConnection();
                OracleCommand oracleCommand = oracleConnection.CreateCommand();

                oracleCommand.CommandText = dirWatcher.Descriptor.DatabaseFileExistsStatement;
                this.BindParametersToCommand(oracleConnection, oracleCommand, dirWatcher, watchedFile.FilePath);

                MessageLog.LogInfo($"Executing file existence query: {oracleCommand.CommandText}");
                object result = oracleCommand.ExecuteScalar();
                decimal count = (decimal)result;
                return count >= 1;
            }
            catch (Exception ex)
            {
                MessageLog.LogError($"An exception occurred when determining whether {watchedFile.FilePath} is in the database: {ex.Message}");
                throw new FileSynchronisationCheckException($"An exception occurred when determining whether {watchedFile.FilePath} is in the database: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="DirectoryWatcher"/> and the <see cref="WatchedFile"/> for the file with the given path, if it exists
        /// </summary>
        /// <param name="filepath">The full path of the file to find.</param>
        /// <returns>A tuple with the <see cref="DirectoryWatcher"/> and the <see cref="WatchedFile"/> respecively if the file exists, otherwise null </returns>
        public Tuple<DirectoryWatcher, WatchedFile> GetWatchedNodeForPath(string filepath)
        {
            foreach (var watchedDir in this.DirectoryWatcherList)
            {
                if (watchedDir.RootDirectory.FindNode(filepath) is WatchedFile node)
                {
                    return Tuple.Create(watchedDir, node);
                }
            }

            return null;
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

            if (this.directoryDescriptorFileWatcher != null)
            {
                this.directoryDescriptorFileWatcher.Dispose();
                this.directoryDescriptorFileWatcher = null;
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
                catch (IOException ex)
                {
                    MessageLog.LogInfo($"An error occurred when deleting temporary file {filename}: {ex}");
                }
            }

            this.eventProcessingTimer?.Dispose();
            this.eventProcessingTimer = null;

            if (this.storedConnection != null)
            {
                this.storedConnection.Close();
                this.storedConnection.Dispose();
                this.storedConnection = null;
            }
        }

        /// <summary>
        /// Closes the current connection to the database.
        /// </summary>
        private void CloseConnection()
        {
            this.storedConnection.Close();
            this.storedConnection = null;
        }

        /// <summary>
        /// Binds all available parameters to the given command.
        /// This can be called by itself if the data type of the file is required (this can cause an infinite loop if the data type SQL needs the data type)
        /// </summary>
        /// <param name="connection">The Oracle connection to use.</param>
        /// <param name="command">The command to bind parameters to.</param>
        /// <param name="watcher">The parent directory watcher of the file.</param>
        /// <param name="path">The path of the file to bind the parameters for.</param>
        private void BindParametersToCommand(OracleConnection connection, OracleCommand command, DirectoryWatcher watcher, string path)
        {
            MessageLog.LogInfo($"Binding parameters to command: \n{command.CommandText}");

            command.BindByName = true;

            if (command.ContainsParameter(FilenameParameterName))
            {
                var param = new OracleParameter(FilenameParameterName, Path.GetFileName(path));
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter(FilenameWithoutExtensionParameterName))
            {
                var param = new OracleParameter(FilenameWithoutExtensionParameterName, Path.GetFileNameWithoutExtension(path));
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter(FileExtensionParameterName))
            {
                var param = new OracleParameter(FileExtensionParameterName, Path.GetExtension(path));
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter(RelativePathParameterName))
            {
                Uri baseUri = new Uri(watcher.DirectoryPath);
                Uri fileUri = new Uri(path);
                Uri relativeUri = baseUri.MakeRelativeUri(fileUri);
                var param = new OracleParameter(RelativePathParameterName, relativeUri.OriginalString);
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter(ParentDirectoryParameterName))
            {
                var param = new OracleParameter(ParentDirectoryParameterName, new FileInfo(path).Directory.Name);
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter(FullPathParameterName))
            {
                var param = new OracleParameter(FullPathParameterName, path);
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter(MimeTypeParameterName))
            {
                string mimeType = this.GetMimeTypeForFile(watcher, path);
                var param = new OracleParameter();
                param.ParameterName = MimeTypeParameterName;
                param.Value = mimeType;
                command.Parameters.Add(param);
            }

            // You need to be very careful here, if the GetDataTypeForFile query needs the data type parameter, an infinite loop will occur
            if (command.ContainsParameter(DataTypeParameterName))
            {
                string dataType = this.GetDataTypeForFile(watcher, path);
                var param = new OracleParameter();
                param.ParameterName = DataTypeParameterName;
                param.Value = dataType;
                command.Parameters.Add(param);
            }

            if (command.ContainsParameter(FileContentClobParameterName) || command.ContainsParameter(FileContentBlobParameterName))
            {
                // Wait for the file to unlock
                using (FileStream fs = Utils.WaitForFile(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    // Binary mode
                    if (command.ContainsParameter(FileContentBlobParameterName))
                    {
                        fs.Position = 0;
                        byte[] fileData = new byte[fs.Length];
                        fs.Read(fileData, 0, Convert.ToInt32(fs.Length));

                        OracleParameter param = new OracleParameter();
                        param.Value = fileData;
                        param.OracleDbType = OracleDbType.Blob;
                        param.ParameterName = FileContentBlobParameterName;
                        command.Parameters.Add(param);
                    }

                    if (command.ContainsParameter(FileContentClobParameterName))
                    {
                        // Text mode
                        fs.Position = 0;
                        var tr = new StreamReader(fs);

                        string contents = tr.ReadToEnd();

                        OracleParameter param = new OracleParameter();
                        param.OracleDbType = OracleDbType.Clob;
                        param.Value = contents;
                        param.OracleDbType = OracleDbType.Clob;
                        param.ParameterName = FileContentClobParameterName;
                        command.Parameters.Add(param);
                    }
                }
            }

            if (command.ContainsParameter(FooterMessageParameterName))
            {
                string footer = Settings.Default.AppendFooterToDatabaseFiles ?
                                $" Last clobbed by user {Environment.UserName}"
                              + $" on machine {Environment.MachineName}"
                              + $" at {DateTime.Now}"
                              + $" (Lobster {Assembly.GetExecutingAssembly().GetName().Version.ToString()}"
                              + $" built on {Utils.RetrieveLinkerTimestamp().ToShortDateString()})" : string.Empty;

                var param = new OracleParameter();
                param.ParameterName = FooterMessageParameterName;
                param.Value = footer;
                command.Parameters.Add(param);
            }

            foreach (OracleParameter parameter in command.Parameters)
            {
                string parameterValue = parameter.Value.ToString();
                parameterValue = parameterValue.Substring(0, parameterValue.Length < ParameterLogLength ? parameterValue.Length : ParameterLogLength);
                MessageLog.LogInfo($"Added parameter '{parameter.ParameterName}' with value '{parameterValue}'");
            }
        }

        /// <summary>
        /// Queries the database for the data type for the content of the given file.
        /// </summary>
        /// <param name="watcher">The directory watcher that the given file is the parent of.</param>
        /// <param name="path">The path of the file to find the data type for.</param>
        /// <returns>"BLOB" if the given file is a binary file, otherwise "CLOB".</returns>
        private string GetDataTypeForFile(DirectoryWatcher watcher, string path)
        {
            OracleConnection connection = this.OpenConnection();

            if (watcher.Descriptor.FileDataTypeStatement == null)
            {
                if (watcher.Descriptor.DefaultDataType == null)
                {
                    // Default to CLOB if there is no default or statement
                    return "CLOB";
                }
                else
                {
                    // Use the default if there is no statement (but there is a default)
                    return watcher.Descriptor.DefaultDataType;
                }
            }

            // Otherwise use the statement
            OracleCommand command = connection.CreateCommand();
            command.CommandText = watcher.Descriptor.FileDataTypeStatement;
            if (command.ContainsParameter(DataTypeParameterName))
            {
                throw new InvalidOperationException($"The FileDataTypeStatement cannot contain the '{DataTypeParameterName}' parameter");
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
        /// Queries the database for the mimetype of the given file
        /// </summary>
        /// <param name="watcher">The directory watcher that the given file is the parent of.</param>
        /// <param name="path">The path of the file to find the data type for.</param>
        /// <returns>The mime type of the file</returns>
        private string GetMimeTypeForFile(DirectoryWatcher watcher, string path)
        {
            OracleConnection connection = this.OpenConnection();

            OracleCommand command = connection.CreateCommand();
            command.CommandText = watcher.Descriptor.FileMimeTypeStatement;
            if (command.ContainsParameter(MimeTypeParameterName))
            {
                throw new InvalidOperationException($"The FileDataTypeStatement cannot contain the '{DataTypeParameterName}' parameter");
            }

            this.BindParametersToCommand(connection, command, watcher, path);

            object result = command.ExecuteScalar();

            if (!(result is string))
            {
                throw new InvalidOperationException($"No MimeType was returned for {path}");
            }

            return (string)result;
        }

        /// <summary>
        /// The listener for when a file is changed within the directory descriptor folder.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnDirectoryDescriptorChangeEvent(object sender, FileSystemEventArgs args)
        {
            lock (this.fileEventQueue)
            {
                lock (this.fileEventProcessingSemaphore)
                {
                    this.DirectoryDescriptorChangeEvent?.Invoke(this, args);
                }
            }
        }

        /// <summary>
        /// Called when a file changes in a watched directory.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments</param>
        private void OnDirectoryWatcherFileChangeEvent(object sender, DirectoryWatcherFileChangeEventArgs args)
        {
            Thread thread = new Thread(() => this.EnqueueFileEvent(args.Watcher, args.WatchedFile, args.Args));
            thread.Start();
        }

        /// <summary>
        /// Takes a single file event, and pushes it onto the event stack, before processing that event.
        /// </summary>
        /// <param name="watcher">The directory from where the event originated.</param>
        /// <param name="watchedFile">The file that caused the event.</param>
        /// <param name="args">The event arguments.</param>
        private void EnqueueFileEvent(DirectoryWatcher watcher, WatchedFile watchedFile, FileSystemEventArgs args)
        {
            this.LogFileEvent($"File change event of type {args.ChangeType} for file {args.FullPath} with a write time of " + File.GetLastWriteTime(args.FullPath).ToString("yyyy-MM-dd HH:mm:ss.fff"));

            lock (this.fileEventProcessingSemaphore)
            {
                if (args.ChangeType != WatcherChangeTypes.Changed)
                {
                    this.fileTreeChangeInQueue = true;
                }

                // Ignore the event if it is already in the event list.
                // This often occurs, as most programs invoke several file change events when saving.
                if (this.fileEventQueue.Find(x => x.EventArgs.FullPath.Equals(args.FullPath, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    this.LogFileEvent("Ignoring event, as it is already in the list.");
                    return;
                }

                if (this.fileEventQueue.Count == 0)
                {
                    this.LogFileEvent("Event stack populated.");
                    this.OnEventProcessingStart();
                }

                this.fileEventQueue.Add(new FileChangeEvent(watcher, watchedFile, args));

                if (this.eventProcessingTimer == null)
                {
                    this.eventProcessingTimer = new Timer(_ => this.BulkProcessFileEvents());
                }

                // Start the delayed event (or restart if it has already been started)
                this.eventProcessingTimer.Change(TimeSpan.FromMilliseconds(Settings.Default.FileUpdateTimeoutMilliseconds), TimeSpan.Zero);
            }
        }

        /// <summary>
        /// Processes all file events waiting in the queue.
        /// This method is run on <see cref="eventProcessingTimer"/>.
        /// </summary>
        private void BulkProcessFileEvents()
        {
            this.eventProcessingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            lock (this.fileEventProcessingSemaphore)
            {
                while (this.fileEventQueue.Count > 0)
                {
                    FileChangeEvent change = this.fileEventQueue[0];
                    this.fileEventQueue.RemoveAt(0);
                    this.ProcessFileEvent(change);
                }
            }

            this.LogFileEvent("Event stack empty.");
            this.OnFileProcessingFinished(this.fileTreeChangeInQueue);
        }

        /// <summary>
        /// Processes a single file event. Sending a file update to the database if necessary.
        /// </summary>
        /// <param name="e">The event to process.</param>
        private void ProcessFileEvent(FileChangeEvent e)
        {
            this.LogFileEvent($"Processing file event of type {e.EventArgs.ChangeType} for file {e.EventArgs.FullPath}");

            FileInfo fileInfo = new FileInfo(e.EventArgs.FullPath);

            if (e.EventArgs.ChangeType != WatcherChangeTypes.Changed)
            {
                this.LogFileEvent($"Unsupported file event {e.EventArgs.ChangeType}");
                return;
            }

            if (!this.IsAutomaticClobbingEnabled)
            {
                this.LogFileEvent($"Automatic clobbing is disabled, ignoring event.");
                return;
            }

            if (!fileInfo.Exists)
            {
                this.LogFileEvent($"File could not be found {e.EventArgs.FullPath}");
                return;
            }

            if (e.WatchedFile == null || !this.IsFileSynchronised(e.Watcher, e.WatchedFile))
            {
                this.LogFileEvent("The file is not synchronised and does not need to be updated.");
                return;
            }

            if (fileInfo.IsReadOnly)
            {
                this.LogFileEvent($"File is read only and will be skipped {e.EventArgs.FullPath}");
                return;
            }

            if (!e.Watcher.Descriptor.PushOnFileChange)
            {
                this.LogFileEvent("The Directory Descriptor does not allow autuomatic file updates, and the file will be skipped.");
                return;
            }

            this.LogFileEvent($"Auto-updating file {e.EventArgs.FullPath}");

            try
            {
                this.UpdateDatabaseFile(e.Watcher, e.EventArgs.FullPath);
            }
            catch (Exception ex)
            {
                this.OnAutoUpdateComplete(e.EventArgs.FullPath, false, ex);
                return;
            }

            this.OnAutoUpdateComplete(e.EventArgs.FullPath, true, null);
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
        /// <param name="ex">The exception that was thrown (on failure)</param>
        private void OnAutoUpdateComplete(string filename, bool success, Exception ex)
        {
            var handler = this.UpdateCompleteEvent;
            FileUpdateCompleteEventArgs args = new FileUpdateCompleteEventArgs(filename, success, ex);
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

        /// <summary>
        /// Signals that an error occurred when loading the directory descriptors
        /// </summary>
        /// <param name="errorMessage">The message for the error that occurred.</param>
        private void OnConnectionLoadError(string errorMessage)
        {
            this.ConnectionLoadErrorEvent?.Invoke(this, new ConnectionLoadErrorEventArgs(errorMessage));
        }

        /// <summary>
        /// The arguments in case of an error during connection creation.
        /// </summary>
        public class ConnectionLoadErrorEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionLoadErrorEventArgs"/> class. 
            /// </summary>
            /// <param name="errorMessage">The error message.</param>
            public ConnectionLoadErrorEventArgs(string errorMessage)
            {
                this.ErrorMessage = errorMessage;
            }

            /// <summary>
            /// Gets the message of this event.
            /// </summary>
            public string ErrorMessage { get; }
        }

        /// <summary>
        /// A class used to cache file events until they are processed by the <see cref="eventProcessingTimer"/>. 
        /// </summary>
        private class FileChangeEvent
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FileChangeEvent"/> class.
            /// </summary>
            /// <param name="watcher">The watcher.</param>
            /// <param name="watchedFile">The file that the event is for.</param>
            /// <param name="args">The args.</param>
            public FileChangeEvent(DirectoryWatcher watcher, WatchedFile watchedFile, FileSystemEventArgs args)
            {
                this.Watcher = watcher;
                this.WatchedFile = watchedFile;
                this.EventArgs = args;
            }

            /// <summary>
            /// Gets the directory watcher under which the file was that triggered the event
            /// </summary>
            public DirectoryWatcher Watcher { get; }

            /// <summary>
            /// Gets the watched file that triggered this event
            /// </summary>
            public WatchedFile WatchedFile { get; }

            /// <summary>
            /// Gets the arguments of the file change event.
            /// </summary>
            public FileSystemEventArgs EventArgs { get; }
        }
    }
}
