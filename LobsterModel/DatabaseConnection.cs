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
        /// The internal mime type list.
        /// </summary>
        [Obsolete]
        private MimeTypeList mimeTypeList;

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
        /// The event for when a Table selection is required by the user.
        /// </summary>
        [Obsolete]
        public event EventHandler<TableRequestEventArgs> RequestTableEvent;

        /// <summary>
        /// The event for when a mime type is needed.
        /// </summary>
        [Obsolete]
        public event EventHandler<MimeTypeRequestEventArgs> RequestMimeTypeEvent;

        /// <summary>
        /// The event for when a ClobType in the LobsterTypes directory has changed.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> ClobTypeChangedEvent;

        /// <summary>
        /// Gets or sets the list of mime types that are used to translate from file names to database mnemonics and vice-sersa.
        /// </summary>
        [Obsolete]
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
        public ConnectionConfig Config { get; }

        /// <summary>
        /// Gets the list of ClobDirectories for each ClobType located in directory in the configuration settings.
        /// </summary>
        [Obsolete]
        public List<ClobDirectory> ClobDirectoryList { get; private set; }

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

            MessageLog.LogInfo("Connection change successful");

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
            foreach(var dirDesc in directoryDescriptors)
            {
                try
                {
                    DirectoryWatcher dirWatcher = new DirectoryWatcher(this.Config.Parent.CodeSourceDirectory, dirDesc);
                    //TODO: dirWatcher.FileChangeEvent += this.OnClobDirectoryFileChangeEvent;
                    this.DirectoryWatcherList.Add(dirWatcher);
                }
                catch(ClobTypeLoadException ex)
                {
                    MessageLog.LogError($"An error occurred when loading the DirectoryDescriptor {dirDesc.Name}: {ex}");
                }
            }
        }

        /// <summary>
        /// Loads each of the xml files in the ClobTypeDir (if they are valid).
        /// </summary>
        /// <param name="errors">Any errors that are raised during loading.</param>
        [Obsolete]
        public void LoadClobTypes(ref List<ClobTypeLoadException> errors)
        {
            string clobTypeDir = this.Config.Parent.ClobTypeDirectory;

            this.ClobDirectoryList = new List<ClobDirectory>();
            if (!Directory.Exists(this.Config.Parent.ClobTypeDirectory))
            {
                MessageLog.LogWarning($"The directory {clobTypeDir} could not be found when loading connection {this.Config.Name}");
                errors.Add(new ClobTypeLoadException($"The directory {clobTypeDir} could not be found when loading connection {this.Config.Name}"));
                return;
            }

            List<ClobType> clobTypes = ClobType.GetClobTypeList(clobTypeDir);
            clobTypes.ForEach(x => x.Initialise());

            foreach (ClobType clobType in clobTypes)
            {
                try
                {
                    ClobDirectory clobDir = new ClobDirectory(this.Config.Parent.CodeSourceDirectory, clobType);
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
        [Obsolete]
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
        [Obsolete]
        public void ReloadClobTypes()
        {
            List<ClobTypeLoadException> errors = new List<ClobTypeLoadException>();
            List<FileListRetrievalException> fileLoadErrors = new List<FileListRetrievalException>();
            this.LoadClobTypes(ref errors);
            this.GetDatabaseFileLists(ref fileLoadErrors);
        }

        /// <summary>
        /// Queries the database for all files in all ClobDireectories and stores them in the directory.
        /// </summary>
        /// <param name="errorList">The list of errors that were encountered when getting the database files.</param>
        [Obsolete]
        public void GetDatabaseFileLists(ref List<FileListRetrievalException> errorList)
        {
            OracleConnection oracleConnection = this.Config.OpenSqlConnection(this.Password);
            if (oracleConnection == null)
            {
                MessageLog.LogError("Connection failed, cannot diff files with database.");
                return;
            }

            foreach (ClobDirectory clobDir in this.ClobDirectoryList)
            {
                if (!clobDir.Directory.Exists)
                {
                    continue;
                }

                try
                {
                    this.GetDatabaseFileListForDirectory(clobDir, oracleConnection);
                }
                catch (FileListRetrievalException ex)
                {
                    errorList.Add(ex);
                }
            }
        }

        /// <summary>
        /// Updates the database record of the target file with the data from the source file (which are normally the same file).
        /// </summary>
        /// <param name="clobDir">The directoy that the file reside in.</param>
        /// <param name="targetFilename">The file to update the database record for.</param>
        /// <param name="sourceFilename">The file to get the data fro the update the record with.</param>
        [Obsolete]
        public void UpdateClobWithExternalFile(ClobDirectory clobDir, string targetFilename, string sourceFilename)
        {
            try
            {
                OracleConnection oracleConnection = this.Config.OpenSqlConnection(this.Password);
                DBClobFile clobFile = clobDir?.GetDatabaseFileForFullpath(targetFilename);

                if (Settings.Default.BackupEnabled)
                {
                    this.BackupClobFile(oracleConnection, clobFile, targetFilename);
                }

                this.UpdateDatabaseClob(sourceFilename, clobFile, clobDir, oracleConnection);
                oracleConnection.Dispose();
            }
            catch (Exception ex) when (ex is ConnectToDatabaseException || ex is ClobFileLookupException || ex is FileDownloadException)
            {
                throw new FileUpdateException("An error occurred when connecting to the database.", ex);
            }
        }

        /// <summary>
        /// Downloads a copy of the given DBClobFile and stores a copy in the backup folder located in settings.
        /// </summary>
        /// <param name="oracleConnection">The database connection to download the file with.</param>
        /// <param name="clobFile">The file information that is going to be backed up.</param>
        /// <param name="fullpath">The path of the file as it exists locally.</param>
        [Obsolete]
        public void BackupClobFile(OracleConnection oracleConnection, DBClobFile clobFile, string fullpath)
        {
            FileInfo backupFile = BackupLog.AddBackup(this.Config.Parent.CodeSourceDirectory, fullpath);
            this.DownloadClobDataToFile(clobFile, oracleConnection, backupFile.FullName);
        }

        /// <summary>
        /// Updates a database file with its local content.
        /// </summary>
        /// <param name="clobDir">The directory that the file was updated in.</param>
        /// <param name="fullpath">The file to update.</param>
        [Obsolete]
        public void SendUpdateClobMessage(ClobDirectory clobDir, string fullpath)
        {
            this.UpdateClobWithExternalFile(clobDir, fullpath, fullpath);
        }

        /// <summary>
        /// Inserts the given file into the database, polling the event listener 
        /// for table and mimetype information if required.
        /// </summary>
        /// <param name="clobDir">The directory the file resides in.</param>
        /// <param name="fullpath">The path of the file to insert.</param>
        /// <param name="returnFile">The database file that represents what was just inserted.</param>
        /// <returns>A value indicating whether the insert was followed through.</returns>
        [Obsolete]
        public bool SendInsertClobMessage(ClobDirectory clobDir, string fullpath, ref DBClobFile returnFile)
        {
            Table table = null;
            if (clobDir.ClobType.Tables.Count > 1)
            {
                table = this.OnTableRequest(this, fullpath, clobDir.ClobType.Tables.ToArray());
                if (table == null)
                {
                    return false;
                }
            }
            else
            {
                table = clobDir.ClobType.Tables[0];
            }

            string mimeType = null;
            Column mimeTypeColumn;
            Column dataColumn;
            if (table.TryGetColumnWithPurpose(Column.Purpose.MIME_TYPE, out mimeTypeColumn)
                && table.TryGetColumnWithPurpose(Column.Purpose.CLOB_DATA, out dataColumn))
            {
                mimeType = this.OnMimeTypeRequest(this, fullpath, dataColumn.MimeTypeList.ToArray());

                if (mimeType == null)
                {
                    return false;
                }
            }

            OracleConnection oracleConnection = this.Config.OpenSqlConnection(this.Password);
            try
            {
                DBClobFile databaseFile = this.InsertDatabaseClob(fullpath, clobDir, table, mimeType, oracleConnection);
                clobDir.DatabaseFileList.Add(databaseFile);
                returnFile = databaseFile;
            }
            finally
            {
                oracleConnection.Dispose();
            }

            return true;
        }

        /// <summary>
        /// Public access for downloading database files.
        /// </summary>
        /// <param name="databaseFile">The file name to download the file for.</param>
        /// <returns>The path of the resulting file.</returns>
        [Obsolete]
        public string SendDownloadClobDataToFileMessage(DBClobFile databaseFile)
        {
            OracleConnection oracleConnection = this.Config.OpenSqlConnection(this.Password);
            if (oracleConnection == null)
            {
                return null;
            }

            string filepath = Utils.GetTempFilepath(databaseFile.Filename);
            try
            {
                this.DownloadClobDataToFile(databaseFile, oracleConnection, filepath);
                this.TempFileList.Add(filepath);
                return filepath;
            }
            finally
            {
                oracleConnection.Dispose();
            }
        }

        /// <summary>
        /// Converts the name of a local file to a suitable database mnemonic.
        /// </summary>
        /// <param name="fullpath">The file to convert.</param>
        /// <param name="table">The table the file would be inserted into.</param>
        /// <param name="mimeType">The mime type the file would be inserted as.</param>
        /// <returns>The database mnemonic representation of the file.</returns>
        [Obsolete]
        public string ConvertFilenameToMnemonic(string fullpath, Table table, string mimeType)
        {
            string mnemonic = Path.GetFileNameWithoutExtension(fullpath);

            // If the table stores mime types, then the mnemonic will also need to have 
            // the prefix representation of the mime type prefixed to it.
            Column mimeTypeColumn;
            if (table.TryGetColumnWithPurpose(Column.Purpose.MIME_TYPE, out mimeTypeColumn))
            {
                MimeTypeList.MimeType mt = this.MimeTypeList.MimeTypes.Find(x => x.Name.Equals(mimeType));
                if (mt == null)
                {
                    throw new MimeTypeNotFoundException($"Unknown mime-to-prefix key {mimeType}");
                }

                if (!string.IsNullOrEmpty(mt.Prefix))
                {
                    mnemonic = $"{mt.Prefix}/{mnemonic}";
                }
            }

            return mnemonic;
        }

        /// <summary>
        /// Inserts the given local file into the database.
        /// </summary>
        /// <param name="fullpath">The path of the file to insert ito the database.</param>
        /// <param name="clobDir">The clob directory that the file is being inserted into.</param>
        /// <param name="table">The table to insert the file into.</param>
        /// <param name="mimeType">The mimetype to insert the file as.</param>
        /// <param name="oracleConnection">The Oracle connection to use.</param>
        /// <returns>True if the file was inserted successfully, otherwise false.</returns>
        [Obsolete]
        public DBClobFile InsertDatabaseClob(string fullpath, ClobDirectory clobDir, Table table, string mimeType, OracleConnection oracleConnection)
        {
            OracleCommand command = oracleConnection.CreateCommand();

            // Execute the insert in a transaction so that if inserting the child record fails, the parent insert can be rolled back
            OracleTransaction trans = oracleConnection.BeginTransaction();
            string mnemonic = this.ConvertFilenameToMnemonic(fullpath, table, mimeType);

            bool usingCustomLoader = table.CustomStatements?.UpsertStatement != null;

            // First insert the parent record in the table (if applicable)
            if (table.ParentTable != null && !usingCustomLoader)
            {
                try
                {
                    command.CommandText = SqlBuilder.BuildInsertParentStatement(table, mnemonic);
                    MessageLog.LogInfo($"Executing Insert query on parent table: {command.CommandText}");
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (Exception e) when (e is InvalidOperationException || e is OracleException)
                {
                    MessageLog.LogError($"Error creating new clob when executing command: {command.CommandText} {e}");
                    throw new FileInsertException("An exception ocurred when attempting to insert a file into the parent table.", e);
                }
            }

            command = oracleConnection.CreateCommand();
            if (usingCustomLoader)
            {
                command.CommandText = table.CustomStatements.UpsertStatement;

                if (command.CommandText.Contains(":name"))
                {
                    command.Parameters.Add(new OracleParameter(":name", Path.GetFileName(fullpath)));
                }
            }
            else
            {
                command.CommandText = SqlBuilder.BuildInsertChildStatement(table, mnemonic, mimeType);
            }

            try
            {
                command.Parameters.Add(SqlBuilder.CreateFileDataParameter(fullpath, table, mimeType));
                MessageLog.LogInfo($"Executing insert query: {command.CommandText}");
                command.ExecuteNonQuery();
                command.Dispose();
                trans.Commit();
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleException || e is IOException || e is ColumnNotFoundException)
            {
                // Discard the insert made into the parent table
                trans.Rollback();
                MessageLog.LogError($"Error creating new clob when executing command: {command.CommandText} {e}");
                throw new FileInsertException("An exception ocurred when attempting to insert a file into the child table.", e);
            }

            DBClobFile clobFile = new DBClobFile(table, mnemonic, mimeType, Path.GetFileName(fullpath));
            MessageLog.LogInfo($"Clob file creation successful: {fullpath}");
            return clobFile;
        }

        /// <summary>
        /// Downloads the current content of a file on the database to a local temp file.
        /// </summary>
        /// <param name="clobFile">The file to download.</param>
        /// <param name="oracleConnection">The connection to use.</param>
        /// <param name="filename">The filepath to download the data into.</param>
        [Obsolete]
        public void DownloadClobDataToFile(DBClobFile clobFile, OracleConnection oracleConnection, string filename)
        {
            OracleCommand command = oracleConnection.CreateCommand();

            Table table = clobFile.ParentTable;
            Column column = null;
            try
            {
                if (table.CustomStatements?.DownloadStatement != null)
                {
                    command.CommandText = table.CustomStatements.DownloadStatement;
                }
                else
                {
                    column = clobFile.GetDataColumn();
                    command.CommandText = SqlBuilder.BuildGetDataCommand(table, clobFile);
                }

                command.Parameters.Add("mnemonic", clobFile.Mnemonic);
            }
            catch (ColumnNotFoundException e)
            {
                MessageLog.LogError($"The data colum could not be found when creating the download data statement: {e}");
                throw new FileDownloadException("The statement could not be constructed.", e);
            }

            try
            {
                OracleDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    MessageLog.LogError($"No data found on clob retrieval of {clobFile.Mnemonic} when executing command: {command.CommandText}");
                    throw new FileDownloadException($"No data was found for the given command {command.CommandText}");
                }

                using (FileStream fs = Utils.WaitForFile(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                {
                    if (column?.DataType == Column.Datatype.BLOB)
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
                    MessageLog.LogError($"Too many rows found on clob retrieval of {clobFile.Mnemonic}");
                    throw new FileDownloadException($"Too many rows were found for the given command {command.CommandText}");
                }
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleException || e is OracleNullValueException || e is IOException)
            {
                MessageLog.LogError($"Error retrieving data when executing command {command.CommandText} {e}");
                throw new FileDownloadException($"An exception ocurred when executing the command {command.CommandText}");
            }
        }

        private void BindParametersToCommand(OracleConnection connection, OracleCommand command, DirectoryWatcher watcher, string path)
        {
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
        /// Finds all DBClobFIles from the tables in the given ClobDirectory and populates its lists with them.
        /// </summary>
        /// <param name="clobDir">The directory to get the file lists for.</param>
        /// <param name="oracleConnection">The Oracle connection to use.</param>
        [Obsolete]
        public void GetDatabaseFileListForDirectory(ClobDirectory clobDir, OracleConnection oracleConnection)
        {
            clobDir.DatabaseFileList = new List<DBClobFile>();
            ClobType ct = clobDir.ClobType;
            OracleCommand oracleCommand = oracleConnection.CreateCommand();

            foreach (Table table in ct.Tables)
            {
                if (table.CustomStatements?.FileListStatement != null)
                {
                    oracleCommand.CommandText = table.CustomStatements.FileListStatement;
                }
                else
                {
                    oracleCommand.CommandText = SqlBuilder.GetFileListCommand(table);
                }

                this.ProcessFileList(clobDir, oracleCommand, table);
            }
        }

        /// <summary>
        /// Process the given SQL command to get files for a clob directory.
        /// </summary>
        /// <param name="clobDirectory">The clob directory to add the file to.</param>
        /// <param name="oracleCommand">The command to execut.</param>
        /// <param name="table">The table to insert with.</param>
        [Obsolete]
        public void ProcessFileList(ClobDirectory clobDirectory, OracleCommand oracleCommand, Table table)
        {
            try
            {
                OracleDataReader reader = oracleCommand.ExecuteReader();
                while (reader.Read())
                {
                    string mnemonic = reader.GetString(0);
                    string mimeType = null;

                    if (reader.FieldCount > 1)
                    {
                        mimeType = reader.GetString(1);
                    }

                    DBClobFile databaseFile = new DBClobFile(
                        parentTable: table,
                        mnemonic: mnemonic,
                        mimetype: mimeType,
                        filename: this.ConvertMnemonicToFilename(mnemonic, table, mimeType));

                    clobDirectory.DatabaseFileList.Add(databaseFile);
                }

                reader.Close();
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleException || e is ColumnNotFoundException)
            {
                MessageLog.LogError($"Error retrieving file lists from database for {clobDirectory.ClobType.Name} when executing command {oracleCommand.CommandText}: {e}");
                throw new FileListRetrievalException($"Error retrieving file lists from database for {clobDirectory.ClobType.Name} when executing command {oracleCommand.CommandText}", e);
            }
            finally
            {
                oracleCommand.Dispose();
            }
        }

        /// <summary>
        /// Inserts the given file into the database.
        /// If the insert is successful, then the database information about that file will be set.
        /// </summary>
        /// <param name="fullpath">The path of the local file to use as the dat asource.</param>
        /// <param name="clobFile">The file to insert into the database.</param>
        /// <param name="clobDir">The clob directory that the file is being inserted into.</param>
        /// <param name="oracleConnection">The Oracle connction to use.</param>
        [Obsolete]
        public void UpdateDatabaseClob(string fullpath, DBClobFile clobFile, ClobDirectory clobDir, OracleConnection oracleConnection)
        {
            OracleTransaction trans = oracleConnection.BeginTransaction();
            OracleCommand command = oracleConnection.CreateCommand();
            Table table = clobFile.ParentTable;

            bool usingCustomLoader = table.CustomStatements?.UpsertStatement != null;
            if (usingCustomLoader)
            {
                command.CommandText = table.CustomStatements.UpsertStatement;

                if (command.CommandText.Contains(":name"))
                {
                    command.Parameters.Add(new OracleParameter(":name", Path.GetFileName(fullpath)));
                }
            }
            else
            {
                try
                {
                    command.CommandText = SqlBuilder.BuildUpdateStatement(table, clobFile);
                }
                catch (ColumnNotFoundException e)
                {
                    MessageLog.LogError($"The data column couldn't be found when building the update statement: {e}");
                    throw new FileUpdateException("The Clob Column could not be found.", e);
                }
            }

            try
            {
                command.Parameters.Add(SqlBuilder.CreateFileDataParameter(fullpath, clobFile.ParentTable, clobFile.MimeType));
            }
            catch (IOException e)
            {
                MessageLog.LogError($"Clob update failed with  for command {command.CommandText} {e}");
                throw new FileUpdateException("An IOException ocurred when attempting to add the file as a data parameter.", e);
            }

            int rowsAffected;
            try
            {
                MessageLog.LogInfo($"Executing Update query: {command.CommandText}");
                rowsAffected = command.ExecuteNonQuery();

                // The row check can't be relied upon when using a custom loader, as it may be in an anonymous block
                if (rowsAffected != 1 && !usingCustomLoader)
                {
                    trans.Rollback();
                    MessageLog.LogError($"In invalid number of rows ({rowsAffected}) were updated for command: {command.CommandText}");

                    throw new FileUpdateException($"{rowsAffected} rows were affected during the update (expected only 1). The transaction has been rolled back.");
                }

                trans.Commit();
                command.Dispose();
                MessageLog.LogInfo($"Clob file update successful: {fullpath}");
                clobFile.LastUpdatedTime = DateTime.Now;
                return;
            }
            catch (Exception e) when (e is OracleException || e is InvalidOperationException)
            {
                trans.Rollback();
                MessageLog.LogError($"Clob update failed for command: {command.CommandText} {e}");
                throw new FileUpdateException($"An invalid operation occurred when updating the database: {e.Message}", e);
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

        /// <summary>
        /// The listener when a child ClobDirectory raises a file change event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        [Obsolete]
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
        [Obsolete]
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
        [Obsolete]
        private void ProcessFileEvent(ClobDirectory clobDirectory, FileSystemEventArgs e)
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
                //this.SendUpdateClobMessage(clobDirectory, e.FullPath);
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

        /// <summary>
        /// Converts the mnemonic of a file on the database to the local filename that it would represent.
        /// </summary>
        /// <param name="mnemonic">The database mnemonic.</param>
        /// <param name="table">The table the mnemonic is from (can be null).</param>
        /// <param name="mimeType">The mime type of the database file, if applicable.</param>
        /// <returns>The name as converted from the mnemonic.</returns>
        [Obsolete]
        private string ConvertMnemonicToFilename(string mnemonic, Table table, string mimeType)
        {
            string filename = mnemonic;
            string prefix = null;

            if (mnemonic.Contains('/'))
            {
                prefix = mnemonic.Substring(0, mnemonic.LastIndexOf('/'));
                filename = mnemonic.Substring(mnemonic.LastIndexOf('/') + 1);
            }

            // Assume xml data types for tables without a datatype column, or a prefix
            Column mimeTypeColumn;
            if (table == null || !table.TryGetColumnWithPurpose(Column.Purpose.MIME_TYPE, out mimeTypeColumn) || prefix == null)
            {
                filename += table?.DefaultExtension ?? ".xml";
            }
            else
            {
                MimeTypeList.MimeType mt = this.MimeTypeList.MimeTypes.Find(x => x.Name.Equals(mimeType));

                if (mt == null)
                {
                    throw new MimeTypeNotFoundException($"Unkown mime-to-extension key {mimeType}");
                }

                filename += mt.Extension;
            }

            return filename;
        }
    }
}
