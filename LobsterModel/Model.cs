﻿//-----------------------------------------------------------------------
// <copyright file="Model.cs" company="marshl">
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
    using System.Runtime.Serialization;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;
    using Properties;

    /// <summary>
    /// The database connection model 
    /// </summary>
    public class Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// <param name="eventListener">The listener that will receive events generated by this model.</param>
        /// </summary>
        public Model(IModelEventListener eventListener)
        {
            this.eventListener = eventListener;
            this.MimeList = Utils.DeserialiseXmlFileUsingSchema<MimeTypeList>("LobsterSettings/MimeTypes.xml", null);
        }

        private IModelEventListener eventListener;

        /// <summary>
        /// The current connection that is being used by this model.
        /// </summary>
        public DatabaseConnection CurrentConnection { get; set; }

        /// <summary>
        /// The list of temporary files that have been downloaded so far.
        /// These files are deleted when the model is disposed.
        /// </summary>
        public List<string> TempFileList { get; private set; } = new List<string>();

        /// <summary>
        /// The list of mime types that are used to translate from file names to database mnemonics and vice-sersa.
        /// </summary>
        private MimeTypeList MimeList { get; set; }

        /// <summary>
        /// public facing method for updating a database file with its local content.
        /// </summary>
        /// <param name="clobFile">The file to update.</param>
        public void SendUpdateClobMessage(string fullpath)
        {
            DbConnection con = OpenConnection(this.CurrentConnection.Config);

            ClobDirectory clobDir = this.CurrentConnection.GetClobDirectoryForFile(fullpath);
            DBClobFile clobFile = clobDir?.GetDatabaseFileForFullpath(fullpath);

            if (Settings.Default.BackupEnabled)
            {
                this.BackupClobFile(con, clobFile, fullpath);
            }

            this.UpdateDatabaseClob(fullpath, clobFile, con);
            con.Dispose();
        }

        private void BackupClobFile(DbConnection con, DBClobFile clobFile, string fullpath)
        {
            DirectoryInfo backupDir = new DirectoryInfo(Settings.Default.BackupDirectory);
            if (!backupDir.Exists)
            {
                backupDir.Create();
            }

            string tempName = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now)
                + Path.GetFileName(fullpath);
            string filepath = Path.Combine(Settings.Default.BackupDirectory, tempName);
            this.DownloadClobDataToFile(clobFile, con, filepath);
        }

        /// <summary>
        /// Public access for inserting files into the database.
        /// </summary>
        /// <param name="clobFile">The file to insert into the database.</param>
        /// <param name="table">The table to insert the file into.</param>
        /// <param name="mimeType">The mimetype to insert the file as (if applicable, otherwise null).</param>
        /// <returns>True if the file was inserted, otherwise false.</returns>
        public void SendInsertClobMessage(string fullpath)
        {
            ClobDirectory clobDir = this.CurrentConnection.GetClobDirectoryForFile(fullpath);

            Table table;
            if (clobDir.ClobType.Tables.Count > 1)
            {
                table = this.eventListener.PromptForTable(fullpath);
            }
            else
            {
                table = clobDir.ClobType.Tables[0];
            }

            string mimeType = null;
            Column mimeTypeColumn;
            if (table.TryGetColumnWithPurpose(Column.Purpose.MIME_TYPE, out mimeTypeColumn))
            {
                this.eventListener.PromptForMimeType(fullpath, table);
            }

            DbConnection con = OpenConnection(this.CurrentConnection.Config);
            try
            {
                this.InsertDatabaseClob(fullpath, table, mimeType, con);
            }
            finally
            {
                con.Dispose();
            }
        }

        /// <summary>
        /// Public access for downloading database files.
        /// </summary>
        /// <param name="clobFile">The database file that will be downloaded.</param>
        /// <returns>The path of the resulting file.</returns>
        public string SendDownloadClobDataToFileMessage(DBClobFile clobFile)
        {
            DbConnection con = OpenConnection(this.CurrentConnection.Config);
            if (con == null)
            {
                return null;
            }

            string filepath = Utils.GetTempFilepath(clobFile.Filename);
            try
            {
                this.DownloadClobDataToFile(clobFile, con, filepath);
                return filepath;
            }
            finally
            {
                con.Dispose();
            }
        }

        public List<DatabaseConfig> GetConfigList()
        {
            List<DatabaseConfig> configList = new List<DatabaseConfig>();

            if (!this.IsConnectionDirectoryValid)
            {
                return configList;
            }

            foreach (string filename in Directory.GetFiles(Settings.Default.ConnectionDir))
            {
                DatabaseConfig connection = DatabaseConfig.LoadDatabaseConfig(filename);
                if (connection != null)
                {
                    configList.Add(connection);
                }
            }
            return configList;
        }

        /// <summary>
        /// Sets the current connection to the given connection, if able.
        /// </summary>
        /// <param name="config">The connection to open.</param>
        /// <returns>Whether making the connection was successful or not.</returns>
        public bool SetDatabaseConnection(DatabaseConfig config)
        {
            MessageLog.LogInfo($"Changing connection to {config.Name}");
            using (DbConnection con = OpenConnection(config))
            {
                if (con == null)
                {
                    MessageLog.LogError($"Could not change connection to {config.Name}");
                    return false;
                }
            }

            if (config.ClobTypeDir == null || !Directory.Exists(config.ClobTypeDir))
            {
                config.ClobTypeDir = Utils.PromptForDirectory($"Please select your Clob Type directory for {config.Name}", config.CodeSource);
                if (config.ClobTypeDir != null)
                {
                    DatabaseConfig.SerialiseToFile(config.FileLocation, config);
                }
                else
                {
                    // Ignore config files that don't have a valid CodeSource folder
                    MessageLog.LogWarning("User cancelled change to ClobTypeDir, aborting connection change");
                    return false;
                }
            }

            this.CurrentConnection = new DatabaseConnection(this, config);
            this.CurrentConnection.LoadClobTypes();
            this.GetDatabaseFileLists();

            MessageLog.LogInfo("Connection change successful");
            return true;
        }

        /// <summary>
        /// Converts the name of a local file to a suitable database mnemonic.
        /// </summary>
        /// <param name="clobFile">The file to convert.</param>
        /// <param name="table">The table the file would be inserted into.</param>
        /// <param name="mimeType">The mime type the file would be inserted as.</param>
        /// <returns>The database mnemonic representation of the file.</returns>
        public string ConvertFilenameToMnemonic(string fullpath, Table table, string mimeType)
        {
            string mnemonic = Path.GetFileNameWithoutExtension(fullpath);

            // If the table stores mime types, then the mnemonic will also need to have 
            // the prefix representation of the mime type prefixed to it.
            Column mimeTypeColumn;
            if (table.TryGetColumnWithPurpose(Column.Purpose.MIME_TYPE, out mimeTypeColumn))
            {
                MimeTypeList.MimeType mt = this.MimeList.MimeTypes.Find(x => x.Name.Equals(mimeType));
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
        /// <param name="mnemonic">The database mnemonic.</param>
        /// <param name="table">The table the mnemonic is from.</param>
        /// <param name="mimeType">The mime type of the database file, if applicable.</param>
        /// <returns>The name as converted from the mnemonic.</returns>
        public string ConvertMnemonicToFilename(string mnemonic, Table table, string mimeType)
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
            if (!table.TryGetColumnWithPurpose(Column.Purpose.MIME_TYPE, out mimeTypeColumn) || prefix == null)
            {
                filename += table.DefaultExtension ?? ".xml";
            }
            else
            {
                MimeTypeList.MimeType mt = this.MimeList.MimeTypes.Find(x => x.Name.Equals(mimeType));

                if (mt == null)
                {
                    throw new MimeTypeNotFoundException($"Unkown mime-to-extension key {mimeType}");
                }

                filename += mt.Extension;
            }

            return filename;
        }

        /// <summary>
        /// Opens a new OracleConnection and returns it.
        /// </summary>
        /// <param name="config">The connection configuration settings to use.</param>
        /// <returns>A new connectionif it opened successfully, otherwise null.</returns>
        private static DbConnection OpenConnection(DatabaseConfig config)
        {
            try
            {
                OracleConnection con = new OracleConnection();
                con.ConnectionString = "User Id=" + config.Username
                    + (string.IsNullOrEmpty(config.Password) ? null : ";Password=" + config.Password)
                    + ";Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)("
                    + "HOST=" + config.Host + ")"
                    + "(PORT=" + config.Port + ")))(CONNECT_DATA="
                    + "(SID=" + config.SID + ")(SERVER=DEDICATED)))"
                    + ";Pooling=" + (config.UsePooling ? "true" : "false");

                MessageLog.LogInfo("Connecting to database " + config.Name + " using connection string " + con.ConnectionString);
                con.Open();
                return con;
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleException)
            {
                MessageLog.LogError($"Connection to Oracle failed: {e.Message}");
                throw new DatabaseConnectionFailedException(e);
            }
        }

        /// <summary>
        /// Loads all database connections found in the 
        /// </summary>
        private void LoadDatabaseConnections()
        {
            // TODO: Get rid of this shit
            if (Settings.Default.ConnectionDir == null || !Directory.Exists(Settings.Default.ConnectionDir))
            {
                string connectionDir = Utils.PromptForDirectory("Please select your DatabaseConnections folder", null);

                if (connectionDir == null)
                {
                    throw new ConnectionDirNotFoundException();
                }

                Settings.Default.ConnectionDir = connectionDir;
                Settings.Default.Save();
            }
        }

        public bool IsConnectionDirectoryValid
        {
            get
            {
                return Settings.Default.ConnectionDir != null && Directory.Exists(Settings.Default.ConnectionDir);
            }
        }

        public string ConnectionDirectory
        {
            get
            {
                return Settings.Default.ConnectionDir;
            }
        }

        /// <summary>
        /// Queries the database for all files in all ClobDireectories and stores them in the directory.
        /// </summary>
        private void GetDatabaseFileLists()
        {
            DbConnection con = OpenConnection(this.CurrentConnection.Config);
            if (con == null)
            {
                MessageLog.LogError("Connection failed, cannot diff files with database.");
                return;
            }

            try
            {
                foreach (ClobDirectory clobDir in this.CurrentConnection.ClobDirectoryList)
                {
                    this.GetDatabaseFileListForDirectory(clobDir, con);
                }
            }
            finally
            {
                con.Dispose();
            }
        }

        /// <summary>
        /// Inserts the given file into the database.
        /// If the insert is successful, then the database information about that file will be set.
        /// </summary>
        /// <param name="clobFile">The file to insert into the database.</param>
        /// <param name="con">The Oracle connction to use.</param>
        /// <returns>True if the file was updated successfully, otherwise false.</returns>
        private void UpdateDatabaseClob(string fullpath, DBClobFile clobFile, DbConnection con)
        {
            DbTransaction trans = con.BeginTransaction();
            DbCommand command = con.CreateCommand();
            Table table = clobFile.ParentTable;

            try
            {
                command.CommandText = table.BuildUpdateStatement(clobFile);
            }
            catch (ColumnNotFoundException e)
            {
                MessageLog.LogError(e.Message);
                throw new FileUpdateFailedException("The Clob Column could not be found.", e);
            }

            try
            {
                this.AddFileDataParameter(command, fullpath, clobFile.ParentTable, clobFile.MimeType);
            }
            catch (IOException e)
            {
                MessageLog.LogError("Clob update failed with message \"" + e.Message + "\" for command \"" + command.CommandText + "\"");
                throw new FileUpdateFailedException("An IOException ocurred when attempting to add the file as a data parameter.", e);
            }

            int rowsAffected;
            try
            {
                MessageLog.LogInfo($"Executing Update query: {command.CommandText}");
                rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected != 1)
                {
                    trans.Rollback();
                    MessageLog.LogError($"In invalid number of rows ({rowsAffected}) were updated for command: {command.CommandText}");

                    throw new FileUpdateFailedException(rowsAffected + " rows were affected during the update (expected only 1). The transaction has been rolled back.");
                }

                trans.Commit();
                command.Dispose();
                MessageLog.LogInfo($"Clob file update successful: {fullpath}");
                return;
            }
            catch (Exception e) when (e is OracleException || e is InvalidOperationException)
            {
                trans.Rollback();
                MessageLog.LogError($"Clob update failed: {e.Message} for command: {command.CommandText}");
                throw new FileUpdateFailedException($"An invalid operation occurred when updating the database: {e.Message}", e);
            }
        }

        /// <summary>
        /// Adds the contents of the given file into the given command under the alias ":data"
        /// </summary>
        /// <param name="command">The command to add the data to.</param>
        /// <param name="clobFile">The local file which will have its data bound to the query.</param>
        /// <param name="table">The table that the file will be added to.</param>
        /// <param name="mimeType">The mime type the file will be added as, if any.</param>
        private void AddFileDataParameter(DbCommand command, string fullpath, Table table, string mimeType)
        {
            // Wait for the file to unlock
            using (FileStream fs = Utils.WaitForFile(
                fullpath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                Column column = table.Columns.Find(
                    x => x.ColumnPurpose == Column.Purpose.CLOB_DATA
                        && (mimeType == null || x.MimeTypeList.Contains(mimeType)));

                DbParameter param = command.CreateParameter();
                param.ParameterName = "data";

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
                    contents += this.GetClobFooterMessage(mimeType);

                    param.Value = contents;
                    OracleParameter op = (OracleParameter)param;
                    op.OracleDbType = column.DataType == Column.Datatype.XMLTYPE ? OracleDbType.XmlType : OracleDbType.Clob;
                }

                command.Parameters.Add(param);
            }
        }

        /// <summary>
        /// Inserts the given local file into the database.
        /// </summary>
        /// <param name="clobFile">The local-only file to insert.</param>
        /// <param name="table">The table to insert the file into.</param>
        /// <param name="mimeType">The mimetype to insert the file as.</param>
        /// <param name="con">The Oracle connection to use.</param>
        /// <returns>True if the file was inserted successfully, otherwise false.</returns>
        private DBClobFile InsertDatabaseClob(string fullpath, Table table, string mimeType, DbConnection con)
        {
            DbCommand command = con.CreateCommand();
            // Execute the insert in a transaction so that if inserting the child record fails, the parent insert can be rolled back
            DbTransaction trans = con.BeginTransaction();
            string mnemonic = this.ConvertFilenameToMnemonic(fullpath, table, mimeType);

            // First insert the parent record in the table (if applicable)
            if (table.ParentTable != null)
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
                    MessageLog.LogError($"Error creating new clob: {e.Message} when executing command: {command.CommandText}");
                    throw new FileInsertFailedException("An exception ocurred when attempting to insert a file into the parent table.", e);
                }
            }

            try
            {
                command = con.CreateCommand();
                command.CommandText = table.BuildInsertChildStatement(mnemonic, mimeType);
                this.AddFileDataParameter(command, fullpath, table, mimeType);
                MessageLog.LogInfo($"Executing insert query: {command.CommandText}");
                command.ExecuteNonQuery();
                command.Dispose();
                trans.Commit();
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleException || e is IOException)
            {
                // Discard the insert amde into the parent table
                trans.Rollback();
                MessageLog.LogError($"Error creating new clob: {e.Message} when executing command: {command.CommandText}");
                throw new FileInsertFailedException("An exception ocurred when attempting to insert a file into the child table.", e);
            }

            DBClobFile clobFile = new DBClobFile(table, mnemonic, mimeType, fullpath);
            MessageLog.LogInfo($"Clob file creation successful: {fullpath}");
            return clobFile;
        }

        /// <summary>
        /// Downloads the current content of a file on the database to a local temp file.
        /// </summary>
        /// <param name="clobFile">The file to download.</param>
        /// <param name="con">The connection to use.</param>
        /// <param name="filename">The filepath to download the data into.</param>
        /// <returns>The path of the temporary file, if it exists.</returns>
        private void DownloadClobDataToFile(DBClobFile clobFile, DbConnection con, string filename)
        {
            DbCommand command = con.CreateCommand();

            Table table = clobFile.ParentTable;
            Column column;
            try
            {
                column = clobFile.GetDataColumn();
                command.CommandText = table.BuildGetDataCommand(clobFile);
            }
            catch (ColumnNotFoundException e)
            {
                MessageLog.LogError(e.Message);
                throw new FileDownloadException("The statement could not be constructed.", e);
            }

            try
            {
                DbDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    MessageLog.LogError($"No data found on clob retrieval of {clobFile.Mnemonic} when executing command: " + command.CommandText);
                    throw new FileDownloadException($"No data was found for the given command {command.CommandText}");
                }

                if (column.DataType == Column.Datatype.BLOB)
                {
                    byte[] b = new Byte[(reader.GetBytes(0, 0, null, 0, int.MaxValue))];
                    reader.GetBytes(0, 0, b, 0, b.Length);
                    File.WriteAllBytes(filename, b);
                }
                else
                {
                    string result = reader.GetString(0);
                    StreamWriter streamWriter = File.AppendText(filename);
                    streamWriter.Write(result);
                    streamWriter.Close();
                }

                if (!reader.Read())
                {
                    reader.Close();
                    return;
                }
                else // Too many rows
                {
                    reader.Close();
                    MessageLog.LogError($"Too many rows found on clob retrieval of {clobFile.Mnemonic}");
                    throw new FileDownloadException($"Too many rows were found for the given command {command.CommandText}");
                }
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleNullValueException)
            {
                MessageLog.LogError("Error retrieving data: " + e.Message + " when executing command " + command.CommandText);
                throw new FileDownloadException($"An exception ocurred when executing the command {command.CommandText}");
            }
        }

        /// <summary>
        /// Finds all DBClobFIles from the tables in the given ClobDirectory and populates its lists with them.
        /// </summary>
        /// <param name="clobDir">The directory to get the file lists for.</param>
        /// <param name="con">The Oracle connection to use.</param>
        private void GetDatabaseFileListForDirectory(ClobDirectory clobDir, DbConnection con)
        {
            clobDir.DatabaseFileList = new List<DBClobFile>();
            ClobType ct = clobDir.ClobType;
            foreach (Table table in ct.Tables)
            {
                DbCommand command = con.CreateCommand();
                command.CommandText = table.GetFileListCommand();
                DbDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string mnemonic = reader.GetString(0);
                        Column mimeTypeCol;
                        string mimeType = null;
                        if (table.TryGetColumnWithPurpose(Column.Purpose.MIME_TYPE, out mimeTypeCol))
                        {
                            mimeType = reader.GetString(1);
                        }

                        DBClobFile databaseFile = new DBClobFile(
                            parentTable: table,
                            mnemonic: mnemonic,
                            mimetype: mimeType,
                            filename: this.ConvertMnemonicToFilename(mnemonic, table, mimeType)
                        );
                        clobDir.DatabaseFileList.Add(databaseFile);
                    }

                    reader.Close();
                }
                catch (Exception e) when (e is InvalidOperationException || e is OracleException)
                {
                    command.Dispose();
                    throw new FileListRetrievalException($"Error retrieving file lists from database for {clobDir.ClobType.Name} when executing command {command.CommandText}", e);
                }
                finally
                {
                    command.Dispose();
                }
            }
        }

        /// <summary>
        /// Creates a comment to place at the bottom of a non-binary file of the given mime type.
        /// </summary>
        /// <param name="mimeType">The mime type of the file the footer is for.</param>
        /// <returns>The footer string.</returns>
        private string GetClobFooterMessage(string mimeType)
        {
            string openingComment = "<!--";
            string closingComment = "-->";
            if (mimeType != null && mimeType.Equals("text/javascript", StringComparison.OrdinalIgnoreCase))
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

        public void ChnageConnectionDirectory(string fileName)
        {
            Settings.Default.ConnectionDir = fileName;
            Settings.Default.Save();
        }
    }
}