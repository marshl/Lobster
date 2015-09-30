﻿//-----------------------------------------------------------------------
// <copyright file="LobsterModel.cs" company="marshl">
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
namespace Lobster
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;
    using Properties;

    /// <summary>
    /// The database connection model 
    /// </summary>
    public class LobsterModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LobsterModel"/> class.
        /// </summary>
        public LobsterModel()
        {
            this.MimeList = Common.DeserialiseXmlFileUsingSchema<MimeTypeList>("LobsterSettings/MimeTypes.xml", null);

            this.LoadDatabaseConnections();
        }

        /// <summary>
        /// The list of valid connection files fond in the connection directory.
        /// </summary>
        public List<DatabaseConnection> ConnectionList { get; private set; }

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
        public void SendUpdateClobMessage(ClobFile clobFile)
        {
            OracleConnection con = OpenConnection(this.CurrentConnection);
            bool result;
            if (con == null)
            {
                result = false;
            }
            else
            {
                result = this.UpdateDatabaseClob(clobFile, con);
                con.Dispose();
            }

            LobsterMain.Instance.OnFileUpdateComplete(clobFile, result);
        }

        /// <summary>
        /// Public access for inserting files into the database.
        /// </summary>
        /// <param name="clobFile">The file to insert into the database.</param>
        /// <param name="table">The table to insert the file into.</param>
        /// <param name="mimeType">The mimetype to insert the file as (if applicable, otherwise null).</param>
        /// <returns>True if the file was inserted, otherwise false.</returns>
        public bool SendInsertClobMessage(ClobFile clobFile, Table table, string mimeType)
        {
            OracleConnection con = OpenConnection(this.CurrentConnection);
            if (con == null)
            {
                return false;
            }

            bool result = this.InsertDatabaseClob(clobFile, table, mimeType, con);
            con.Dispose();
            return result;
        }

        /// <summary>
        /// Public access for downloading database files.
        /// </summary>
        /// <param name="clobFile">The database file that will be downloaded.</param>
        /// <returns>The path of the resulting file.</returns>
        public string SendDownloadClobDataToFileMessage(ClobFile clobFile)
        {
            OracleConnection con = OpenConnection(this.CurrentConnection);
            if (con == null)
            {
                return null;
            }

            string filepath = this.DownloadClobDataToFile(clobFile, con);
            con.Dispose();
            return filepath;
        }

        /// <summary>
        /// Sets the current connection to the given connection, if able.
        /// </summary>
        /// <param name="connection">The connection to open.</param>
        /// <returns>Whether making the connection was successful or not.</returns>
        public bool SetDatabaseConnection(DatabaseConnection connection)
        {
            MessageLog.LogInfo("Changing connection to " + connection.Name);
            using (OracleConnection con = OpenConnection(connection))
            {
                if (con == null)
                {
                    MessageLog.LogError("Could not change connection to " + connection.Name);
                    return false;
                }
            }

            if (connection.ClobTypeDir == null || !Directory.Exists(connection.ClobTypeDir))
            {
                connection.ClobTypeDir = Common.PromptForDirectory("Please select your Clob Type directory for " + connection.Name, connection.CodeSource);
                if (connection.ClobTypeDir != null)
                {
                    DatabaseConnection.SerialiseToFile(connection.FileLocation, connection);
                }
                else
                {
                    // Ignore config files that don't have a valid CodeSource folder
                    MessageLog.LogWarning("User cancelled change to ClobTypeDir, aborting connection change");
                    return false;
                }
            }

            this.CurrentConnection = connection;
            this.CurrentConnection.PopulateClobDirectories();
            this.RebuildLocalAndDatabaseFileLists();

            MessageLog.LogInfo("Connection change successful");
            return true;
        }

        /// <summary>
        /// Requeries the database and local directories for files. 
        /// </summary>
        public void RebuildLocalAndDatabaseFileLists()
        {
            Debug.Assert(this.CurrentConnection != null, "Cannot requery the database without being connected to one first");

            this.GetDatabaseFileLists();

            foreach (KeyValuePair<ClobType, ClobDirectory> pair in this.CurrentConnection.ClobTypeToDirectoryMap)
            {
                pair.Value.GetLocalFiles();
            }
        }

        /// <summary>
        /// Converts the name of a local file to a suitable database mnemonic.
        /// </summary>
        /// <param name="clobFile">The file to convert.</param>
        /// <param name="table">The table the file would be inserted into.</param>
        /// <param name="mimeType">The mime type the file would be inserted as.</param>
        /// <returns>The database mnemonic representation of the file.</returns>
        public string ConvertFilenameToMnemonic(ClobFile clobFile, Table table, string mimeType)
        {
            Debug.Assert(clobFile.LocalFile != null, "The file must be local to construct its database mnemonic");
            string mnemonic = Path.GetFileNameWithoutExtension(clobFile.LocalFile.FilePath);

            // If the table stores mime types, then the mnemonic will also need to have 
            // the prefix representation of the mime type prefixed to it.
            if (table.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE) != null)
            {
                MimeTypeList.MimeType mt = this.MimeList.MimeTypes.Find(x => x.Name == mimeType);
                if (mt == null)
                {
                    throw new ArgumentException("Unknown mime-to-prefix key " + mimeType);
                }

                if (mt.Prefix.Length > 0)
                {
                    mnemonic = mt.Prefix + '/' + mnemonic;
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
            if (table.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE) == null || prefix == null)
            {
                filename += table.DefaultExtension ?? ".xml";
            }
            else
            {
                MimeTypeList.MimeType mt = this.MimeList.MimeTypes.Find(x => x.Name == mimeType);

                if (mt == null)
                {
                    throw new ArgumentException("Unkown mime-to-extension key " + mimeType);
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
        private static OracleConnection OpenConnection(DatabaseConnection config)
        {
            try
            {
                OracleConnection con = new OracleConnection();
                con.ConnectionString = "User Id=" + config.Username
                    + ";Password=" + config.Password
                    + ";Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)("
                    + "HOST=" + config.Host + ")"
                    + "(PORT=" + config.Port + ")))(CONNECT_DATA="
                    + "(SID=" + config.SID + ")(SERVER=DEDICATED)))"
                    + ";Pooling=" + (config.UsePooling ? "true" : "false");

                MessageLog.LogInfo("Connecting to database " + config.Name + " using connection string " + con.ConnectionString);
                con.Open();
                return con;
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException || e is OracleException || e is FormatException)
                {
                    Common.ShowErrorMessage("Database Connection Failure", "Cannot open connection to database: " + e.Message);
                    MessageLog.LogError("Connection to Oracle failed: " + e.Message);
                    return null;
                }

                throw;
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
                string connectionDir = Common.PromptForDirectory("Please select your DatabaseConnections folder", null);

                if (connectionDir == null)
                {
                    throw new ConnectionDirNotFoundException();
                }

                Settings.Default.ConnectionDir = connectionDir;
                Settings.Default.Save();
            }

            this.ConnectionList = new List<DatabaseConnection>();
            foreach (string filename in Directory.GetFiles(Settings.Default.ConnectionDir))
            {
                DatabaseConnection connection = DatabaseConnection.LoadDatabaseConnection(filename, this);
                if (connection != null)
                {
                    this.ConnectionList.Add(connection);
                }
            }
        }

        /// <summary>
        /// Queries the database for all files in all ClobDireectories and stores them in the directory.
        /// </summary>
        private void GetDatabaseFileLists()
        {
            OracleConnection con = OpenConnection(this.CurrentConnection);
            if (con == null)
            {
                MessageLog.LogError("Connection failed, cannot diff files with database.");
                return;
            }

            try
            {
                foreach (KeyValuePair<ClobType, ClobDirectory> pair in this.CurrentConnection.ClobTypeToDirectoryMap)
                {
                    this.GetDatabaseFileListForDirectory(pair.Value, con);
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
        private bool UpdateDatabaseClob(ClobFile clobFile, OracleConnection con)
        {
            Debug.Assert(clobFile.IsSynced, "A clobfile must be synchronised with the database to reclob it");

            OracleTransaction trans = con.BeginTransaction();
            OracleCommand command = con.CreateCommand();
            Table table = clobFile.DatabaseFile.ParentTable;

            try
            {
                command.CommandText = table.BuildUpdateStatement(clobFile);
            }
            catch (ClobColumnNotFoundException e)
            {
                Common.ShowErrorMessage("Clob Data Fetch Error", e.Message);
                MessageLog.LogError(e.Message);
                return false;
            }

            try
            {
                this.AddFileDataParameter(command, clobFile, clobFile.DatabaseFile.ParentTable, clobFile.DatabaseFile.MimeType);
            }
            catch (IOException e)
            {
                trans.Rollback();
                Common.ShowErrorMessage("Clob Update Failed", "An IO Exception occurred when updating the database: " + e.Message);
                MessageLog.LogError("Clob update failed with message \"" + e.Message + "\" for command \"" + command.CommandText + "\"");
                return false;
            }

            int rowsAffected;
            try
            {
                MessageLog.LogInfo("Executing Update query: " + command.CommandText);
                rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected != 1)
                {
                    trans.Rollback();
                    Common.ShowErrorMessage("Clob Update Failed", rowsAffected + " rows were affected during the update (expected only 1). The transaction has been rolled back.");
                    MessageLog.LogError("In invalid number of rows (" + rowsAffected + ") were updated for command: " + command.CommandText);
                    return false;
                }

                trans.Commit();
                command.Dispose();
                MessageLog.LogInfo("Clob file update successful: " + clobFile.LocalFile.FilePath);
                return true;
            }
            catch (Exception e)
            {
                if (e is OracleException || e is InvalidOperationException)
                {
                    trans.Rollback();
                    Common.ShowErrorMessage("Clob Update Failed", "An invalid operation occurred when updating the database: " + e.Message);
                    MessageLog.LogError("Clob update failed: " + e.Message + " for command: " + command.CommandText);
                    return false;
                }

                throw;
            }
        }

        /// <summary>
        /// Adds the contents of the given file into the given command under the alias ":data"
        /// </summary>
        /// <param name="command">The command to add the data to.</param>
        /// <param name="clobFile">The local file which will have its data bound to the query.</param>
        /// <param name="table">The table that the file will be added to.</param>
        /// <param name="mimeType">The mime type the file will be added as, if any.</param>
        private void AddFileDataParameter(OracleCommand command, ClobFile clobFile, Table table, string mimeType)
        {
            Debug.Assert(clobFile.LocalFile != null, "Adding the contents of a local file to a command requires the file to be local");

            // Wait for the file to unlock
            using (FileStream fs = Common.WaitForFile(
                clobFile.LocalFile.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                Column column = table.Columns.Find(
                    x => x.ColumnPurpose == Column.Purpose.CLOB_DATA
                        && (mimeType == null || x.MimeTypeList.Contains(mimeType)));

                // Binary mode
                if (column.DataType == Column.Datatype.BLOB)
                {
                    byte[] fileData = new byte[fs.Length];
                    fs.Read(fileData, 0, Convert.ToInt32(fs.Length));
                    OracleParameter param = command.Parameters.Add("data", OracleDbType.Blob);
                    param.Value = fileData;
                }
                else
                {
                    // Text mode
                    StreamReader sr = new StreamReader(fs);
                    string contents = sr.ReadToEnd();
                    contents += this.GetClobFooterMessage(mimeType);
                    OracleDbType insertType = column.DataType == Column.Datatype.CLOB ? OracleDbType.Clob : OracleDbType.XmlType;
                    command.Parameters.Add("data", insertType, contents, ParameterDirection.Input);
                }
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
        private bool InsertDatabaseClob(ClobFile clobFile, Table table, string mimeType, OracleConnection con)
        {
            Debug.Assert(clobFile.IsLocalOnly, "The file must be local only for it to be inserted into the database");

            OracleCommand command = con.CreateCommand();
            OracleTransaction trans = con.BeginTransaction();
            string mnemonic = this.ConvertFilenameToMnemonic(clobFile, table, mimeType);

            if (table.ParentTable != null)
            {
                try
                {
                    command.CommandText = table.BuildInsertParentStatement(mnemonic);
                    MessageLog.LogInfo("Executing Insert query on parent table: " + command.CommandText);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (Exception e)
                {
                    if (e is InvalidOperationException || e is OracleException)
                    {
                        Common.ShowErrorMessage(
                            "Clob Insert Error",
                            "An exception occurred when inserting into the parent table of " + clobFile.LocalFile.FilePath + ": " + e.Message);
                        MessageLog.LogError("Error creating new clob: " + e.Message + " when executing command: " + command.CommandText);
                        return false;
                    }

                    throw;
                }
            }

            command = con.CreateCommand();
            command.CommandText = table.BuildInsertChildStatement(mnemonic, mimeType);

            this.AddFileDataParameter(command, clobFile, table, mimeType);

            try
            {
                MessageLog.LogInfo("Executing Insert query: " + command.CommandText);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException || e is OracleException)
                {
                    // Discard the insert amde into the parent table
                    trans.Rollback();
                    Common.ShowErrorMessage(
                        "Clob Insert Error",
                        "An invalid operation occurred when inserting " + clobFile.LocalFile.FilePath + ": " + e.Message);
                    MessageLog.LogError("Error creating new clob: " + e.Message + " when executing command: " + command.CommandText);
                    return false;
                }

                throw;
            }

            command.Dispose();
            trans.Commit();

            clobFile.DatabaseFile = new DBClobFile()
            {
                Mnemonic = mnemonic,
                Filename = Path.GetFileName(clobFile.LocalFile.FilePath),
                ParentTable = table,
                MimeType = mimeType
            };

            MessageLog.LogInfo("Clob file creation successful: " + clobFile.LocalFile.FilePath);
            return true;
        }

        /// <summary>
        /// Downloads the current content of a file on the database to a local temp file.
        /// </summary>
        /// <param name="clobFile">The file to download.</param>
        /// <param name="con">The Oracle connection to use.</param>
        /// <returns>The path of the temporary file, if it exists.</returns>
        private string DownloadClobDataToFile(ClobFile clobFile, OracleConnection con)
        {
            Debug.Assert(clobFile.DatabaseFile != null, "The ClobFile must be on the database for it to be downloaded");
            OracleCommand command = con.CreateCommand();

            Table table = clobFile.DatabaseFile.ParentTable;
            Column column;
            try
            {
                column = clobFile.DatabaseFile.GetColumn();
                command.CommandText = table.BuildGetDataCommand(clobFile);
            }
            catch (ClobColumnNotFoundException e)
            {
                Common.ShowErrorMessage("Clob Data Fetch Error", e.Message);
                MessageLog.LogError(e.Message);
                return null;
            }

            try
            {
                OracleDataReader reader = command.ExecuteReader();
                string filepath = Common.CreateTempFile(clobFile.DatabaseFile.Filename);

                if (reader.Read())
                {
                    if (column.DataType == Column.Datatype.BLOB)
                    {
                        OracleBlob blob = reader.GetOracleBlob(0);
                        File.WriteAllBytes(filepath, blob.Value);
                    }
                    else
                    {
                        string result;
                        Column clobColumn = clobFile.DatabaseFile.ParentTable.Columns.Find(x => x.ColumnPurpose == Column.Purpose.CLOB_DATA);
                        if (clobColumn.DataType == Column.Datatype.CLOB)
                        {
                            OracleClob clob = reader.GetOracleClob(0);
                            result = clob.Value;
                        }
                        else
                        {
                            OracleXmlType xml = reader.GetOracleXmlType(0);
                            result = xml.Value;
                        }

                        StreamWriter streamWriter = File.AppendText(filepath);
                        streamWriter.Write(result);
                        streamWriter.Close();
                    }

                    if (!reader.Read())
                    {
                        reader.Close();
                        return filepath;
                    }
                    else
                    {
                        reader.Close();
                        Common.ShowErrorMessage(
                            "Clob Data Fetch Error",
                            "Too many rows were found for " + clobFile.DatabaseFile.Mnemonic);

                        MessageLog.LogError("Too many rows found on clob retrieval of " + clobFile.DatabaseFile.Mnemonic);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException || e is OracleNullValueException)
                {
                    Common.ShowErrorMessage(
                        "Clob Data Fetch Error",
                        "An invalid operation occurred when retreiving the data of " + clobFile.DatabaseFile.Mnemonic + ": " + e.Message);
                    MessageLog.LogError("Error retrieving data: " + e.Message + " when executing command " + command.CommandText);
                    return null;
                }

                throw;
            }

            Common.ShowErrorMessage(
                "Clob Data Fetch Error",
                "No data was found for " + clobFile.DatabaseFile.Mnemonic);
            MessageLog.LogError("No data found on clob retrieval of " + clobFile.DatabaseFile.Mnemonic + " when executing command: " + command.CommandText);
            return null;
        }

        /// <summary>
        /// Finds all DBClobFIles from the tables in the given ClobDirectory and populates its lists with them.
        /// </summary>
        /// <param name="clobDir">The directory to get the file lists for.</param>
        /// <param name="con">The Oracle connection to use.</param>
        private void GetDatabaseFileListForDirectory(ClobDirectory clobDir, OracleConnection con)
        {
            clobDir.DatabaseFileList = new List<DBClobFile>();
            ClobType ct = clobDir.ClobType;
            foreach (Table table in ct.Tables)
            {
                OracleCommand command = con.CreateCommand();
                command.CommandText = table.GetFileListCommand();
                OracleDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        DBClobFile databaseFile = new DBClobFile();
                        databaseFile.Mnemonic = reader.GetString(0);
                        databaseFile.MimeType = table.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE) != null ? reader.GetString(1) : null;
                        databaseFile.Filename = this.ConvertMnemonicToFilename(databaseFile.Mnemonic, table, databaseFile.MimeType);
                        databaseFile.ParentTable = table;
                        clobDir.DatabaseFileList.Add(databaseFile);
                    }

                    reader.Close();
                }
                catch (InvalidOperationException e)
                {
                    command.Dispose();
                    Common.ShowErrorMessage(
                        "Directory Comparison Error",
                        "An invalid operation occurred when retriving the file list for  " + ct.Name);
                    MessageLog.LogError("Error comparing to database: " + e.Message + " when executing command " + command.CommandText);
                    return;
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
            return (mimeType == "text/javascript" ? "/*" : "<!--")
                + " Last clobbed by user " + Environment.UserName
                + " on machine " + Environment.MachineName
                + " at " + DateTime.Now
                + " (Lobster build " + Common.RetrieveLinkerTimestamp().ToShortDateString() + ")"
                + (mimeType == "text/javascript" ? "*/" : "-->");
        }

        /// <summary>
        /// An exception for when the user refuses to select a Connection Directory
        /// </summary>
        /// TODO: Kill this sumbitch
        [Serializable]
        public class ConnectionDirNotFoundException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
            /// </summary>
            public ConnectionDirNotFoundException()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
            /// </summary>
            /// <param name="message">The exception messaage.</param>
            public ConnectionDirNotFoundException(string message) : base(message)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
            /// </summary>
            /// <param name="message">The exception message.</param>
            /// <param name="innerException">The inner exception.</param>
            public ConnectionDirNotFoundException(string message, Exception innerException) : base(message, innerException)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
            /// </summary>
            /// <param name="info">The serialisation info.</param>
            /// <param name="context">The streaming context.</param>
            protected ConnectionDirNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}
