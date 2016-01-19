//-----------------------------------------------------------------------
// <copyright file="Model.cs" company="marshl">
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
//      'Long ago they fell under the dominion of the One, and they became Ringwraiths,
//      shadows under his great Shadow, his most terrible servants.'
//          -- Gandalf
// 
//      [ _The Lord of the Rings_, I/ii: "The Shadow of the Past"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;
    using Properties;

    /// <summary>
    /// The database connection model 
    /// </summary>
    public static class Model
    {
        /// <summary>
        /// Gets a value indicating whether the currently set connection directory is valid or not.
        /// </summary>
        public static bool IsConnectionDirectoryValid
        {
            get
            {
                return Settings.Default.ConnectionDir != null && Directory.Exists(Settings.Default.ConnectionDir);
            }
        }

        /// <summary>
        /// Gets or sets the directory where connection files are stored.
        /// </summary>
        public static string ConnectionDirectory
        {
            get
            {
                return Settings.Default.ConnectionDir;
            }

            set
            {
                Settings.Default.ConnectionDir = value;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Tests a connection configuration and returns true if a connection could be made.
        /// </summary>
        /// <param name="databaseConfig">The configuration file to test</param>
        /// <param name="e">The exception that was raised, if any.</param>
        /// <returns>True if the connection was successful, otherwise false.</returns>
        public static bool TestConnection(DatabaseConfig databaseConfig, ref Exception e)
        {
            try
            {
                DbConnection con = OpenConnection(databaseConfig);
                con.Close();
                return true;
            }
            catch (ConnectToDatabaseException ex)
            {
                e = ex;
                return false;
            }
        }

        /// <summary>
        /// Updates the database recrod of the target file with the data from the source file (which are normally the same file).
        /// </summary>
        /// <param name="databaseConnection">The connection to update with.</param>
        /// <param name="targetFilename">The file to update the database record for.</param>
        /// <param name="sourceFilename">The file to get the data fro the update the record with.</param>
        public static void UpdateClobWithExternalFile(DatabaseConnection databaseConnection, string targetFilename, string sourceFilename)
        {
            try
            {
                OracleConnection oracleConnection = Model.OpenConnection(databaseConnection.Config);
                ClobDirectory clobDir = databaseConnection.GetClobDirectoryForFile(targetFilename);
                DBClobFile clobFile = clobDir?.GetDatabaseFileForFullpath(targetFilename);

                if (Settings.Default.BackupEnabled)
                {
                    Model.BackupClobFile(databaseConnection, oracleConnection, clobFile, targetFilename);
                }

                Model.UpdateDatabaseClob(databaseConnection, sourceFilename, clobFile, clobDir, oracleConnection);
                oracleConnection.Dispose();
            }
            catch (Exception ex) when (ex is ConnectToDatabaseException || ex is ClobFileLookupException || ex is FileDownloadException)
            {
                throw new FileUpdateException("An error occurred when connecting to the database.", ex);
            }
        }

        /// <summary>
        /// Updates a database file with its local content.
        /// </summary>
        /// <param name="databaseConnection">The connection to update with.</param>
        /// <param name="fullpath">The file to update.</param>
        public static void SendUpdateClobMessage(DatabaseConnection databaseConnection, string fullpath)
        {
            Model.UpdateClobWithExternalFile(databaseConnection, fullpath, fullpath);
        }

        /// <summary>
        /// Inserts the given file into the database, polling the event listener 
        /// for table and mimetype information if required.
        /// </summary>
        /// <param name="databaseConnection">The connection to insert for.</param>
        /// <param name="fullpath">The path of the file to insert.</param>
        /// <param name="returnFile">The database file that represents what was just inserted.</param>
        /// <returns>A value indicating whether the insert was followed through.</returns>
        public static bool SendInsertClobMessage(DatabaseConnection databaseConnection, string fullpath, ref DBClobFile returnFile)
        {
            ClobDirectory clobDir = databaseConnection.GetClobDirectoryForFile(fullpath);

            Table table = null;
            if (clobDir.ClobType.Tables.Count > 1)
            {
                bool result = databaseConnection.EventListener.PromptForTable(fullpath, clobDir.ClobType.Tables.ToArray(), ref table);
                if (!result)
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
                bool result = databaseConnection.EventListener.PromptForMimeType(fullpath, dataColumn.MimeTypeList.ToArray(), ref mimeType);

                if (!result)
                {
                    return false;
                }
            }

            OracleConnection oracleConnection = OpenConnection(databaseConnection.Config);
            try
            {
                DBClobFile databaseFile = Model.InsertDatabaseClob(databaseConnection, fullpath, clobDir, table, mimeType, oracleConnection);
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
        /// <param name="databaseConnection">The connection to download for.</param>
        /// <param name="fullpath">The file name to download the file for.</param>
        /// <returns>The path of the resulting file.</returns>
        public static string SendDownloadClobDataToFileMessage(DatabaseConnection databaseConnection, string fullpath)
        {
            OracleConnection oracleConnection = OpenConnection(databaseConnection.Config);
            if (oracleConnection == null)
            {
                return null;
            }

            DBClobFile clobFile;
            try
            {
                ClobDirectory clobDir = databaseConnection.GetClobDirectoryForFile(fullpath);
                clobFile = clobDir.GetDatabaseFileForFullpath(fullpath);
            }
            catch (Exception e) when (e is ClobFileLookupException)
            {
                throw new FileDownloadException("The path could not be mapped to a DbClobFile");
            }

            if (clobFile == null)
            {
                throw new FileDownloadException("The path was found in a directory, but could be found as a file.");
            }

            string filepath = Utils.GetTempFilepath(clobFile.Filename);
            try
            {
                Model.DownloadClobDataToFile(databaseConnection, clobFile, oracleConnection, filepath);
                databaseConnection.TempFileList.Add(filepath);
                return filepath;
            }
            finally
            {
                oracleConnection.Dispose();
            }
        }

        /// <summary>
        /// Sets the current connection to the given connection, if able.
        /// </summary>
        /// <param name="config">The connection to open.</param>
        /// <param name="eventListener">The event listener that will be used to populate the connection wtih.</param>
        /// <returns>The successfully created database connection.</returns>
        public static DatabaseConnection SetDatabaseConnection(DatabaseConfig config, IModelEventListener eventListener)
        {
            MessageLog.LogInfo($"Changing connection to {config.Name}");
            try
            {
                DbConnection con = OpenConnection(config);
                con.Close();
            }
            catch (ConnectToDatabaseException ex)
            {
                throw new SetConnectionException("A test connection could not be made to the database", ex);
            }

            if (config.ClobTypeDir == null || !Directory.Exists(config.ClobTypeDir))
            {
                throw new SetConnectionException($"The clob type directory {config.ClobTypeDir} could not be found.");
            }

            DatabaseConnection databaseConnection = new DatabaseConnection(config, eventListener);

            List<ClobTypeLoadException> errors = new List<ClobTypeLoadException>();
            List<FileListRetrievalException> fileLoadErrors = new List<FileListRetrievalException>();

            databaseConnection.LoadClobTypes(ref errors);
            Model.GetDatabaseFileLists(databaseConnection, ref fileLoadErrors);

            MessageLog.LogInfo("Connection change successful");

            return databaseConnection;
        }

        /// <summary>
        /// Converts the name of a local file to a suitable database mnemonic.
        /// </summary>
        /// <param name="databaseConnection">The connection to create the mnemonic with.</param>
        /// <param name="fullpath">The file to convert.</param>
        /// <param name="table">The table the file would be inserted into.</param>
        /// <param name="mimeType">The mime type the file would be inserted as.</param>
        /// <returns>The database mnemonic representation of the file.</returns>
        public static string ConvertFilenameToMnemonic(DatabaseConnection databaseConnection, string fullpath, Table table, string mimeType)
        {
            string mnemonic = Path.GetFileNameWithoutExtension(fullpath);

            // If the table stores mime types, then the mnemonic will also need to have 
            // the prefix representation of the mime type prefixed to it.
            Column mimeTypeColumn;
            if (table.TryGetColumnWithPurpose(Column.Purpose.MIME_TYPE, out mimeTypeColumn))
            {
                MimeTypeList.MimeType mt = databaseConnection.MimeTypeList.MimeTypes.Find(x => x.Name.Equals(mimeType));
                if (mt == null)
                {
                    throw new MimeTypeNotFoundException($"Unknown mime-to-prefix key {mimeType}");
                }

                if (mt.Prefix.Length > 0)
                {
                    mnemonic = $"{mt.Prefix}/{mnemonic}";
                }
            }

            return mnemonic;
        }

        /// <summary>
        /// Converts the mnemonic of a file on the database to the local filename that it would represent.
        /// </summary>
        /// <param name="databaseConnection">The connection to make the file with.</param>
        /// <param name="mnemonic">The database mnemonic.</param>
        /// <param name="table">The table the mnemonic is from (can be null).</param>
        /// <param name="mimeType">The mime type of the database file, if applicable.</param>
        /// <returns>The name as converted from the mnemonic.</returns>
        public static string ConvertMnemonicToFilename(DatabaseConnection databaseConnection, string mnemonic, Table table, string mimeType)
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
                MimeTypeList.MimeType mt = databaseConnection.MimeTypeList.MimeTypes.Find(x => x.Name.Equals(mimeType));

                if (mt == null)
                {
                    throw new MimeTypeNotFoundException($"Unkown mime-to-extension key {mimeType}");
                }

                filename += mt.Extension;
            }

            return filename;
        }

        /// <summary>
        /// Queries the database for all files in all ClobDireectories and stores them in the directory.
        /// </summary>
        /// <param name="databaseConnection">The connection to get the file lists for.</param>
        /// <param name="errorList">The list of errors that were encountered when getting the database files.</param>
        public static void GetDatabaseFileLists(DatabaseConnection databaseConnection, ref List<FileListRetrievalException> errorList)
        {
            OracleConnection oracleConnection = OpenConnection(databaseConnection.Config);
            if (oracleConnection == null)
            {
                MessageLog.LogError("Connection failed, cannot diff files with database.");
                return;
            }

            foreach (ClobDirectory clobDir in databaseConnection.ClobDirectoryList)
            {
                if (!Directory.Exists(clobDir.GetFullPath(databaseConnection)))
                {
                    continue;
                }

                try
                {
                    Model.GetDatabaseFileListForDirectory(databaseConnection, clobDir, oracleConnection);
                }
                catch (FileListRetrievalException ex)
                {
                    errorList.Add(ex);
                }
            }
        }

        /// <summary>
        /// Opens a new OracleConnection and returns it.
        /// </summary>
        /// <param name="config">The connection configuration settings to use.</param>
        /// <returns>A new connectionif it opened successfully, otherwise null.</returns>
        private static OracleConnection OpenConnection(DatabaseConfig config)
        {
            try
            {
                OracleConnection con = new OracleConnection();
                con.ConnectionString = "User Id=" + config.Username
                    + (string.IsNullOrEmpty(config.Password) ? null : ";Password=" + config.Password)
                    + ";Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)("
                    + $"HOST={config.Host})"
                    + $"(PORT={config.Port})))(CONNECT_DATA="
                    + $"(SID={config.SID})(SERVER=DEDICATED)))"
                    + $";Pooling=" + (config.UsePooling ? "true" : "false");

                MessageLog.LogInfo($"Connecting to database {config.Name}");
                MessageLog.LogSensitive($"Connection string is {con.ConnectionString}");
                con.Open();
                return con;
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleException || e is ArgumentException || e is SocketException)
            {
                MessageLog.LogError($"Connection to Oracle failed: {e}");
                throw new ConnectToDatabaseException("Failed to open connection.", e);
            }
        }

        /// <summary>
        /// Downloads a copy of the given DBClobFile and stores a copy in the backup folder located in settings.
        /// </summary>
        /// <param name="databaseConnection">The connection to backup the file for.</param>
        /// <param name="oracleConnection">The database connection to download the file with.</param>
        /// <param name="clobFile">The file information that is going to be backed up.</param>
        /// <param name="fullpath">The path of the file as it exists locally.</param>
        private static void BackupClobFile(DatabaseConnection databaseConnection, OracleConnection oracleConnection, DBClobFile clobFile, string fullpath)
        {
            DirectoryInfo backupDir = new DirectoryInfo(Settings.Default.BackupDirectory);
            if (!backupDir.Exists)
            {
                backupDir.Create();
            }

            string tempName = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now)
                + Path.GetFileName(fullpath);
            string backupFilename = Path.GetFullPath(Path.Combine(Settings.Default.BackupDirectory, tempName));

            if (!File.Exists(backupFilename))
            {
                Model.DownloadClobDataToFile(databaseConnection, clobFile, oracleConnection, backupFilename);
                databaseConnection.FileBackupLog.AddBackup(fullpath, backupFilename);
            }
        }

        /// <summary>
        /// Inserts the given file into the database.
        /// If the insert is successful, then the database information about that file will be set.
        /// </summary>
        /// <param name="databaseConnection">The connection to update the file for.</param>
        /// <param name="fullpath">The path of the local file to use as the dat asource.</param>
        /// <param name="clobFile">The file to insert into the database.</param>
        /// <param name="clobDir">The clob directory that the file is being inserted into.</param>
        /// <param name="oracleConnection">The Oracle connction to use.</param>
        private static void UpdateDatabaseClob(DatabaseConnection databaseConnection, string fullpath, DBClobFile clobFile, ClobDirectory clobDir, DbConnection oracleConnection)
        {
            DbTransaction trans = oracleConnection.BeginTransaction();
            DbCommand command = oracleConnection.CreateCommand();
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
                    command.CommandText = table.BuildUpdateStatement(clobFile);
                }
                catch (ColumnNotFoundException e)
                {
                    MessageLog.LogError($"The data column couldn't be found when building the update statement: {e}");
                    throw new FileUpdateException("The Clob Column could not be found.", e);
                }
            }

            try
            {
                command.Parameters.Add(Model.CreateFileDataParameter(fullpath, clobFile.ParentTable, clobFile.MimeType));
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
        /// Adds the contents of the given file into the given command under the alias ":clob"
        /// </summary>
        /// <param name="fullpath">The file which will have its data bound to the query.</param>
        /// <param name="table">The table that the file will be added to.</param>
        /// <param name="mimeType">The mime type the file will be added as, if any.</param>
        /// <returns>The parameter that was created.</returns>
        private static OracleParameter CreateFileDataParameter(string fullpath, Table table, string mimeType)
        {
            OracleParameter param = new OracleParameter();
            param.ParameterName = "clob";

            Column column = table.Columns.Find(
                    x => x.ColumnPurpose == Column.Purpose.CLOB_DATA
                        && (mimeType == null || x.MimeTypeList.Contains(mimeType)));

            // Wait for the file to unlock
            using (FileStream fs = Utils.WaitForFile(
                fullpath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                // Binary mode
                if (column.DataType == Column.Datatype.BLOB)
                {
                    byte[] fileData = new byte[fs.Length];
                    fs.Read(fileData, 0, Convert.ToInt32(fs.Length));

                    param.Value = fileData;
                    OracleParameter op = (OracleParameter)param;
                    op.OracleDbType = OracleDbType.Blob;
                }
                else
                {
                    // Text mode
                    StreamReader sr = new StreamReader(fs);
                    string contents = sr.ReadToEnd();

                    if (Settings.Default.AppendFooterToDatabaseFiles)
                    {
                        contents += Model.GetClobFooterMessage(mimeType);
                    }

                    param.Value = contents;
                    OracleParameter op = (OracleParameter)param;
                    op.OracleDbType = column.DataType == Column.Datatype.XMLTYPE ? OracleDbType.XmlType : OracleDbType.Clob;
                }
            }

            return param;
        }

        /// <summary>
        /// Inserts the given local file into the database.
        /// </summary>
        /// <param name="databaseConnection">The connection to insert the file for.</param>
        /// <param name="fullpath">The path of the file to insert ito the database.</param>
        /// <param name="clobDir">The clob directory that the file is being inserted into.</param>
        /// <param name="table">The table to insert the file into.</param>
        /// <param name="mimeType">The mimetype to insert the file as.</param>
        /// <param name="oracleConnection">The Oracle connection to use.</param>
        /// <returns>True if the file was inserted successfully, otherwise false.</returns>
        private static DBClobFile InsertDatabaseClob(DatabaseConnection databaseConnection, string fullpath, ClobDirectory clobDir, Table table, string mimeType, OracleConnection oracleConnection)
        {
            DbCommand command = oracleConnection.CreateCommand();

            // Execute the insert in a transaction so that if inserting the child record fails, the parent insert can be rolled back
            DbTransaction trans = oracleConnection.BeginTransaction();
            string mnemonic = Model.ConvertFilenameToMnemonic(databaseConnection, fullpath, table, mimeType);

            bool usingCustomLoader = table.CustomStatements?.UpsertStatement != null;

            // First insert the parent record in the table (if applicable)
            if (table.ParentTable != null && !usingCustomLoader)
            {
                try
                {
                    command.CommandText = table.BuildInsertParentStatement(mnemonic);
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
                command.CommandText = table.BuildInsertChildStatement(mnemonic, mimeType);
            }

            try
            {
                command.Parameters.Add(Model.CreateFileDataParameter(fullpath, table, mimeType));
                MessageLog.LogInfo($"Executing insert query: {command.CommandText}");
                command.ExecuteNonQuery();
                command.Dispose();
                trans.Commit();
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleException || e is IOException || e is ColumnNotFoundException)
            {
                // Discard the insert amde into the parent table
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
        /// <param name="databaseConnection">The connection to download the file for.</param>
        /// <param name="clobFile">The file to download.</param>
        /// <param name="oracleConnection">The connection to use.</param>
        /// <param name="filename">The filepath to download the data into.</param>
        private static void DownloadClobDataToFile(DatabaseConnection databaseConnection, DBClobFile clobFile, OracleConnection oracleConnection, string filename)
        {
            OracleCommand command = oracleConnection.CreateCommand();

            Table table = clobFile.ParentTable;
            Column column;
            try
            {
                column = clobFile.GetDataColumn();
                if (table.CustomStatements?.DownloadStatement != null)
                {
                    command.CommandText = table.CustomStatements.DownloadStatement;
                }
                else
                {
                    command.CommandText = table.BuildGetDataCommand(clobFile);
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
                DbDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    MessageLog.LogError($"No data found on clob retrieval of {clobFile.Mnemonic} when executing command: {command.CommandText}");
                    throw new FileDownloadException($"No data was found for the given command {command.CommandText}");
                }

                using (FileStream fs = Utils.WaitForFile(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                {
                    if (column.DataType == Column.Datatype.BLOB)
                    {
                        byte[] b = new byte[reader.GetBytes(0, 0, null, 0, int.MaxValue)];
                        reader.GetBytes(0, 0, b, 0, b.Length);
                        fs.Write(b, 0, b.Length);
                    }
                    else
                    {
                        string result = reader.GetString(0);
                        StreamWriter streamWriter = new StreamWriter(fs);
                        streamWriter.NewLine = "\n";
                        streamWriter.Write(result);
                        streamWriter.Close();
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

        /// <summary>
        /// Finds all DBClobFIles from the tables in the given ClobDirectory and populates its lists with them.
        /// </summary>
        /// <param name="databaseConnection">The connection to get the files for.</param>
        /// <param name="clobDir">The directory to get the file lists for.</param>
        /// <param name="oracleConnection">The Oracle connection to use.</param>
        private static void GetDatabaseFileListForDirectory(DatabaseConnection databaseConnection, ClobDirectory clobDir, OracleConnection oracleConnection)
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
                    oracleCommand.CommandText = table.GetFileListCommand();
                }

                ProcessFileList(databaseConnection, clobDir, oracleCommand, table);
            }
        }

        private static void ProcessFileList(DatabaseConnection databaseConnection, ClobDirectory clobDirectory, OracleCommand oracleCommand, Table table)
        {
            try
            {
                DbDataReader reader;

                reader = oracleCommand.ExecuteReader();
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
                        filename: Model.ConvertMnemonicToFilename(databaseConnection, mnemonic, table, mimeType));

                    clobDirectory.DatabaseFileList.Add(databaseFile);
                }

                reader.Close();
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleException || e is ColumnNotFoundException)
            {
                oracleCommand.Dispose();
                MessageLog.LogError($"Error retrieving file lists from database for {clobDirectory.ClobType.Name} when executing command {oracleCommand.CommandText}: {e}");
                throw new FileListRetrievalException($"Error retrieving file lists from database for {clobDirectory.ClobType.Name} when executing command {oracleCommand.CommandText}", e);
            }
            finally
            {
                oracleCommand.Dispose();
            }
        }

        /// <summary>
        /// Creates a comment to place at the bottom of a non-binary file of the given mime type.
        /// </summary>
        /// <param name="mimeType">The mime type of the file the footer is for.</param>
        /// <returns>The footer string.</returns>
        private static string GetClobFooterMessage(string mimeType)
        {
            string openingComment = "<!--";
            string closingComment = "-->";
            if (mimeType != null && (mimeType.Equals("text/javascript", StringComparison.OrdinalIgnoreCase) || mimeType.Equals("text/css", StringComparison.OrdinalIgnoreCase)))
            {
                openingComment = "/*";
                closingComment = "*/";
            }

            return $"{openingComment}"
                + $" Last clobbed by user {Environment.UserName}"
                + $" on machine {Environment.MachineName}"
                + $" at {DateTime.Now}"
                + $" (Lobster build {Utils.RetrieveLinkerTimestamp().ToShortDateString()})"
                + $"{closingComment}";
        }
    }
}
